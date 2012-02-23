using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;
using Microsoft.Data.Services.Toolkit.QueryModel;

namespace Presec.Service.Models
{
    [Serializable]
    [DataServiceKey("key")]
    public class Station
    {
        //id or sarch term
        public string key { get; set; }

        public int id { get; set; }

        public string matchType { get; set; } 

        public Address station { get; set; }

        public Address uik { get; set; }

        public Line[] boundary { get; set; }

        public Ref[] near { get; set; }

        public RefLines[] similar { get; set; }

        //used only if matchType == geo
        public FoundBy foundBy { get; set; }
    }

    [Serializable]
    [DataServiceKey("term")]
    public class FoundBy
    {
        public string term { get; set; } 

        public Line found { get; set; }

        public GeoPoint point { get; set; }
    }

    [Serializable]
    [DataServiceKey("addr")]
    public class Line
    {
        public string addr { get; set; }

        public MatchedSubstring[] matches { get; set; }
    }

    [Serializable]
    [DataServiceKey("id")]
    public class Ref
    {
        public int id { get; set; }

        public string descr { get; set; }
    }

    [Serializable]
    [DataServiceKey("id")]
    public class RefLines
    {
        public int id { get; set; }

        public Line[] lines { get; set; }
    }

    [Serializable]
    [DataServiceKey("offset", "length")]
    public class MatchedSubstring
    {
        public int offset { get; set; }

        public int length { get; set; }
    }

    [Serializable]
    [DataServiceKey("addr")]
    public class Address
    {
        public string addr { get; set; }

        public string org { get; set; }

        public string aux { get; set; }

        public string phone { get; set; }

        public GeoPoint geo { get; set; }
    }

    [Serializable]
    [DataServiceKey("lat", "lon")]
    public class GeoPoint
    {
        public double lon { get; set; }

        public double lat { get; set; }
    }

    //http://code.google.com/apis/maps/documentation/places/autocomplete.html
    [DataServiceKey("term")]
    [Serializable]
    public class GeoSuggestion
    {
        public string term { get; set; }

        public Suggestion[] suggestions { get; set; }
    }

    [DataServiceKey("id")]
    [Serializable]
    public class Suggestion
    {
        public string id { get; set; }

        public string term { get; set; }

        public string descr { get; set; }

        public string refer { get; set; }

        public MatchedSubstring [] matches { get; set; }

    }

    [DataServiceKey("lat", "lon")]
    [Serializable]
    public class MapCoord
    {
        public double lat { get; set; }

        public double lon { get; set; }

        public string descr { get; set; }

        public int count { get; set; }
    }

    [DataServiceKey("id")]
    [Serializable]
    public class MapRegion
    {
        public string id { get; set; }

        public MapCoord[] coords { get; set; }
    }


}