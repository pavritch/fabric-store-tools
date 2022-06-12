using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ProductScanner.Core.DataEntities.Store;

namespace ProductScanner.StoreData
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// Updates the ShowBuyButton for a single product.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="productID"></param>
        /// <param name="bShowBuyButton"></param>
        /// <returns>True if product found and updated.</returns>
        public static bool UpdateProductShowBuyButton(this StoreContext db, int productID, bool bShowBuyButton)
        {
            try
            {
                var rowCount = db.Database.ExecuteSqlCommand("Update [dbo].[Product] set [ShowBuyButton] = {0} where [ProductID] = {1}", (bShowBuyButton ? 1 : 0), productID);
                return rowCount == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Set inventory count to 0 for all variants of the given productID.
        /// </summary>
        /// <remarks>
        /// Used mainly when a product is discontinued.
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="productID"></param>
        /// <returns>Row count</returns>
        public static int MarkAllVariantsForProductOutOfStock(this StoreContext db, int productID)
        {
            try
            {
                int rowCount = db.Database.ExecuteSqlCommand("Update [dbo].[ProductVariant] set Inventory=0 where ProductID={0}", productID);
                return rowCount;
            }
            catch
            {
                return 0;
            }

        }

        /// <summary>
        /// Remove the product from the category.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="productID"></param>
        /// <param name="categoryID"></param>
        /// <returns>Row count</returns>
        public static int RemoveProductFromCategory(this StoreContext db, int productID, int categoryID)
        {
            try
            {
                int rowCount = db.Database.ExecuteSqlCommand("Delete [dbo].[ProductCategory] where [ProductID] = {0} and [CategoryID] = {1}", productID, categoryID);
                return rowCount;
            }
            catch
            {
                return 0;
            }

        }

        /// <summary>
        /// Remove the product variant.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="productID"></param>
        /// <param name="categoryID"></param>
        /// <returns>True if found and removed.</returns>
        public static bool RemoveProductVariant(this StoreContext db, int variantID)
        {
            try
            {
                int rowCount = db.Database.ExecuteSqlCommand("Delete [dbo].[ProductVariant] where [VariantID] = {0}", variantID);
                return rowCount == 1;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Remove a variantID from the ShoppingCart table - which includes both true cart and bookmarks.
        /// </summary>
        /// <remarks>
        /// Intended for when a variant is removed from the system.
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="variantID"></param>
        /// <returns></returns>
        public static int RemoveProductVariantFromShoppingCartAndBookmarks(this StoreContext db, int variantID)
        {
            try
            {
                int rowCount = db.Database.ExecuteSqlCommand("Delete [dbo].[ShoppingCart] where [VariantID] = {0}", variantID);
                return rowCount;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Remove all variants for ProductID from cart, but okay to leave in bookmarks.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static int RemoveProductFromShoppingCart(this StoreContext db, int productID)
        {
            try
            {
                // CartType:
                //    ShoppingCart = 0,
                //    WishCart = 1,
                //    RecurringCart = 2,
                //    GiftRegistryCart = 3,
                //    Deleted = 101

                int rowCount = db.Database.ExecuteSqlCommand("Delete [dbo].[ShoppingCart] where [ProductID] = {0} and [CartType]=0", productID);
                return rowCount;
            }
            catch
            {
                return 0;
            }
        }


        /// <summary>
        /// Updates the stock count for the given variant.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="variantId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool UpdateProductVariantInventoryCount(this StoreContext db, int variantId, int count)
        {
            try
            {
                int rowCount = db.Database.ExecuteSqlCommand("Update [dbo].[ProductVariant] set [Inventory]={1} where [VariantID]={0}", variantId, count);
                return rowCount == 1;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Write out the dic of public product properties to the ProductLabels table so can easily be used by other system pieces.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="productId"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static bool UpdateProductLabels(this StoreContext db, int productId, Dictionary<string, string> dic)
        {
            try
            {
                db.Database.ExecuteSqlCommand("Delete [dbo].[ProductLabels] where [ProductID]={0}", productId);

                foreach (var item in dic)
                    db.Database.ExecuteSqlCommand("Insert [dbo].[ProductLabels] ([ProductID], [Label], [Value]) values ({0}, {1}, {2})", productId, item.Key, item.Value);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Update Ext4 for a product - this column holds our JSON class.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="productID"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool UpdateProductExtensionData4(this StoreContext db, int productID, string Value)
        {
            try
            {
                int rowCount = db.Database.ExecuteSqlCommand("Update [dbo].[Product] set [ExtensionData4] = {1} where [ProductID] = {0}", productID, Value);
                return rowCount == 1;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Queues up this productID for having its images processed.
        /// </summary>
        /// <remarks>
        /// Processing is performed periodically by the InsideStoresDataService (separate web app).
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool EnqueueProductForImageProcessing(this StoreContext db, int productId)
        {
            try
            {
                // must be unique ProductID, and will silently throw here if ever a duplicate - we don't care
                db.Database.ExecuteSqlCommand("Insert [dbo].[ImageProcessingQueue] (ProductID, CreatedOn) values ({0}, GetDate())", productId);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Swap default variants. To be called within a transaction.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="oldDefaultVariantID"></param>
        /// <param name="newDefaultVariantID"></param>
        /// <returns></returns>
        public static bool ChangeDefaultProductVariant(this StoreContext db, int oldDefaultVariantID, int newDefaultVariantID)
        {
            try
            {
                db.Database.ExecuteSqlCommand("Update [dbo].[ProductVariant] set [IsDefault]=0 where [VariantID]={0}", oldDefaultVariantID);
                db.Database.ExecuteSqlCommand("Update [dbo].[ProductVariant] set [IsDefault]=1 where [VariantID]={0}", newDefaultVariantID);
                return true;
            }
            catch
            {
                return false;
            }

        }


        public static bool UpdateProductVariantPricing(this StoreContext db, int variantID, decimal Cost, decimal RetailPrice, decimal OurPrice, decimal? SalePrice)
        {
            try
            {
                var rowCount = db.Database.ExecuteSqlCommand("Update [dbo].[ProductVariant] set [Cost]={1}, [MSRP]={2}, [Price]={3}, [SalePrice]={4} where [VariantID]={0}", variantID, Cost, RetailPrice, OurPrice, SalePrice);
                return rowCount == 1;
            }
            catch
            {
                return false;
            }

        }


        public static bool UpdateProductWithSwatchInventoryCount(this StoreContext db, int productId, int count)
        {
            try
            {
                int rowCount = db.Database.ExecuteSqlCommand("Update [dbo].[ProductVariant] set [Inventory]={1} where [ProductID]={0} and ([IsDefault]=1 or [Name]='Swatch')", productId, count);
                return rowCount > 0;
            }
            catch
            {
                return false;
            }

        }


        public static bool AddProductManufacturerAssociation(this StoreContext db, int productID, int manufacturerID, bool requiresNewTransaction = false)
        {
            try
            {
                
                // if already in a transaction, do not nest, seems to cause problems
                if (!requiresNewTransaction)
                {
                    // remove any existing, just to be sure
                    db.Database.ExecuteSqlCommand("Delete [dbo].[ProductManufacturer] where [ProductID]={0}", productID);
                    int rowCount = db.Database.ExecuteSqlCommand("Insert [dbo].[ProductManufacturer] ([ProductID], [ManufacturerID]) values ({0}, {1})", productID, manufacturerID);
                    return rowCount == 1;
                }

                using (var scope = new TransactionScope())
                {
                    // remove any existing, just to be sure
                    db.Database.ExecuteSqlCommand("Delete [dbo].[ProductManufacturer] where [ProductID]={0}", productID);

                    int rowCount = db.Database.ExecuteSqlCommand("Insert [dbo].[ProductManufacturer] ([ProductID], [ManufacturerID]) values ({0}, {1})", productID, manufacturerID);

                    scope.Complete();
                    return rowCount == 1;
                }
            }
            catch
            {
                return false;
            }
        }


        public static bool AddProductCategoryAssociation(this StoreContext db, int productID, int categoryID, bool requiresNewTransaction=false)
        {
            try
            {

                // if (db.Database.CurrentTransaction != null) is always null even when I think it should have some trx object

                // if already in a transaction, do not nest, seems to cause problems
                if (!requiresNewTransaction)
                {
                    if (db.ProductCategory.Count(e => e.ProductID == productID && e.CategoryID == categoryID) > 0)
                        return true;

                    int rowCount = db.Database.ExecuteSqlCommand("Insert [dbo].[ProductCategory] ([ProductID], [CategoryID]) values ({0}, {1})", productID, categoryID);

                    return rowCount == 1;
                }

                using (var scope = new TransactionScope())
                {
                    if (db.ProductCategory.Count(e => e.ProductID == productID && e.CategoryID == categoryID) > 0)
                        return true;

                    int rowCount = db.Database.ExecuteSqlCommand("Insert [dbo].[ProductCategory] ([ProductID], [CategoryID]) values ({0}, {1})", productID, categoryID);

                    scope.Complete();
                    return rowCount == 1;
                }
            }
            catch
            {
                return false;
            }
        }


        public static bool RemoveProductCategoryAssociation(this StoreContext db, int productID, int categoryID)
        {
            try
            {
                db.Database.ExecuteSqlCommand("Delete [dbo].[ProductCategory] where [ProductID]={0} and [CategoryID]={1}", productID, categoryID);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove a product from a category using its variantID.
        /// </summary>
        /// <remarks>
        /// Notice that the logic checks to act only on the default variant. If not default, then skips action.
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="variantID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public static bool RemoveProductCategoryAssociationByVariantID(this StoreContext db, int variantID, int categoryID)
        {
            try
            {
                db.Database.ExecuteSqlCommand("if exists(select [ProductID] from [dbo].[ProductVariant] where [VariantID]={0} and [IsDefault]=1)\nbegin \n   delete from [dbo].[ProductCategory] where [ProductID]=(select [ProductID] from [ProductVariant] where [VariantID]={0} and [IsDefault]=1) and [CategoryID]={1}\nend", variantID, categoryID);
                return true;
            }
            catch
            {
                return false;
            }
        }


    }

}
