using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Metrics for a single day for display in control panel.
    /// </summary>
    public class ProductCountsByManufacturerMetric
    {

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

        public int Total { get; set; }
        public int LiveWithImage { get; set; } // has image, not discontinued
        public int InStock { get; set; }
        public int OutOfStock { get; set; }

        public int Under25 { get; set; }
        public int D25to49 { get; set; }
        public int D50to74 { get; set; }
        public int D75to99 { get; set; }
        public int D100to149 { get; set; }
        public int D150to199 { get; set; }
        public int D200to299 { get; set; }
        public int D300Plus { get; set; }

        public int Under25InStock { get; set; }
        public int D25to49InStock { get; set; }
        public int D50to74InStock { get; set; }
        public int D75to99InStock { get; set; }
        public int D100to149InStock { get; set; }
        public int D150to199InStock { get; set; }
        public int D200to299InStock { get; set; }
        public int D300PlusInStock { get; set; }

        public ProductCountsByManufacturerMetric()
        {
        }

        public static List<ProductCountsByManufacturerMetric> GetMetrics(IWebStore store)
        {
            var list = new List<ProductCountsByManufacturerMetric>();
            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                var dicManufacturers = dc.Manufacturers.Where(e => e.Published == 1 && e.Deleted == 0).Select(e => new { e.ManufacturerID, e.Name }).ToDictionary(k => k.ManufacturerID, v => v.Name);
                var productRecords = store.ProductData.Products;

                foreach(var item in dicManufacturers)
                {
                    try
                    {
                        var manufacturerID = item.Key;
                        var manufacturerName = item.Value;

                        // list of productID for this manufacturer
                        var productList = store.ProductData.Manufacturers[manufacturerID];
                        var productHash = new HashSet<int>(productList);

                        // only when has products, because IF has wallpaper, etc, which is filtered out at this point
                        if (productList.Count() == 0)
                            continue;

                        var prices = (from pv in dc.ProductVariants
                                      where pv.IsDefault == 1
                                      join pm in dc.ProductManufacturers on pv.ProductID equals pm.ProductID
                                      where pm.ManufacturerID == manufacturerID
                                      select new { pv.ProductID, pv.Price }).ToDictionary(k => k.ProductID, v => v.Price);

                        Func<decimal, decimal, int> GetPriceRangeCount = (low, high) =>
                            {
                                int count = 0;

                                foreach(var productPrice in prices)
                                {
                                    var price = productPrice.Value;
                                    var productID = productPrice.Key;

                                    // must be live with image; this is valid existing filter on that criteria
                                    if (!productHash.Contains(productID))
                                        continue;

                                    if (price >= low && price <= high)
                                        count++;
                                }
                                return count;
                            };

                        Func<decimal, decimal, int> GetPriceRangeCountInStock = (low, high) =>
                        {
                            int count = 0;

                            foreach (var productPrice in prices)
                            {
                                var price = productPrice.Value;
                                var productID = productPrice.Key;

                                // must be live with image AND in stock; this is valid existing filter on that criteria
                                if (!productHash.Contains(productID) || !productRecords.ContainsKey(productID) || productRecords[productID].StockStatus != InventoryStatus.InStock)
                                    continue;

                                if (price >= low && price <= high)
                                    count++;
                            }
                            return count;
                        };
                        var info = new ProductCountsByManufacturerMetric()
                        {
                            ManufacturerID = manufacturerID,
                            ManufacturerName = manufacturerName,
                            Total = dc.ProductManufacturers.Where(e => e.ManufacturerID == manufacturerID).Count(),
                            
                            LiveWithImage = productList.Where(e => !productRecords[e].IsDiscontinued).Count(),

                            InStock = productList.Where(e => !productRecords[e].IsDiscontinued && !productRecords[e].IsMissingImage && productRecords[e].StockStatus == InventoryStatus.InStock).Count(),
                            OutOfStock = productList.Where(e => !productRecords[e].IsDiscontinued && !productRecords[e].IsMissingImage && productRecords[e].StockStatus == InventoryStatus.OutOfStock).Count(),

                            Under25 = GetPriceRangeCount(0M, 24.99M),
                            D25to49 = GetPriceRangeCount(25M, 49.99M),
                            D50to74 = GetPriceRangeCount(50M, 74.99M),
                            D75to99 = GetPriceRangeCount(75M, 99.99M),
                            D100to149 = GetPriceRangeCount(100M, 149.99M),
                            D150to199 = GetPriceRangeCount(150M, 199.99M),
                            D200to299 = GetPriceRangeCount(200M, 299.99M),
                            D300Plus = GetPriceRangeCount(300M, Decimal.MaxValue),

                            Under25InStock = GetPriceRangeCountInStock(0M, 24.99M),
                            D25to49InStock = GetPriceRangeCountInStock(25M, 49.99M),
                            D50to74InStock = GetPriceRangeCountInStock(50M, 74.99M),
                            D75to99InStock = GetPriceRangeCountInStock(75M, 99.99M),
                            D100to149InStock = GetPriceRangeCountInStock(100M, 149.99M),
                            D150to199InStock = GetPriceRangeCountInStock(150M, 199.99M),
                            D200to299InStock = GetPriceRangeCountInStock(200M, 299.99M),
                            D300PlusInStock = GetPriceRangeCountInStock(300M, Decimal.MaxValue),

                        };

                        list.Add(info);
                    }
                    catch(Exception Ex)
                    {
                        Debug.WriteLine(Ex.Message);
                    }
                }
            }

            // return ordered by name
            return list.OrderBy(e => e.ManufacturerName).ToList();
        }
    }
}