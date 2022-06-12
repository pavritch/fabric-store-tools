using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;

namespace ProductScanner.Website
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            var constraintResolver = new DefaultInlineConstraintResolver();
            constraintResolver.ConstraintMap.Add("vendor", typeof(VendorConstraint));
            config.MapHttpAttributeRoutes(constraintResolver);

            //config.EnableSystemDiagnosticsTracing();
        }
    }
}
