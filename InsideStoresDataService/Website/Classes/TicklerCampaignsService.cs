//------------------------------------------------------------------------------
// 
// Class: TicklerCampaignsService 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Website.Emails;

namespace Website
{
    /// <summary>
    /// Background thread to monitor tickler campaigns.
    /// </summary>
    public class TicklerCampaignsService : SimpleWorkerServiceBase
    {
#if DEBUG
        private const int intervalSeconds = 60 * 2; // 2min
        private const int initialDelaySeconds = 120; 
#else
        private const int intervalSeconds = 60 * 1; // 1min
        private const int initialDelaySeconds = 60 * 15; // 15min
#endif

        private bool isBusy = false;

        public TicklerCampaignsService()
            : base(intervalSeconds, "TicklerCampaignsService", initialDelaySeconds)
        {
        }

        /// <summary>
        /// Performs one unit (one pass) of work.
        /// </summary>
        protected override void RunSingleTask()
        {
            if (MvcApplication.Current.WebStores == null || isBusy)
                return;

            isBusy = true;
            foreach (var store in MvcApplication.Current.WebStores.Values.Where(e => e.IsTicklerCampaignsEnabled && e.IsTicklerCampaignsTimerEnabled && e.TicklerCampaignsManager != null))
            {
                if (IsStopRequested())
                    break;

                // all we do here is wake up the manager so it can do whatever independently
                store.TicklerCampaignsManager.WakeUp(() => IsStopRequested());
            }
            isBusy = false;
        }
    }
}