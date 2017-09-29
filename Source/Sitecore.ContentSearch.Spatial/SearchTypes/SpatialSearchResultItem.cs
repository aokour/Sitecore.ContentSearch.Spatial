using Sitecore.ContentSearch.SearchTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.ContentSearch.Spatial.SearchTypes
{
    public class SpatialSearchResultItem: SearchResultItem
    {
        [IndexField(Sitecore.ContentSearch.Spatial.Common.Constants.LocationFieldName)]
        public virtual LocationPoint GeoLocation { get; set; }

        [IndexField("spatialgeolocation__x")]
        public virtual double GeoLocation_X { get; set; }

        [IndexField("spatialgeolocation__y")]
        public virtual double GeoLocation_Y { get; set; }

        public virtual double Distance(double latitude, double longitude)
        {
            return CalculateDistance(latitude, longitude, GeoLocation_Y, GeoLocation_X, 'M');
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }
        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }

    public class LocationPoint
    {
        /// <summary>
        /// Search results within radius of a specific point
        /// </summary>
        /// <param name="latitude">Latitude of the search point</param>
        /// <param name="longitude">Logitude of the search point</param>
        /// <param name="distance">distance im miles</param>
        /// <returns></returns>
        public bool WithinRadius(double latitude, double longitude, double distance)
        {
            return true; 
        }
    }
}
