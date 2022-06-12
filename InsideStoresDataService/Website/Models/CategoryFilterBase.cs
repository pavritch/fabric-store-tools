//------------------------------------------------------------------------------
// 
// Class: CategoryFilterBase 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using Website.Entities;
using System.Data.Linq;

namespace Website
{
    #region Class CategoryFilterInformation
    public class CategoryFilterInformation
    {
        /// <summary>
        /// SQL CategoryID assigned to this entity.
        /// </summary>
        /// <remarks>
        /// Must be 0 for insert, gets set to actual ID.
        /// </remarks>
        public int CategoryID { get; set; }

        /// <summary>
        /// A fixed GUID to be associated with this category.
        /// </summary>
        /// <remarks>
        /// Cannot let this be auto-assigned! Since CategoryID is an identity key,
        /// we use this GUID as a fail-safe way to identify this record even if other things change.
        /// </remarks>
        public Guid CategoryGUID { get; set; }

        /// <summary>
        /// CategoryID of the parent.
        /// </summary>
        /// <remarks>
        /// Only the very top root (Filters) can have 0 here.
        /// </remarks>
        public int ParentCategoryID { get; set; }

        /// <summary>
        /// Name to be used in menus. Should not include suffix like Rugs or Fabric.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Used for creating SETitle, Description. No HTML.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Determines the order of display in menus.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// URL fragment.
        /// </summary>
        public string SEName { get; set; }

        /// <summary>
        /// Keyword phrases to be associated with the category.
        /// </summary>
        public string Keywords { get; set; }

        public CategoryFilterInformation()
        {
            CategoryGUID = Guid.NewGuid();
        }
    } 
    #endregion

    public class CategoryFilterBase
    {
        protected IWebStore Store { get; private set; }

        public CategoryFilterBase(IWebStore Store)
        {
            this.Store = Store;

        }

        protected bool CategoryExists(string Name, int ParentCategoryID)
        {
            using (var dc = new AspStoreDataContext(Store.ConnectionString))
            {
                var countForName = dc.Categories.Where(e => e.ParentCategoryID == ParentCategoryID && e.Name == Name).Count();
                return countForName > 0;
            }
        }

        protected bool CategoryExists(Guid CategoryGUID)
        {
            using (var dc = new AspStoreDataContext(Store.ConnectionString))
            {
                var countForGUID = dc.Categories.Where(e => e.CategoryGUID == CategoryGUID).Count();
                return countForGUID > 0;
            }
        }

        protected int? GetCategoryID(Guid CategoryGUID)
        {
            using (var dc = new AspStoreDataContext(Store.ConnectionString))
            {
                var id = dc.Categories.Where(e => e.CategoryGUID == CategoryGUID).Select(e => e.CategoryID).FirstOrDefault();

                if (id != 0)
                    return id;

                return null;
            }
        }

        protected int EnsureCategoryExists(CategoryFilterInformation info)
        {
            var id = GetCategoryID(info.CategoryGUID);
            if (id.HasValue)
            {
                info.CategoryID = id.Value;
                return id.Value;
            }

            MakeCategory(info);
            return info.CategoryID;
        }

        protected bool MakeCategory(CategoryFilterInformation info)
        {
            try
            {
                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    // make sure none with that GUID exist anywhere

                    if (CategoryExists(info.CategoryGUID))
                        throw new Exception("Attempting to create new SQL Category using duplicate GUID.");

                    var countForName = dc.Categories.Where(e => e.ParentCategoryID == info.ParentCategoryID && e.Name == info.Name).Count();
                    if (countForName > 0)
                        throw new Exception("Attempting to create new SQL Category using duplicate name under same parent.");

                    var productKind = Store.StoreKey == StoreKeys.InsideRugs ? " Rugs" : string.Empty;

                    var cat = new Category()
                    {
                        CategoryGUID = info.CategoryGUID,
                        ParentCategoryID = info.ParentCategoryID,
                        Name = info.Name,
                        Description = info.Title != null ? string.Format("<h1>{0}</h1>", info.Title.HtmlEncode()) : string.Format("<h1>{0}{1}</h1>", info.Name.HtmlEncode(), productKind.HtmlEncode()),
                        DisplayOrder = info.DisplayOrder,
                        SEName = info.SEName ?? info.Name.ToLower().Replace(" ", "-").Replace("&", "and"),
                        XmlPackage = "entity.gridwithprices.xml.config",
                        SETitle = info.Title ?? string.Format("{0}{1}", info.Name, productKind),
                        SEDescription = (info.Title ?? string.Format("{0}{1}", info.Name, productKind)) + ". Shop thousands of top-quality area rugs. Free shipping available.",
                        SEKeywords = info.Keywords ?? info.Name + productKind,
                        Summary = info.Keywords, // reverse lookup keywords to index into member products to help full text search
                        Published = 1,
                        Deleted = 0,
                        ExtensionData = info.Keywords != null ? info.Keywords.ToLower() : null,
                        ImageFilenameOverride = null,

                        // below - required for insert to work
                        TemplateName = string.Empty,
                        CreatedOn = DateTime.Now,
                    };

                    dc.Categories.InsertOnSubmit(cat);
                    dc.SubmitChanges();
                    info.CategoryID = cat.CategoryID;

                    Debug.WriteLine(string.Format("Created category: {0}", info.Name));

                }

                return true;
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        protected void RemoveUnprotectedCategories(int parentCategoryID, List<int> protectedCategories)
        {
            // intended for allowing manager to clean up categories no longer referenced

            Debug.Assert(parentCategoryID != 0);
            Debug.Assert(protectedCategories != null && protectedCategories.Count() > 0);

            if (parentCategoryID == 0)
                throw new Exception("Not allowed to prune at the absolute root.");

            if (protectedCategories == null || protectedCategories.Count() == 0)
                throw new Exception("Invalid all to RemoveUnprotectedCategories().");

            try
            {
                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    var orphanCategories = dc.Categories.Where(e => e.ParentCategoryID == parentCategoryID && !protectedCategories.Contains(e.CategoryID)).Select(e => e.CategoryID).ToList();

                    if (orphanCategories.Count() == 0)
                        return;

                    foreach (var id in orphanCategories)
                    {
                        // remove associations
                        dc.ProductCategories.RemoveProductCategoryAssociationsForCategory(id);

                        // remove the category itself
                        dc.Categories.RemoveCategory(id);
                    }
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        protected void RemoveProductCategoryAssociationsForProduct(int productID, List<int> categories)
        {
            try
            {
                if (categories == null || categories.Count() == 0)
                    return;

                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    dc.ProductCategories.RemoveProductCategoryAssociationsForProduct(productID, categories);
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        protected void AddProductCategoryAssociationsForProduct(int productID, List<int> categories)
        {
            try
            {
                if (categories == null || categories.Count() == 0)
                    return;

                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    dc.ProductCategories.AddProductCategoryAssociationsForProduct(productID, categories);
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }
    }
}