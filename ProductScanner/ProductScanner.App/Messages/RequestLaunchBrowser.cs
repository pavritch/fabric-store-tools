using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    /// <summary>
    /// Sent by any module which wishes to have the system open a browser to the specified Url.
    /// </summary>
    class RequestLaunchBrowser : IMessage
    {
        public string Url { get; private set; }

        public RequestLaunchBrowser(string Url)
        {
            this.Url = Url; 
        }
    }
}
