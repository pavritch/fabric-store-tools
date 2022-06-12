using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Website
{
    public class ManufacturerMetric
    {
        public int TotalCount { get; set; }
        public int DiscontinuedCount { get; set; }

        /// <summary>
        /// Total not discontinued - in theory, same as InStock plus OutOfStock
        /// </summary>
        public int AvailableCount { get; set; }

        /// <summary>
        /// In stock and not discontinued.
        /// </summary>
        public int InStockCount { get; set; }

        /// <summary>
        /// Not discontinued, out of stock.
        /// </summary>
        public int OutOfStockCount { get; set; }

        public string ManufacturerName { get; set; }
        public StoreKeys StoreKey { get; set; }

        public ManufacturerMetric()
        {

        }

        public ManufacturerMetric(StoreKeys storeKey, string name, int total, int discontinued, int available, int inStock, int outOfStock)
        {
            TotalCount = total;
            DiscontinuedCount = discontinued;
            AvailableCount = available;
            InStockCount = inStock;
            OutOfStockCount = outOfStock;
            ManufacturerName = name;
            StoreKey = storeKey;
        }

        public static List<ManufacturerMetric> GetCounts(IWebStore store)
        {
            const int CacheTimeSeconds = 60 * 60; // 1hr;

            Func<string> makeCacheKey = () =>
                {
                    return string.Format("ListOfManufacturerMetrics:{0}", store.StoreKey);
                };

            List<ManufacturerMetric> data;

            data = HttpRuntime.Cache[makeCacheKey()] as List<ManufacturerMetric>;

            if (data != null)
                return data;

            data = new List<ManufacturerMetric>();

            // fabric site uses product variants differently and therefore needs special case

            if (store.StoreKey == StoreKeys.InsideFabric || store.StoreKey == StoreKeys.InsideWallpaper)
            {
                using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                {
                    data = (from m in dc.Manufacturers
                            where m.Published == 1 && m.Deleted == 0
                            orderby m.Name
                            let productList = dc.ProductManufacturers.Where(e => e.ManufacturerID == m.ManufacturerID).Select(e => e.ProductID)
                            let productListCount = productList.Count()
                            let availableProducts = dc.Products.Where(e => e.ShowBuyButton == 1 && e.Published == 1 && e.Deleted == 0 && productList.Contains(e.ProductID)).Select(e => e.ProductID)
                            let availableCount = availableProducts.Count()
                            let inStockCount = dc.ProductVariants.Where(e => availableProducts.Contains(e.ProductID) && e.Inventory > 0 && e.IsDefault == 1).Count()
                            let outOfStockCount = availableCount - inStockCount
                            select new ManufacturerMetric()
                            {
                                ManufacturerName = m.Name,
                                StoreKey = store.StoreKey,
                                TotalCount = productListCount,
                                AvailableCount = availableCount,
                                DiscontinuedCount = productListCount - availableCount,
                                InStockCount = inStockCount,
                                OutOfStockCount = outOfStockCount,
                            }).ToList();

                    // remove the word fabric/fabrics or wallcoverings, etc from names

                    foreach (var m in data)
                        m.ManufacturerName = m.ManufacturerName.Replace(" Fabrics", string.Empty).Replace(" Fabric", string.Empty)
                            .Replace(" Wallpapers", string.Empty).Replace(" Wallpaper", string.Empty).Replace(" Wallcoverings", string.Empty).Replace(" Wallcovering", string.Empty).Trim();

                }
            }
            else
            {
                using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                {
                    data = (from m in dc.Manufacturers
                            where m.Published == 1 && m.Deleted == 0
                            orderby m.Name
                            let productList = dc.ProductManufacturers.Where(e => e.ManufacturerID == m.ManufacturerID).Select(e => e.ProductID)
                            let productVariantList = dc.ProductVariants.Where(e => productList.Contains(e.ProductID))
                            let productVariantListCount = productVariantList.Count()
                            let availableProductVariants = productVariantList.Where(e => e.Deleted == 0 && e.Published == 1)
                            let availableCount = availableProductVariants.Count()
                            let inStockCount = availableProductVariants.Where(e => e.Inventory > 0).Count()
                            let outOfStockCount = availableCount - inStockCount
                            select new ManufacturerMetric()
                            {
                                ManufacturerName = m.Name,
                                StoreKey = store.StoreKey,
                                TotalCount = productVariantListCount,
                                AvailableCount = availableCount,
                                DiscontinuedCount = productVariantListCount - availableCount,
                                InStockCount = inStockCount,
                                OutOfStockCount = outOfStockCount,
                            }).ToList();
                }
            }

            HttpRuntime.Cache.Insert(makeCacheKey(), data, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(CacheTimeSeconds), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);

            return data;
        }

    }
}