using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.Net;
using System.Web;
using System.Transactions;
using InsideFabric.Data;
using Ionic.Zip;
using Gen4.Util.Misc;
using System.Threading;
using System.Reflection;

namespace Website
{
    public class InsideWallpaperProductSync
    {
        #region Locals

        private const int PARENT_DESIGNER_CATEGORY = 162;

        private CancellationToken cancelToken { get; set; }

        private Action<int> reportProgress { get; set; }

        private Action<string> reportStatus { get; set; }

        private string insideFabricConnectionString { get; set; }

        private string insideWallpaperConnectionString { get; set; }

        private string insideFabricRootPath { get; set; }

        private string insideWallpaperRootPath { get; set; }

        private int OutletCategoryID { get; set; }

        List<string> imageFolderNames { get; set; }

        private int lastReportedProgress; 
        #endregion

        public InsideWallpaperProductSync(string insideFabricConnectionString, string insideWallpaperConnectionString, string insideFabricRootPath, string insideWallpaperRootPath, List<string> imageFolderNames, int outletCategoryID)
        {
            this.insideFabricConnectionString = insideFabricConnectionString;
            this.insideWallpaperConnectionString = insideWallpaperConnectionString;
            this.insideFabricRootPath = insideFabricRootPath;
            this.insideWallpaperRootPath = insideWallpaperRootPath;
            this.imageFolderNames = imageFolderNames;
            this.OutletCategoryID = outletCategoryID;
        }

        public void RunFullSyncAll(CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus)
        {
            this.cancelToken = cancelToken;
            this.reportProgress = reportProgress;
            this.reportStatus = reportStatus;
            lastReportedProgress = -1;

            try
            {
                SyncManufacturers(null);
                SyncProducts();
                SyncOutletCategory(null);
                SyncDesignerCategories(null);
            }
            finally
            {
                // cleanup
            }
        }

        public void RunFullSyncSingleVendor(int manufacturerID, CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus)
        {
            this.cancelToken = cancelToken;
            this.reportProgress = reportProgress;
            this.reportStatus = reportStatus;
            lastReportedProgress = -1;

            try
            {
                ReportStatus("Sync Products for One Vendor", 2000);
                UpdateWallpaperProducts(manufacturerID);
                ReportStatus("Sync Products for One Vendor - Completed", 2000);

                SyncOutletCategory(manufacturerID);
                SyncDesignerCategories(manufacturerID);
            }
            finally
            {
                // cleanup
            }
        }

        public void RunInsert(CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus)
        {
            this.cancelToken = cancelToken;
            this.reportProgress = reportProgress;
            this.reportStatus = reportStatus;
            lastReportedProgress = -1;

            try
            {
                SyncManufacturers(null);

                ReportStatus("Insert Products", 2000);

                InsertWallpaperProducts();

                ReportStatus("Insert Products - Completed", 2000);

                SyncOutletCategory(null);
                SyncDesignerCategories(null);
            }
            catch(Exception Ex)
            {
                Debug.WriteLine("Exception: " + Ex.ToString());
                throw;
            }
            finally
            {
                // cleanup
            }
        }

        
        /// <summary>
        /// Quick sync updates only items already in wallpaper store which have recently been updated in fabric store.
        /// </summary>
        /// <remarks>
        /// Only deals with cost, price, msrp, sale price, in/out, discontinued.
        /// </remarks>
        /// <param name="cancelToken"></param>
        /// <param name="reportProgress"></param>
        /// <param name="reportStatus"></param>
        public void RunQuick(CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus)
        {
            this.cancelToken = cancelToken;
            this.reportProgress = reportProgress;
            this.reportStatus = reportStatus;
            lastReportedProgress = -1;

            try
            {
                SyncProductsQuick();
                SyncOutletCategory();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                // cleanup
            }
        }


        #region Reporting
        private void ReportStatus(string msg, int? delayAfterSending = null)
        {
            if (reportStatus != null)
                reportStatus(msg);

            if (delayAfterSending.HasValue)
                System.Threading.Thread.Sleep(delayAfterSending.Value);
        }

        private void ReportPercentComplete(int countCompleted, int countTotal)
        {
            if (reportProgress == null)
                return;

            var pct = countTotal == 0 ? 0 : (countCompleted * 100) / countTotal;
        
            if (lastReportedProgress != pct)
            {
                lastReportedProgress = pct;
                reportProgress(pct);
                System.Threading.Thread.Sleep(100);
            }
        } 
        #endregion

        private List<int> GetQualifyingProductIdentifiersFromInsideFabric()
        {
            using (var dc = new AspStoreDataContextReadOnly(insideFabricConnectionString))
            {
                var products = dc.Products.Where(e => e.ProductGroup == "Wallcovering" && e.Deleted == 0).Select(e => e.ProductID).ToList();
                return products;
            }
        }

        private List<int> GetQualifyingProductIdentifiersFromInsideWallpaper(int? manufacturerID = null, bool mustBePublished = false)
        {
            using (var dc = new AspStoreDataContextReadOnly(insideWallpaperConnectionString))
            {
                List<int> products;

                if (mustBePublished)
                    products = dc.Products.Where(e => e.ProductGroup == "Wallcovering" && e.Deleted == 0 && e.Published == 1).Select(e => e.ProductID).ToList();
                else
                    products = dc.Products.Where(e => e.ProductGroup == "Wallcovering" && e.Deleted == 0).Select(e => e.ProductID).ToList();

                // prune if need to filter to a single vendor

                if (manufacturerID.HasValue)
                {
                    var vendorProducts = new HashSet<int>(dc.ProductManufacturers.Where(e => e.ManufacturerID == manufacturerID.Value).Select(e => e.ProductID));
                    products = products.Where(e => vendorProducts.Contains(e)).ToList();
                }

                return products;
            }
        }

        #region Properties

        string InsideWallpaperProductImagesRoot
        {
            get
            {
                return Path.Combine(insideWallpaperRootPath, @"images\product");
            }
        }

        string InsideFabricProductImagesRoot
        {
            get
            {
                return Path.Combine(insideFabricRootPath, @"images\product");
            }
        }


        #endregion

        #region Manufacturers

        /// <summary>
        /// Get list of manufacturerID for any which has Wallpaper products.
        /// </summary>
        /// <remarks>
        /// Can optionally supply a single ID when just wanting info on one vendor.
        /// </remarks>
        /// <returns></returns>
        private List<Entities.Manufacturer> GetQualifyingManufacturersFromInsideFabric(int? manufacaturerID=null)
        {
            using (var dc = new AspStoreDataContextReadOnly(insideFabricConnectionString))
            {
                List<int> IDs;

                if (manufacaturerID.HasValue)
                {
                    IDs = new List<int>() { manufacaturerID.Value };
                }
                else
                {
                    IDs = (from p in dc.Products
                           where p.ProductGroup == "Wallcovering" && p.Deleted == 0
                           join pm in dc.ProductManufacturers on p.ProductID equals pm.ProductID
                           select pm.ManufacturerID).Distinct().ToList();
                }

                var ifManufacturers = dc.Manufacturers.Where(e => IDs.Contains(e.ManufacturerID)).ToList();
                return ifManufacturers;
            }
        }

        /// <summary>
        /// Sync the manufacturer records. Can optionally supply a single ID when working with one vendor at a time.
        /// </summary>
        /// <param name="manufacturerID"></param>
        private void SyncManufacturers(int? manufacturerID=null)
        {
            ReportStatus("Sync Manufacturers", 2000);

            var insideFabricManufacturers = GetQualifyingManufacturersFromInsideFabric(manufacturerID);

            using (var dc = new InsideWallpaperSyncDataContext(insideWallpaperConnectionString))
            {
                dc.Connection.Open();

                dc.ExecuteCommand("SET IDENTITY_INSERT Manufacturer ON");
                var insideWallpaperManufacturers = dc.Manufacturers.ToList();

                foreach (var m in insideFabricManufacturers)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    var id = m.ManufacturerID;

                    // skip over any which are already there in inside wallpaper
                    if (insideWallpaperManufacturers.Where(e => e.ManufacturerID == id).Count() > 0)
                        continue;

                    ReportStatus(string.Format("Adding manufacturer: {0}", m.Name));

                    Func<string, string> cleanName = (s) =>
                    {
                        if (s.ContainsIgnoreCase("Wallcovering"))
                            return s;

                        var clean = s.Replace("Fabricut", "F123").Replace("Fabrics", "").Replace("Fabric", "").Replace("Wallpaper", "").Replace("F123", "Fabricut").Trim();

                        return clean + " wallpaper";
                    };

                    Func<string, string> transformSEName = (s) =>
                    {
                        return s.Replace("fabricut", "F123").Replace("fabrics", "fabric").Replace("fabric", "wallpaper").Replace("F123", "fabricut").Trim();
                    };

                    Func<string, string> transformName = (s) =>
                    {
                        return s.Replace("Fabricut", "F123").Replace("Fabrics", "Fabric").Replace("Fabric", "Wallpaper").Replace("F123", "Fabricut").Trim();
                    };

                    Func<string, string> transformDescription = (s) =>
                    {
                        if (!s.ContainsIgnoreCase("fabric"))
                            return s;

                        // <div class="gridPageHdr"><div class="gridPageHdrH1"><h1>Scalamandre Fabric</h1></div><div class="gridPageHdrLinks"><a href="/new-59-scalamandre-fabric.aspx">new</a>
                        // <a href="/books-59-scalamandre-fabric.aspx">books</a><a href="/collections-59-scalamandre-fabric.aspx">collections</a><a href="/patterns-59-scalamandre-fabric.aspx">
                        // patterns</a></div><div style="float:clear;"></div></div><img alt="" src="/Images/scalamandre-fabric.jpg" />
                        var clean = s.Replace(m.Name, transformName(m.Name)).Replace(m.SEName, transformSEName(m.SEName)).Trim();
                        return clean;
                    };


                    Func<string, string> transformSETitle = (s) =>
                    {
                        return s.Replace("Fabricut", "F123").Replace("Fabrics", "Fabric").Replace("Fabric", "Wallpaper").Replace("  ", " ").Replace("F123", "Fabricut").Trim();
                    };

                    Func<string, string> transformSEDescription = (s) =>
                    {
                        // Lowest prices anywhere on Kravet wallpaper. Pick from over 75,000 wallpaper patterns and colors. Fast delivery. Free shipping!
                        return string.Format("Lowest prices anywhere on {0}. Pick from over 75,000 wallpaper patterns and colors. Fast delivery. Free shipping!", cleanName(m.Name));
                    };



                    Func<string, string> transformSEKeywords = (s) =>
                    {
                        var words = s.Replace("fabricut", "F123").Replace("Fabrics", "").Replace("fabrics", "fabric").Replace("fabric", "wallpaper").Replace("drapery", "").Replace("upholstery", "").Replace("  ", " ").Replace("F123", "Fabricut");
                        if (!words.ContainsIgnoreCase("wallcovering"))
                            words += ", wallcovering";

                        return words;
                    };

                    Func<string, string> transformSummary = (s) =>
                    {
                        // not presently filled in
                        return s;
                    };

                    Func<string, string> transformImageFilenameOverride = (s) =>
                    {
                        return s.Replace("fabricut", "F123").Replace("fabrics", "fabric").Replace("fabric", "wallpaper").Replace("F123", "fabricut").Trim();
                    };

                    var manufacturer = new SyncEntities.Manufacturer()
                    {
                        ManufacturerID = m.ManufacturerID,
                        ManufacturerGUID = m.ManufacturerGUID,
                        Name = transformName(m.Name),
                        SEName = transformSEName(m.SEName),
                        SEKeywords = transformSEKeywords(m.SEKeywords),
                        SEDescription = transformSEDescription(m.SEDescription),
                        SETitle = transformSETitle(m.SETitle),
                        SENoScript = m.SENoScript,
                        SEAltText = m.SEAltText,
                        Address1 = m.Address1,
                        Address2 = m.Address2,
                        Suite = m.Suite,
                        City = m.City,
                        State = m.State,
                        ZipCode = m.ZipCode,
                        Country = m.Country,
                        Phone = m.Phone,
                        FAX = m.FAX,
                        URL = m.URL,
                        Email = m.Email,
                        QuantityDiscountID = m.QuantityDiscountID,
                        SortByLooks = m.SortByLooks,
                        Summary = transformSummary(m.Summary),
                        Description = transformDescription(m.Description),
                        Notes = m.Notes,
                        RelatedDocuments = m.RelatedDocuments,
                        XmlPackage = m.XmlPackage,
                        ColWidth = m.ColWidth,
                        DisplayOrder = m.DisplayOrder,
                        ExtensionData = m.ExtensionData,
                        ContentsBGColor = m.ContentsBGColor,
                        PageBGColor = m.PageBGColor,
                        GraphicsColor = m.GraphicsColor,
                        ImageFilenameOverride = transformImageFilenameOverride(m.ImageFilenameOverride),
                        Published = m.Published,
                        Wholesale = m.Wholesale,
                        ParentManufacturerID = m.ParentManufacturerID,
                        IsImport = m.IsImport,
                        Deleted = m.Deleted,
                        CreatedOn = m.CreatedOn,
                        PageSize = m.PageSize,
                        SkinID = m.SkinID,
                        TemplateName = m.TemplateName,
                        UpdatedOn = DateTime.Now,
                    };

                    dc.Manufacturers.InsertOnSubmit(manufacturer);
                    dc.SubmitChanges();

                    //Debug.WriteLine("---------------------------------------------"); 
                    //Debug.WriteLine(manufacturer.Name);
                    //Debug.WriteLine(manufacturer.Description);
                    //Debug.WriteLine(manufacturer.SEName);
                    //Debug.WriteLine(manufacturer.SETitle);
                    //Debug.WriteLine(manufacturer.SEDescription);
                    //Debug.WriteLine(manufacturer.SEKeywords);
                    //Debug.WriteLine(manufacturer.ImageFilenameOverride);


                    System.Threading.Thread.Sleep(2000);
                }

                dc.ExecuteCommand("SET IDENTITY_INSERT Manufacturer OFF");
            }

            ReportStatus("Sync Manufacturers - Completed", 2000);

        } 
        #endregion

        #region Products

        private void SyncProducts()
        {
            ReportStatus("Sync Products", 2000);

            // 1. If IW has any products which are not in IF, then delete them (product and variant, ProductManufacturer, ProductCategory).
            // 2. Any remaining products in IW are to be updated (product and variant)
            // 3. Any that IF has which are not in IW, insert (product and variant, ProductManufacturer).

            DeleteOrphanWallpaperProducts();
            UpdateWallpaperProducts();
            InsertWallpaperProducts();

            ReportStatus("Sync Products - Completed", 2000);
        }

        private void SyncProductsQuick()
        {
            ReportStatus("Sync Products Quick", 2000);

            // productIDs we're interested in - which we know are all also on InsideFabric since
            // IW is a true subset.

            var iwProducts = GetQualifyingProductIdentifiersFromInsideWallpaper(null, true);

            using (var dcWallpaper = new AspStoreDataContext(insideWallpaperConnectionString))
            {
                using (var dcFabric = new AspStoreDataContextReadOnly(insideFabricConnectionString))
                {

                    int countTotal = iwProducts.Count();
                    int countRemaining = countTotal;
                    int countCompleted = 0;

                    int batchSize = 100; // how many records to grab each time through the loop

                    var q = new Queue<int>(iwProducts);

                    while (q.Count > 0)
                    {
                        if (cancelToken != null && cancelToken.IsCancellationRequested)
                            return;

                        var takeCount = Math.Min(batchSize, q.Count);

                        // retrieve N productIDs from the queue

                        var batchProductID = new List<int>();
                        for (int i = 0; i < takeCount; i++)
                            batchProductID.Add(q.Dequeue());

                        // process the retrieved products

                        ProcessQuickBatch(dcWallpaper, dcFabric, batchProductID);

                        countCompleted += takeCount;
                        countRemaining -= takeCount;
                        ReportPercentComplete(countCompleted, countTotal);
                    }
                }
            }

            ReportStatus("Sync Products Quick- Completed", 2000);
        }

        private void ProcessQuickBatch(AspStoreDataContext dcWallpaper, AspStoreDataContext dcFabric, List<int> productIDs)
        {
            var ifVariants = (from pv in dcFabric.ProductVariants
                              where productIDs.Contains(pv.ProductID)
                              select new
                              {
                                  pv.VariantID,
                                  pv.ProductID,
                                  pv.Cost,
                                  pv.Price,
                                  pv.MSRP,
                                  pv.SalePrice,
                                  pv.Inventory,
                              }
                             ).ToDictionary(k => k.VariantID, v => v);

            var iwVariants = (from pv in dcWallpaper.ProductVariants
                              where productIDs.Contains(pv.ProductID)
                              select new
                              {
                                  pv.VariantID,
                                  pv.ProductID,
                                  pv.Cost,
                                  pv.Price,
                                  pv.MSRP,
                                  pv.SalePrice,
                                  pv.Inventory,
                              }
                             ).ToList();

            var changedProductIDs = new HashSet<int>();

            foreach(var iwVariant in iwVariants)
            {
                if (cancelToken != null && cancelToken.IsCancellationRequested)
                    return;

                if (!ifVariants.ContainsKey(iwVariant.VariantID))
                    continue;

                var ifVariant = ifVariants[iwVariant.VariantID];

                if (ifVariant.Cost != iwVariant.Cost || ifVariant.Price != iwVariant.Price || ifVariant.MSRP != iwVariant.MSRP || ifVariant.SalePrice != iwVariant.SalePrice)
                {
                    // one of the prices is different, update the bunch of them
                    dcWallpaper.ProductVariants.UpdatePricing(iwVariant.VariantID, ifVariant.Cost, ifVariant.Price, ifVariant.MSRP, ifVariant.SalePrice);
                }

                var iwInStock = iwVariant.Inventory > 0;
                var ifInstock = ifVariant.Inventory > 0;

                if (iwInStock != ifInstock)
                {
                    // inventory needs to be updated
                    dcWallpaper.ProductVariants.UpdateInventory(iwVariant.VariantID, ifVariant.Inventory);
                    changedProductIDs.Add(iwVariant.ProductID);
                }
            }

            var productList = changedProductIDs.ToList();

            // deal with discontinued
            // iw products having a change in discontinued are naturally constrained by products which have changed
            // inventory status, so that limits the necessary bounds of what we do below

            var ifProducts = (from p in dcFabric.Products
                              where productList.Contains(p.ProductID)
                              select new
                              {
                                  p.ProductID,
                                  IsDiscontinued = p.ShowBuyButton == 0,
                              }).ToDictionary(k => k.ProductID, v => v.IsDiscontinued);

            var iwProducts = (from p in dcWallpaper.Products
                              where productList.Contains(p.ProductID)
                              select new
                              {
                                  p.ProductID,
                                  IsDiscontinued = p.ShowBuyButton == 0,
                              }).ToList();

            foreach(var product in iwProducts)
            {
                bool isDiscontinued;
                if (!ifProducts.TryGetValue(product.ProductID, out isDiscontinued))
                    continue;

                // if differnt, then update iw to match if
                if (isDiscontinued != product.IsDiscontinued)
                    dcWallpaper.Products.UpdateShowBuyButton(product.ProductID, isDiscontinued ? 0 : 1);
            }
        }

        private void DeleteOrphanWallpaperProducts()
        {
            // note that this does not deal with products where the swatch variant was removed;
            // that is handled elsewhere

            ReportStatus("Deleting Orphan Products", 2000);

            var ifProducts = new HashSet<int>(GetQualifyingProductIdentifiersFromInsideFabric());
            var iwProducts = GetQualifyingProductIdentifiersFromInsideWallpaper();

            // create work list
            var iwProductsToDelete = new List<int>();
            foreach (var productID in iwProducts)
            {
                if (!ifProducts.Contains(productID))
                    iwProductsToDelete.Add(productID);
            }

            int countCompleted = 0;
            int countTotal = iwProductsToDelete.Count();
            int statusReportCounter = 0;

            using (var dc = new AspStoreDataContext(insideWallpaperConnectionString))
            {
                foreach (var productID in iwProductsToDelete)
                {
                    // product is not in IF, so must delete
                    // products, productvariant, productmanufacturer, productcategory

                    if (statusReportCounter == 0)
                    {
                        ReportStatus(string.Format("Remaining product deletes: {0:N0}", countTotal - countCompleted), 5);
                        statusReportCounter = 25;
                    }

                    statusReportCounter--;

                    cancelToken.ThrowIfCancellationRequested();

                    var imageFilename = dc.Products.Where(e => e.ProductID == productID).Select(e => e.ImageFilenameOverride).FirstOrDefault();

                    dc.Products.CompletelyDeleteProduct(productID);

                    if (!string.IsNullOrWhiteSpace(imageFilename))
                        DeleteImageFiles(imageFilename);

                    countCompleted++;
                    ReportPercentComplete(countCompleted, countTotal);

                }
            }

            ReportStatus(string.Format("Deleted {0:N0} products.", countCompleted), 2000);

        }

        private void UpdateWallpaperProducts(int? manufacturerID = null)
        {
            ReportStatus("Updating Products", 2000);


            var iwProducts = GetQualifyingProductIdentifiersFromInsideWallpaper(manufacturerID);

            using (var dcWallpaper = new AspStoreDataContext(insideWallpaperConnectionString))
            {
                using (var dcFabric = new AspStoreDataContextReadOnly(insideFabricConnectionString))
                {
                    int countUpdated = 0;
                    int countCompleted = 0;
                    int countTotal = iwProducts.Count();
                    int statusReportCounter = 0;

                    ReportStatus(string.Format("Begin updating {0:N0} products.", countTotal), 2000);

                    foreach (var productID in iwProducts)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        if (statusReportCounter == 0)
                        {
                            ReportStatus(string.Format("Remaining product updates: {0:N0}", countTotal - countCompleted), 5);
                            statusReportCounter = 25;
                        }

                        statusReportCounter--;

                        var ifProduct = dcFabric.Products.Where(e => e.ProductID == productID).FirstOrDefault();
                        var iwProduct = dcWallpaper.Products.Where(e => e.ProductID == productID).FirstOrDefault();
                        var ifProductFeatures = dcFabric.ProductFeatures.Where(e => e.ProductID == productID).FirstOrDefault();
                        var iwProductFeatures = dcWallpaper.ProductFeatures.Where(e => e.ProductID == productID).FirstOrDefault();

                        if (ifProduct != null || iwProduct != null)
                        {
                            var ifVariants = dcFabric.ProductVariants.Where(e => e.ProductID == productID).ToList();
                            var iwVariants = dcWallpaper.ProductVariants.Where(e => e.ProductID == productID).ToList();

                            // save, will need later to check images if SQL update works
                            var ifImageFilenameOverride = ifProduct.ImageFilenameOverride;
                            var iwImageFilenameOverride = iwProduct.ImageFilenameOverride;

                            foreach(var variantID in iwVariants.Select(e => e.VariantID).ToList())
                            {
                                if (ifVariants.Where(e => e.VariantID == variantID).Count() > 0)
                                    continue;

                                // does not exist for IF, so delete this IW variant
                                dcWallpaper.ProductVariants.DeleteByVariantID(variantID);
                                // also remove from collection here so know not to update it
                                iwVariants.RemoveAll(e => e.VariantID == variantID);
                            }

                            // update product record and any remaining variants

                            var countChanged = 0;

                            countChanged += CopyProperties(iwProduct, ifProduct, new string[] {"ProductID" });
                            foreach(var variant in iwVariants)
                            {
                                var ifVariant = ifVariants.Where(e => e.VariantID == variant.VariantID).FirstOrDefault();
                                if (ifVariant == null) 
                                    continue;

                                countChanged += CopyProperties(variant, ifVariant, new string[] {"VariantID" });
                            }

                            // deal with ProductLabels table - blow away, re-insert what we have now

                            try
                            {
                                var extData4 = ExtensionData4.Deserialize(iwProduct.ExtensionData4);
                                if (extData4.Data.ContainsKey(ExtensionData4.OriginalRawProperties))
                                {
                                    var dicPublicProperties = extData4.Data[ExtensionData4.OriginalRawProperties] as Dictionary<string, string>;
                                    dcWallpaper.UpdateProductLabels(iwProduct.ProductID, dicPublicProperties);
                                }
                            }
                            catch (Exception Ex)
                            {
                                Debug.WriteLine(string.Format("Exception updating ProductLabels table: {0}", Ex.Message));
                            }


                            // deal with product features table

                            if (ifProductFeatures != null)
                            {
                                if (iwProductFeatures != null)
                                {
                                    // update
                                    iwProductFeatures.ProductID = ifProductFeatures.ProductID;
                                    iwProductFeatures.TinyImageDescriptor = ifProductFeatures.TinyImageDescriptor;
                                    iwProductFeatures.ImageDescriptor = ifProductFeatures.ImageDescriptor;
                                    iwProductFeatures.Colors = ifProductFeatures.Colors;
                                    iwProductFeatures.Similar = ifProductFeatures.Similar;
                                    iwProductFeatures.SimilarColors = ifProductFeatures.SimilarColors;
                                    iwProductFeatures.SimilarStyle = ifProductFeatures.SimilarStyle;
                                    iwProductFeatures.Shapes = ifProductFeatures.Shapes;
                                }
                                else
                                {
                                    // insert
                                    var iwProductFeatures2 = new Website.Entities.ProductFeature()
                                    {
                                        ProductID = ifProductFeatures.ProductID,
                                        TinyImageDescriptor = ifProductFeatures.TinyImageDescriptor,
                                        ImageDescriptor = ifProductFeatures.ImageDescriptor,
                                        Colors = ifProductFeatures.Colors,
                                        Similar = ifProductFeatures.Similar,
                                        SimilarColors = ifProductFeatures.SimilarColors,
                                        SimilarStyle = ifProductFeatures.SimilarStyle,
                                        Shapes = ifProductFeatures.Shapes
                                    };

                                    dcWallpaper.ProductFeatures.InsertOnSubmit(iwProductFeatures2);

                                }
                            }

                            dcWallpaper.SubmitChanges(ConflictMode.ContinueOnConflict);

                            // deal with images - low level functions skip physical delete if in debug mode

                            if (string.IsNullOrWhiteSpace(ifImageFilenameOverride))
                            {
                                // if fabric image empty, then ensure nothing for wallpaper either
                                if (!string.IsNullOrWhiteSpace(iwImageFilenameOverride))
                                    DeleteImageFiles(iwImageFilenameOverride);
                            }
                            else if (string.IsNullOrWhiteSpace(iwImageFilenameOverride) || !ifImageFilenameOverride.Equals(iwImageFilenameOverride, StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrWhiteSpace(iwImageFilenameOverride) && !AllImageFilesExists(iwImageFilenameOverride)))
                            {
                                // since is update, this only copies over the new image if the name is different; so will need to
                                // add extra logic if want to force an update.

                                if (!string.IsNullOrWhiteSpace(iwImageFilenameOverride))
                                    DeleteImageFiles(iwImageFilenameOverride);

                                CopyImageFiles(ifImageFilenameOverride);
                            }

                            if (countChanged > 0)
                                countUpdated++;
                        }

                        countCompleted++;
                        ReportPercentComplete(countCompleted, countTotal);
                    }

                    ReportPercentComplete(countTotal, countTotal);
                    ReportStatus(string.Format("Updated products: {0:N0}", countUpdated), 2000);
                }
            }
        }

        private void InsertWallpaperProducts()
        {
            ReportStatus("Inserting Products", 2000);

            var allInsideFabricfProducts = GetQualifyingProductIdentifiersFromInsideFabric();
            var allInsideWallpaperProducts = new HashSet<int>(GetQualifyingProductIdentifiersFromInsideWallpaper());

            var ifProducts = new List<int>(); // the ones which are new

            // get a list of new productIDs
            foreach(var productID in allInsideFabricfProducts)
            {
                if (!allInsideWallpaperProducts.Contains(productID))
                    ifProducts.Add(productID);
            }

            using (var dcWallpaper = new InsideWallpaperSyncDataContext(insideWallpaperConnectionString))
            {

                dcWallpaper.Connection.Open();

                using (var dcFabric = new AspStoreDataContextReadOnly(insideFabricConnectionString))
                {
                    int countCompleted = 0;
                    int countTotal = ifProducts.Count();
                    int statusReportCounter = 0;

                    ReportStatus(string.Format("Begin inserting {0:N0} products.", countTotal), 2000);

                    foreach (var productID in ifProducts)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        if (statusReportCounter == 0)
                        {
                            ReportStatus(string.Format("Remaining product inserts: {0:N0}", countTotal - countCompleted), 5);
                            statusReportCounter = 25;
                        }

                        statusReportCounter--;

                        var ifProduct = dcFabric.Products.Where(e => e.ProductID == productID).FirstOrDefault();
                        var iwProduct = new SyncEntities.Product();
                        var manufacturerID = dcFabric.ProductManufacturers.Where(e => e.ProductID == productID).Select(e => e.ManufacturerID).FirstOrDefault();
                        var ifProductFeatures = dcFabric.ProductFeatures.Where(e => e.ProductID == productID).FirstOrDefault();

                        if (ifProduct != null && manufacturerID != 0)
                        {
                            var ifVariants = dcFabric.ProductVariants.Where(e => e.ProductID == productID).ToList();

                            using (var scope = new TransactionScope())
                            {

                                InsertProperties(iwProduct, ifProduct);
                                dcWallpaper.Products.InsertOnSubmit(iwProduct);
                                dcWallpaper.ExecuteCommand("SET IDENTITY_INSERT Product ON");
                                dcWallpaper.SubmitChanges(ConflictMode.ContinueOnConflict);

                                foreach (var ifVariant in ifVariants)
                                {
                                    var iwVariant = new SyncEntities.ProductVariant();
                                    InsertProperties(iwVariant, ifVariant);
                                    dcWallpaper.ProductVariants.InsertOnSubmit(iwVariant);
                                }

                                dcWallpaper.ExecuteCommand("SET IDENTITY_INSERT Product OFF");
                                dcWallpaper.ExecuteCommand("SET IDENTITY_INSERT ProductVariant ON");

                                dcWallpaper.SubmitChanges(ConflictMode.ContinueOnConflict);

                                dcWallpaper.ExecuteCommand("SET IDENTITY_INSERT ProductVariant OFF");

                                var pm = new SyncEntities.ProductManufacturer()
                                {
                                    ProductID = iwProduct.ProductID,
                                    ManufacturerID = manufacturerID,
                                    CreatedOn = DateTime.Now,
                                    UpdatedOn = DateTime.Now
                                };

                                // deal with ProductLabels table 

                                try
                                {
                                    var extData4 = ExtensionData4.Deserialize(iwProduct.ExtensionData4);
                                    if (extData4.Data.ContainsKey(ExtensionData4.OriginalRawProperties))
                                    {
                                        var dicPublicProperties = extData4.Data[ExtensionData4.OriginalRawProperties] as Dictionary<string, string>;
                                        dcWallpaper.UpdateProductLabels(iwProduct.ProductID, dicPublicProperties);
                                    }
                                }
                                catch (Exception Ex)
                                {
                                    Debug.WriteLine(string.Format("Exception inserting ProductLabels table: {0}", Ex.Message));
                                }
                                                                       

                                // deal with product features table

                                if (ifProductFeatures != null)
                                {

                                    // some bug or whatever observed whereby the ProductID already exists. Could have been interrupted op.
                                    // need to deal with that so don't have duplicate primary key
                                    if (dcWallpaper.ProductFeatures.Where(e => e.ProductID == productID).Count() > 0)
                                        dcWallpaper.ProductFeatures.Context.ExecuteCommand("Delete [dbo].[ProductFeatures] where [ProductID] = {0}", productID);

                                    var iwProductFeatures = new Website.SyncEntities.ProductFeature()
                                    {
                                        ProductID = ifProductFeatures.ProductID,
                                        TinyImageDescriptor = ifProductFeatures.TinyImageDescriptor,
                                        ImageDescriptor = ifProductFeatures.ImageDescriptor,
                                        Colors = ifProductFeatures.Colors,
                                        Similar = ifProductFeatures.Similar,
                                        SimilarColors = ifProductFeatures.SimilarColors,
                                        SimilarStyle = ifProductFeatures.SimilarStyle,
                                        Shapes = ifProductFeatures.Shapes
                                    };

                                    dcWallpaper.ProductFeatures.InsertOnSubmit(iwProductFeatures);
                                }


                                dcWallpaper.ProductManufacturers.InsertOnSubmit(pm);
                                dcWallpaper.SubmitChanges(ConflictMode.ContinueOnConflict);

                                scope.Complete();
                            }

                            // deal with images - low level functions skip physical delete if in debug mode

                            if (!string.IsNullOrWhiteSpace(iwProduct.ImageFilenameOverride))
                                CopyImageFiles(ifProduct.ImageFilenameOverride);
                        }

                        countCompleted++;
                        ReportPercentComplete(countCompleted, countTotal);
                    }

                    ReportPercentComplete(countTotal, countTotal);
                }

            }
        }

        #endregion

        #region Outlet Category

        private void SyncOutletCategory(int? manufacturerID =  null)
        {
            ReportStatus("Sync Outlet Category", 2000);
            ReportPercentComplete(0, 5); // faked a bit

            // list of IW productID
            var iwProducts = new HashSet<int>(GetQualifyingProductIdentifiersFromInsideWallpaper(manufacturerID, true));

            ReportPercentComplete(1, 5); 

            using (var dcWallpaper = new AspStoreDataContext(insideWallpaperConnectionString))
            {
                ReportPercentComplete(2, 5); 
                var iwOutletProducts = new HashSet<int>();
                // must filter iwOutletProducts to include only those that are in iwProducts too (due to manufacturer qualification)
                // didn't use contains clause because the list might get pretty big and this way is just as fast in the end.
                foreach (var productID in dcWallpaper.ProductCategories.Where(e => e.CategoryID == OutletCategoryID).Select(e => e.ProductID).ToList().Where(e => iwProducts.Contains(e)))
                    iwOutletProducts.Add(productID);
                    
                using (var dcFabric = new AspStoreDataContextReadOnly(insideFabricConnectionString))
                {
                    
                    // create list of products from IF which are outlet and also exist as a product on IW
                    var ifOutletProducts = new HashSet<int>();
                    foreach (var id in dcFabric.ProductCategories.Where(e => e.CategoryID == OutletCategoryID).Select(e => e.ProductID).ToList())
                    {
                        if (iwProducts.Contains(id))
                            ifOutletProducts.Add(id);
                    }

                    ReportPercentComplete(3, 5); 

                    // add
                    foreach(var id in ifOutletProducts)
                    {
                        if (!iwOutletProducts.Contains(id))
                            dcWallpaper.ProductCategories.AddProductToCategory(OutletCategoryID, id);
                    }

                    ReportPercentComplete(4, 5); 

                    // remove
                    foreach (var id in iwOutletProducts)
                    {
                        if (!ifOutletProducts.Contains(id))
                            dcWallpaper.ProductCategories.RemoveProductFromCategory(OutletCategoryID, id);
                    }

                    ReportPercentComplete(5, 5); 
                }
            }
        }

        #endregion

        #region Designer Categories

        private void SyncDesignerCategories(int? manufacturerID)
        {
            ReportStatus("Sync Designer Categories", 2000);

            using (var dcWallpaper = new AspStoreDataContext(insideWallpaperConnectionString))
            {
                using (var dcFabric = new AspStoreDataContextReadOnly(insideFabricConnectionString))
                {
                    var ifDesignerCategories = (from c in dcFabric.Categories where c.ParentCategoryID == PARENT_DESIGNER_CATEGORY && c.Deleted == 0
                                                select new
                                                {
                                                    CategoryID = c.CategoryID,
                                                    Name = c.Name,
                                                    Published = c.Published,
                                                    ProductCount = (from pc in dcFabric.ProductCategories where pc.CategoryID == c.CategoryID join p in dcFabric.Products on pc.ProductID equals p.ProductID where p.ProductGroup == "Wallcovering" && p.Deleted == 0 && p.Published == 1 select p.ProductID).Count()
                                                }).ToList();
                        
                    var iwDesignerCategories = dcWallpaper.Categories.Where(e => e.ParentCategoryID == PARENT_DESIGNER_CATEGORY && e.Deleted == 0)
                        .Select(e => new { e.CategoryID, e.Name, e.Published}).ToList();

                    // TODO - add missing categories, else will only be able to work with ones which already exist

                    int countCompleted = 0;
                    int countTotal = iwDesignerCategories.Count();

                    foreach(var iwCategory in iwDesignerCategories)
                    {
                        var ifCategory = ifDesignerCategories.Where(e => e.CategoryID == iwCategory.CategoryID).FirstOrDefault();
                        if (ifCategory == null)
                            continue;

                        ReportStatus(string.Format("Sync category: {0}", iwCategory.Name), 1000);

                        // if IF not published, then make sure IW isn't published and move on
                        if (ifCategory.Published == 0)
                        {
                            if (iwCategory.Published == 1)
                                dcWallpaper.Categories.SetPublished(iwCategory.CategoryID, false);  

                            continue;
                        }

                        // IF is published

                        if (ifCategory.ProductCount > 0)
                        {
                            // IF count > 0

                            if (iwCategory.Published == 0)
                            {
                                // must set to published
                                dcWallpaper.Categories.SetPublished(iwCategory.CategoryID, true);
                            }
                        }
                        else
                        {
                            // IF count = 0

                            if (iwCategory.Published == 1)
                            {
                                // must set to not published
                                dcWallpaper.Categories.SetPublished(iwCategory.CategoryID, false);
                            }
                        }


                        // now sync the ProductCategory table

                        // first clear out, then put back what we want, if the category is published
                        dcWallpaper.ProductCategories.DelelteAllByCategory(iwCategory.CategoryID);

                        if (ifCategory.Published == 1)
                        {
                            var ifProductsInCategory = dcFabric.ProductCategories.Where(e => e.CategoryID == iwCategory.CategoryID).Select(e => e.ProductID).ToList();
                            var iwProducts = new HashSet<int>(GetQualifyingProductIdentifiersFromInsideWallpaper(mustBePublished:true));

                            foreach(var productID in ifProductsInCategory)
                            {
                                // only add products which are good in IW
                                if (!iwProducts.Contains(productID))
                                    continue;

                                dcWallpaper.ProductCategories.AddProductToCategory(iwCategory.CategoryID, productID);
                            }
                        }

                        countCompleted++;
                        ReportPercentComplete(countCompleted, countTotal);
                    }

                }
            }


        }

        #endregion

        #region Image Utilities

        /// <summary>
        /// Copy all images by this name (all folders) from IF to IW.
        /// </summary>
        /// <param name="filename"></param>
        private void CopyImageFiles(string imageFilename)
        {
            // skip if empty image
            if (string.IsNullOrWhiteSpace(imageFilename))
                return;

            Action<string> copyImage = (f) =>
            {
                try
                {
#if !DEBUG
                    var src = Path.Combine(InsideFabricProductImagesRoot, f, imageFilename);
                    var dst = Path.Combine(InsideWallpaperProductImagesRoot, f, imageFilename);

                    // skip if source does not actually exist
                    if (File.Exists(src))
                    {
                        File.Copy(src, dst, true);
                        if (!File.Exists(dst))
                            throw new Exception(string.Format("CopyImageFiles() dst does not exist after copy\n{0}", dst));
                    }
#endif
                }
                catch(Exception Ex)
                {
                    var src2 = Path.Combine(InsideFabricProductImagesRoot, f, imageFilename);
                    var dst2 = Path.Combine(InsideWallpaperProductImagesRoot, f, imageFilename);

                    var msg = string.Format("Fail on CopyImageFiles()\n{0}\n{1}\n", src2, dst2);
                    var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                    ev2.Raise();
                }
            };

            foreach (var folderName in imageFolderNames)
                copyImage(folderName);

        }

        private void DeleteImageFiles(string imageFilename)
        {
            if (string.IsNullOrWhiteSpace(imageFilename))
                return;

            Action<string> removeImageFromFolder = (f) =>
            {
                try
                {
#if !DEBUG
                var filepath = Path.Combine(InsideWallpaperProductImagesRoot, f, imageFilename);
                if (File.Exists(filepath))
                    File.Delete(filepath);
#endif
                }
                catch { }
            };

            foreach (var folderName in imageFolderNames)
                removeImageFromFolder(folderName);
        }

        private bool AllImageFilesExists(string imageFilename)
        {
            if (string.IsNullOrWhiteSpace(imageFilename))
                return false;

            try
            {
                foreach (var folderName in imageFolderNames)
                {
                    var filepath = Path.Combine(InsideWallpaperProductImagesRoot, folderName, imageFilename);
                    if (!File.Exists(filepath))
                        return false;
                }
            }
            catch { }

            return true;
        }

        #endregion

        #region Copy and Insert Properties

        /// <summary>
        /// Copies the readable and writable public property values from the source object to the target and
        /// optionally allows for the ignoring of any number of properties.
        /// </summary>
        /// <remarks>The source and target objects must be of the same type.</remarks>
        /// <param name="target">The target object</param>
        /// <param name="source">The source object</param>
        /// <param name="ignoreProperties">An array of property names to ignore</param>
        public int CopyProperties(object target, object source, string[] ignoreProperties)
        {
            // Get and check the object types
            Type type = source.GetType();

            // Build a clean list of property names to ignore
            List<string> ignoreList = new List<string>();
            foreach (string item in ignoreProperties)
            {
                if (!string.IsNullOrEmpty(item) && !ignoreList.Contains(item))
                {
                    ignoreList.Add(item);
                }
            }

            int countChanged = 0;

            // Copy the properties
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanWrite
                    && property.CanRead
                    && !ignoreList.Contains(property.Name))
                {

                    object valSource = property.GetValue(source, null);

                    object valTarget = property.GetValue(target, null);

                    if (valSource != null && valTarget != null)
                    {
                        if (valSource.ToString() != valTarget.ToString())
                            countChanged++;
                    }
                    else if (valSource == null && valTarget != null)
                    {
                        countChanged++;
                    }
                    else if (valSource != null && valTarget == null)
                    {
                        countChanged++;
                    }

                    property.SetValue(target, valSource, null);
                }
            }
            return countChanged;
        }

        public void InsertProperties(SyncEntities.Product target, Entities.Product source)
        {
            target.ProductID = source.ProductID;
            target.ProductGUID = source.ProductGUID;
            target.Name = source.Name;
            target.Summary = source.Summary;
            target.Description = source.Description;
            target.SEKeywords = source.SEKeywords;
            target.SEDescription = source.SEDescription;
            target.SpecTitle = source.SpecTitle;
            target.MiscText = source.MiscText;
            target.SwatchImageMap = source.SwatchImageMap;
            target.IsFeaturedTeaser = source.IsFeaturedTeaser;
            target.FroogleDescription = source.FroogleDescription;
            target.SETitle = source.SETitle;
            target.SENoScript = source.SENoScript;
            target.SEAltText = source.SEAltText;
            target.SizeOptionPrompt = source.SizeOptionPrompt;
            target.ColorOptionPrompt = source.ColorOptionPrompt;
            target.TextOptionPrompt = source.TextOptionPrompt;
            target.ProductTypeID = source.ProductTypeID;
            target.TaxClassID = source.TaxClassID;
            target.SKU = source.SKU;
            target.ManufacturerPartNumber = source.ManufacturerPartNumber;
            target.SalesPromptID = source.SalesPromptID;
            target.SpecCall = source.SpecCall;
            target.SpecsInline = source.SpecsInline;
            target.IsFeatured = source.IsFeatured;
            target.XmlPackage = source.XmlPackage;
            target.ColWidth = source.ColWidth;
            target.Published = source.Published;
            target.Wholesale = source.Wholesale;
            target.RequiresRegistration = source.RequiresRegistration;
            target.Looks = source.Looks;
            target.Notes = source.Notes;
            target.QuantityDiscountID = source.QuantityDiscountID;
            target.RelatedProducts = source.RelatedProducts;
            target.UpsellProducts = source.UpsellProducts;
            target.UpsellProductDiscountPercentage = source.UpsellProductDiscountPercentage;
            target.RelatedDocuments = source.RelatedDocuments;
            target.TrackInventoryBySizeAndColor = source.TrackInventoryBySizeAndColor;
            target.TrackInventoryBySize = source.TrackInventoryBySize;
            target.TrackInventoryByColor = source.TrackInventoryByColor;
            target.IsAKit = source.IsAKit;
            target.ShowInProductBrowser = source.ShowInProductBrowser;
            target.IsAPack = source.IsAPack;
            target.PackSize = source.PackSize;
            target.ShowBuyButton = source.ShowBuyButton;
            target.RequiresProducts = source.RequiresProducts;
            target.HidePriceUntilCart = source.HidePriceUntilCart;
            target.IsCalltoOrder = source.IsCalltoOrder;
            target.ExcludeFromPriceFeeds = source.ExcludeFromPriceFeeds;
            target.RequiresTextOption = source.RequiresTextOption;
            target.TextOptionMaxLength = source.TextOptionMaxLength;
            target.SEName = source.SEName;
            target.ExtensionData = source.ExtensionData;
            target.ExtensionData2 = source.ExtensionData2;
            target.ExtensionData3 = source.ExtensionData3;
            target.ExtensionData4 = source.ExtensionData4;
            target.ExtensionData5 = source.ExtensionData5;
            target.ContentsBGColor = source.ContentsBGColor;
            target.PageBGColor = source.PageBGColor;
            target.GraphicsColor = source.GraphicsColor;
            target.ImageFilenameOverride = source.ImageFilenameOverride;
            target.IsImport = source.IsImport;
            target.IsSystem = source.IsSystem;
            target.Deleted = source.Deleted;
            target.CreatedOn = source.CreatedOn;
            target.PageSize = source.PageSize;
            target.WarehouseLocation = source.WarehouseLocation;
            target.AvailableStartDate = source.AvailableStartDate;
            target.AvailableStopDate = source.AvailableStopDate;
            target.GoogleCheckoutAllowed = source.GoogleCheckoutAllowed;
            target.SkinID = source.SkinID;
            target.TemplateName = source.TemplateName;
            target.ProductGroup = source.ProductGroup;
            target.UpdatedOn = DateTime.Now;
        }

        public void InsertProperties(SyncEntities.ProductVariant target, Entities.ProductVariant source)
        {
            target.VariantID = source.VariantID;
            target.VariantGUID = source.VariantGUID;
            target.IsDefault = source.IsDefault;
            target.Name = source.Name;
            target.Description = source.Description;
            target.SEKeywords = source.SEKeywords;
            target.SEDescription = source.SEDescription;
            target.Colors = source.Colors;
            target.ColorSKUModifiers = source.ColorSKUModifiers;
            target.Sizes = source.Sizes;
            target.SizeSKUModifiers = source.SizeSKUModifiers;
            target.FroogleDescription = source.FroogleDescription;
            target.ProductID = source.ProductID;
            target.SKUSuffix = source.SKUSuffix;
            target.ManufacturerPartNumber = source.ManufacturerPartNumber;
            target.Price = source.Price;
            target.SalePrice = source.SalePrice;
            target.Weight = source.Weight;
            target.MSRP = source.MSRP;
            target.Cost = source.Cost;
            target.Points = source.Points;
            target.Dimensions = source.Dimensions;
            target.Inventory = source.Inventory;
            target.DisplayOrder = source.DisplayOrder;
            target.Notes = source.Notes;
            target.IsTaxable = source.IsTaxable;
            target.IsShipSeparately = source.IsShipSeparately;
            target.IsDownload = source.IsDownload;
            target.DownloadLocation = source.DownloadLocation;
            target.FreeShipping = source.FreeShipping;
            target.Published = source.Published;
            target.Wholesale = source.Wholesale;
            target.IsSecureAttachment = source.IsSecureAttachment;
            target.IsRecurring = source.IsRecurring;
            target.RecurringInterval = source.RecurringInterval;
            target.RecurringIntervalType = source.RecurringIntervalType;
            target.SubscriptionInterval = source.SubscriptionInterval;
            target.RewardPoints = source.RewardPoints;
            target.SEName = source.SEName;
            target.RestrictedQuantities = source.RestrictedQuantities;
            target.MinimumQuantity = source.MinimumQuantity;
            target.ExtensionData = source.ExtensionData;
            target.ExtensionData2 = source.ExtensionData2;
            target.ExtensionData3 = source.ExtensionData3;
            target.ExtensionData4 = source.ExtensionData4;
            target.ExtensionData5 = source.ExtensionData5;
            target.ContentsBGColor = source.ContentsBGColor;
            target.PageBGColor = source.PageBGColor;
            target.GraphicsColor = source.GraphicsColor;
            target.ImageFilenameOverride = source.ImageFilenameOverride;
            target.IsImport = source.IsImport;
            target.Deleted = source.Deleted;
            target.CreatedOn = source.CreatedOn;
            target.SubscriptionIntervalType = source.SubscriptionIntervalType;
            target.CustomerEntersPrice = source.CustomerEntersPrice;
            target.CustomerEntersPricePrompt = source.CustomerEntersPricePrompt;
            target.SEAltText = source.SEAltText;
            target.Condition = source.Condition;
            target.UpdatedOn = DateTime.Now;
        } 
        #endregion

    }
}