using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using Presec.Service.Models;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Presec.Service.MongoModels;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Globalization;

namespace Presec.Service.Repositories
{
    public class StationRepository
    {
        private static string[] routeNames = new[] { "улица", "площадь", "набережная", "проспект", "тупик", "шоссе", "проезд", "наб.", "ул.", "пр.", "ш.", "пл." };
        private static string[] routeNamesWell = new[] { "улица", "площадь", "набережная", "проспект", "тупик", "шоссе", "проезд", "набережная", "улица", "проезд", "шоссе", "площадь" };
        private static string[] premiseNames = new[] { "дом", "строение", "корпус", "здание", "стр.", "к.", "д." };
        private static string[] premiseNamesWell = new[] { "дом", "строение", "корпус", "здание", "строение", "корпус", "дом" };

        public Station GetOne(string id)
        {
            return new Station { id = 0 };
        }

        public IEnumerable<Station> GetAll(ODataQueryOperation operation)
        {
            var addr = operation.ContextParameters.ContainsKey("addr") ? operation.ContextParameters["addr"] : null;
            var gref = operation.ContextParameters.ContainsKey("gref") ? operation.ContextParameters["gref"] : null;

            if (!string.IsNullOrEmpty(addr))
            {
                
                var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

                var hostName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$1");
                var dbName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$2");


                var server = MongoServer.Create(hostName);
                server.Connect();
                var db = server.GetDatabase(dbName);
                var collection = db.GetCollection<Doc>("moscow");

                var res = GetAllByStrictSearch(collection, addr);

                if (res.Count() == 0)
                {
                    //strict condition not found

                    //search by loosed

                    //search by geo
                    if (!string.IsNullOrEmpty(gref))
                    {
                        res = GetAllByGoogleRef(collection, gref);
                    }

                }

                return this.Map(collection, res);
            }

            return new Station[0];
        }

        public IEnumerable<Doc> GetAllByStrictSearch(MongoCollection<Doc> Collection, string Addr)
        {
            var spts = GetAddr(Addr);

            string regex = null;

            if (spts[1] == null)
            {
                regex = string.Format(".*{0}.*", spts[0].Split('|')[1]);
            }
            else
            {
                regex = string.Format(@".*{0}.*{1}.*", spts[0].Split('|')[1], spts[1].Split('|')[1]);
            }

            return Collection.Find(Query.Matches("boundary", BsonRegularExpression.Create(regex, "i"))).Take(5).ToArray();
        }

        public IEnumerable<Doc> GetAllByGoogleRef(MongoCollection<Doc> Collection, string GoogleRef)
        {
            //https://maps.googleapis.com/maps/api/place/details/output?parameters


            System.Net.WebClient client = new System.Net.WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.QueryString.Add("reference", GoogleRef);
            client.QueryString.Add("sensor", "false");
            client.QueryString.Add("types", "geocode");
            client.QueryString.Add("key", ConfigurationManager.AppSettings["GoogleAPIKey"]);


            System.IO.Stream data = client.OpenRead("https://maps.googleapis.com/maps/api/place/details/xml");
            System.IO.StreamReader reader = new System.IO.StreamReader(data);
            string str = reader.ReadToEnd();

            data.Close();
            reader.Close();

            XDocument xdoc = XDocument.Parse(str);
            var status = xdoc.Root.Descendants("status").Single();
            if (status.Value == "OK")
            {
                var geoNode = xdoc.Root.Descendants("result").Single().Descendants("geometry").Single().Descendants("location").Single();
                double lat = double.Parse(geoNode.Descendants("lat").Single().Value, CultureInfo.InvariantCulture);
                double lng = double.Parse(geoNode.Descendants("lng").Single().Value, CultureInfo.InvariantCulture);


                return Collection.Find(Query.Near("station.geo", lat, lng)).Take(5).ToArray();;
            }

            return new Doc[0];

        }

        private IEnumerable<Station> Map(MongoCollection<Doc> Collection,  IEnumerable<Doc> Docs)
        {
            return Docs.ToArray().Select((p) =>
            {
                var st = new Station
                {
                    id = p._id
                };

                st.lines = p.boundary.ToArray().Select(s => new Line { addr = s }).ToArray();

                st.station = new Address
                {
                    addr = p.station.addr,
                    aux = p.station.aux,
                    org = p.station.org,
                    phone = p.station.phone,
                    geo = p.station.geo != null ? new GeoPoint { lat = p.station.geo[0], lon = p.station.geo[1] } : null
                };

                st.uik = new Address
                {
                    addr = p.uik.addr,
                    aux = p.uik.aux,
                    org = p.uik.org,
                    phone = p.station.phone,
                    geo = p.uik.geo != null ? new GeoPoint { lat = p.uik.geo[0], lon = p.uik.geo[1] } : null
                };

                if (st.station.geo != null)
                {
                    var q1 = Collection.Find(Query.Near("station.geo", st.station.geo.lat, st.station.geo.lon, 5));

                    st.near = q1.Take(5).Select(s => new Station { id = s._id, lines = s.boundary.Select(x => new Line { addr = x }).ToArray() }).ToArray();
                }

                return st;
            });

        }


        public string[] GetAddr(string RawAddr)
        {
            // var regexStr = @"^([\d]*?[\w\s]+?[^\d])([\w\.\,]*[^\d])([\d]+.*)$";
            var regexStr = @"^([\d]*?\b[^\d]+)\b(\d+.*)";
            RawAddr = Regex.Replace(RawAddr, @"[\.,;:]", " ");

            //find in clenead address - route, then premise, (premise contains any number + к or c)
            var matches = Regex.Matches(RawAddr, regexStr, RegexOptions.IgnoreCase);

            //if matches.cnt = 0 consider string consists only from route part
            //else
            //1st - route part + type
            //2nd - premise type or part of the 1st
            //3d - premise part

            string routePart = RawAddr;
            string premisePart = null;
            if (matches.Count > 0)
            {
                routePart = matches[0].Groups[1].Value.Trim();
                premisePart = matches[0].Groups[2].Value.Trim();
                string premisePartWell = null;
                var i = routePart.LastIndexOf(' ');
                if (i != -1)
                {
                    var mabyRoutePartContainsPremiseType = routePart.Substring(i).Trim();
                    premisePartWell = GetPremiseWell(mabyRoutePartContainsPremiseType);
                }
                if (premisePartWell != null)
                {
                    routePart = routePart.Remove(i);
                    premisePart = premisePartWell + " " + premisePart;
                }
            }

            var routeRegex = string.Format(@"^(.*)\b({0})\s?(.*)", string.Join("|", routeNames.Select(p => p.Replace(".", @"\.?"))));

            matches = Regex.Matches(routePart, routeRegex, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                var well = GetRouteWell(matches[0].Groups[2].Value);

                routePart = Regex.Replace(routePart, routeRegex, "$1$3", RegexOptions.IgnoreCase).Trim();
                routePart = well + "|" + routePart;
            }
            else
            {
                routePart = "|" + routePart;
            }

            if (premisePart != null)
            {
                var premiseRegex = string.Format(@"^(.*)\b({0})\s?(.*)", string.Join("|", premiseNames.Select(p => p.Replace(".", @"\.?"))));

                matches = Regex.Matches(premisePart, premiseRegex, RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    var well = GetPremiseWell(matches[0].Groups[2].Value);
                    premisePart = Regex.Replace(premisePart, premiseRegex, "$1$3", RegexOptions.IgnoreCase).Trim();
                    premisePart = well + "|" + premisePart;
                }
                else
                {
                    premisePart = "|" + premisePart;
                }
            }

            return new string[] { routePart.ToLower().Trim(), premisePart != null ? premisePart.ToLower().Trim() : null };
        }

        private string GetRouteWell(string Route)
        {
            var i = routeNames.Select(p => p.Trim('.')).ToList().IndexOf(Route.Trim('.'));

            return i != -1 ? routeNamesWell.ToArray()[i] : null;
        }

        private string GetPremiseWell(string Premise)
        {
            var i = premiseNames.Select(p => p.Trim('.')).ToList().IndexOf(Premise.Trim('.'));

            return i != -1 ? premiseNamesWell.ToArray()[i] : null;
        }

    }
}