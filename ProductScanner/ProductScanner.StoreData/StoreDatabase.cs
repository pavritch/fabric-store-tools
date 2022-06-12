using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.DataEntities.Store;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.StoreData
{
    public class FabricStoreDatabase : BaseStoreDatabase<InsideFabricStore> { }
    public class WallpaperStoreDatabase : BaseStoreDatabase<InsideWallpaperStore> { }

    public class StoreDatabaseUpdateResultException : Exception
    {
        public StoreDatabaseUpdateResult Result { get; set; }

        public StoreDatabaseUpdateResultException()
        {

        }

        public StoreDatabaseUpdateResultException(StoreDatabaseUpdateResult result)
        {
            this.Result = result;
        }
    }

    public class RugStoreDatabase : BaseStoreDatabase<InsideRugsStore>
    {
        // any rug-specific implementations can be added here
    }

    public class HomewareStoreDatabase : BaseStoreDatabase<InsideAvenueStore>
    {
        // any homeware specific implementations can be added here
    }


    // used for Fabric+Wallpaper
    public class BaseStoreDatabase<T> : IStoreDatabase<T> where T : Store, new()
    {
        #region Constants

        /// <summary>
        /// CategoryID in store SQL for our clearance products.
        /// </summary>
        private int ClearanceCategoryID { get; set; }

        /// <summary>
        /// Root CategoryID in store SQL for designers. Individual designers reference this ID for their parent.
        /// </summary>
        private const int DesignerParentCategoryID = 162;

        #endregion

        protected readonly string _connectionStringName;
        public BaseStoreDatabase()
        {
            _connectionStringName = new T().ConnectionStringName;

            // the outlet/clearance categoryID is configurable per store in app.config
            var keyOutletCategoryID = string.Format("{0}OutletCategoryID", LibEnum.GetName(new T().StoreType));
            Debug.Assert(keyOutletCategoryID.StartsWith("Inside"));
            var id = ConfigurationManager.AppSettings[keyOutletCategoryID];
            if (id.IsInteger())
                ClearanceCategoryID = int.Parse(id);
        }


        private void WriteEventLog(Exception Ex)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Exception in StoreDatabase.cs\n{0}\n\n", Ex.Message);
            sb.AppendLine(Ex.ToJSON());
            WriteEventLog(EventLogEntryType.Error, sb.ToString());
        }

        private void WriteEventLog(EventLogEntryType evtType, string msg)
        {
            string _source = "ProductScanner.App";
            string _log = "Application";

            if (!EventLog.SourceExists(_source))
                EventLog.CreateEventSource(_source, _log);

            EventLog.WriteEntry(_source, msg, evtType);
        }


        #region GetProductsAsync
        /// <summary>
        /// Preferred method to retrieve products from the SQL store database.
        /// </summary>
        /// <param name="vendorId">Fetch products for this vendor.</param>
        /// <param name="cancelToken">Optional cancellation token, but strongly recommended</param>
        /// <param name="progress">Optional progress indicator, but strongly recommended</param>
        /// <returns>Collection of StoreProduct.</returns>
        public async Task<List<StoreProduct>> GetProductsAsync(int vendorId, CancellationToken cancelToken = default(CancellationToken), IProgress<RetrieveStoreProductsProgress> progress = null)
        {
            try
            {
                int countTotal = 0;
                int countCompleted = 0;
                int countRemaining = 0;
                double percentCompleted = 0;
                double lastReportedPercentCompleted = -1;

                // build up collection of products to return
                var products = new List<StoreProduct>();

                Action reportProgress = () =>
                {
                    percentCompleted = countTotal == 0 ? 0 : (countCompleted * 100.0) / (double)countTotal;

                    if (percentCompleted != lastReportedPercentCompleted && progress != null)
                    {
                        progress.Report(new RetrieveStoreProductsProgress(countTotal, countCompleted, countRemaining, percentCompleted));
                        lastReportedPercentCompleted = percentCompleted;
                    }
                };


                // the products to grab include all from this vendor which are not marked deleted in SQL. 
                // This will therefore return all discontinued and those marked as unpublished.

                // note that unpublished products are still valid and discovered, etc. Just not presently
                // showing on the website for some business reason.

                using (var db = new StoreContext(_connectionStringName))
                {
                    // have observed some timeouts now and then on large vendors - don't let it time out.
                    db.Database.CommandTimeout = 60 * 5;

                    // two sets of logic perform the same function - one uses a queue approach, the other skip/take.
                    // the queue approach is likely cleaner since it presents a stable snapshot of products
#if true
                    #region Fetch using Queue

                    var productIDs = await
                    (
                        from p in db.Product
                        where p.Deleted == 0
                        join pm in db.ProductManufacturer on p.ProductID equals pm.ProductID
                        where pm.ManufacturerID == vendorId
                        select p.ProductID
                     )
                    .ToListAsync();

                    countTotal = productIDs.Count();
                    countRemaining = countTotal;
                    // initial report with correct totals and zero progress
                    reportProgress();

                    int batchSize = 50; // how many records to grab each time through the loop

                    var q = new Queue<int>(productIDs);

                    while (q.Count > 0)
                    {
                        if (cancelToken != null && cancelToken.IsCancellationRequested)
                            return new List<StoreProduct>();

                        var takeCount = Math.Min(batchSize, q.Count);

                        // retrieve N productIDs from the queue

                        var batchProductID = new List<int>();
                        for (int i = 0; i < takeCount; i++)
                            batchProductID.Add(q.Dequeue());

                        var batch = await db.Product
                            .Where(e => batchProductID.Contains(e.ProductID))
                            .AsNoTracking()
                            .Include("ProductVariants")
                            .Include("ProductCategories")
                            .ToListAsync();

                        // process the retrieved products

                        products.AddRange(ProcessFetchedProductBatch(batch, vendorId, cancelToken));

                        countCompleted += takeCount;
                        countRemaining -= takeCount;
                        reportProgress();
                    }

                    #endregion

#else

                    #region Fetch using Skip/Take

                    // tested to work

                    countTotal = await db.Product
                        .Where(x => x.Deleted == 0)
                        .Where(x => x.ProductManufacturer.FirstOrDefault().ManufacturerID == vendorId)
                        .CountAsync();

                    countRemaining = countTotal;

                    // initial report with correct totals and zero progress
                    reportProgress();

                    int batchSize = 50; // how many records to grab each time through the loop

                    int skipCount = 0;
                    int takeCount;

                    while(countRemaining > 0)
                    {
                        if (cancelToken != null && cancelToken.IsCancellationRequested)
                            return  new List<StoreProduct>();

                        takeCount = Math.Min(batchSize, countRemaining);

                        // fetch one chunk of products (50)

                        var batch = await
                            (
                                from p in db.Product where p.Deleted == 0
                                join pm in db.ProductManufacturer on p.ProductID equals pm.ProductID where pm.ManufacturerID == vendorId
                                orderby p.ProductID    
                                select p      
                             )
                            .Skip(skipCount)
                            .Take(takeCount)
                            .AsNoTracking()
                            .Include("ProductVariants")
                            .Include("ProductCategories")
                            .ToListAsync();
                    
                        // process the retrieved products

                        products.AddRange(ProcessFetchedProductBatch(batch, vendorId, cancelToken));

                        countCompleted += takeCount;
                        countRemaining -= takeCount;
                        skipCount += takeCount;
                        reportProgress();
                    }

                    #endregion
#endif
                    return products;
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }
        }
        #endregion

        #region Process Fetched Products

        /// <summary>
        /// Process a batch of fetched SQL products and return a collection of StoreProducts with child StoreProductVariants.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        private List<StoreProduct> ProcessFetchedProductBatch(List<Product> batch, int vendorId, CancellationToken cancelToken)
        {
            var products = new List<StoreProduct>();

            foreach (var p in batch)
            {
                if (cancelToken != null && cancelToken.IsCancellationRequested)
                    return new List<StoreProduct>();

                products.Add(ProcessFetchedProduct(p, vendorId));
            }

            return products;
        }


        private StoreProduct ProcessFetchedProduct(Product p, int vendorId)
        {
            var extData = ExtensionData4.Deserialize(p.ExtensionData4);
            var extDic = extData.Data;

            var firstVariant = p.ProductVariants.Where(e => e.IsDefault == 1).FirstOrDefault();

            UnitOfMeasure uom = UnitOfMeasure.None;

            if (firstVariant != null)
                uom = firstVariant.Name.ToUnitOfMeasure();

            var storeProduct = new StoreProduct()
            {
                ProductID = p.ProductID,
                VendorID = vendorId,
                SKU = p.SKU,
                Correlator = p.ManufacturerPartNumber,
                Description = p.Description.TrimToNull(),
                ManufacturerDescription = p.ExtensionData5.TrimToNull(),
                Name = p.Name.TrimToNull(),
                ImageFilename = p.ImageFilenameOverride.TrimToNull(),
                IsDiscontinued = p.ShowBuyButton == 0,
                SEName = p.SEName.TrimToNull(),
                SETitle = p.SETitle.TrimToNull(),
                SEKeywords = p.SEKeywords.TrimToNull(),
                SEDescription = p.SEDescription.TrimToNull(),
                ProductClass = ProductClass.None,
                ProductGroup = p.ProductGroup.TrimToNull().ToProductGroup(),
                UnitOfMeasure = uom, // although technically variants have the true value, all variants for a product must match
                IsPublished = p.Published == 1,
                IsClearance = p.ProductCategories.Any(e => e.CategoryID == ClearanceCategoryID),

                // init these with empty collections, fill in below when data present

                PrivateProperties = new Dictionary<string, string>(),
                PublicProperties = new Dictionary<string, string>(),
                AvailableImages = new List<string>(),
                RugFeatures = null,
                HomewareFeatures = null
            };

            // available images are the image filenames (filename.jpg) that are currently physically
            // present on the server

            if (extDic.ContainsKey(ExtensionData4.AvailableImageFilenames))
                storeProduct.AvailableImages = extDic[ExtensionData4.AvailableImageFilenames] as List<string>;

            // the product images are the instructions provided by the scanner which include url, proposed filename
            // and any associated meta data. These instructions are persisted with the product record, and when images
            // are fetched and physically present, they're noted in the available images list.

            if (extDic.ContainsKey(ExtensionData4.ProductImages))
                storeProduct.ProductImages = extDic[ExtensionData4.ProductImages] as List<InsideFabric.Data.ProductImage>;

            // OriginalRawProperties are the "public" name/value attributes displayed on web store product detail pages.
            // The name is for legacy reasons, but simply consider this collection as anything here will display on
            // the web page

            if (extDic.ContainsKey(ExtensionData4.OriginalRawProperties))
                storeProduct.PublicProperties = extDic[ExtensionData4.OriginalRawProperties] as Dictionary<string, string>;

            // the private properties were added Nov 2014 - initially empty or missing (missing is okay, treated as empty).
            // None of the values here will be directly displayed on the website. However, it is certainly possible and 
            // intended that meta data stored here could be operated upon by the web store to support specific new features.
            // Any private data saved with new/updated products will be persisted and made available to the web store and
            // the scanner. We presently do not have any kind of formal data schema for keys, but would expect to as
            // different applications begin to share and integrate with this private data.

            if (extDic.ContainsKey(ExtensionData4.PrivateProperties))
                storeProduct.PrivateProperties = extDic[ExtensionData4.PrivateProperties] as Dictionary<string, string>;


            if (typeof(T) == typeof(InsideRugsStore))
            {
                if (extDic.ContainsKey(ExtensionData4.RugProductFeatures))
                    storeProduct.RugFeatures = extDic[ExtensionData4.RugProductFeatures] as RugProductFeatures;
            }

            if (typeof(T) == typeof(InsideAvenueStore))
            {
                if (extDic.ContainsKey(ExtensionData4.HomewareProductFeatures))
                    storeProduct.HomewareFeatures = extDic[ExtensionData4.HomewareProductFeatures] as HomewareProductFeatures;
            }

            // process product variants

            var variants = new List<StoreProductVariant>();

            if (p.ProductVariants != null)
            {
                Func<string, int> makeOrderIncrementQuantity = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return 1;

                    int value;
                    if (int.TryParse(s, out value))
                        return value;

                    return 1;
                };

                // the first variant placed in the list is always the default (there is only one marked as IsDefault in SQL)

                foreach (var v in p.ProductVariants.OrderByDescending(e => e.IsDefault).ThenBy(e => e.VariantID))
                {
                    // this extension data is new for product variants starting November 2014. The data in SQL may be missing,
                    // so we are careful here to allow for such

                    ExtensionData4 vExtData = null;
                    Dictionary<string, object> vExtDic = new Dictionary<string, object>();

                    if (!string.IsNullOrWhiteSpace(v.ExtensionData4))
                    {
                        vExtData = ExtensionData4.Deserialize(v.ExtensionData4);

                        if (vExtData != null)
                            vExtDic = vExtData.Data;
                    }


                    var storeVariant = new StoreProductVariant()
                    {
                        VariantID = v.VariantID,
                        ProductID = v.ProductID,
                        SKUSuffix = v.SKUSuffix,
                        ManufacturerPartNumber = v.ManufacturerPartNumber,
                        MinimumQuantity = v.MinimumQuantity ?? 1,
                        // this value is saved as a string in ExtensionData column - since we needed a place (missing means null, to use system default logic).
                        OrderIncrementQuantity = makeOrderIncrementQuantity(v.ExtensionData),
                        IsDefault = v.IsDefault == 1,
                        Cost = v.Cost ?? 0M,
                        RetailPrice = v.MSRP.GetValueOrDefault(),
                        OurPrice = v.Price,
                        SalePrice = v.SalePrice,
                        IsSwatch = v.Name == UnitOfMeasure.Swatch.ToDescriptionString(),
                        InStock = v.Inventory > 0,
                        Description = p.ProductGroup == "Rug" ? v.Name : v.Description.TrimToNull(),
                        Shape = v.Dimensions.TrimToNull().ToProductShape(), // yes, shape is stored in pv.Dimensions, fabric/wallpaper will be none. Used mostly for rugs.
                        OrderRequirementsNotice = v.ExtensionData2.TrimToNull(),
                        DisplayOrder = v.DisplayOrder,
                        IsFreeShipping = v.FreeShipping == 1,
                        IsPublished = v.Published == 1,
                        UnitOfMeasure = p.ProductGroup == "Rug" ? UnitOfMeasure.Each : v.Name.ToUnitOfMeasure(),

                        // init these with empty collections, fill in below when data present

                        PrivateProperties = new Dictionary<string, string>(),
                        PublicProperties = new Dictionary<string, string>(),
                        RugFeatures = null,
                    };


                    // these dictionaries are new for product variants starting Nov 2014. May be missing or empty. But once you stick
                    // something in it, it will be correctly persisted.

                    // note also that for the memoment, nothing in the web store service looks at these dictionaries - although that will 
                    // change once we start making use of them.

                    // OriginalRawProperties are the "public" name/value attributes displayed on web store product detail pages.
                    // The name is for legacy reasons, but simply consider this collection as anything here will display on
                    // the web page

                    if (vExtDic.ContainsKey(ExtensionData4.OriginalRawProperties))
                        storeVariant.PublicProperties = vExtDic[ExtensionData4.OriginalRawProperties] as Dictionary<string, string>;

                    // the private properties were added Nov 2014 - initially empty or missing (missing is okay, treated as empty).
                    // None of the values here will be directly displayed on the website. However, it is certainly possible and 
                    // intended that meta data stored here could be operated upon by the web store to support specific new features.
                    // Any private data saved with new/updated products will be persisted and made available to the web store and
                    // the scanner. We presently do not have any kind of formal data schema for keys, but would expect to as
                    // different applications begin to share and integrate with this private data.

                    if (vExtDic.ContainsKey(ExtensionData4.PrivateProperties))
                        storeVariant.PublicProperties = vExtDic[ExtensionData4.PrivateProperties] as Dictionary<string, string>;

                    if (typeof(T) == typeof(InsideRugsStore))
                    {
                        if (vExtDic.ContainsKey(ExtensionData4.RugProductVariantFeatures))
                            storeVariant.RugFeatures = vExtDic[ExtensionData4.RugProductVariantFeatures] as RugProductVariantFeatures;
                    }

                    storeVariant.StoreProduct = storeProduct;
                    variants.Add(storeVariant);
                }
                storeProduct.ProductVariants = variants;
            }
            return storeProduct;
        }
        #endregion

        #region AddProduct
        /// <summary>
        /// Add a new product to SQL.
        /// </summary>
        /// <remarks>
        /// Includes all related variants and images.
        /// </remarks>
        /// <param name="product"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult AddProduct(StoreProduct storeProduct)
        {

            // For reference - the following actions are taken by the data service
            // when RebuildAll invoked after a bunch of SQL updates.
            //
            // RebuildAll()
            //    RebuildProductSearchExtensionData (full text and autosuggest)
            //    RebuildProductCategoryTable (fully rebuilds category associations, except for a very few protected categories (outlet, etc.)
            //    RepopulateProducts (loads up cached lookups by manufacturer and categoryID)
            //    RebuildAllStoreSpecificTasks
            //        SpinUpMissingDescriptions (depends on categories associations to be done first to work)
            //            p.Description - detail page rich snippets meta. Computer generated. Similar to product feed, not very sales-ey.
            //            p.SEDescription - computer generated
            //            p.SEKeywords - computer generated.
            //            p.Summary - our breadcrumbs-like taxonomy, mimics what google uses for the most part. Detail page rich snippets meta.


            // NOTE - available columns in SQL we can hijack if needed:
            //
            // p.FroogleDescription
            // p.SpecTitle
            // p.Notes
            // p.RelatedProducts
            // p.RequiredProducts
            // pv.FroogleDescription

            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    using (var scope = new TransactionScope())
                    {
                        if (db.Product.Count(x => x.SKU == storeProduct.SKU) > 0)
                            return StoreDatabaseUpdateResult.Duplicate;

                        var sqlProduct = new Product();

                        // will throw if some error
                        PopulateSqlProduct(db, true, sqlProduct, storeProduct);

                        // save main product record
                        db.Product.Add(sqlProduct);
                        db.SaveChanges();

                        AssignProductCategoryAssociations(db, sqlProduct.ProductID, storeProduct, null);

                        if (storeProduct.ProductImages.Any())
                            db.EnqueueProductForImageProcessing(sqlProduct.ProductID);

                        if (storeProduct.IsClearance)
                            db.AddProductCategoryAssociation(sqlProduct.ProductID, ClearanceCategoryID);
                        else
                            db.RemoveProductCategoryAssociation(sqlProduct.ProductID, ClearanceCategoryID);

                        // create the association with the manufacturer
                        db.AddProductManufacturerAssociation(sqlProduct.ProductID, storeProduct.VendorID);

                        foreach (var productVariant in storeProduct.ProductVariants)
                        {
                            var pv = PopulateSqlProductVariant(true, sqlProduct.ProductID, new ProductVariant(), productVariant, sqlProduct.CreatedOn);

                            db.ProductVariant.Add(pv);
                            db.SaveChanges();
                        }

                        db.UpdateProductLabels(sqlProduct.ProductID, storeProduct.PublicProperties);

                        scope.Complete();
                    }
                    return StoreDatabaseUpdateResult.Success;
                }
            }
            catch (StoreDatabaseUpdateResultException Ex)
            {
                return Ex.Result;
            }
#if DEBUG
            catch (DbEntityValidationException Ex)
            {
                Debug.WriteLine(Ex.Message);
                foreach (var err in Ex.EntityValidationErrors)
                {
                    foreach (var err2 in err.ValidationErrors)
                    {
                        Debug.WriteLine(err2.ErrorMessage);
                    }
                }
                return StoreDatabaseUpdateResult.Error;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                if (Ex.InnerException != null)
                {
                    Debug.WriteLine(Ex.InnerException.Message);
                    if (Ex.InnerException.InnerException != null)
                    {
                        Debug.WriteLine(Ex.InnerException.InnerException.Message);
                    }
                }
                return StoreDatabaseUpdateResult.Error;
            }
#else
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
#endif
        }
        #endregion

        #region PopulateSqlProduct

        private void SqlProductAssertions(bool isAdd, Product sqlProduct, StoreProduct storeProduct)
        {
#if DEBUG
            // presently using two-letter prefixes
            //Debug.Assert(Regex.IsMatch(storeProduct.SKU, @"^[A-Z][A-Z]\-([A-Z]|[0-9]|[\-]){1,64}$"));

            //Debug.Assert(storeProduct.ProductVariants != null && storeProduct.ProductVariants.Any()); // must always have at least one variant

            //Debug.Assert(storeProduct.ProductVariants.Count(e => e.IsDefault) == 1); // exactly 1 default variant
            //Debug.Assert(storeProduct.ProductVariants.Select(e => e.SKUSuffix ?? "").Distinct().Count() == storeProduct.ProductVariants.Count()); // all variants must be distinct SKU

            // all variants must be distinct SKU
            //Debug.Assert(storeProduct.ProductVariants.Where(e => e.UnitOfMeasure != UnitOfMeasure.Swatch).Select(e => e.ManufacturerPartNumber).Distinct().Count()
                //== storeProduct.ProductVariants.Where(e => e.UnitOfMeasure != UnitOfMeasure.Swatch).Select(e => e.ManufacturerPartNumber).Count());

            // clearly not valid for rugs
            //Debug.Assert(storeProduct.ProductVariants.Where(e => e.UnitOfMeasure != UnitOfMeasure.Swatch).Count() <= 1);

            if (isAdd)
            {
                //Debug.Assert(!storeProduct.ProductID.HasValue); // would  never have a productID before inserting
                //Debug.Assert(storeProduct.AvailableImages == null || storeProduct.AvailableImages.Count == 0); // avail images are what's actually on server, so would be none for a new product
                //Debug.Assert(!storeProduct.IsDiscontinued); // should not be adding products which are already discontinued
            }
            else
            {
                //Debug.Assert(storeProduct.ProductID.HasValue && storeProduct.ProductID == sqlProduct.ProductID); // for update, must have productID

                // do not want to be using UpdateProudct to change from live to discontinued - as there are other considerations
                //Debug.Assert(!storeProduct.IsDiscontinued);
            }

            //Debug.Assert(!string.IsNullOrWhiteSpace(storeProduct.Name));
            //Debug.Assert(!string.IsNullOrWhiteSpace(storeProduct.Correlator)); // never blank, and if nothing to correlate, use sku for self

            //if (storeProduct.ProductGroup == ProductGroup.Rug)
                //Debug.Assert(storeProduct.PublicProperties != null); // even if empty, cannot be null
            //else
                //Debug.Assert(storeProduct.PublicProperties != null && storeProduct.PublicProperties.Count() > 0); // should always have something here

            if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
            {
                //Debug.Assert(storeProduct.ProductGroup == ProductGroup.Fabric || storeProduct.ProductGroup == ProductGroup.Wallcovering || storeProduct.ProductGroup == ProductGroup.Trim);
            }

            if (typeof(T) == typeof(InsideRugsStore))
            {
                //Debug.Assert(storeProduct.ProductGroup == ProductGroup.Rug);
                // the features data must be provided
                //Debug.Assert(storeProduct.RugFeatures != null);

                // this key is required for all insertes and updates, we use it to know when we need to
                // re-run the classifiers for filters/discovery/etc.
                //Debug.Assert(storeProduct.PrivateProperties.ContainsKey(ProductPropertyType.LastFullUpdate.DescriptionAttr()));
            }

            if (typeof(T) == typeof(InsideAvenueStore))
            {
                //Debug.Assert(storeProduct.ProductGroup == ProductGroup.Homeware);
                // the features data must be provided
                //Debug.Assert(storeProduct.HomewareFeatures != null);
                //Debug.Assert(storeProduct.HomewareFeatures.Category > 0);

                // this key is required for all insertes and updates, we use it to know when we need to
                // re-run the classifiers for filters/discovery/etc.
                //Debug.Assert(storeProduct.PrivateProperties.ContainsKey(ProductPropertyType.LastFullUpdate.DescriptionAttr()));
            }


            if (storeProduct.ProductImages.Count > 0)
            {
                // exactly one must be designated as default
                //Debug.Assert(storeProduct.ProductImages.Count(e => e.IsDefault) == 1);

                if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                {
                    //Debug.Assert(storeProduct.ProductImages.Count(e => e.ImageVariant == "Primary") == 1);
                }

                if (typeof(T) == typeof(InsideAvenueStore))
                {
                    // must have exactly one primary
                    //Debug.Assert(storeProduct.ProductImages.Count(e => e.ImageVariant == "Primary") == 1);
                    // only Primary and Scene allowed
                    //Debug.Assert(!storeProduct.ProductImages.Any(e => !(e.ImageVariant == "Primary" || e.ImageVariant == "Scene")));
                }

                if (typeof(T) == typeof(InsideRugsStore))
                {
                    // Rugs not allowed to be identified as primary
                    //Debug.Assert(!storeProduct.ProductImages.Any(e => e.ImageVariant == "Primary"));
                    //Debug.Assert(!storeProduct.ProductImages.Any(e => e.ImageVariant == "None"));
                    //Debug.Assert(!storeProduct.ProductImages.Any(e => e.ImageVariant == "Alternate")); // see Peter if a need comes up for this
                }

                //foreach (var img in storeProduct.ProductImages)
                //{
                    //Debug.Assert(!string.IsNullOrWhiteSpace(img.Filename)); // must designate a filename
                    //Debug.Assert(!string.IsNullOrWhiteSpace(img.SourceUrl)); // must say where the image comes from
                //}

                //var distinctCount = storeProduct.ProductImages.Select(x => x.SourceUrl).Distinct().Count();
                //Debug.Assert(distinctCount == storeProduct.ProductImages.Count);
            }
#endif
        }

        /// <summary>
        /// Add or update product.
        /// </summary>
        /// <remarks>
        /// Pass in a Product object to be populated from the StoreProduct.
        /// </remarks>
        /// <param name="isAdd">indicates if adding so can shape conditional logic.</param>
        /// <param name="sqlProduct"></param>
        /// <param name="storeProduct"></param>
        /// <returns></returns>
        private void PopulateSqlProduct(StoreContext db, bool isAdd, Product sqlProduct, StoreProduct storeProduct)
        {
#if DEBUG
            SqlProductAssertions(isAdd, sqlProduct, storeProduct);
#endif
            // ExtensionData - XML representation of public properties, used to populate table in product detail page
            // ExtensionData2 - string which has one word or word phrase on each line which is to be associated with this product. These phrases will participate in 
            //                  full text search and contribute to the unique phrase list which powers the auto suggest feature
            // ExtensionData3 - Used by DataService, one line per phrase which will contribute to full text search - but not autocomplete.
            // ExtensionData4 - JSON data
            // ExtensionData5 - manufacturer description, when available (most vendors do not provide a description)

            // must always have at least one variant
            if (storeProduct.ProductVariants == null && storeProduct.ProductVariants.Count() == 0)
                throw new StoreDatabaseUpdateResultException(StoreDatabaseUpdateResult.InvalidData);

            // must have exactly 1 default variant
            if (storeProduct.ProductVariants.Where(e => e.IsDefault).Count() != 1)
                throw new StoreDatabaseUpdateResultException(StoreDatabaseUpdateResult.InvalidData);

            // all variants must be distinct SKU
            if (storeProduct.ProductVariants.Select(e => e.SKUSuffix ?? "").Distinct().Count() != storeProduct.ProductVariants.Count())
                throw new StoreDatabaseUpdateResultException(StoreDatabaseUpdateResult.InvalidData);

            // must be unique MPN for variants
            if (storeProduct.ProductVariants.Where(e => e.UnitOfMeasure != UnitOfMeasure.Swatch).Select(e => e.ManufacturerPartNumber).Distinct().Count()
                    != storeProduct.ProductVariants.Where(e => e.UnitOfMeasure != UnitOfMeasure.Swatch).Select(e => e.ManufacturerPartNumber).Count())
                throw new StoreDatabaseUpdateResultException(StoreDatabaseUpdateResult.InvalidData);

            // never more than one swatch
            //if (storeProduct.ProductVariants.Where(e => e.UnitOfMeasure != UnitOfMeasure.Swatch).Count() > 1)
            //throw new StoreDatabaseUpdateResultException(StoreDatabaseUpdateResult.InvalidData);

            ExtensionData4 extData4;

            string limitedAvailability;
            bool islimitedAvailability = false;

            // if limited supply, fake out as not new
            if (storeProduct.PrivateProperties.TryGetValue(ProductPropertyType.IsLimitedAvailability.DescriptionAttr(), out limitedAvailability) && limitedAvailability != string.Empty && bool.Parse(limitedAvailability))
                islimitedAvailability = true;

            string vendorProductDetailPage = null;
            storeProduct.PrivateProperties.TryGetValue(ProductPropertyType.ProductDetailUrl.DescriptionAttr(), out vendorProductDetailPage);

            // make sure SKU does not already exist
            if (isAdd)
            {
                //product.ProductID filled in by SQL

                sqlProduct.ProductGUID = Guid.NewGuid();
                sqlProduct.Summary = null;

                sqlProduct.FroogleDescription = null;
                sqlProduct.ExtensionData2 = null; // filled in by data service for full text search and autocsuggest
                sqlProduct.ExtensionData3 = null; // filled in by data service for full text search

                // on add, this is null until the data service image processor 
                // comes around and processes the JSON image information and 
                // provisions the images into the right folders, etc. Only then does the system
                // fill in this field from the JSON data - now that we actually have an image.
                sqlProduct.ImageFilenameOverride = null;

                // choose the correct website prdouct detail page xml package (template)

                if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                {
                    sqlProduct.XmlPackage = "product.variantsinrightbar.xml.config";
                }
                else if (typeof(T) == typeof(InsideRugsStore))
                {
                    sqlProduct.XmlPackage = "product.rug.xml.config";
                }
                else if (typeof(T) == typeof(InsideAvenueStore))
                {
                    sqlProduct.XmlPackage = "product.simpleproduct.xml.config";
                }

                extData4 = new ExtensionData4();
                extData4.Data[ExtensionData4.AvailableImageFilenames] = new List<string>();
                extData4.Data[ExtensionData4.ProductImages] = storeProduct.ProductImages;

                sqlProduct.CreatedOn = DateTime.Now;

                #region Product Fixed Values

                // we had previously used these fields to store the demensions of the largest image
                // no longer now that we have new image processing
                sqlProduct.TextOptionMaxLength = null; // numeric image width
                sqlProduct.GraphicsColor = null; // string in format 300x500, else null

                sqlProduct.SpecTitle = null;

                sqlProduct.SwatchImageMap = null;

                sqlProduct.IsFeaturedTeaser = null;

                sqlProduct.SENoScript = string.Empty;

                sqlProduct.SEAltText = string.Empty;

                sqlProduct.SizeOptionPrompt = null;

                sqlProduct.ColorOptionPrompt = null;

                sqlProduct.TextOptionPrompt = null;

                sqlProduct.ProductTypeID = 1;

                sqlProduct.TaxClassID = 1;

                sqlProduct.SalesPromptID = 1;

                sqlProduct.SpecCall = null;

                sqlProduct.SpecsInline = 0;

                sqlProduct.IsFeatured = 0;

                sqlProduct.ColWidth = 4;

                sqlProduct.Published = 1;

                sqlProduct.Wholesale = 0;

                sqlProduct.RequiresRegistration = 0;

                sqlProduct.Looks = 0;

                sqlProduct.Notes = null;

                sqlProduct.QuantityDiscountID = null;

                sqlProduct.RelatedProducts = null;

                sqlProduct.UpsellProducts = null;

                sqlProduct.UpsellProductDiscountPercentage = 0.00M;

                sqlProduct.TrackInventoryBySizeAndColor = 0;

                sqlProduct.TrackInventoryBySize = 0;

                sqlProduct.TrackInventoryByColor = 0;

                sqlProduct.IsAKit = 0;

                sqlProduct.ShowInProductBrowser = 1;

                sqlProduct.IsAPack = 0;

                sqlProduct.PackSize = 0;

                sqlProduct.RequiresProducts = null;

                sqlProduct.HidePriceUntilCart = 0;

                sqlProduct.IsCalltoOrder = 0;

                sqlProduct.ExcludeFromPriceFeeds = 0;

                sqlProduct.RequiresTextOption = 0;

                sqlProduct.ContentsBGColor = null;

                sqlProduct.PageBGColor = null;

                sqlProduct.IsImport = 0;

                sqlProduct.IsSystem = 0;

                sqlProduct.Deleted = 0;

                sqlProduct.UpdatedOn = DateTime.Now;

                sqlProduct.PageSize = 20;

                sqlProduct.WarehouseLocation = null;

                sqlProduct.AvailableStartDate = DateTime.Now.Date;

                sqlProduct.AvailableStopDate = null;

                sqlProduct.GoogleCheckoutAllowed = 1;

                sqlProduct.SkinID = 0;

                sqlProduct.TemplateName = string.Empty;


                #endregion
            }
            else // update
            {
                // not allowed to use an update to change a product to discontinued,
                // but you are allowed to call this method to change a product to being live if was discontinued

                if (sqlProduct.ShowBuyButton == 1 && storeProduct.IsDiscontinued)
                    throw new StoreDatabaseUpdateResultException(StoreDatabaseUpdateResult.InvalidData);

                extData4 = ExtensionData4.Deserialize(sqlProduct.ExtensionData4);

                // ensure we do not lose existing images which are no longer found through vendor - because if they now report
                // nothing, we surely don't want to overwrite the empty collection on top of our previous information.

                if (storeProduct.ProductImages.Count() > 0)
                    extData4.Data[ExtensionData4.ProductImages] = storeProduct.ProductImages;
            }

            sqlProduct.MiscText = islimitedAvailability ? "Limited Availability" : null;

            // use related documents field to point to vendor detail page when known/available
            sqlProduct.RelatedDocuments = string.IsNullOrWhiteSpace(vendorProductDetailPage) ? null : vendorProductDetailPage;

            sqlProduct.SKU = storeProduct.SKU;
            sqlProduct.Name = storeProduct.Name;
            sqlProduct.SEKeywords = storeProduct.SEKeywords;
            sqlProduct.SEDescription = storeProduct.SEDescription;
            sqlProduct.Description = storeProduct.Description;
            sqlProduct.ShowBuyButton = storeProduct.IsDiscontinued ? 0 : 1;
            sqlProduct.ProductGroup = storeProduct.ProductGroup.DescriptionAttr();
            sqlProduct.SETitle = storeProduct.SETitle;
            sqlProduct.Published = storeProduct.IsPublished ? (byte)1 : (byte)0;
            sqlProduct.ManufacturerPartNumber = storeProduct.Correlator;
            sqlProduct.SEName = storeProduct.SEName;

            if (typeof(T) == typeof(InsideRugsStore))
            {
                // the website logic for putting together groups of these things uses the productlabels table, so it's
                // important to have these pushed out to that table when present

                if (!string.IsNullOrWhiteSpace(storeProduct.RugFeatures.PatternName))
                    storeProduct.PublicProperties[ProductPropertyType.PatternName.DescriptionAttr()] = storeProduct.RugFeatures.PatternName;

                if (!string.IsNullOrWhiteSpace(storeProduct.RugFeatures.PatternNumber))
                    storeProduct.PublicProperties[ProductPropertyType.PatternNumber.DescriptionAttr()] = storeProduct.RugFeatures.PatternNumber;

                if (!string.IsNullOrWhiteSpace(storeProduct.RugFeatures.Designer))
                    storeProduct.PublicProperties[ProductPropertyType.Designer.DescriptionAttr()] = storeProduct.RugFeatures.Designer;

                if (!string.IsNullOrWhiteSpace(storeProduct.RugFeatures.Collection))
                    storeProduct.PublicProperties[ProductPropertyType.Collection.DescriptionAttr()] = storeProduct.RugFeatures.Collection;
            }

            // update name value properties and XML data

            storeProduct.PrivateProperties[ExtensionData4.PrivatePropertiesKeys.RequiresClassifyUpdate] = true.ToString();

            extData4.Data[ExtensionData4.OriginalRawProperties] = storeProduct.PublicProperties;
            extData4.Data[ExtensionData4.PrivateProperties] = storeProduct.PrivateProperties;

            if (typeof(T) == typeof(InsideRugsStore))
                extData4.Data[ExtensionData4.RugProductFeatures] = storeProduct.RugFeatures;

            if (typeof(T) == typeof(InsideAvenueStore))
                extData4.Data[ExtensionData4.HomewareProductFeatures] = storeProduct.HomewareFeatures;

            sqlProduct.ExtensionData4 = extData4.Serialize();

            var xml = ProductProperties.MakePropertiesXml(storeProduct.PublicProperties).ToString();
            sqlProduct.ExtensionData = xml;

            sqlProduct.ExtensionData5 = storeProduct.ManufacturerDescription; // frequently null, unless they've given us a nice description 
        }

        /// <summary>
        /// Handle adds/updates to SQL ProductCategory table.
        /// </summary>
        /// <param name="db">Database context to use for any SQL requirements</param>
        /// <param name="productID">target product</param>
        /// <param name="storeProduct">Data collected by scanner</param>
        /// <param name="humanChanges">existing record of changes, else null when none on file</param>
        private void AssignProductCategoryAssociations(StoreContext db, int productID, StoreProduct storeProduct, HumanHomewareProductFeatures humanChanges = null)
        {
            const int HOMEWARE_UNCLASSIFIED_CATEGORYID = 117;
            //const int HOMEWARE_REVIEW_CATEGORYID = 121;

            // categories to be associated with this product - once we have a productID
            // will be inserted with list of categoryID.
            var categoryAssociations = new List<int>();

            // attempt to auto-associate designer - only for InsideFabric/InsideWallpaper sties for now

            if (typeof(T) == typeof(InsideAvenueStore))
            {
                Dictionary<int, int> lookupMap;
                var sqlRootNode = db.Category.Where(e => e.CategoryID == 1).FirstOrDefault();
                Debug.Assert(sqlRootNode != null && !string.IsNullOrEmpty(sqlRootNode.ExtensionData));

                lookupMap = sqlRootNode.ExtensionData.FromJSON<Dictionary<int, int>>();
                Debug.Assert(lookupMap.Count() > 20); // make sure we have a populated mapping

                var primarySqlCategories = new HashSet<int>(lookupMap.Values);
                var existingCategories = db.ProductCategory.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();

                Func<bool> hasHumanCategory = () =>
                {
                    return humanChanges != null && humanChanges.PrimarySqlCategory.HasValue;
                };

                // we only want at most a single primary category
                Action<int?> sanitizeExistingPrimaryCategories = (keeper) =>
                {
                    foreach (var catID in existingCategories)
                    {
                        if (primarySqlCategories.Contains(catID))
                        {
                            if (keeper.HasValue && catID == keeper.Value)
                                continue;

                            db.RemoveProductCategoryAssociation(productID, catID);
                        }
                    }
                };

                if (storeProduct.HomewareFeatures.Category > 1)
                {
                    int sqlCategoryID = 0;
                    int internalCatID = storeProduct.HomewareFeatures.Category;

                    if (lookupMap.TryGetValue(internalCatID, out sqlCategoryID))
                    {
                        // do not mess with existing SQL primary category if has been established by human

                        if (!hasHumanCategory())
                        {
                            // we want to make an association, but if there is already one, if not the same,
                            // then need to remove so can add the right one back in. If is the same, no need to change anything.

                            sanitizeExistingPrimaryCategories(/* keep only */ sqlCategoryID);
                            categoryAssociations.Add(sqlCategoryID);
                        }
                    }
                }
                else
                {
                    // unclassified
                    // remove any prior classification under the assumption was wrong,
                    // add to a category reserved for unclassified which must be reviewed by humans.
                    if (!hasHumanCategory())
                    {
                        sanitizeExistingPrimaryCategories(null);
                        categoryAssociations.Add(HOMEWARE_UNCLASSIFIED_CATEGORYID);
                    }
                }

                // if there is a human classification - ensure it is there and the only primary
                if (hasHumanCategory())
                {
                    sanitizeExistingPrimaryCategories(/* keep only */ humanChanges.PrimarySqlCategory);
                    if (!existingCategories.Contains(humanChanges.PrimarySqlCategory.Value))
                        categoryAssociations.Add(humanChanges.PrimarySqlCategory.Value);
                }
            }

            if (typeof(T) == typeof(InsideFabricStore))
            {
                if (storeProduct.ProductGroup == ProductGroup.Fabric ||
                    storeProduct.ProductGroup == ProductGroup.Wallcovering ||
                    storeProduct.ProductGroup == ProductGroup.Trim)
                {

                    if
                        (storeProduct.PublicProperties.ContainsKey("Designer"))
                    {
                        var designerCategoryID = FindCategoryForDesigner(storeProduct.PublicProperties["Designer"]);
                        if (designerCategoryID.HasValue)
                            categoryAssociations.Add(designerCategoryID.Value);
                    }
                }
            }

            // create category associatons for any discovered along the way
            foreach (var categoryID in categoryAssociations)
                db.AddProductCategoryAssociation(productID, categoryID);
        }

        #endregion

        #region PopulateSqlProductVariant

        private void SqlProductVariantAssertions(bool isAdd, ProductVariant sqlProductVariant, StoreProductVariant storeProductVariant)
        {
#if DEBUG

            if (isAdd)
            {
                //Debug.Assert(!storeProductVariant.ProductID.HasValue); // would  never have a productID before inserting
                //Debug.Assert(!storeProductVariant.VariantID.HasValue); // would  never have a variantID before inserting
            }
            else
            {
                //Debug.Assert(storeProductVariant.ProductID.HasValue && storeProductVariant.ProductID.Value == sqlProductVariant.ProductID); // must have a productID to update
                //Debug.Assert(storeProductVariant.VariantID.HasValue && storeProductVariant.VariantID == sqlProductVariant.VariantID); // must have a variantID to update
            }

            //Debug.Assert(!string.IsNullOrWhiteSpace(storeProductVariant.ManufacturerPartNumber));

            //Debug.Assert(storeProductVariant.OurPrice > 0M);
            //Debug.Assert(storeProductVariant.RetailPrice > 0M);

            // if an increment is specified, must be greater than 1
            //Debug.Assert(storeProductVariant.OrderIncrementQuantity > 0);
            //Debug.Assert(storeProductVariant.MinimumQuantity > 0);

            if (storeProductVariant.IsSwatch)
            {
                // concept of swatches does not exist for rugs
                //Debug.Assert(storeProductVariant.UnitOfMeasure == UnitOfMeasure.Swatch);
                //Debug.Assert(storeProductVariant.SKUSuffix == "-Swatch");
                //Debug.Assert(storeProductVariant.DisplayOrder == 2);
                //Debug.Assert(storeProductVariant.IsFreeShipping);
                //Debug.Assert(storeProductVariant.Shape == ProductShapeType.None);
                //Debug.Assert(storeProductVariant.Description == null);
                //Debug.Assert(storeProductVariant.OrderIncrementQuantity == 1);
                //Debug.Assert(storeProductVariant.MinimumQuantity == 1);
            }
            else
            {
                // not swatch, any store

                //Debug.Assert(storeProductVariant.Cost > 0M);

                //Debug.Assert(storeProductVariant.OurPrice > storeProductVariant.Cost);
                //Debug.Assert(storeProductVariant.RetailPrice >= storeProductVariant.OurPrice);

                //Debug.Assert(!storeProductVariant.IsFreeShipping);

                if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                {
                    // if not a swatch, core product suffix must be empty
                    //Debug.Assert(string.IsNullOrEmpty(storeProductVariant.SKUSuffix));
                    //Debug.Assert(storeProductVariant.DisplayOrder == 1);
                    //Debug.Assert(storeProductVariant.Shape == ProductShapeType.None);
                    //Debug.Assert(storeProductVariant.Description == null);

                }
                else if (typeof(T) == typeof(InsideAvenueStore))
                {
                    //Debug.Assert(storeProductVariant.DisplayOrder == 1);
                    //Debug.Assert(storeProductVariant.Shape == ProductShapeType.None);
                }
                else if (typeof(T) == typeof(InsideRugsStore))
                {
                    // rugs only allow for each unit of measure
                    //Debug.Assert(storeProductVariant.UnitOfMeasure == UnitOfMeasure.Each);

                    // rug variants cannot have a blank suffix - because suffix indicates size/shape
                    //Debug.Assert(!string.IsNullOrWhiteSpace(storeProductVariant.SKUSuffix) && storeProductVariant.SKUSuffix.StartsWith("-"));
                    //Debug.Assert(storeProductVariant.DisplayOrder > 0); // start at one
                    //Debug.Assert(storeProductVariant.Shape != ProductShapeType.None);
                    //Debug.Assert(!string.IsNullOrWhiteSpace(storeProductVariant.Description));

                    // we'll need to ponder/review what our policy will be for rug sku suffix
                    //Debug.Assert(Regex.IsMatch(storeProductVariant.SKUSuffix, @"^\-([A-Z]|[a-z]|[0-9]|[\-]){1,64}$"));

                    //Debug.Assert(storeProductVariant.RugFeatures != null);
                    //Debug.Assert(!string.IsNullOrWhiteSpace(storeProductVariant.RugFeatures.Shape));
                    //Debug.Assert(!string.IsNullOrWhiteSpace(storeProductVariant.RugFeatures.Description));

                    // samples should always be some huge display order to make last - 100 is the recommended value
                    //Debug.Assert(storeProductVariant.RugFeatures.IsSample == false || storeProductVariant.DisplayOrder >= 100);

                    // these values from the typesafe data and the SQL variant are expected to be identical.
                    //Debug.Assert(storeProductVariant.RugFeatures.Description == storeProductVariant.Description);
                    //Debug.Assert(storeProductVariant.RugFeatures.Shape == storeProductVariant.Shape.DescriptionAttr());
                    //Debug.Assert(!string.IsNullOrWhiteSpace(storeProductVariant.RugFeatures.ImageFilename));

                    //Debug.Assert(storeProductVariant.RugFeatures.Width > 0.0);
                    //Debug.Assert(storeProductVariant.RugFeatures.Length > 0.0);
                    //Debug.Assert(storeProductVariant.RugFeatures.AreaSquareFeet > 0.0);
                }

            }
#endif
        }

        /// <summary>
        /// Create a SQL ProductVariant record from our StoreProductVariant class.
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="storeProductVariant"></param>
        /// <returns>Returns the same sql variant passed in.</returns>
        private ProductVariant PopulateSqlProductVariant(bool isAdd, int productID, ProductVariant sqlProductVariant, StoreProductVariant storeProductVariant, DateTime createdOn)
        {
#if DEBUG
            //SqlProductVariantAssertions(isAdd, sqlProductVariant, storeProductVariant);
#endif

            // XML data

            // ExtensionData - holds OrderIncrementQuantity as string when not 1
            // ExtensionData2 - holds optional OrderRequirementsNotice
            // ExtensionData3 - unused
            // ExtensionData4 - JSON data
            // ExtensionData5 - unused

            ExtensionData4 variantExtData4;
            if (isAdd)
            {
                // pv.VariantID set by SQL

                sqlProductVariant.VariantGUID = Guid.NewGuid();
                sqlProductVariant.ProductID = productID;

                sqlProductVariant.ExtensionData3 = null;
                sqlProductVariant.ExtensionData5 = null;
                variantExtData4 = new ExtensionData4();

                sqlProductVariant.CreatedOn = createdOn;

                #region Fixed Values

                sqlProductVariant.SEName = null;

                sqlProductVariant.SEKeywords = null;

                sqlProductVariant.SEDescription = null;

                sqlProductVariant.Colors = null;

                sqlProductVariant.ColorSKUModifiers = null;

                sqlProductVariant.Sizes = null;

                sqlProductVariant.SizeSKUModifiers = null;

                sqlProductVariant.FroogleDescription = null;

                sqlProductVariant.Weight = null;

                sqlProductVariant.MSRP = null;

                sqlProductVariant.Points = 0;

                sqlProductVariant.Notes = null;

                sqlProductVariant.IsTaxable = 1;

                sqlProductVariant.IsShipSeparately = 0;

                sqlProductVariant.IsDownload = 0;

                sqlProductVariant.DownloadLocation = null;

                sqlProductVariant.Wholesale = 0;

                sqlProductVariant.IsSecureAttachment = 0;

                sqlProductVariant.IsRecurring = 0;

                sqlProductVariant.RecurringInterval = 0;

                sqlProductVariant.RecurringIntervalType = 0;

                sqlProductVariant.SubscriptionInterval = null;

                sqlProductVariant.RewardPoints = null;

                sqlProductVariant.RestrictedQuantities = null;


                sqlProductVariant.ContentsBGColor = null;

                sqlProductVariant.PageBGColor = null;

                sqlProductVariant.GraphicsColor = null;

                sqlProductVariant.ImageFilenameOverride = null;

                sqlProductVariant.IsImport = 0;

                sqlProductVariant.Deleted = 0;

                sqlProductVariant.UpdatedOn = DateTime.Now;

                sqlProductVariant.SubscriptionIntervalType = 0;

                sqlProductVariant.CustomerEntersPrice = 0;

                sqlProductVariant.CustomerEntersPricePrompt = null;

                sqlProductVariant.SEAltText = null;

                #endregion
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sqlProductVariant.ExtensionData4))
                    variantExtData4 = new ExtensionData4();
                else
                    variantExtData4 = ExtensionData4.Deserialize(sqlProductVariant.ExtensionData4);
            }

            sqlProductVariant.IsDefault = storeProductVariant.IsDefault ? 1 : 0;

            if (typeof(T) == typeof(InsideRugsStore) || typeof(T) == typeof(InsideAvenueStore))
            {
                // for rugs, the Name column has the actual description, not Each, Roll, etc.
                // Unit of measure for rugs is always each.
                sqlProductVariant.Name = storeProductVariant.Description;
            }
            else
            {
                sqlProductVariant.Name = storeProductVariant.UnitOfMeasure.DescriptionAttr();
            }

            sqlProductVariant.Published = storeProductVariant.IsPublished ? (byte)1 : (byte)0;

            // suffix - for swatch, must be "-Swatch", for core fabric/wallcovering product, must be empty string
            // for rugs, would be the component of the SKU representing the size/shape variant

            sqlProductVariant.SKUSuffix = storeProductVariant.SKUSuffix;
            sqlProductVariant.MinimumQuantity = storeProductVariant.MinimumQuantity;

            // note that when stored proc ProductInfo2 loads up for product page, it is named OrderIncrement
            sqlProductVariant.ExtensionData = storeProductVariant.OrderIncrementQuantity.ToString();

            // note that when stored proc ProductInfo2 loads up for product page, it is named OrderRequirementsNotice
            sqlProductVariant.ExtensionData2 = storeProductVariant.OrderRequirementsNotice;

            variantExtData4.Data[ExtensionData4.OriginalRawProperties] = storeProductVariant.PublicProperties;
            variantExtData4.Data[ExtensionData4.PrivateProperties] = storeProductVariant.PrivateProperties;

            if (typeof(T) == typeof(InsideRugsStore))
                variantExtData4.Data[ExtensionData4.RugProductVariantFeatures] = storeProductVariant.RugFeatures;

            sqlProductVariant.ExtensionData4 = variantExtData4.Serialize();

            sqlProductVariant.ManufacturerPartNumber = storeProductVariant.ManufacturerPartNumber;

            sqlProductVariant.DisplayOrder = storeProductVariant.DisplayOrder; // for fabric wallpaper, 1 for main product, 2 for swatch, else incrementing for each variant starting at 1

            sqlProductVariant.FreeShipping = storeProductVariant.IsFreeShipping ? (byte)1 : (byte)0; // 0 for product, 1 for swatch

            sqlProductVariant.Cost = storeProductVariant.Cost;

            sqlProductVariant.MSRP = storeProductVariant.RetailPrice;

            sqlProductVariant.Price = storeProductVariant.OurPrice;

            sqlProductVariant.SalePrice = storeProductVariant.SalePrice;

            sqlProductVariant.Inventory = storeProductVariant.InStock ? 999999 : 0;

            // for rugs, description is same as pv.Name
            // for wallpaper\fabric, description is null
            sqlProductVariant.Description = storeProductVariant.Description;

            if (typeof (T) == typeof (InsideAvenueStore))
            {
                sqlProductVariant.Description = null;
            }

            if (typeof(T) == typeof(InsideRugsStore))
            {
                sqlProductVariant.Dimensions = storeProductVariant.Shape.DescriptionAttr();
            }
            else
                sqlProductVariant.Dimensions = null;

            return sqlProductVariant;
        }
        #endregion

        #region UpdateProduct
        /// <summary>
        /// Full refresh of product, including variants.
        /// </summary>
        /// <remarks>
        /// ProductID and VariantIDs remain the same - but everything else (except typically SKU) is up for grabs.
        /// </remarks>
        /// <param name="product"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult UpdateProduct(StoreProduct storeProduct)
        {
            try
            {
                // gets populated with changes object, else remains null, meaning no changes record associated with this product.
                // will always be null when not InsideAvenue since other stores do not support editing
                HumanHomewareProductFeatures humanChanges = null;

                using (var db = new StoreContext(_connectionStringName))
                {
                    using (var scope = new TransactionScope())
                    {
                        if (!storeProduct.ProductID.HasValue)
                            return StoreDatabaseUpdateResult.InvalidData;

                        // must all be for the same, and correct productID
                        //Debug.Assert(storeProduct.ProductVariants.All(e => e.ProductID == storeProduct.ProductID));

                        //if (!storeProduct.ProductVariants.All(e => e.ProductID == storeProduct.ProductID))
                        //return StoreDatabaseUpdateResult.InvalidData;

                        var sqlProduct = db.Product.Single(e => e.ProductID == storeProduct.ProductID);

                        if (sqlProduct == null)
                            return StoreDatabaseUpdateResult.NotFound;

                        Func<HumanHomewareProductFeatures> getHumanChanges = () =>
                        {
                            // called only for inside avenue

                            var extData = ExtensionData4.Deserialize(sqlProduct.ExtensionData4);

                            object obj;
                            if (extData.Data.TryGetValue(ExtensionData4.HumanHomewareProductFeatures, out obj))
                            {
                                var f = obj as HumanHomewareProductFeatures;
                                return f;
                            }

                            return null;
                        };


                        if (typeof(T) == typeof(InsideAvenueStore))
                        {
                            humanChanges = getHumanChanges(); // will be null when no changes on file
                        }

                        // will throw if some error
                        PopulateSqlProduct(db, false, sqlProduct, storeProduct);


                        db.SaveChanges();

                        // there is an edge condition whereby the new scan yielded nothing for images, yet we had some before.
                        // the logic pattern for update ensures we do not overwrite the existing image with an empty collection.

                        if (storeProduct.ProductImages.Any())
                            db.EnqueueProductForImageProcessing(sqlProduct.ProductID);

                        if (storeProduct.IsClearance)
                            db.AddProductCategoryAssociation(sqlProduct.ProductID, ClearanceCategoryID);
                        else
                            db.RemoveProductCategoryAssociation(sqlProduct.ProductID, ClearanceCategoryID);

                        // on variants, this set of provided records is the "truth" set at this point, so 
                        // need to account for add/remove/update - with the assumption that updates are done
                        // on the same variantID as currently in SQL.

                        var existingVariantIDs = db.ProductVariant.Where(e => e.ProductID == storeProduct.ProductID).Select(e => e.VariantID).ToList();

                        // handle removes - note that this also removes variants in SQL presently marked as deleted if not found in the presently-supplied list
                        foreach (var existingVariantID in existingVariantIDs)
                        {
                            // if existing, but not in provided set, then to be deleted
                            if (storeProduct.ProductVariants.Where(e => e.VariantID.HasValue && e.VariantID.Value == existingVariantID).Count() == 0)
                            {
                                db.RemoveProductVariantFromShoppingCartAndBookmarks(existingVariantID);
                                db.RemoveProductVariant(existingVariantID);
                            }
                        }

                        // handle adds - any variant record without an already-associated variantID
                        foreach (var productVariant in storeProduct.ProductVariants.Where(e => !e.VariantID.HasValue))
                        {
                            // will throw if some error
                            var pv = PopulateSqlProductVariant(true, sqlProduct.ProductID, new ProductVariant(), productVariant, sqlProduct.CreatedOn);
                            db.ProductVariant.Add(pv);
                            db.SaveChanges();
                        }

                        // handle updates - any variant record which has an associatedID
                        foreach (var productVariant in storeProduct.ProductVariants.Where(e => e.VariantID.HasValue))
                        {
                            // will throw if some error
                            var sqlProductVariant = db.ProductVariant.Single(e => e.VariantID == productVariant.VariantID);
                            if (sqlProductVariant == null)
                                return StoreDatabaseUpdateResult.NotFound;

                            PopulateSqlProductVariant(false, sqlProduct.ProductID, sqlProductVariant, productVariant, sqlProduct.CreatedOn);
                            db.SaveChanges();
                        }

                        db.UpdateProductLabels(sqlProduct.ProductID, storeProduct.PublicProperties);

                        scope.Complete();
                    }

                    var p = db.Product.Single(e => e.ProductID == storeProduct.ProductID);
                    AssignProductCategoryAssociations(db, p.ProductID, storeProduct, humanChanges);
                    return StoreDatabaseUpdateResult.Success;
                }
            }
            catch (StoreDatabaseUpdateResultException Ex)
            {
                return Ex.Result;
            }
#if DEBUG
            catch (DbEntityValidationException Ex)
            {
                Debug.WriteLine(Ex.Message);
                foreach (var err in Ex.EntityValidationErrors)
                {
                    foreach (var err2 in err.ValidationErrors)
                    {
                        Debug.WriteLine(err2.ErrorMessage);
                    }
                }
                return StoreDatabaseUpdateResult.Error;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                if (Ex.InnerException != null)
                {
                    Debug.WriteLine(Ex.InnerException.Message);
                    if (Ex.InnerException.InnerException != null)
                    {
                        Debug.WriteLine(Ex.InnerException.InnerException.Message);
                    }
                }
                return StoreDatabaseUpdateResult.Error;
            }
#else
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
#endif
        }
        #endregion

        #region AddProductVariant
        /// <summary>
        /// Add another variant to an existing product.
        /// </summary>
        /// <remarks>
        /// Variant must use same unit of measure as parent.
        /// </remarks>
        /// <param name="productId"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult AddProductVariant(int productId, StoreProductVariant variant)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    if (variant == null)
                        return StoreDatabaseUpdateResult.InvalidData;
#if DEBUG
                    Debug.Assert(!variant.IsDefault); // our concepts don't presently call for adding a default variant - should already exist

                    if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                    {
                        // for fabric/wallpaper, only swatches are allowed to be added
                        Debug.Assert(variant.UnitOfMeasure == UnitOfMeasure.Swatch);
                    }
#endif

                    var productFound = db.Product.Count(e => e.ProductID == productId) == 1;

                    if (!productFound)
                        return StoreDatabaseUpdateResult.NotFound;

                    if (variant.IsDefault)
                        return StoreDatabaseUpdateResult.NotAllowed;

                    // make sure not duplicate SKU
                    var isDuplicate = db.ProductVariant.Count(e => e.ProductID == productId && e.SKUSuffix == variant.SKUSuffix) > 0;
                    if (isDuplicate)
                        return StoreDatabaseUpdateResult.Duplicate;

                    if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                    {
                        // for fabric wallpaper, since this is a swatch (since that's all that is allowed here)
                        // we need to ensure that the MPN matches the companion core product

                        var companionVariants = (from pv2 in db.ProductVariant
                                                 where pv2.ProductID == productId
                                                 select new
                                                 {
                                                     pv2.ManufacturerPartNumber,
                                                     pv2.IsDefault
                                                 }).ToList();

                        // make sure is same MPN as core/parent product since that is required of any swatch
                        if (companionVariants.Where(e => e.IsDefault == 1 && e.ManufacturerPartNumber == variant.ManufacturerPartNumber).Count() == 0)
                            return StoreDatabaseUpdateResult.InvalidData;

                        // cannot have another variant with this MPN for same productID

                        if (companionVariants.Where(e => e.IsDefault == 0 && e.ManufacturerPartNumber == variant.ManufacturerPartNumber).Count() > 0)
                            return StoreDatabaseUpdateResult.Duplicate;
                    }
                    else
                    {
                        // TODO: this else clause has not be tested out

                        // make sure not duplicate MPN, taking into account that some vendors are part of a group

                        var manufacturerIDs = new List<int>();

                        // TODO: Shane - need some better way to come up with list of manufacturers in same group
                        manufacturerIDs = (from pv3 in db.ProductVariant
                                           where pv3.ProductID == productId
                                           join pm3 in db.ProductManufacturer on pv3.ProductID equals pm3.ProductID
                                           select pm3.ManufacturerID).Distinct().ToList();

                        var isAnotherSameMPN = (from pv3 in db.ProductVariant
                                                where pv3.ManufacturerPartNumber == variant.ManufacturerPartNumber
                                                join pm in db.ProductManufacturer on pv3.ProductID equals pm.ProductID
                                                where manufacturerIDs.Contains(pm.ManufacturerID)
                                                select pv3.VariantID
                                                   ).Count() > 0;

                        if (isAnotherSameMPN)
                            return StoreDatabaseUpdateResult.Duplicate;
                    }

                    var pv = PopulateSqlProductVariant(true, productId, new ProductVariant(), variant, DateTime.Now);

                    db.ProductVariant.Add(pv);
                    db.SaveChanges();

                    return StoreDatabaseUpdateResult.Success;
                }
            }
            catch (StoreDatabaseUpdateResultException Ex)
            {
                return Ex.Result;
            }
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
        }


        #endregion

        #region UpdateProductVariantPrice
        /// <summary>
        /// Update the cost/price for an existing variant.
        /// </summary>
        /// <remarks>
        /// Also adjusts clearance status where needed.
        /// </remarks>
        /// <param name="pricing"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult UpdateProductVariantPrice(VariantPriceChange pricing)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
#if DEBUG
                    Debug.Assert(pricing.OurPrice > pricing.Cost);
                    Debug.Assert(pricing.RetailPrice >= pricing.OurPrice);
#endif
                    var found = db.UpdateProductVariantPricing(pricing.VariantId, pricing.Cost, pricing.RetailPrice, pricing.OurPrice, pricing.SalePrice);

                    // see if need to deal with clearance

                    if (found && pricing.IsClearance.HasValue)
                    {
                        // get associated productID
                        var productID = db.ProductVariant.Where(e => e.VariantID == pricing.VariantId).Select(e => e.ProductID).Single();

                        if (pricing.IsClearance.Value)
                        {
                            // must add to clearance
                            db.AddProductCategoryAssociation(productID, ClearanceCategoryID);
                        }
                        else
                        {
                            // must remove from clearance
                            db.RemoveProductCategoryAssociation(productID, ClearanceCategoryID);
                        }
                    }

                    return found ? StoreDatabaseUpdateResult.Success : StoreDatabaseUpdateResult.NotFound;
                }
            }
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
        }
        #endregion

        #region UpdateProductImages
        /// <summary>
        /// Full refresh of images associated with an existing product.
        /// </summary>
        /// <remarks>
        /// The image collection here completely replaces the existing collection. No merge.
        /// If an empty collection is provided, then the product will then have none! For now,
        /// until found to be needed, don't allow 0 images.
        /// </remarks>
        /// <param name="productID"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult UpdateProductImages(int productID, List<ProductImage> images)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {

                    Debug.Assert(images != null && images.Count() > 0);

                    if (images == null || images.Count() == 0)
                        return StoreDatabaseUpdateResult.InvalidData;

#if DEBUG
                    if (images.Count > 0)
                    {
                        // exactly one must be designated as default
                        Debug.Assert(images.Where(e => e.IsDefault).Count() == 1);

                        if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                        {
                            Debug.Assert(images.Where(e => e.ImageVariant == "Primary").Count() == 1);
                        }

                        foreach (var img in images)
                        {
                            Debug.Assert(!string.IsNullOrWhiteSpace(img.Filename)); // must designate a filename
                            Debug.Assert(!string.IsNullOrWhiteSpace(img.SourceUrl)); // must say where the image comes from
                        }
                    }
#endif

                    var extensionData = db.Product.Where(e => e.ProductID == productID).Select(e => e.ExtensionData4).SingleOrDefault();

                    if (extensionData == null)
                        return StoreDatabaseUpdateResult.NotFound;

                    var extData4 = ExtensionData4.Deserialize(extensionData);

                    // replace the existing image collection with this new one
                    extData4.Data[ExtensionData4.ProductImages] = images;

                    var json = extData4.Serialize();

                    using (var scope = new TransactionScope())
                    {
                        // persist updated json data to disk, enque for image processing
                        db.UpdateProductExtensionData4(productID, json);
                        db.EnqueueProductForImageProcessing(productID);

                        scope.Complete();
                        return StoreDatabaseUpdateResult.Success;
                    }
                }
            }
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
        }

        #endregion

        #region MarkProductDiscontinued
        /// <summary>
        /// Perform all business logic associated with marking a productID as discontinued.
        /// </summary>
        /// <remarks>
        /// All variants associated with product are no longer for sale. Remove from clearance too.
        /// </remarks>
        /// <param name="productID"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult MarkProductDiscontinued(int productID, int vendorId)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    if (db.Product.Count(x => x.ProductID == productID) == 0)
                        return StoreDatabaseUpdateResult.NotFound;

                    using (var scope = new TransactionScope())
                    {
                        db.RemoveProductFromShoppingCart(productID);

                        // Innovations: Tessa would like to never show a discontinued (p.showbuybutton=0) status, and instead, simply just show 0 stock.
                        if (vendorId != 107) db.UpdateProductShowBuyButton(productID, false);
                        db.MarkAllVariantsForProductOutOfStock(productID);
                        // discontinued products cannot be in outlet section
                        db.RemoveProductFromCategory(productID, ClearanceCategoryID);

                        scope.Complete();
                        return StoreDatabaseUpdateResult.Success;
                    }
                }
            }
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
        }
        #endregion

        #region UpdateProductVariantInventoryCount
        /// <summary>
        /// Set the count of items in stock for the given variant.
        /// </summary>
        /// <remarks>
        /// For products such as fabric/wallpaper where there is a swatch, separate calls to this
        /// method are required for both variants.
        /// If a default variant for a discontinued product is marked in stock, then the product then becomes not discontinued.
        /// </remarks>
        /// <param name="variantId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult UpdateProductVariantInventory(int variantId, InventoryStatus status)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    var isSuccess = db.UpdateProductVariantInventoryCount(variantId, status == InventoryStatus.InStock ? 999999 : 0);

                    if (isSuccess && status == InventoryStatus.InStock)
                    {
                        // if corresponding product happened to be discontinued and this variantID is the default variant,
                        // then mark product as now being not discontinued

                        var productInfo = (from pv in db.ProductVariant
                                           where pv.VariantID == variantId && pv.IsDefault == 1
                                           select new
                                           {
                                               pv.ProductID,
                                               IsDicontinued = pv.Product.ShowBuyButton == 0
                                           }).FirstOrDefault();

                        if (productInfo != null && productInfo.IsDicontinued)
                            db.UpdateProductShowBuyButton(productInfo.ProductID, true);
                    }

                    // if out of stock and is outlet product, must remove from outlet (if happens to be there)
                    if (isSuccess && status == InventoryStatus.OutOfStock)
                        db.RemoveProductCategoryAssociationByVariantID(variantId, ClearanceCategoryID);

                    return isSuccess ? StoreDatabaseUpdateResult.Success : StoreDatabaseUpdateResult.NotFound;
                }
            }
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
        }
        #endregion

        #region UpdateProductWithSwatchInventory
        public StoreDatabaseUpdateResult UpdateProductWithSwatchInventory(int productId, InventoryStatus status)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    var isSuccess = db.UpdateProductWithSwatchInventoryCount(productId, status == InventoryStatus.InStock ? 999999 : 0);

                    if (isSuccess && status == InventoryStatus.InStock)
                    {
                        // if corresponding product happened to be discontinued then mark product as now being not discontinued

                        if (db.Product.Count(e => e.ProductID == productId && e.ShowBuyButton == 0) > 0)
                            db.UpdateProductShowBuyButton(productId, true);
                    }

                    return isSuccess ? StoreDatabaseUpdateResult.Success : StoreDatabaseUpdateResult.NotFound;
                }
            }
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
        }

        #endregion

        #region RemoveProductVariant
        /// <summary>
        /// Remove (delete) and existing product variant.
        /// </summary>
        /// The corresponding row will be deleted from SQL. Frequently used for clearance fabric/wallpaper
        /// when we no longer allow swatches to be purchased.
        /// <param name="variantId"></param>
        /// <returns></returns>
        public StoreDatabaseUpdateResult RemoveProductVariant(int variantId)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    // if is a default variant, if fabric/wallpaper, something is wrong. If rugs, need to make another the default

                    var info = db.ProductVariant.Where(x => x.VariantID == variantId && x.Deleted == 0).Select(e => new { e.ProductID, e.IsDefault }).SingleOrDefault();

                    if (info == null)
                        return StoreDatabaseUpdateResult.NotFound;

                    using (var scope = new TransactionScope())
                    {
                        if (info.IsDefault == 1)
                        {

                            if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                            {
                                // it is illegal to delete the default variant for fabric/wallpaper since that 
                                // would be the core product - hence, what is needed is to discontinue the ProductID

                                Debug.Assert(info.IsDefault != 1);
                                return StoreDatabaseUpdateResult.NotAllowed;
                            }

                            // make sure not the last variant for this product

                            var variantCount = db.ProductVariant.Count(e => e.ProductID == info.ProductID && e.Published == 1 && e.Deleted == 0);
                            if (variantCount <= 1)
                                return StoreDatabaseUpdateResult.NotAllowed;

                            // we now know we're removing the default variant, but there are others, so need to 
                            // make one of the others the default, which we'll do using display order, lowest wins

                            var newDefaultVariantID = db.ProductVariant
                                .Where(e => e.ProductID == info.ProductID && e.IsDefault == 0 && e.Published == 1 && e.Deleted == 0)
                                .OrderBy(e => e.DisplayOrder).Select(e => e.VariantID).First();

                            db.ChangeDefaultProductVariant(variantId, newDefaultVariantID);
                        }

                        // remove from bookmarks and shopping cart since no longer in our system
                        db.RemoveProductVariantFromShoppingCartAndBookmarks(variantId);

                        var isSuccess = db.RemoveProductVariant(variantId);

                        scope.Complete();
                        return isSuccess ? StoreDatabaseUpdateResult.Success : StoreDatabaseUpdateResult.NotFound;
                    }
                }
            }
            catch
            {
                return StoreDatabaseUpdateResult.Error;
            }
        }
        #endregion

        #region GetStockCheckInfoAsync
        /// <summary>
        /// Returns information for the set of variants which is used by stock check API logic
        /// to determine the appropriate actions to take when a query comes in.
        /// </summary>
        /// <param name="variantIds"></param>
        /// <returns></returns>
        public async Task<List<StockQueryResult>> GetStockCheckInfoAsync(IEnumerable<int> variantIds)
        {
            //var extData = ExtensionData4.Deserialize(p.ExtensionData4);
            //var extDic = extData.Data;
            //storeProduct.PublicProperties = extDic[ExtensionData4.OriginalRawProperties] as Dictionary<string, string>;
            using (var db = new StoreContext(_connectionStringName))
            {
                var stockCheckInfo = await (from pv in db.ProductVariant
                                            where variantIds.Contains(pv.VariantID)
                                            select new StockQueryResult
                                            {
                                                VendorId = pv.Product.ProductManufacturer.FirstOrDefault().ManufacturerID,
                                                MPN = pv.ManufacturerPartNumber,
                                                VariantId = pv.VariantID,
                                                CurrentStock = pv.Inventory,
                                                IsDiscontinued = pv.Product.ShowBuyButton == 0,
                                                ProductID = pv.ProductID,
                                                HasSwatch = db.ProductVariant.Count(x => x.ProductID == pv.ProductID && x.Name == "Swatch") > 0,
                                                ProductGroup = pv.Product.ProductGroup,
                                                ExtensionData4 = pv.Product.ExtensionData4
                                            }
                            ).ToListAsync();
                stockCheckInfo.ForEach(x => x.PublicProperties = ExtensionData4.Deserialize(x.ExtensionData4).Data[ExtensionData4.OriginalRawProperties] as Dictionary<string, string>);
                return stockCheckInfo;
            }
        }

        #endregion

        #region GetProductSupplementalDataAsync
        /// <summary>
        /// Returns a set of extended product data for the specified set of productIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 products per request. 
        /// Should return list ordered by ProductID.
        /// </remarks>
        public async Task<List<ProductSupplementalData>> GetProductSupplementalDataAsync(List<int> productIds)
        {
            var store = new T();
            string imageUrlPrefix = store.Url + "/images/product/medium/";

            using (var db = new StoreContext(_connectionStringName))
            {
                var products = await (from p in db.Product
                                      where productIds.Contains(p.ProductID)
                                      orderby p.ProductID
                                      select new ProductSupplementalData()
                                      {
                                          Name = p.Name,
                                          ProductGroup = p.ProductGroup,
                                          ProductID = p.ProductID,
                                          SKU = p.SKU,
                                          ImageUrl = p.ImageFilenameOverride == null ? null : imageUrlPrefix + p.ImageFilenameOverride,
                                          VendorUrl = p.RelatedDocuments,
                                          StoreUrl = store.Url + "/p-" + p.ProductID + "-" + p.SEName + ".aspx"
                                      }).ToListAsync();

                return products;
            }
        }
        #endregion

        #region GetVariantSupplementalDataAsync
        /// <summary>
        /// Returns a set of extended product variant data for the specified set of variantIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 variants per request. 
        /// Should return list ordered by VariantID.
        /// </remarks>
        public async Task<List<VariantSupplementalData>> GetVariantSupplementalDataAsync(List<int> variantIds)
        {
            var store = new T();

            string imageUrlPrefix = store.Url + "/images/product/medium/";

            using (var db = new StoreContext(_connectionStringName))
            {

                List<VariantSupplementalData> variants;

                if (typeof(T) == typeof(InsideFabricStore) || typeof(T) == typeof(InsideWallpaperStore))
                {
                    // TODO: fabric/wallpaper sites presently use pv.Name as a unit of measure property (this should be changed to have its own true column)

                    variants = await (from pv in db.ProductVariant
                                      where variantIds.Contains(pv.VariantID)
                                      orderby pv.VariantID
                                      select new VariantSupplementalData()
                                      {
                                          Name = pv.Product.Name,
                                          ProductGroup = pv.Product.ProductGroup,
                                          ProductID = pv.ProductID,
                                          SKU = pv.Product.SKU + pv.SKUSuffix ?? "",
                                          ImageUrl = pv.Product.ImageFilenameOverride == null ? null : imageUrlPrefix + pv.Product.ImageFilenameOverride,
                                          VendorUrl = pv.Product.RelatedDocuments,
                                          StoreUrl = store.Url + "/p-" + pv.ProductID + "-" + pv.Product.SEName + ".aspx",
                                          UnitOfMeasure = pv.Name,
                                          VariantID = pv.VariantID
                                      }).ToListAsync();
                }
                else
                {
                    // rugs, homeware
                    // unit of measure is "each" (for now)
                    variants = await (from pv in db.ProductVariant
                                      where variantIds.Contains(pv.VariantID)
                                      orderby pv.VariantID
                                      select new VariantSupplementalData()
                                      {
                                          Name = pv.Product.Name,
                                          ProductGroup = pv.Product.ProductGroup,
                                          ProductID = pv.ProductID,
                                          SKU = pv.Product.SKU + pv.SKUSuffix ?? "",
                                          ImageUrl = pv.Product.ImageFilenameOverride == null ? null : imageUrlPrefix + pv.Product.ImageFilenameOverride,
                                          VendorUrl = pv.Product.RelatedDocuments,
                                          StoreUrl = store.Url + "/p-" + pv.ProductID + "-" + pv.Product.SEName + ".aspx",
                                          UnitOfMeasure = "Each",
                                          VariantID = pv.VariantID
                                      }).ToListAsync();
                }
                return variants;
            }
        }

        #endregion

        #region DoesStoreExistAsync
        /// <summary>
        /// Determine if the store database exists at all.
        /// </summary>
        /// <remarks>
        /// See if the manufacture table has anything - will throw exception if missing.
        /// </remarks>
        /// <returns></returns>
        public async Task<bool> DoesStoreExistAsync()
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    var count = db.Manufacturer.Select(e => e.ManufacturerID).Count();
                    await Task.Delay(1);
                    return count > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region DoesVendorExistAsync
        /// <summary>
        /// Determine if the given vendor has an entry in the Manufacturer table.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public async Task<bool> DoesVendorExistAsync(int vendorId)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    await Task.Delay(1);
                    return db.Manufacturer.Where(x => x.ManufacturerID == vendorId).Count() == 1;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region GetProductCountMetricsAsync
        /// <summary>
        /// Fetch a complete set of product/variant metrics for the given vendor.
        /// </summary>
        /// <remarks>
        /// Product stock in/out counts are based on the default variant.
        /// It is up to the caller to know if working with a product centric or variant centric
        /// vendor - which determines which set of counts and related math makes sense.
        /// Correctly ignores deleted products, but does not attempt to isolate deleted variants.
        /// </remarks>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public async Task<ProductCountMetrics> GetProductCountMetricsAsync(int vendorId)
        {
            try
            {
                using (var db = new StoreContext(_connectionStringName))
                {
                    var metrics = (from m in db.Manufacturer
                                   where m.ManufacturerID == vendorId
                                   let rawProductIds = db.ProductManufacturer.Where(e => e.ManufacturerID == vendorId).Select(e => e.ProductID)
                                   let productIDs = db.Product.Where(x => x.Deleted == 0 && rawProductIds.Contains(x.ProductID)).Select(x => x.ProductID)
                                   let pCount = productIDs.Count()
                                   let pDiscontinuedCount = db.Product.Where(e => e.ShowBuyButton == 0 && productIDs.Contains(e.ProductID)).Count()
                                   let pvCount = db.ProductVariant.Where(e => productIDs.Contains(e.ProductID)).Count()
                                   let clearanceCount = db.ProductCategory.Where(e => e.CategoryID == ClearanceCategoryID && productIDs.Contains(e.ProductID)).Count()
                                   let inStockProductCount = db.ProductVariant.Where(e => e.Inventory > 0 && e.IsDefault == 1 && productIDs.Contains(e.ProductID)).Count()
                                   let outStockProductCount = db.ProductVariant.Where(e => e.Inventory < 1 && e.IsDefault == 1 && productIDs.Contains(e.ProductID)).Count()
                                   let inStockVariantCount = db.ProductVariant.Where(e => e.Inventory > 0 && productIDs.Contains(e.ProductID)).Count()
                                   let outStockVariantCount = db.ProductVariant.Where(e => e.Inventory < 1 && productIDs.Contains(e.ProductID)).Count()
                                   select new ProductCountMetrics()
                                   {
                                       ProductCount = pCount,
                                       ProductVariantCount = pvCount,
                                       DiscontinuedProductCount = pDiscontinuedCount,
                                       ClearanceProductCount = clearanceCount,
                                       InStockProductCount = inStockProductCount,
                                       OutOfStockProductCount = outStockProductCount,
                                       InStockProductVariantCount = inStockVariantCount,
                                       OutOfStockProductVariantCount = outStockVariantCount,
                                   }).Single();

                    await Task.Delay(1);
                    return metrics;
                }

            }
            catch (Exception Ex)
            {
                // was getting exceptions here on live server until changed MaxDOP (maximum degree of parallelism) setting to 6
                // which was the recommended setting using a script which was available.

                Debug.WriteLine(Ex.Message);
                WriteEventLog(Ex);
                return new ProductCountMetrics()
                {
                    ProductCount = 0,
                    ProductVariantCount = 0,
                    DiscontinuedProductCount = 0,
                    ClearanceProductCount = 0,
                    InStockProductCount = 0,
                    OutOfStockProductCount = 0,
                    InStockProductVariantCount = 0,
                    OutOfStockProductVariantCount = 0,
                };

            }
        }
        #endregion

        #region GetProductSKUsAsync
        public async Task<List<string>> GetProductSKUsAsync(int vendorId)        /// <summary>
        /// Return a list of SKUs from only the Product record - not taking any variant component of the true SKUs into account.
        /// </summary>
        /// <remarks>
        /// For fabric/wallpaper, this would be the non-swatch SKU, but for something like rugs, it would only be the 
        /// first half of the SKU, so would not be all that meaningful.
        /// </remarks>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        {
            using (var db = new StoreContext(_connectionStringName))
            {
                return await db.Product.Where(x => x.ProductManufacturer.FirstOrDefault().ManufacturerID == vendorId).Select(x => x.SKU).ToListAsync();
            }
        }
        #endregion

        #region GetVariantIds
        // just used to run some tests through the stock checker
        public async Task<List<int>> GetVariantIds(int vendorId)
        {
            using (var db = new StoreContext(_connectionStringName))
            {
                return await db.Product.Where(x => x.ProductManufacturer.FirstOrDefault().ManufacturerID == vendorId && x.ShowBuyButton == 1)
                    .Select(x => x.ProductVariants.FirstOrDefault(v => v.IsDefault == 1)).Select(v => v.VariantID).ToListAsync();
            }
        }
        #endregion

        #region Helper Methods
        private int? FindCategoryForDesigner(string designer)
        {
            if (string.IsNullOrWhiteSpace(designer))
                return null;

            var dicDesigners = GetDesignerLookupTable();

            int designerCategoryID;
            if (dicDesigners.TryGetValue(designer, out designerCategoryID))
                return designerCategoryID;

            return null;
        }

        private Dictionary<string, int> GetDesignerLookupTable()
        {
            using (var db = new StoreContext(_connectionStringName))
            {
                var sqlCategories = db.Category.Where(e => e.Deleted == 0 && e.ParentCategoryID == DesignerParentCategoryID).Select(e => new { CategoryID = e.CategoryID, Name = e.Name }).ToList();

                // create a lookup combining categories in SQL with some fixed ones

                Func<string, string> cleanUp = (s) =>
                {
                    // remove Fabric from tail if exists

                    var badWords = new string[] { " Fabric", " Fabrics", " Wallpaper", "-Children Fabric" };

                    foreach (var word in badWords)
                    {
                        if (s.EndsWith(word))
                        {
                            return s.Left(s.Length - word.Length).Trim();
                        }
                    }
                    return s.Trim();
                };

                var dicDesigners = new Dictionary<string, int>()
                                {
                                    // this is the KR/LJ list - which we don't have true categories for all yet

                                    //{ "Aerin Lauder", 0 },
                                    { "Alexa Hampton", 175 },
                                    { "Barbara Barry", 163 },
                                    { "Barclay Butera", 171 },
                                    //{ "Calvin Klein Home", 0 },
                                    { "Candice Olson", 158 },
                                    //{ "David Easton Design", 0 },
                                    { "David Hicks", 211 },
                                    //{ "Diamond Baratta Design", 0 },
                                    //{ "Echo Home", 0 },
                                    //{ "Eric Cohler", 0 },
                                    { "Jonathan Adler", 190 },
                                    //{ "Joseph Abboud", 0 },
                                    { "Kelly Wearstler", 161 },
                                    { "Lilly Pulitzer", 212 },
                                    { "Michael Berman", 174 },
                                    { "Michael Weiss Design", 173 },
                                    //{ "Museum of New Mexico", 0 },
                                    { "Oscar de la Renta", 169 },
                                    //{ "Pierre Deux", 0 },
                                    //{ "Plaza", 0 },
                                    //{ "Royal Oak", 0 },
                                    //{ "Sarah Richardson Design", 0 },
                                    { "Suzanne Kasler", 168 },
                                    { "Suzanne Rheinstein", 170 },
                                    { "Thom Filicia", 172 },
                                    { "Thomas O'Brien", 7 },
                                    //{ "Threads", 0 },
                                    //{ "Waterworks", 0 },
                                    //{ "Wesley Mancini", 0 },
                                    { "Windsor Smith", 164 },
                                    //{ "Winterthur", 0 }
                                };

                // merge in the ones from SQL

                foreach (var sqlCat in sqlCategories.Where(e => !string.IsNullOrWhiteSpace(e.Name)))
                    dicDesigners[cleanUp(sqlCat.Name)] = sqlCat.CategoryID;

                return dicDesigners;
            }
        }
        #endregion

        public async Task<Dictionary<int, int>> GetAssociatedProductIds(List<int> variantIds)
        {
            using (var db = new StoreContext(_connectionStringName))
            {
                var matchingVariants = await db.ProductVariant.Where(x => variantIds.Contains(x.VariantID))
                    .ToDictionaryAsync(k => k.VariantID, v => v.ProductID);
                return matchingVariants;
            }
        }
    }
}
