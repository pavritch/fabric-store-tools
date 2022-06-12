using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data.Linq;
using Website.Entities;
using System.Diagnostics;

namespace Website
{
    public static class DatabaseExtensions
    {

        public static void InsertProductLabel(this AspStoreDataContext dc, int ProductID, string Label, string Value)
        {
            dc.ExecuteCommand("Insert [dbo].[ProductLabels] ([ProductID], [Label], [Value]) values ({0}, {1}, {2})", ProductID, Label, Value);
        }

        public static void DeleteProductLabels(this AspStoreDataContext dc, int ProductID)
        {
            dc.ExecuteCommand("Delete [dbo].[ProductLabels] where [ProductID]={0}", ProductID);
        }

        public static void MarkAlgoliaProductOrphansToBeDeleted(this AspStoreDataContext dc)
        {
            dc.ExecuteCommand("update [dbo].[AlgoliaProducts] set [Action]=2 where [ProductID] not in (select [ProductID] from [dbo].[Product] where [Deleted]=0 and [Published]=1) ");
        }

        /// <summary>
        /// Write out the dic of public product properties to the ProductLabels table so can easily be used by other system pieces.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="productId"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static bool UpdateProductLabels(this AspStoreDataContext dc, int productId, Dictionary<string, string> dic)
        {
            try
            {
                dc.ExecuteCommand("Delete [dbo].[ProductLabels] where [ProductID]={0}", productId);

                foreach (var item in dic)
                    dc.ExecuteCommand("Insert [dbo].[ProductLabels] ([ProductID], [Label], [Value]) values ({0}, {1}, {2})", productId, item.Key, item.Value);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdateProductLabels(this InsideWallpaperSyncDataContext dc, int productId, Dictionary<string, string> dic)
        {
            try
            {
                dc.ExecuteCommand("Delete [dbo].[ProductLabels] where [ProductID]={0}", productId);

                foreach (var item in dic)
                    dc.ExecuteCommand("Insert [dbo].[ProductLabels] ([ProductID], [Label], [Value]) values ({0}, {1}, {2})", productId, item.Key, item.Value);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteImageProcessingQueueRecord(this AspStoreDataContext dc, int ProductID)
        {
            dc.ExecuteCommand("Delete [dbo].[ImageProcessingQueue] where [ProductID]={0}", ProductID);
        }

        public static List<string> FindSimilarSkuByPattern(this Table<Product> entity, int ManufacturerID, string Pattern)
        {
            try
            {
                var list = entity.Context.ExecuteQuery<string>("Select [SKU] from [dbo].[Product] where [ProductID] in (select [ProductID] from [dbo].[ProductManufacturer] where [ManufacturerID]={0}) and [ManufacturerPartNumber]={1} and ShowBuyButton=1 and Deleted=0", ManufacturerID, Pattern);
                return list.ToList();
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }

            return new List<string>();
        }

        public static void TruncateTable<T>(this Table<T> entity) where T : class
        {
            var mapping = entity.Context.Mapping.GetTable(typeof(T));
            string tableName = mapping.TableName;
            var sql = string.Format("Truncate table {0}", tableName);
            entity.Context.ExecuteCommand(sql);
        }

        public static void InsertAutoSuggestPhrase(this Table<AutoSuggestPhrase> entity, AutoSuggestPhrase record)
        {
            entity.Context.ExecuteCommand("insert [dbo].[AutoSuggestPhrases] ([PhraseListID], [Phrase], [Priority]) values ({0}, {1}, {2}) ", record.PhraseListID, record.Phrase, record.Priority);
        }

        public static void DeleteAutoSuggestPhrase(this Table<AutoSuggestPhrase> entity, int PhraseID)
        {
            entity.Context.ExecuteCommand("delete [dbo].[AutoSuggestPhrases] where PhraseID={0}", PhraseID);
        }

        public static void InsertProductCrossLink(this Table<ProductCrossLink> entity, int fromProductID, int toProductID, string linkText)
        {
            entity.Context.ExecuteCommand("insert [dbo].[ProductCrossLinks] ([ProductID], [TargetProductID], [TargetLinkText]) values ({0}, {1}, {2}) ", fromProductID, toProductID, linkText);
        }

        public static void UpdateExtensionData4(this Table<Product> entity, int ProductID, string Value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [ExtensionData4] = {1} where [ProductID] = {0}", ProductID, Value);
        }

        public static void UpdateExtensionData4(this Table<ProductVariant> entity, int VariantID, string Value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[ProductVariant] set [ExtensionData4] = {1} where [VariantID] = {0}", VariantID, Value);
        }

        public static void UpdateExtensionData1(this Table<Product> entity, int ProductID, string Value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [ExtensionData] = {1} where [ProductID] = {0}", ProductID, Value);
        }

        public static void UpdateExtensionData2(this Table<Product> entity, int ProductID, string Value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [ExtensionData2] = {1} where [ProductID] = {0}", ProductID, Value);
        }

        public static void UpdateExtensionData3(this Table<Product> entity, int ProductID, string Value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [ExtensionData3] = {1} where [ProductID] = {0}", ProductID, Value);
        }

        public static void UpdateSEDescription(this Table<Product> entity, int ProductID, string SEDescription)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [SEDescription] = {1} where [ProductID] = {0}", ProductID, SEDescription);
        }

        public static void UpdateShowBuyButton(this Table<Product> entity, int ProductID, int value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [ShowBuyButton] = {1} where [ProductID] = {0}", ProductID, value);
        }

        public static void DeleteByMPN(this Table<Product> entity, string MPN, int ManufacturerID)
        {
            entity.Context.ExecuteCommand("update [dbo].[Product] set [Deleted]=1 where [ProductID] in (select distinct([ProductID]) from [dbo].[ProductVariant] where [ProductID] in (select [ProductID] from [dbo].[ProductManufacturer] where [ManufacturerID]={1}) and [ManufacturerPartNumber]={0} and IsDefault=1)", MPN, ManufacturerID);
        }

        public static void UpdateProductManufacturer(this Table<ProductManufacturer> entity, int ProductID, int NewManufacturerID)
        {
            entity.Context.ExecuteCommand("Update [dbo].[ProductManufacturer] set [ManufacturerID] = {1} where [ProductID] = {0}", ProductID, NewManufacturerID);
        }

        public static void UpdatePricing(this Table<ProductVariant> entity, int VariantID, decimal? Cost, decimal Price, decimal? MSRP, decimal? SalePrice)
        {
            // need to work around MS bug regarding nulls

            var sCost = Cost.HasValue ? Cost.ToString() : "null";
            var sMSRP = MSRP.HasValue ? MSRP.ToString() : "null";
            var sSalePrice = SalePrice.HasValue ? SalePrice.ToString() : "null";

            var sql = string.Format("Update [dbo].[ProductVariant] set [Cost]={0}, [Price]={1}, [MSRP]={2}, [SalePrice]={3} where [VariantID]={4}", sCost, Price, sMSRP, sSalePrice, VariantID);

            entity.Context.ExecuteCommand(sql);
        }

        public static void UpdateInventory(this Table<ProductVariant> entity, int VariantID, int quantity)
        {
            entity.Context.ExecuteCommand("Update [dbo].[ProductVariant] set [Inventory]={1} where [VariantID]={0}", VariantID, quantity);
        }

        public static void UpdateName(this Table<Product> entity, int ProductID, string Name)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [Name] = {1} where [ProductID] = {0}", ProductID, Name);
        }

        public static void UpdateNameAndTitle(this Table<Product> entity, int ProductID, string Name)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [Name] = {1}, [SETitle] = {1} where [ProductID] = {0}", ProductID, Name);
        }

        public static void UpdateImageFilenameOverride(this Table<Product> entity, int ProductID, string Filename)
        {
            if (string.IsNullOrEmpty(Filename))
                entity.Context.ExecuteCommand("Update [dbo].[Product] set [ImageFilenameOverride] = null where [ProductID] = {0}", ProductID);
            else
                entity.Context.ExecuteCommand("Update [dbo].[Product] set [ImageFilenameOverride] = {1} where [ProductID] = {0}", ProductID, Filename);
        }

        public static void UpdateImageDimensions(this Table<Product> entity, int ProductID, int? width, int? height)
        {
            if (width.HasValue && height.HasValue)
            {
                var dimensions = string.Format("{0}x{1}", width.GetValueOrDefault(), height.GetValueOrDefault());
                entity.Context.ExecuteCommand("Update [dbo].[Product] set [TextOptionMaxLength] = {1}, [GraphicsColor] = {2} where [ProductID] = {0}", ProductID, width.GetValueOrDefault(), dimensions);
            }
            else
            {
                entity.Context.ExecuteCommand("Update [dbo].[Product] set [TextOptionMaxLength] = null, [GraphicsColor] = null where [ProductID] = {0}", ProductID);
            }
        }

        public static void UpdateDescription(this Table<Product> entity, int ProductID, string Description)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [Description] = {1} where [ProductID] = {0}", ProductID, Description);
        }

        public static void UpdateGoogleProductCategory(this Table<Product> entity, int ProductID, string googleCategory)
        {
            UpdateProductSummary(entity, ProductID, googleCategory);
        }

        public static void UpdateLooksCount(this Table<Product> entity, int ProductID, int value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [Looks] = {1} where [ProductID] = {0}", ProductID, value);
        }

        public static void UpdatePatternCorrelator(this Table<Product> entity, int ProductID, string Value)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [ManufacturerPartNumber] = {1} where [ProductID] = {0}", ProductID, Value);
        }


        public static void UpdateProductSummary(this Table<Product> entity, int ProductID, string Summary)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [Summary] = {1} where [ProductID] = {0}", ProductID, Summary);
        }


        public static void UpdateProductSKU(this Table<Product> entity, int ProductID, string NewSKU)
        {
            entity.Context.ExecuteCommand("Update [dbo].[Product] set [SKU] = {1} where [ProductID] = {0}", ProductID, NewSKU);
        }

        public static void UpdateDescriptions(this Table<Product> entity, int ProductID, string Description, string SEDescription, string SEKeywords)
        {
            try
            {
                if (Description == null)
                    entity.Context.ExecuteCommand("Update [dbo].[Product] set [Description] = null, [SEDescription] = {1}, [SEKeywords] = {2} where [ProductID] = {0}", ProductID, SEDescription, SEKeywords);
                else
                    entity.Context.ExecuteCommand("Update [dbo].[Product] set [Description] = {1}, [SEDescription] = {2}, [SEKeywords] = {3} where [ProductID] = {0}", ProductID, Description, SEDescription, SEKeywords);

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                throw;
            }
        }

        public static void UpdateDescriptions(this Table<Product> entity, int ProductID, string Description, string SEDescription, string SEKeywords, string FroogleDescription)
        {
            try
            {
                entity.Context.ExecuteCommand("Update [dbo].[Product] set [Description] = {1}, [SEDescription] = {2}, [SEKeywords] = {3}, [FroogleDescription] = {4} where [ProductID] = {0}", ProductID, Description, SEDescription, SEKeywords, FroogleDescription);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                throw;
            }
        }


        public static void SetVariantIDExpired(this Table<StockCheckNotification> entity, int VariantID)
        {
            entity.Context.ExecuteCommand("Update [dbo].[StockCheckNotifications] set [Status] = 2 where [Status] = 0 and [VariantID] = {0}", VariantID);
        }

        public static void SetExpiredByAge(this Table<StockCheckNotification> entity, int ageDays)
        {
            var cutOff = DateTime.Now.AddDays(0 - ageDays);
            entity.Context.ExecuteCommand("Update [dbo].[StockCheckNotifications] set [Status] = 2 where [Status] = 0 and [Created] < {0}", cutOff);
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
                foreach(var catID in categories)
                    entity.Context.ExecuteCommand(string.Format("Insert [dbo].[ProductCategory] ([ProductID], [CategoryID]) values ({0}, {1})", ProductID, catID));

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        public static void AddProductCategoryAssociationsForCategory(this Table<ProductCategory> entity, int CategoryID, IEnumerable<int> products)
        {
            Debug.Assert(CategoryID != 0);
            Debug.Assert(products != null && products.Count() > 0);

            if (products == null || products.Count() == 0)
                return;

            try
            {
                foreach (var productID in products)
                    entity.Context.ExecuteCommand(string.Format("Insert [dbo].[ProductCategory] ([ProductID], [CategoryID]) values ({0}, {1})", productID, CategoryID));

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        public static void RemoveProductCategoryAssociationsForProduct(this Table<ProductCategory> entity, int productID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[ProductCategory] where [ProductID] = {0}", productID);
        }


        public static void RemoveProductCategoryAssociationsForCategory(this Table<ProductCategory> entity, int categoryID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[ProductCategory] where [CategoryID] = {0}", categoryID);
        }

        public static void RemoveCategory(this Table<Category> entity, int categoryID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[Category] where [CategoryID] = {0}", categoryID);
        }

        public static void RemoveProductFeatures(this Table<ProductFeature> entity, int productID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[ProductFeatures] where [ProductID] = {0}", productID);
        }

        public static void MoveStagedTicklerCampaignsToRunning(this Table<TicklerCampaign> entity)
        {
            entity.Context.ExecuteCommand("Update [dbo].[TicklerCampaigns] set [Status] = {0} where [Status] = {1}", TicklerCampaignStatus.Running.DescriptionAttr(), TicklerCampaignStatus.Staged.DescriptionAttr());
        }

        public static void SuspendRunningTicklerCampaigns(this Table<TicklerCampaign> entity)
        {
            entity.Context.ExecuteCommand("Update [dbo].[TicklerCampaigns] set [Status] = {0} where [Status] = {1}", TicklerCampaignStatus.Suspended.DescriptionAttr(), TicklerCampaignStatus.Running.DescriptionAttr());
        }

        public static void ResumeSuspendedTicklerCampaigns(this Table<TicklerCampaign> entity)
        {
            entity.Context.ExecuteCommand("Update [dbo].[TicklerCampaigns] set [Status] = {0} where [Status] = {1}", TicklerCampaignStatus.Running.DescriptionAttr(), TicklerCampaignStatus.Suspended.DescriptionAttr());
        }

        public static void DeleteByStatus(this Table<TicklerCampaign> entity, TicklerCampaignStatus status)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[TicklerCampaigns] where [Status] = {0}", status.DescriptionAttr());
        }

        public static void RemoveRecord(this Table<AlgoliaProduct> entity, int productID)
        {
            entity.Context.ExecuteCommand("Delete [dbo].[AlgoliaProducts] where [ProductID] = {0}", productID);
        }

        public static void RemoveRecords(this Table<AlgoliaProduct> entity, IEnumerable<int> products)
        {
            try
            {
                var sbList = new StringBuilder();
                bool isFirst = true;
                foreach (var product in products)
                {
                    if (!isFirst)
                        sbList.Append(",");

                    sbList.Append(product.ToString());
                    isFirst = false;
                }

                var sql = string.Format("delete from [dbo].[AlgoliaProducts] where [ProductID] in ({0})", sbList.ToString());
                entity.Context.ExecuteCommand(sql);

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        public static void FindAndInsertNewProducts(this Table<AlgoliaProduct> entity, IEnumerable<ProductGroup> groups)
        {
            try
            {
                foreach (var group in groups)
                {
                    entity.Context.ExecuteCommand("insert into [dbo].[AlgoliaProducts] ([ProductID], [Action])	select [ProductID], 1 from [dbo].[Product] where [Deleted]=0 and [Published]=1 and [ProductGroup] = {0} and [ProductID] not in (select [ProductID] from [dbo].[AlgoliaProducts])", group.ToString());
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }


        public static void UpdateAlgoliaProductsAction(this Table<AlgoliaProduct> entity, IEnumerable<int> products, AlgoliaAction action)
        {
            try
            {
                var sbList = new StringBuilder();
                bool isFirst = true;
                foreach(var product in products)
                {
                    if (!isFirst)
                        sbList.Append(",");

                    sbList.Append(product.ToString());
                    isFirst = false;
                }

                var sql = string.Format("Update [dbo].[AlgoliaProducts] set [Action] = {0} where [ProductID] in ({1})", (int)action, sbList.ToString());
                entity.Context.ExecuteCommand(sql);
        
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }


        public static void MarkUnpublished(this Table<SearchGallery> entity, IEnumerable<int> records)
        {
            try
            {
                var sbList = new StringBuilder();
                bool isFirst = true;
                foreach (var record in records)
                {
                    if (!isFirst)
                        sbList.Append(",");

                    sbList.Append(record.ToString());
                    isFirst = false;
                }

                var sql = string.Format("Update [dbo].[SearchGalleries] set [Published]=0, UpdatedOn=GetDate() where [SearchGalleryID] in ({0})", sbList.ToString());
                entity.Context.ExecuteCommand(sql);

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

    }
}