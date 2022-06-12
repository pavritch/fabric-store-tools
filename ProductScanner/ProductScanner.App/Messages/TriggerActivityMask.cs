using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    class TriggerActivityMask : IMessage
    {
        /// <summary>
        /// True indicates to show the mask, false to hide it.
        /// </summary>
        public bool IsShowing { get; set; }

        public TriggerActivityMask(bool isShowing)
        {
            this.IsShowing = isShowing;
        }
    }
}
