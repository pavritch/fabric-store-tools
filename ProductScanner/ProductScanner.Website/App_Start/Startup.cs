using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;

[assembly: OwinStartup(typeof(ProductScanner.Website.Startup))]
namespace ProductScanner.Website
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new StringEnumConverter());
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

            app.MapSignalR();
        }
    }
}