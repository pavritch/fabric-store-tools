using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{
    /// <summary>
    /// Initiate a single long running activity.
    /// </summary>
    class PerformActivity : IMessage
    {
        public ActivityRequest Activity { get; private set; }

        public PerformActivity(ActivityRequest activity)
        {
            this.Activity = activity;
        }
    }
}
