using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data.Linq;
using StoreCurator.Entities;
using System.Diagnostics;

namespace StoreCurator
{
    public static class DatabaseExtensions
    {

        public static void UpdatePublished(this Table<Product> entity, int ProductID, bool isPublished)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [Published] = {1} where [ProductID] = {0}", ProductID, isPublished.ToInteger());
        }

        public static void UpdatePretty(this Table<ProductManufacturer> entity, int ProductID, bool isPretty)
        {
            entity.Context.ExecuteCommand("Update [dbo].[ProductManufacturer] set [DisplayOrder] = {1} where [ProductID] = {0}", ProductID, isPretty.ToInteger());
        }


        public static void RemoveProductCategoryAssociationsForProduct(this Table<ProductCategory> entity, int ProductID, IEnumerable<int> categories)
        {
            Debug.Assert(ProductID != 0);
            Debug.Assert(categories != null && categories.Count() > 0);

            if (categories == null || categories.Count() == 0)
                return;

            try
            {
                var sbList = new StringBuilder();
                foreach (var c in categories)
                {
                    if (sbList.Length > 0)
                        sbList.Append(",");
                    sbList.Append(c.ToString());
                }

                var sql = string.Format("Delete [dbo].[ProductCategory] where [ProductID] = {0} and [CategoryID] in ({1})", ProductID, sbList.ToString());
                entity.Context.ExecuteCommand(sql);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        public static void AddProductCategoryAssociationsForProduct(this Table<ProductCategory> entity, int ProductID, IEnumerable<int> categories)
        {
            Debug.Assert(ProductID != 0);
            Debug.Assert(categories != null && categories.Count() > 0);

            if (categories == null || categories.Count() == 0)
                return;

            try
            {
                foreach (var catID in categories)
                    entity.Context.ExecuteCommand(string.Format("Insert [dbo].[ProductCategory] ([ProductID], [CategoryID]) values ({0}, {1})", ProductID, catID));

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

    }
}