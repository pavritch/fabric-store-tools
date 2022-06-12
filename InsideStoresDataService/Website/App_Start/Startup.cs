using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Website.Startup))]

namespace Website
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            try
            {
                app.MapSignalR();
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(string.Format("Error initializing SignalR: {0}", Ex.Message)); 
            }
        }
    }
}
