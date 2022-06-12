using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{
    /// <summary>
    /// Sent by the navigation service to dictate that the specified vm should be 
    /// made visible the content page screen area.
    /// </summary>
    class ShowContentPage : IMessage
    {
        /// <summary>
        /// The vm to the page to display.
        /// </summary>
        /// <remarks>
        /// The content host will use implicit styles to determine which view to invoke and will
        /// set this vm as the DataContext.
        /// </remarks>
        public IContentPage Page { get; set; }

        /// <summary>
        /// The desired transition.
        /// </summary>
        public ContentPageTransition Transition { get; set; }

        public ShowContentPage(IContentPage page, ContentPageTransition transition = ContentPageTransition.Fade)
        {
            this.Page = page;
            this.Transition = transition;
        }
    }
}
