using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Extensions;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Nodes;
using Sitecore.ContentSearch.Linq.Parsing;
using Sitecore.ContentSearch.Spatial.Linq.Nodes;
using Sitecore.ContentSearch.Spatial.SearchTypes;

namespace Sitecore.ContentSearch.Spatial.Parsing
{
    public class ExpressionParserWithSpatial : ExpressionParser
    {
        public ExpressionParserWithSpatial(Type elementType, Type itemType, FieldNameTranslator fieldNameTranslator)
			: base(elementType, itemType, fieldNameTranslator) { }

        protected override QueryNode VisitMethodCall(MethodCallExpression methodCall)
        {
            MethodInfo method = methodCall.Method;
            if (method.DeclaringType == typeof(LocationPoint))
            {
                return this.VisitLocationPointMethods(methodCall);
            }
            return base.VisitMethodCall(methodCall);
           
        }

        protected QueryNode VisitLocationPointMethods(MethodCallExpression methodCall)
        {
            switch (methodCall.Method.Name)
            {
                case "WithinRadius":
                    return VisitWithinRadiusMethod(methodCall);

            }
            throw new NotSupportedException(string.Format("Unsupported extension method: {0}.", methodCall.Method.Name));
        }

        protected QueryNode VisitWithinRadiusMethod(MethodCallExpression methodCall)
        {
            QueryNode queryNode = this.Visit(this.GetArgument(methodCall.Arguments, 0));
            string fieldKey = this.MapPropertyToField(((MemberExpression)methodCall.Object).Member);
            
            QueryNode nodeLatitude = this.Visit(this.GetArgument(methodCall.Arguments, 0));
            QueryNode nodeLongitude = this.Visit(this.GetArgument(methodCall.Arguments, 1));
            QueryNode nodeRadius = this.Visit(this.GetArgument(methodCall.Arguments, 2));
            object latitudeValue = this.GetConstantValue<object>(nodeLatitude);
            object longitudeValue = this.GetConstantValue<object>(nodeLongitude);
            object radiusValue = this.GetConstantValue<object>(nodeRadius);
            return new WithinRadiusNode(queryNode, fieldKey, latitudeValue, longitudeValue, radiusValue);
        }
    }
}