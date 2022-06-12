using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{
    /// <summary>
    /// Announces that navigation is about to take place between pages.
    /// </summary>
    /// <remarks>
    /// Sent by the navigation service when responding to navigation requests.
    /// Either of the parameters can be null when not meaningful for the current context.
    /// </remarks>
    class BeginNavigation : IMessage
    {
        /// <summary>
        /// The vm for the page about to go away.
        /// </summary>
        public IContentPage FromPage { get; set; }

        /// <summary>
        /// The vm for the page about to be shown.
        /// </summary>
        public IContentPage ToPage { get; set; }

        public BeginNavigation(IContentPage fromPage, IContentPage toPage)
        {
            this.FromPage = fromPage;
            this.ToPage = toPage;
        }
    }
}
