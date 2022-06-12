using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using Website.Entities;

namespace Website
{
    public static partial class SqlQueryExtensions
    {
        public static void ClearAllFeaturedProducts(this Table<Product> entity)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [IsFeatured]=0");
        }

        public static void MarkAsFeatured(this Table<Product> entity, int ProductID)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [IsFeatured]=1 where [ProductID] = {0} and [ShowBuyButton]=1", ProductID);
        }

        public static void MarkProductDiscontinued(this Table<Product> entity, int ProductID)
        {
            // Inside Fabric
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [ShowBuyButton] = 0 where [ProductID] = {0}", ProductID);
        }

        public static void CompletelyDeleteProduct(this Table<Product> entity, int ProductID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[ProductCategory] where [ProductID] = {0}", ProductID);
            entity.Context.ExecuteCommand("Delete [dbo].[ProductManufacturer] where [ProductID] = {0}", ProductID);
            entity.Context.ExecuteCommand("Delete [dbo].[ProductVariant] where [ProductID] = {0}", ProductID);
            entity.Context.ExecuteCommand("Delete [dbo].[Product] where [ProductID] = {0}", ProductID);
            entity.Context.ExecuteCommand("Delete [dbo].[ProductFeatures] where [ProductID] = {0}", ProductID);
        }

        public static void DeleteSwatchVariant(this Table<ProductVariant> entity, int ProductID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[ProductVariant] where [ProductID] = {0} and [IsDefault]=0 ", ProductID);
        }

        public static void DeleteByVariantID(this Table<ProductVariant> entity, int VariantID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[ProductVariant] where [VariantID] = {0}", VariantID);
        }

        public static void SetPublished(this Table<Category> entity, int CategoryID, bool isPublished)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Category] set [Published] = {1} where [CategoryID] = {0}", CategoryID, isPublished ? 1 : 0);
        }

        public static void DelelteAllByCategory(this Table<ProductCategory> entity, int CategoryID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[ProductCategory] where [CategoryID] = {0}", CategoryID);
        }

        public static void AddProductToCategory(this Table<ProductCategory> entity, int CategoryID, int ProductID)
        {
            try
            {
                entity.Context.ExecuteCommand("Insert into [dbo].[ProductCategory] (CategoryID, ProductID, CreatedOn, UpdatedOn) values ({0}, {1}, {2}, {2})", CategoryID, ProductID, DateTime.Now);
            }
            catch { }
        }

        public static void RemoveProductFromCategory(this Table<ProductCategory> entity, int CategoryID, int ProductID)
        {

            try
            {
                entity.Context.ExecuteCommand("Delete from [dbo].[ProductCategory] where [CategoryID] = {0} and [ProductID] = {1}", CategoryID, ProductID);
            }
            catch { }
        }

    }
}