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

namespace Presec.Service.Repositories
{
    public class StationRepository
    {
        public Station GetOne(string id)
        {
            return new Station { id = 0 };
        }

        public IEnumerable<Station> GetAll(ODataQueryOperation operation)
        {
            var addr = operation.ContextParameters["addr"];

            if (!string.IsNullOrEmpty(addr))
            {
                var spts = addr.Split(',');

                string regex = null;

                if (spts.Count() == 1)
                {
                    regex = string.Format(".*{0}.*", spts[0].Trim());
                }
                else
                {
                    regex = string.Format(".*{0}^d*{1}^d.*", spts[0], spts[1]);
                }


                //Since Norm doesn't support nested spatials - use native driver

                var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

                var hostName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$1");
                var dbName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$2");


                var server = MongoServer.Create(hostName);
                server.Connect();
                var db = server.GetDatabase(dbName);
                var collection = db.GetCollection<Doc>("moscow");
                var q = collection.Find(Query.Matches("boundary", BsonRegularExpression.Create(regex, "i"))).Take(5);

                return q.ToArray().Select((p) =>
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
                        var q1 = collection.Find(Query.Near("station.geo", st.station.geo.lat, st.station.geo.lon, 5));

                        st.near = q1.Take(5).Select(s => new Station { id = s._id, lines = s.boundary.Select(x => new Line { addr = x }).ToArray() }).ToArray();
                    }

                    return st;
                });
            }

            return new Station[0];
        }
    }
}