using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    /// <summary>
    /// Request navigation to an already instantiated page view model.
    /// </summary>
    /// <remarks>
    /// Navigation requests are received by the navigation service and subsequently tracked and reflected
    /// to the rest of the system as confirmed navigation moves.
    /// </remarks>
    class RequestNavigationToContentPage : IMessage
    {
        public IContentPage Page { get; set; }

        public RequestNavigationToContentPage()
        {

        }

        public RequestNavigationToContentPage(IContentPage Page)
        {
            this.Page = Page;
        }
    }
}
