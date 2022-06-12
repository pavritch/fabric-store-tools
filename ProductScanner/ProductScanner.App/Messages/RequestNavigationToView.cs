using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{
    /// <summary>
    /// Request navigation to an already instantiated element.
    /// </summary>
    /// <remarks>
    /// Navigation requests are received by the navigation service and subsequently tracked and reflected
    /// to the rest of the system as confirmed navigation moves.
    /// </remarks>
    class RequestNavigationToView : IMessage
    {
        public UIElement Element { get; set; }
    }
}
