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
    }

    public class LocationPoint
    {
        /// <summary>
        /// Search results within radius of a specific point
        /// </summary>
        /// <param name="latitude">Latitude of the search point</param>
        /// <param name="longitude">Logitude of the search point</param>
        /// <param name="distance">distance im miles</param>
        /// <param name="orderByDistance">If true, Sort results from nearest to farthest</param>
        /// <returns></returns>
        public bool WithinRadius(double latitude, double longitude, double distance, bool orderByDistance)
        {
            return true;
        }

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
