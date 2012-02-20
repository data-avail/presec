using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presec.Service.Models;
using Microsoft.Data.Services.Toolkit.QueryModel;
using System.Globalization;
using MongoDB.Driver;
using System.Configuration;
using System.Text.RegularExpressions;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using Presec.Service.MongoModels;

namespace Presec.Service.Repositories
{
    public class MapRegionRepository
    {
        public MapRegion GetOne(string id)
        {
            var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

            var hostName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$1");
            var dbName = Regex.Replace(connectionString, "^(.*)/(.*)$", "$2");

            //"#{bounds._left} #{bounds._bottom} #{bounds._right} #{bounds._top} #{map.getZoom()}"
            var spts = id.Split(';');
            double left = double.Parse(spts[0], CultureInfo.InvariantCulture); 
            double bottom = double.Parse(spts[1], CultureInfo.InvariantCulture);
            double right = double.Parse(spts[2], CultureInfo.InvariantCulture);
            double top = double.Parse(spts[3], CultureInfo.InvariantCulture);
            string zoom = spts[4];

            var server = MongoServer.Create(hostName);
            server.Connect();
            var db = server.GetDatabase(dbName);
            var collection = db.GetCollection<Doc>("moscow");

            Doc[] q = null;

            if (zoom == "street")
            {
                q = collection.Find(Query.WithinRectangle("station.geo", left, bottom, right, top)).ToArray();
            }
            else if (zoom == "district")
            {
                var hasParent = Query.NE("parent", null);
                var doesntHaveDescr = Query.Exists("district", false);
                var geo = Query.WithinRectangle("station.geo", left, bottom, right, top);

                var groups = collection.Find(Query.And(hasParent, doesntHaveDescr, geo)).GroupBy(p=>p.parent);

            }
            else
            {
                var q1 = collection.Find(Query.WithinRectangle("station.geo", left, bottom, right, top)).ToArray();
            }



            var coords = Map(q).ToArray();

            return new MapRegion { id = id, coords = coords };
        }

        private IEnumerable<MapCoord> Map(IEnumerable<Doc> Docs)
        {
            return Docs.Select(p => new MapCoord { lat = p.station.geo[0], lon = p.station.geo[1] });
        }

        public IEnumerable<MapRegion> GetAll(ODataQueryOperation operation)
        {

            return null;
        }
    }
}