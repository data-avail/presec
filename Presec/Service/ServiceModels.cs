using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;
using Microsoft.Data.Services.Toolkit.QueryModel;

namespace Presec.Service.Models
{
    [DataServiceKey("id")]
    public class Station
    {
        public int id { get; set; }

        public Line[] lines { get; set; }

        public Address station { get; set; }

        public Address uik { get; set; }

        public Station[] near { get; set; }
    }

    [DataServiceKey("addr")]
    public class Line
    {
        public string addr { get; set; }
    }

    [DataServiceKey("addr")]
    public class Address
    {
        public string addr { get; set; }

        public string org { get; set; }

        public string aux { get; set; }

        public string phone { get; set; }

        public GeoPoint geo { get; set; }
    }

    [DataServiceKey("lon", "lat")]
    public class GeoPoint
    {
        public double lon { get; set; }

        public double lat { get; set; }
    }

    //http://code.google.com/apis/maps/documentation/places/autocomplete.html
    [DataServiceKey("id")]
    [Serializable]
    public class GeoSuggestion
    {
        public string id { get; set; }

        public string term { get; set; }

        public string descr { get; set; }

        public string refer { get; set; }
    }

}