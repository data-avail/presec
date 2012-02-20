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

            var cache = new Cahching.Cache<IEnumerable<MapCoord>>();
            IEnumerable<MapCoord> coords = cache.Get(id);
            if (coords != null)
                return new MapRegion { id = id, coords = coords.ToArray() };


            var server = MongoServer.Create(hostName);
            server.Connect();
            var db = server.GetDatabase(dbName);
            var collection = db.GetCollection<Doc>("moscow");

            //find and store in cache all district and city results, later get results for them from cache
            if (zoom == "district")
            {
                coords = cache.Get("district");

                if (coords == null)
                {
                    var districts = collection.Find(Query.And(Query.NE("parent", BsonType.Null), Query.Exists("district", true)));

                    coords = districts.Select(p => new MapCoord { lat = p.geo != null ? p.geo[0] : 0.0 , lon = p.geo != null ? p.geo[1] : 0.0, count = p.crn != null ? p.crn.Count() : 0, descr = p.district }).ToArray();
                }

                cache.Set("district", coords);
            }
            else if(zoom == "city")
            {
                coords = cache.Get("city");

                var regions = collection.Find(Query.Exists("parent", false)).ToArray();
                
                var districts = collection.Find(Query.In("parent", regions.Select(p=> BsonValue.Create(p._id)))).ToArray();

                var distrCounts = districts.Select(p => new { id = p._id, parent = p.parent, count = p.crn != null ? p.crn.Count() : 0 });

                coords = regions.Select(p =>
                    new MapCoord { lat = p.geo != null ? p.geo[0] : 0.0, lon = p.geo != null ? p.geo[1] : 0.0, count = (int)distrCounts.Where(x => x.parent == p._id).Sum(x => x.count), descr = p.district }).ToArray();

                cache.Set("city", coords);
            }

            var geo = Query.WithinRectangle(zoom == "street" ? "station.geo" : "geo", left, bottom, right, top);
            
            if (zoom == "street")
            {
                var q = collection.Find(Query.And(geo, Query.Exists("station", true)));

                coords = q.Select(p => new MapCoord { lat = p.station.geo != null ? p.station.geo[0] : 0.0, lon = p.station.geo != null ? p.station.geo[1] : 0.0, count = 1, descr = p._id.ToString() }).ToArray();
            }
            else 
            {
                coords = coords.Where(p=>p.lat >= left && p.lat <= right && p.lon >= bottom && p.lon <= top);
            }
            /*
            {

                var districts = collection.Find(Query.And(Query.NE("parent", BsonType.Null), geo, Query.Exists("district", true)));

                coords = districts.Select(p => new MapCoord { lat = p.geo[0], lon = p.geo[1], count = p.crn != null ? p.crn.Count() : 0, descr = p.district }).ToArray();

                coords = coords.Where(p=>p.lat >= left && p.lat <= right && p.lon >= bottom && p.lon <= top);
            }
            else
            {

                var regions = collection.Find(Query.And(Query.Exists("parent", false), geo)).ToArray();
                
                var districts = collection.Find(Query.In("parent", regions.Select(p=> BsonValue.Create(p._id)))).ToArray();

                var distrCounts = districts.Select(p => new { id = p._id, parent = p.parent, count = p.crn != null ? p.crn.Count() : 0 });

                coords = regions.Select(p =>
                    new MapCoord { lat = p.geo[0], lon = p.geo[1], count = (int)distrCounts.Where(x => x.parent == p._id).Sum( x => x.count), descr = p.district }).ToArray();
            }
             */
            if (zoom == "street")
                cache.Set(id, coords);

            return new MapRegion { id = id, coords = coords.ToArray() };
        }


        public IEnumerable<MapRegion> GetAll(ODataQueryOperation operation)
        {

            return null;
        }
    }
}