using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace StoreCurator
{
    public class Database : IDatabase
    {
        public bool SetPretty(int productID, bool isPretty)
        {
            try
            {
                using (var dc = new AspStoreDataContext())
                {
                    // sets display order to 1 when pretty
                    dc.ProductManufacturers.UpdatePretty(productID, isPretty);
                    return true;
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        public bool SetPublished(int productID, bool isPublished)
        {
            try
            {
                using (var dc = new AspStoreDataContext())
                {
                    dc.Products.UpdatePublished(productID, isPublished);
                    return true;
                }
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        public bool RemoveCategory(int productID, int categoryID)
        {
            try
            {
                using (var dc = new AspStoreDataContext())
                {
                    dc.ProductCategories.RemoveProductCategoryAssociationsForProduct(productID, new int[] {categoryID});
                    return true;
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        public bool AddCategory(int productID, int categoryID)
        {
            try
            {
                using (var dc = new AspStoreDataContext())
                {
                    dc.ProductCategories.AddProductCategoryAssociationsForProduct(productID, new int[] { categoryID });
                    return true;
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }
    }
}