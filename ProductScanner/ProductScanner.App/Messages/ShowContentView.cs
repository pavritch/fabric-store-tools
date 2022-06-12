using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{
    /// <summary>
    /// Sent by the nav service to instruct the system to show the given xaml view.
    /// </summary>
    /// <remarks>
    /// Should use ShowContentPage for standard types. Not sure if this class is actually
    /// going to be needed.
    /// </remarks>
    class ShowContentView : IMessage
    {
        /// <summary>
        /// The xaml element to display.
        /// </summary>
        public UIElement Element { get; set; }

        /// <summary>
        /// The desired transition.
        /// </summary>
        public ContentPageTransition Transition { get; set; }

        public ShowContentView(UIElement element, ContentPageTransition transition = ContentPageTransition.Fade)
        {
            this.Element = element;
            this.Transition = transition;
        }
    }
}
