using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Norm.BSON;

namespace Presec.Norm.Geo
{
    /// <summary> 
    /// Interface to limit classes for the Within Qualifier 
    /// </summary> 
    public interface IWhitinShapeQualifier
    {
    }

    /// <summary> 
    /// The within qualifier 
    /// </summary> 
    public class NearQualifier : QualifierCommand
    {
        /// <summary> 
        /// Initializes a new instance of the <see cref="NearQualifier"/> class. 
        /// </summary> 
        /// <param name="center"></param> 
        public NearQualifier(LatLng center) : this(center.Longitude, center.Latitude)
        {

        }

        public NearQualifier(double lat, double lon)
            : base("$near", new [] {lat, lon})
        {

        }

    }

    /// <summary> 
    /// The within qualifier 
    /// </summary> 
    public class WithinQualifier : QualifierCommand
    {
        /// <summary> 
        /// Initializes a new instance of the <see cref="WithinQualifier"/> class. 
        /// </summary> 
        /// <param name="qualifier"></param> 
        public WithinQualifier(IWhitinShapeQualifier qualifier)
            : base("$within", qualifier)
        {
        }
    }

    /// <summary> 
    /// The Box Qualifier 
    /// </summary> 
    public class BoxQualifer : QualifierCommand, IWhitinShapeQualifier
    {
        /// <summary> 
        /// Initializes a new instance of the <see cref="BoxQualifer"/ > class. 
        /// </summary> 
        /// <param name="northEast">The top right corner of the box</ param> 
        /// <param name="southWest">The bottom left corner of the box</ param> 
        public BoxQualifer(LatLng southWest, LatLng northEast)
            : base("$box", new object[] { southWest.ToArray(), northEast.ToArray() })
        {

        }
    }

    /// <summary> 
    /// The Circle Qualifier 
    /// </summary> 
    public class CircleQualifer : QualifierCommand, IWhitinShapeQualifier
    {
        /// <summary> 
        /// Initializes a new instance of the <see cref="CircleQualifer"/> class. 
        /// </summary> 
        /// <param name="center">The center point</param> 
        /// <param name="radius">The radius of the search</param> 
        public CircleQualifer(LatLng center, double radius)
            : base("$center", new object[] { center.ToArray(), radius })
        {

        }
    }
}