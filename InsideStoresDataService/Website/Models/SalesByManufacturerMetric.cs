using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Metrics for a single day for display in control panel.
    /// </summary>
    public class SalesByManufacturerMetric
    {
        /// <summary>
        /// Date used for Sales By Manufacturer report.
        /// </summary>
        /// <remarks>
        /// Not filled in for pie chart report.
        /// </remarks>
        public DateTime Date { get; set; }

        /// <summary>
        /// Name of manufacturer for pie charts.
        /// </summary>
        /// <remarks>
        /// Not filled in for sales by manufacturer bar chart
        /// </remarks>
        public string ManufacturerName { get; set; }

        /// <summary>
        /// ID of manufacturer for pie charts.
        /// </summary>
        /// <remarks>
        /// Not filled in for sales by manufacturer bar chart
        /// </remarks>
        public int ManufacturerID { get; set; }

        /// <summary>
        /// Number of order line items for this day for display in timebar.
        /// </summary>
        public int ProductOrders { get; set; }

        /// <summary>
        /// Number of swatches sold.
        /// </summary>
        public int SwatchOrders { get; set; }

        /// <summary>
        /// Total number of yards sold of actual product (not swatches)
        /// </summary>
        public int ProductYards { get; set; }

        /// <summary>
        /// Sales of true product (not swatches)
        /// </summary>
        public decimal ProductSales { get; set; }

        /// <summary>
        /// Sales from just the swatches
        /// </summary>
        public decimal SwatchSales { get; set; }

        /// <summary>
        /// Combined product and swatch sales.
        /// </summary>
        public decimal TotalSales { get; set; }

        /// <summary>
        /// Margin selectively used - pie charts.
        /// </summary>
        public decimal Margin { get; set; }

        public SalesByManufacturerMetric()
        {
        }
    }
}