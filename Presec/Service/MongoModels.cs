using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presec.Service.MongoModels
{
    public class Doc
    {
        public int _id { get; set; }

        public string district { get; set; }

        public string[] boundary { get; set; }

        public DocAddr station { get; set; }

        public DocAddr uik { get; set; }

        public int? parent {get; set;}

        public int[] crn { get; set; }

        public double[] geo { get; set; }
    }

    public class DocAddr
    {
        public string addr { get; set; }

        public string org { get; set; }

        public string phone { get; set; }

        public string aux { get; set; }

        public double[] geo { get; set; }
    }
}