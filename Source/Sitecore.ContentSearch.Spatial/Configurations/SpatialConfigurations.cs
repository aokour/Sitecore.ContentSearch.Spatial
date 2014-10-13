using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.ContentSearch.Spatial.Configurations
{
    internal class SpatialConfigurations
    {
        public List<LocationSettings> LocationSettings { get; set; }
    }

    internal class LocationSettings
    {
        public string LatitudeField { get; set; }
        public string LongitudeField { get; set; }

        public ID TemplateId { get; set; }

    }
}
