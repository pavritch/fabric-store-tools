using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopifyCommon;

namespace ShopifyUpdateProducts
{
    public class StockTracker
    {

        private string ConnectionString { get; set; }
        private HashSet<int> LiveProducts;
        private Dictionary<int, decimal> VariantPrices { get; set; }

        private bool isLoaded = false;

        public StoreKey Key { get; private set; }

        public StockTracker(StoreKey storeKey, string connectionString)
        {
            Key = storeKey;
            this.ConnectionString = connectionString;
            Load();
        }

        private void Load()
        {
            if (isLoaded)
                return;

            var startTime = DateTime.Now;
            using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
            {
                LiveProducts = new HashSet<int>(dc.Products.Where(e => e.ShowBuyButton == 1 && e.Deleted==0 && e.Published==1).Select(e => e.ProductID).ToList());
                VariantPrices = dc.ProductVariants.Where(e => e.IsDefault == 1 && e.Inventory > 0).Select(e => new { e.ProductID, e.Price }).ToDictionary(k => k.ProductID, v => v.Price);
            }
            isLoaded = true;
            var endTime = DateTime.Now;

            Console.WriteLine(string.Format("Time to load {0}: {1}", Key, endTime - startTime));
        }

        public double? GetPrice(int productID)
        {
            decimal price;
            if (VariantPrices.TryGetValue(productID, out price))
                return (double)price;

            return null;
        }

        public ProductStatus GetStatus(int productID)
        {
            if (!LiveProducts.Contains(productID))
                return ProductStatus.Deleted;

            return VariantPrices.ContainsKey(productID) ? ProductStatus.InStock : ProductStatus.OutOfStock;
        }

    }
}
