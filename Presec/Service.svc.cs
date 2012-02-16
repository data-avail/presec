﻿using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Norm;
using System.Text.RegularExpressions;
using Presec.Models.ServiceModels;
using Presec.Models.MongoModels;
using System.Configuration;
using System.ServiceModel;
using Presec.Norm.Geo;
using Norm.BSON;

namespace Presec
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Service : DataService<CustomDataServiceProvider>
    {
        // Этот метод вызывается только один раз для инициализации серверных политик.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: задайте правила, чтобы указать, какие наборы сущностей и служебные операции являются видимыми, обновляемыми и т.д.
            // Примеры:
            config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
            // config.SetServiceOperationAccessRule("СлужебнаяОперация", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
        }
    }


    public class CustomDataServiceProvider : ODataContext
    {
        public IQueryable<Station> Stations
        {
            get
            {
                return base.CreateQuery<Station>();
            }
        }

        public IQueryable<Line> Lines
        {
            get
            {
                return base.CreateQuery<Line>();
            }
        }


        public override object RepositoryFor(string fullTypeName)
        {
            string typeName = fullTypeName.Replace("[]", string.Empty).Substring(fullTypeName.LastIndexOf('.') + 1);
            Type repoType = Type.GetType(string.Format("Presec.{0}Repository", typeName));
            if (repoType == null) throw new NotSupportedException("The specified type is not an Entity inside the OData API");
            return Activator.CreateInstance(repoType);
        }
    }



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

                using (var db = Mongo.Create(ConfigurationManager.AppSettings["MongoConnectionString"]))
                {
                    var col = db.GetCollection<Doc>("moscow");

                    var q = col.AsQueryable().Where(p => p.boundary.Any(s => Regex.IsMatch(s, regex, RegexOptions.IgnoreCase)));

                    return q.ToArray().Select((p) =>
                    {
                        var st = new Station
                        {
                            id = p._id
                        };

                        st.lines = p.boundary.ToArray().Select(s=> new Line { addr = s}).ToList();

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

                        //var near = col.Find(new { station = new { geo = new NearQualifier(st.station.geo.lat, st.station.geo.lon) } }).Where(s=>s != null).ToArray();

                        return st;
                    });

                    
                }
            }

            return new Station[0];
        }
    }
}