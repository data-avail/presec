using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presec.Service.Models;
using Microsoft.Data.Services.Toolkit.QueryModel;
using System.Xml.Linq;
using System.Configuration;

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
                client.QueryString.Add("key", ConfigurationManager.AppSettings["GoogleAPIKey"]);


                System.IO.Stream data = client.OpenRead("https://maps.googleapis.com/maps/api/place/autocomplete/xml");
                System.IO.StreamReader reader = new System.IO.StreamReader(data);
                string str = reader.ReadToEnd();

                data.Close();
                reader.Close();

                XDocument xdoc = XDocument.Parse(str);
                var status = xdoc.Root.Descendants("status").Single();
                if (status.Value == "OK")
                {
                    return xdoc.Root.Descendants("prediction")
                        //.Where(p => p.Descendants("type").Any(s => s.Value == "geocode" || s.Value == "route"))
                            .Select(p => new GeoSuggestion { 
                                descr = p.Descendants("description").Single().Value, 
                                term = p.Descendants("term").Descendants("value").First().Value });
                }

            }

            return new GeoSuggestion[0];
        }
    }

}