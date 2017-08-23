using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch.Spatial.Configurations;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sitecore.ContentSearch.Spatial.ComputedFields
{
    public class LocationCoordinates : AbstractComputedIndexField
    {

        private static SpatialConfigurations spatialConfigurations;
        public LocationCoordinates()
        {
            if (spatialConfigurations == null)
            {
                BuildSettings();
            }
        }
        private string FieldId(LocationSettings setting)
        {
                if (!this.FieldName.EndsWith("__y"))
                {
                    return setting.LongitudeField;
                }
                return setting.LatitudeField;
            
        }

        public override object ComputeFieldValue(IIndexable indexable)
        {
            double num;
            Item item = indexable as SitecoreIndexableItem;
            var setting = spatialConfigurations.LocationSettings.Where(i => i.TemplateId.Equals(item.TemplateID)).FirstOrDefault();
            
            if (item == null || setting == null || item.Fields[FieldId(setting)] == null )
            {
                return null;
            }
            if (!double.TryParse(item.Fields[FieldId(setting)].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
            {
                return null;
            }
            return num;
        }

        private void BuildSettings()
        {
            spatialConfigurations = new SpatialConfigurations();
            spatialConfigurations.LocationSettings = new List<LocationSettings>();
            XmlNodeList configs = Sitecore.Configuration.Factory.GetConfigNodes("contentSearchSpatial/IncludeTemplates/Template");

            if (configs == null)
            {
                Log.Warn("sitecore/contentSearchSpatial/IncludeTemplates/Template node was not defined; Please include the Sitecore.ContentSearch.Spatial.config file in include folder.", this);
                return;
            }
            foreach (XmlNode node in configs)
            {
                string templateId = XmlUtil.GetAttribute("id", node);
                string latitudeField = XmlUtil.GetAttribute("LatitudeField", node);
                string longitudeField = XmlUtil.GetAttribute("LongitudeField", node);

                LocationSettings locationSetting = new LocationSettings();
                locationSetting.LatitudeField = latitudeField;
                locationSetting.LongitudeField = longitudeField;

                if (ID.IsID(templateId))
                {
                    locationSetting.TemplateId = ID.Parse(templateId);
                }
                spatialConfigurations.LocationSettings.Add(locationSetting);
            }
        }
    }
}
