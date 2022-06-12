using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Metrics for a single day for display in control panel.
    /// </summary>
    public class SalesSummaryMetric
    {
        public DateTime Date { get; set; }

        /// <summary>
        /// Number of orders for this day for display in timebar.
        /// </summary>
        public int OrderCount { get; set; }

        public decimal TotalSales { get; set; }

        public decimal Voided { get; set; }
        public decimal Refunded { get; set; }
        public decimal NetSales { get; set; }

        public decimal Authorized { get; set; }
        public decimal Captured { get; set; }

        // positive number subtracts from the order value,
        // a neg number (rare) would effectively add to the order value
        public decimal Adjustments { get; set; }

        // this group of 3 should add up to total sales

        public decimal SalesTax { get; set; }
        public decimal Shipping { get; set; }
        public decimal ProductSubtotal { get; set; }


        public SalesSummaryMetric()
        {

        }

    }
}