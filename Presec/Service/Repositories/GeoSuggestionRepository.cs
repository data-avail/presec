using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presec.Service.Models;
using Microsoft.Data.Services.Toolkit.QueryModel;

namespace Presec.Service.Repositories
{
    public class GeoSuggestionRepository
    {
        public GeoSuggestion GetOne(string id)
        {
            return new GeoSuggestion();
        }

        public IEnumerable<GeoSuggestion> GetAll(ODataQueryOperation operation)
        {
            var term = operation.ContextParameters["term"];

            if (!string.IsNullOrEmpty(term))
            {
                System.Net.WebClient client = new System.Net.WebClient();

                // Add a user agent header in case the 
                // requested URI contains a query.

                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                client.QueryString.Add("input", term);
                client.QueryString.Add("sensor", "false");
                client.QueryString.Add("types", "geocode");
                client.QueryString.Add("key", "AIzaSyDPSsQMa98n7ZU1WMqpGY3tGJrNcvRmFR0");


                System.IO.Stream data = client.OpenRead("https://maps.googleapis.com/maps/api/place/autocomplete/json");//?input=россия,+москва,+вин&sensor=flase&key=AIzaSyDPSsQMa98n7ZU1WMqpGY3tGJrNcvRmFR0");
                System.IO.StreamReader reader = new System.IO.StreamReader(data);
                string s = reader.ReadToEnd();
                Console.WriteLine(s);
                data.Close();
                reader.Close();
            }

            return new GeoSuggestion[0];
        }
    }

}