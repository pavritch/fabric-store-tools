using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.StockChecks.StockCheckManagers
{
    public class SqlLoadStockCheckManager<T> : IStockCheckManager<T> where T : Store
    {
        private readonly IStockCheckManager<T> _stockCheckManager;
        private readonly IStoreDatabase<T> _storeDatabase;

        // hack
        private readonly IStoreDatabase<InsideWallpaperStore> _wallpaperStoreDatabase;

        public SqlLoadStockCheckManager(IStockCheckManager<T> stockCheckManager, IStoreDatabase<T> storeDatabase, IStoreDatabase<InsideWallpaperStore> wallpaperStoreDatabase)
        {
            _stockCheckManager = stockCheckManager;
            _storeDatabase = storeDatabase;
            _wallpaperStoreDatabase = wallpaperStoreDatabase;
        }

        public async Task<List<StockCheckResult>> CheckStockAsync(List<StockCheck> stockChecks)
        {
            await PopulateStockChecksAsync(stockChecks);
            var currentlyDiscontinued = stockChecks.Where(x => x.IsDiscontinued).ToList();

            // don't want to send through ones where we're already discontinued
            var results = await _stockCheckManager.CheckStockAsync(stockChecks.Where(x => !x.IsDiscontinued).ToList());

            var updatedProducts = FindUpdatedProducts(stockChecks, results);

            foreach (var updatedProduct in updatedProducts)
            {
                StockCheck productInfo = updatedProduct.Item1;
                bool inStock = updatedProduct.Item2;

                if (productInfo.HasSwatch)
                {
                    // note that ProductID is used here
                    updatedProducts.ForEach(x => _storeDatabase.UpdateProductWithSwatchInventory(productInfo.ProductId, inStock ? InventoryStatus.InStock : InventoryStatus.OutOfStock));
                }
                else
                {
                    // just the single variant is updated
                    updatedProducts.ForEach(x => _storeDatabase.UpdateProductVariantInventory(productInfo.VariantId, inStock ? InventoryStatus.InStock : InventoryStatus.OutOfStock));
                }

                // START HACK FOR WALLPAPER
                // START HACK FOR WALLPAPER
                // START HACK FOR WALLPAPER

                // stock queries wallcovering products need to sync updates with both InsideFabric and InsideWallpaper SQL databases.

                // note that the query will be sent to this web service from either InsideFabric or InsideWallpaper websites, each using
                // their correct website store identifier. It is fully up to this web service to virtualize such accordingly

                // Since in theory, we consider the Fabric website as the "truth" -- it would be perfectly acceptable to have the wallpaper
                // controller change the store to be InsideFabric, and then let the entire system work the request as if it were from the 
                // fabric site, and then here....this test below would be correct, and all is good.

                // UPDATE: virtualization now takes place at the controller, so InsideWallpaper queries flow through as InsideFabric.

                if (productInfo.ProductGroup == ProductGroup.Wallcovering && (typeof(T) == typeof(InsideFabricStore)))
                {
                    if (productInfo.HasSwatch)
                    {
                        // note that ProductID is used here
                        updatedProducts.ForEach(x => _wallpaperStoreDatabase.UpdateProductWithSwatchInventory(productInfo.ProductId, inStock ? InventoryStatus.InStock : InventoryStatus.OutOfStock));

                    }
                    else
                    {
                        // just the single variant is updated
                        updatedProducts.ForEach(x => _wallpaperStoreDatabase.UpdateProductVariantInventory(productInfo.VariantId, inStock ? InventoryStatus.InStock : InventoryStatus.OutOfStock));
                    }
                }

                // END HACK FOR WALLPAPER
                // END HACK FOR WALLPAPER
                // END HACK FOR WALLPAPER

            }

            results.AddRange(CreateDiscontinuedResults(currentlyDiscontinued));
            return results;
        }

        private List<StockCheckResult> CreateDiscontinuedResults(List<StockCheck> stockChecks)
        {
            return stockChecks.Select(x => new StockCheckResult
            {
                VariantId = x.VariantId,
                StockCheckStatus = StockCheckStatus.Discontinued
            }).ToList();
        }

        private List<Tuple<StockCheck, bool>> FindUpdatedProducts(IEnumerable<StockCheck> stockChecks, List<StockCheckResult> results)
        {
            var updates = new List<Tuple<StockCheck, bool>>();
            foreach (var check in stockChecks)
            {
                var result = results.FirstOrDefault(x => x.VariantId == check.VariantId);
                if (result != null)
                {
                    // now in stock
                    if (check.CurrentStock == 0 && result.StockCheckStatus == StockCheckStatus.InStock)
                        updates.Add(new Tuple<StockCheck, bool>(check, true));

                    // now out of stock
                    if (check.CurrentStock != 0 && (result.StockCheckStatus == StockCheckStatus.OutOfStock || result.StockCheckStatus == StockCheckStatus.Discontinued))
                        updates.Add(new Tuple<StockCheck, bool>(check, false));
                }
            }
            return updates;
        }

        private async Task<List<StockCheck>> PopulateStockChecksAsync(List<StockCheck> stockChecks)
        {
            var queryResults = await _storeDatabase.GetStockCheckInfoAsync(stockChecks.Select(x => x.VariantId));
            foreach (var queryResult in queryResults)
            {
                var stockCheck = stockChecks.First(x => x.VariantId == queryResult.VariantId);
                stockCheck.VariantId = queryResult.VariantId;
                stockCheck.MPN = queryResult.MPN;
                stockCheck.Vendor = Vendor.GetById(queryResult.VendorId);
                stockCheck.CurrentStock = queryResult.CurrentStock;
                stockCheck.IsDiscontinued = queryResult.IsDiscontinued;
                stockCheck.PublicProperties = queryResult.PublicProperties;

                stockCheck.ProductGroup = queryResult.ProductGroup.ToProductGroup();
                stockCheck.ProductId = queryResult.ProductID;
                stockCheck.HasSwatch = queryResult.HasSwatch;
            }
            return stockChecks;
        }
    }
}