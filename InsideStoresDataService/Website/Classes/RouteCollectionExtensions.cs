//------------------------------------------------------------------------------
// 
// Class: RouteCollectionExtensions 
//
//------------------------------------------------------------------------------

using System;
using System.Web.Routing;

public static class RouteCollectionExtensions
{

    public static void IgnoreRoute(this RouteCollection routes, string url)
    {
        routes.IgnoreRoute(url, null);
    }

    public static void IgnoreRoute(this RouteCollection routes, string url, object constraints)
    {
        if (routes == null)
        {
            throw new ArgumentNullException("routes");
        }
        if (url == null)
        {
            throw new ArgumentNullException("url");
        }
        IgnoreRouteInternal item = new IgnoreRouteInternal(url) {
            Constraints = new RouteValueDictionary(constraints)
        };
        routes.Add(item);
    }

    // Nested Types
    private sealed class IgnoreRouteInternal : Route
    {
        // Methods
        public IgnoreRouteInternal(string url) : base(url, new StopRoutingHandler())
        {
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues)
        {
            return null;
        }
    }
}


