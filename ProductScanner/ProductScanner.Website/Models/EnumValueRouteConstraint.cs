using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace ProductScanner.Website.Models
{
    public class EnumValueRouteConstraint<T> : IHttpRouteConstraint where T : struct
    {
        private readonly HashSet<int> _enumValues;
        public EnumValueRouteConstraint()
        {
            var values = Enum.GetValues(typeof (T)).Cast<int>();
            _enumValues = new HashSet<int>(values);
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            return _enumValues.Contains(Convert.ToInt32(values[parameterName]));
        }
    }
}