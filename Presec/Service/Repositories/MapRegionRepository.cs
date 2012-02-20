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

            IEnumerable<MapCoord> coords = new MapCoord[0];

            if (zoom == "street")
            {
                var q = collection.Find(Query.WithinRectangle("station.geo", left, bottom, right, top));

                coords = q.Select(p => new MapCoord { lat = p.station.geo[0], lon = p.station.geo[1], count = 1, descr = p._id.ToString() });
            }
            else if (zoom == "district")
            {
                var hasParent = Query.NE("parent", BsonType.Null);
                var hasDistrict = Query.Exists("district", true);
                var geo = Query.WithinRectangle("geo", left, bottom, right, top);
                var districts = collection.Find(Query.And(hasParent, hasDistrict, geo));

                coords = districts.Select(p => new MapCoord { lat = p.geo[0], lon = p.geo[1], count = (int)collection.Count(Query.EQ("parent", p._id)), descr = p.district });

            }
            else
            {
                var regNotHasParent = Query.Exists("parent", false);
                var regGeo = Query.WithinRectangle("geo", left, bottom, right, top);
                var hasDistrict = Query.Exists("district", true);
                var regions = collection.Find(Query.And(regNotHasParent, hasDistrict, regGeo)).ToArray();
                
                var districts = collection.Find(Query.In("parent", regions.Select(p=> BsonValue.Create(p._id)))).ToArray();

                var distrCounts = districts.Select(p => new { id = p._id, parent = p.parent, count = (int)collection.Count(Query.EQ("parent", p._id)) }).ToArray();

                coords = regions.Select(p =>
                    new MapCoord { lat = p.geo[0], lon = p.geo[1], count = (int)distrCounts.Where(x => x.parent == p._id).Sum( x => x.count), descr = p.district });

            }

            return new MapRegion { id = id, coords = coords.ToArray() };
        }


        public IEnumerable<MapRegion> GetAll(ODataQueryOperation operation)
        {

            return null;
        }
    }
}