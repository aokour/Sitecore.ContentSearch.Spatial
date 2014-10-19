using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Documents;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Vector;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.LuceneProvider;
using Sitecore.Data;
using Sitecore.Data.Items;
using Spatial4n.Core.Context;
using Spatial4n.Core.Shapes;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.ContentSearch.Spatial.Configurations;
using System.Xml;
using Sitecore.Xml;

namespace Sitecore.ContentSearch.Spatial.Indexing
{
    public class LuceneSpatialDocumentBuilder : LuceneDocumentBuilder
    {
        private string fieldNameX;
        private string fieldNameY;
        public static string SUFFIX_X = "__lng";
        public static string SUFFIX_Y = "__ltd";
        public int precisionStep;
        private SpatialConfigurations spatialConfigurations;
        public LuceneSpatialDocumentBuilder(IIndexable indexable, IProviderUpdateContext context)
            : base(indexable, context)
        {
            BuildSettings();
        }

        public override void AddItemFields()
        {
            base.AddItemFields();
            Item item = (Item)(this.Indexable as SitecoreIndexableItem);
            if (item != null && spatialConfigurations.LocationSettings.Where(i=>i.TemplateId.Equals( item.TemplateID)).Any())
            {
                AddPoint(item).ForEach(i => base.CollectedFields.Enqueue(i));
            }
            
        }

        private List<IFieldable> AddPoint(Item item)
        {
            List<IFieldable> pointFields = new List<IFieldable>();
            var setting = spatialConfigurations.LocationSettings.Where(i => i.TemplateId.Equals(item.TemplateID)).FirstOrDefault();
            if (setting == null)
                return pointFields;

            SpatialContext ctx =  SpatialContext.GEO;
             
            SpatialPrefixTree grid = new GeohashPrefixTree(ctx, 11);
            var strategy = new PointVectorStrategy(ctx, Sitecore.ContentSearch.Spatial.Common.Constants.LocationFieldName);
            
            double lng = 0;
            double lat = 0;
            if (!string.IsNullOrEmpty(item[setting.LatitudeField]))
            {
                Double.TryParse(item[setting.LatitudeField], out lat);
            }

            if (!string.IsNullOrEmpty(item[setting.LongitudeField]))
            {
                Double.TryParse(item[setting.LongitudeField], out lng);
            }

            if (lng == 0 || lat == 0)
                return pointFields;

            Point shape = ctx.MakePoint(lng, lat);
            foreach (var f in strategy.CreateIndexableFields(shape))
            {
                if (f != null)
                {
                    pointFields.Add(f);
                }
            }
            //Create storable fields
            NumericField fieldX = new NumericField(this.fieldNameX, this.precisionStep, Field.Store.YES, false)
            {
                OmitNorms = true,
                OmitTermFreqAndPositions = true
            };
            fieldX.SetDoubleValue(lng);

            NumericField fieldY = new NumericField(this.fieldNameY, this.precisionStep, Field.Store.YES, true)
            {
                OmitNorms = true,
                OmitTermFreqAndPositions = true
            };
            fieldY.SetDoubleValue(lat);
            
            pointFields.Add(fieldX);
            pointFields.Add(fieldY);

            return pointFields;
        }

        private void BuildSettings()
        {
            this.precisionStep = 8;
            this.fieldNameX = Sitecore.ContentSearch.Spatial.Common.Constants.LocationFieldName + SUFFIX_X;
            this.fieldNameY = Sitecore.ContentSearch.Spatial.Common.Constants.LocationFieldName + SUFFIX_Y;

            spatialConfigurations = new SpatialConfigurations();
            spatialConfigurations.LocationSettings = new List<LocationSettings>();
            XmlNodeList configs = Factory.GetConfigNodes("contentSearchSpatial/IncludeTemplates/Template");

            if (configs == null)
            {
                Log.Warn("sitecore/contentSearchSpatial/IncludeTemplates/Template node was not defined; Please include the Sitecore.ContentSearch.Spatial.config file in include folder.", this);
                return;
            }
            foreach(XmlNode node in configs)
            {
                string templateId = XmlUtil.GetAttribute("id", node);
                string latitudeField = XmlUtil.GetAttribute("LatitudeField", node);
                string longitudeField = XmlUtil.GetAttribute("LongitudeField", node);

                LocationSettings locationSetting = new LocationSettings();
                locationSetting.LatitudeField = latitudeField;
                locationSetting.LongitudeField = longitudeField;
                
                if(ID.IsID(templateId))
                {
                    locationSetting.TemplateId = ID.Parse(templateId);
                }
                spatialConfigurations.LocationSettings.Add(locationSetting);
            }
        }
    }
}