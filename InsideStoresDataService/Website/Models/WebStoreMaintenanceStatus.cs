using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Website
{
    public class WebStoreMaintenanceStatus
    {
        public string StoreKey { get; set; }
        public string IsStoreActionRunning { get; set; }
        public string StoreActionName { get; set; }
        public string StoreActionPercentComplete { get; set; }
        public string StoreActionTimeStarted { get; set; }

        public string IsProductActionRunning { get; set; }
        public string ProductActionName { get; set; }
        public string ProductActionPercentComplete { get; set; }
        public string ProductActionTimeStarted { get; set; }

        public string IsRebuildingAll { get; set; }
        public string IsRebuildingCategories { get; set; }
        public string IsRebuildingSearchData { get; set; }

        public string IsBackgroundTaskRunning { get; set; }
    }
}