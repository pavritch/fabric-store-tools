using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Website
{
    public class EnumRouteConstraint<T> : IRouteConstraint where T : struct 
    { 
        private readonly HashSet<string> enumNames; 

        public EnumRouteConstraint()
        {
            string[] names = Enum.GetNames(typeof(T)); 
            enumNames = new HashSet<string>(from name in names select name.ToLowerInvariant());
        } 
        
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) 
        {
            return enumNames.Contains(values[parameterName].ToString().ToLowerInvariant());
        }
    } 
}