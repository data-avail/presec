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
        private static string routeRegex = ".*({0}).*";
        private static string routeWithPremiseRegex = ".*({0}).*({1}).*";
        private static string[][] routeMapping = new[] {
             new[] { "к.", "корпус" },
             new[] { "просп.", "проспект" }
            };
        private static string[][] premiseMapping = new[] {
             new[] { "д.", "дом" },
             new[] { "просп.", "проспект" }
            };


        public Station GetOne(string key)
        {
            var matchType = "not_found";

            if (!string.IsNullOrEmpty(key))
            {
                var cache = new Cahching.Cache<Station>();
                var k = Regex.Replace(key, @"[\s\.,;:]", "");
                Station station = cache.Get(k);
                if (station != null)
                    return station;

                int id;
                var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

                var hostName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$1");
                var dbName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$2");

                var server = MongoServer.Create(hostName);
                server.Connect();
                var db = server.GetDatabase(dbName);
                var collection = db.GetCollection<Doc>("moscow");

                IEnumerable<Doc> found = null;
                FoundBy foundBy = null;

                string foundPattern = null;

                if (!int.TryParse(key, out id))
                {
                    found = GetAllByStrictSearch(collection, key, out foundPattern).OrderBy(p => p.boundary.Count());

                    matchType = "similar";

                    if (found.Count() == 0)
                    {
                        found = GetAllBySuggestion(collection, string.Format("Россия, Москва, {0}", key), out foundBy);

                        matchType = "geo";
                    }
                    
                }
                else
                {
                    found = collection.Find(Query.EQ("_id", id)).ToArray();

                    matchType = "id";
                }

                var res = found.FirstOrDefault();

                if (res != null)
                {
                    var similar = found.Where(p => p._id != res._id);

                    var near = collection.Find(Query.Near("station.geo", res.station.geo[0], res.station.geo[1])).Take(6).ToArray().Where(p => p._id != res._id);

                    var st = new Station
                    {
                        key = key,
                        id = res._id,
                        matchType = matchType, //not_found, similar, geo, id
                        foundBy = foundBy
                    };

                    st.boundary = res.boundary.Select(s => new Line { addr = s, matches = foundPattern != null ? GetMatches(s, foundPattern).ToArray() : new MatchedSubstring[0] }).ToArray();

                    st.station = new Address
                    {
                        addr = res.station.addr,
                        aux = res.station.aux,
                        org = res.station.org,
                        phone = res.station.phone,
                        geo = res.station.geo != null ? new GeoPoint { lat = res.station.geo[0], lon = res.station.geo[1] } : null
                    };

                    st.uik = new Address
                    {
                        addr = res.uik.addr,
                        aux = res.uik.aux,
                        org = res.uik.org,
                        phone = res.station.phone,
                        geo = res.uik.geo != null ? new GeoPoint { lat = res.uik.geo[0], lon = res.uik.geo[1] } : null
                    };

                    st.near = near.Select(p => new Ref { id = p._id, descr = p.station.addr }).ToArray();
                    if (matchType == "similar" && foundPattern != null)
                    {
                        st.similar = 
                            similar.Select((p) =>
                                {
                                    var matches = p.boundary.Select(s => new { s = s, m = GetMatches(s, foundPattern).ToArray() }).Where(s => s.m.Count() != 0).ToArray();
                                    return new RefLines { id = p._id, lines = matches.Select(x => new Line { addr = x.s, matches = x.m }).ToArray() };

                                }).ToArray();
                    }
                    else
                    {
                        st.similar = new RefLines[0];
                    }

                    cache.Set(k, st);

                    return st;
                }
            }

            return new Station { key = key, matchType = "not_found" };
        }

        private static IEnumerable<MatchedSubstring> GetMatches(string Str, string Pattern)
        {
            var matches = Regex.Matches(Str, Pattern, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                var groups = matches[0].Groups;
                for (int i = 1; i < groups.Count; i++)
                {
                    yield return new MatchedSubstring { offset = groups[i].Index, length = groups[i].Length };
                }
            }
        }

        public IEnumerable<Station> GetAll(ODataQueryOperation operation)
        {
            return new Station[0];
        }

        public IEnumerable<Doc> GetAllByStrictSearch(MongoCollection<Doc> Collection, string Addr, out string FoundPattern)
        {
            var spts = GetAddr(Addr);

            string regex = null;

            if (spts[1] == null)
            {
                regex = string.Format(routeRegex, spts[0].Split('|')[1]);
            }
            else
            {
                regex = string.Format(routeWithPremiseRegex, spts[0].Split('|')[1], spts[1].Split('|')[1]);
            }

            FoundPattern = regex;

            return Collection.Find(Query.Matches("boundary", BsonRegularExpression.Create(regex, "i"))).Take(5).ToArray();
        }

        public IEnumerable<Doc> GetAllBySuggestion(MongoCollection<Doc> Collection, string Term, out FoundBy FoundBy)
        {
            FoundBy = null;
            var geoSug = new GeoSuggestionRepository();
            var sug = geoSug.GetOne(Term).suggestions.FirstOrDefault();

            if (sug != null)
            {
                System.Net.WebClient client = new System.Net.WebClient();
                
                client.QueryString.Add("geocode", sug.descr);
                client.QueryString.Add("format", "xml");
                client.QueryString.Add("key", ConfigurationManager.AppSettings["YMapAPIKey"]);

                System.IO.Stream data = client.OpenRead("http://geocode-maps.yandex.ru/1.x");

                System.IO.StreamReader reader = new System.IO.StreamReader(data);
                string str = reader.ReadToEnd();

                data.Close();
                reader.Close();
                
                XDocument xdoc = XDocument.Parse(str);
                
                var geoNode = xdoc.Root.Descendants().Single(p=>p.Name == XName.Get("pos", "http://www.opengis.net/gml"));

                var geo = geoNode.Value.Split(' ');
                double lat = double.Parse(geo[0], CultureInfo.InvariantCulture);
                double lng = double.Parse(geo[1], CultureInfo.InvariantCulture);

                FoundBy = new FoundBy { term = Term, found = new Line { addr = sug.descr, matches = sug.matches }, point = new GeoPoint { lat = lat, lon = lng } };

                return Collection.Find(Query.Near("station.geo", lat, lng)).Take(6).ToArray(); 
            }

            return new Doc[0];
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
                var premiseRegex = string.Format(@"^({0})?\s?(\d+)(\s*({0})\s*(\d+))?$", string.Join("|", premiseNames.Select(p => p.Replace(".", @"\.?"))));

                matches = Regex.Matches(premisePart, premiseRegex, RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    var well = GetPremiseWell(matches[0].Groups[1].Value);
                    //premisePart = Regex.Replace(premisePart, premiseRegex, "$1$2|", RegexOptions.IgnoreCase).Trim();
                    premisePart = well + "|" + matches[0].Groups[2].Value;
                    if (matches[0].Groups[3].Success)
                    {
                        well = GetPremiseWell(matches[0].Groups[4].Value);
                        premisePart = premisePart + "|" + well + "|" + matches[0].Groups[5].Value;
                    }
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
            var i = routeNames.Select(p => p.Trim('.')).ToList().IndexOf(Route.ToLower().Trim('.'));

            return i != -1 ? routeNamesWell.ToArray()[i] : null;
        }

        private string GetPremiseWell(string Premise)
        {
            var i = premiseNames.Select(p => p.Trim('.')).ToList().IndexOf(Premise.ToLower().Trim('.'));

            return i != -1 ? premiseNamesWell.ToArray()[i] : null;
        }

    }
}