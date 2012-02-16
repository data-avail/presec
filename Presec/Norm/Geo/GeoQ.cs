using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presec.Norm.Geo
{
    public static class GeoQ
    {
        /// <summary> 
        /// Builds a $near : [lat, lng] qualifier for the search. 
        /// </summary> 
        /// <param name="center">Center location</param> 
        /// <returns></returns> 
        public static NearQualifier Exists(LatLng center)
        {
            return new NearQualifier(center);
        }

        /// <summary> 
        /// Builds a $within {$box : [southwestnortheast, southwest]} qualifier for the search. 
        /// </summary> 
        /// <param name="northEast">Top right</param> 
        /// <param name="southWest">Left bottom</param> 
        /// <returns></returns> 
        public static WithinQualifier WithinBox(LatLng southWest, LatLng northEast)
        {
            IWhitinShapeQualifier shapeQualifier = new BoxQualifer(southWest, northEast);
            return new WithinQualifier(shapeQualifier);
        }

        /// <summary> 
        /// Builds a $within {$center : [center, radius]}qualifier for the search. 
        /// </summary> 
        /// <param name="center">Center of the circle</param> 
        /// <param name="radius">Radius of the circle</param> 
        /// <returns></returns> 
        public static WithinQualifier WithinCircle(LatLng center, double radius)
        {
            IWhitinShapeQualifier shapeQualifier = new CircleQualifer(center, radius);
            return new WithinQualifier(shapeQualifier);
        }

    }
}