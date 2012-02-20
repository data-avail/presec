using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using System.ServiceModel;
using Presec.Service.Models;

namespace Presec
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class PresecService : DataService<CustomDataServiceProvider>
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

        public IQueryable<GeoSuggestion> GeoSuggestions
        {
            get
            {
                return base.CreateQuery<GeoSuggestion>();
            }
        }

        public IQueryable<MapCoord> MapCoords
        {
            get
            {
                return base.CreateQuery<MapCoord>();
            }
        }

        public IQueryable<MapRegion> MapRegions
        {
            get
            {
                return base.CreateQuery<MapRegion>();
            }
        }


        public override object RepositoryFor(string fullTypeName)
        {
            string typeName = fullTypeName.Replace("[]", string.Empty).Substring(fullTypeName.LastIndexOf('.') + 1);
            Type repoType = Type.GetType(string.Format("Presec.Service.Repositories.{0}Repository", typeName));
            if (repoType == null) throw new NotSupportedException("The specified type is not an Entity inside the OData API");
            return Activator.CreateInstance(repoType);
        }
    }

}
