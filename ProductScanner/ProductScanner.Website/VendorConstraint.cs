using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using Owin;
using ProductScanner.Core;

namespace ProductScanner.Website
{
    public class VendorConstraint : IHttpRouteConstraint
    {
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            try
            {
                var vendorId = Convert.ToInt32(values[parameterName]);
                var vendor = Vendor.GetById(vendorId);
                return vendor != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}