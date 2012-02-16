using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presec.Norm.Geo
{
    /// <summary> 
    /// Represents a coordinate 
    /// </summary> 
    public class LatLng
    {

        /// <summary> 
        /// The latitude 
        /// </summary> 
        public float Latitude { get; set; }
        /// <summary> 
        /// The longitude 
        /// </summary> 
        public float Longitude { get; set; }

        /// <summary> 
        /// Creates an array of doubles 
        /// </summary> 
        /// <returns></returns> 
        public float[] ToArray()
        {
            return new float[]{ 
                this.Latitude , 
                this.Longitude 
           };
        }
    } 
}