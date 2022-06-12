using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using ColorMine.ColorSpaces;
using InsideFabric.Data;
using InsideStores.Imaging;
using Newtonsoft.Json;

namespace Website
{
    public class ProductDataCache : IProductDataCache
    {


        #region Internal Classes
        public class SqlProduct
        {
            public int ProductID { get; set; }
            public string SKU { get; set; }
            public string Name { get; set; }
            public string ImageFilename { get; set; }
            public string SEName { get; set; }
            public decimal RetailPrice { get; set; }
            public decimal OurPrice { get; set; }
            public decimal? SalePrice { get; set; }
            public decimal LowVariantRetailPrice { get; set; }
            public decimal LowVariantOurPrice { get; set; }
            public decimal HighVariantRetailPrice { get; set; }
            public decimal HighVariantOurPrice { get; set; }
            public int ShowBuyButton { get; set; }
            public string MiscText { get; set; }
            public string StockStatus { get; set; }
            public DateTime Created { get; set; }
            public string Correlator { get; set; }
            public string ProductGroup { get; set; }
            public int ManufacturerID { get; set; }
        }

        public class SqlProductExtension
        {
            public int ProductID { get; set; }
            public string Colors { get; set; }  // #F0000;#00FF00;#0000FF
            public string Shapes { get; set; }  // Rectangle=3;Round=2;Oval=1;Runner=8
            public byte[] ImageDescriptor { get; set; } // 144 bytes
            public int TinyImageDescriptor { get; set; }

            public string Similar { get; set; }  // 00000,11111,22222
            public string SimilarColors { get; set; }  // 00000,11111,22222
            public string SimilarStyle { get; set; }  // 00000,11111,22222

        }
        
        #endregion

        #region Locals
        protected object lockObject = new object();
        protected string connectionString { get; set; }
        protected StoreKeys storeKey { get; set; }

        #endregion

        // this is the core set of in-memory data we need per store
        // to drive the fast lookups

        #region Properties
        public Dictionary<int, CacheProduct> Products { get; protected set; }
        public Dictionary<int, List<int>> Categories { get; protected set; }
        public Dictionary<int, List<int>> Manufacturers { get; protected set; }
        public Dictionary<string, List<int>> SortedCategories { get; protected set; }
        public Dictionary<string, List<int>> SortedManufacturers { get; protected set; }
        public Dictionary<int, List<int>> ChildCategories { get; protected set; }
        public Dictionary<int, ManufacturerInformation> ManufacturerInfo { get; protected set; }

        // lookup dic to get a productID from a SKU
        // requires image search or filtered search to get real data, else null
        public Dictionary<string, int> SkuToProductID { get; protected set; }

        public List<int> DiscontinuedProducts { get; protected set; }
        public List<int> MissingImagesProducts { get; protected set; }

        // dic<pkid, phrase> - for all lists
        public Dictionary<int, string> AutoSuggestPhrases { get; protected set; }

        /// <summary>
        /// ProductIDs for products which this store is offering into the pool of 
        /// cross marketed products.
        /// </summary>
        public List<int> FeaturedProducts { get; protected set; }


        /// <summary>
        /// Lookup for weight applied to each manufacturer for creating product list outputs.
        /// </summary>
        public Dictionary<int, int> ManufacturerDisplayWeights { get; private set;}

        /// <summary>
        /// Caches which category has been identified as the related one for the given category root.
        /// </summary>
        /// <remarks>
        /// Dictionary["ParentCategoryID:ProductID", CategoryID]
        /// </remarks>
        public Dictionary<string, int> RelatedCategories { get; protected set; }

        public DateTime? TimeWhenPopulationStarted { get; protected set; }
        public DateTime? TimeWhenPopulationCompleted { get; protected set; }

        public TimeSpan? TimeToPopulate
        {
            get
            {
                if (!TimeWhenPopulationStarted.HasValue || !TimeWhenPopulationCompleted.HasValue)
                    return null;

                return TimeWhenPopulationCompleted - TimeWhenPopulationStarted;
            }
        }

        /// <summary>
        /// Gets a reference to the respective store. 
        /// </summary>
        /// <remarks>
        /// The cache does not keep a hard ptr to the store. It keeps a key, 
        /// which can create the reference to the store on demand. Do not want
        /// to have a circular reference bacl to the store.
        /// </remarks>
        public IWebStore Store
        {
            get
            {
                return MvcApplication.Current.GetWebStore(storeKey);
            }
        }

        /// <summary>
        /// Actual products available for sale.
        /// </summary>
        /// <remarks>
        /// For fabric, does not include swatches. For other stores, includes
        /// variants since they are normal products.
        /// </remarks>
        public virtual int ProductsForSaleCount { get; private set; }

        public string Identity { get; private set; }


        #endregion

        #region Ctor

        public ProductDataCache()
        {
            Identity = Guid.NewGuid().ToString();
        } 

        #endregion

        #region Public Methods
        /// <summary>
        /// Find the productID to match up with a SKU.
        /// </summary>
        /// <param name="SKU"></param>
        /// <returns></returns>
        public int? LookupProductIDFromSKU(string SKU)
        {
            if (SkuToProductID == null)
                return null;

            int productID;
            if (SkuToProductID.TryGetValue(SKU, out productID))
                return productID;

            return null;
        }


        /// <summary>
        /// Return the product matching the given sku. Null if not found.
        /// </summary>
        /// <param name="SKU"></param>
        /// <returns></returns>
        public CacheProduct LookupProduct(string SKU)
        {
            var productID = LookupProductIDFromSKU(SKU);

            if (productID == null)
                return null;

            return LookupProduct(productID.Value);
        }

        /// <summary>
        /// Return the product matching the given productID. Null if not found.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public CacheProduct LookupProduct(int productID)
        {
            CacheProduct product = null;
            Products.TryGetValue(productID, out product);
            return product;
        }

        #endregion

        #region Population

        /// <summary>
        /// Main entry point to commence population.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public virtual bool Populate(StoreKeys storeKey, string connectionString)
        {
            bool result = false;
            TimeWhenPopulationStarted = DateTime.Now;
            this.connectionString = connectionString;
            this.storeKey = storeKey;

            try
            {
                ManufacturerInfo = new Dictionary<int, ManufacturerInformation>();
                Products = new Dictionary<int, CacheProduct>();
                Categories = new Dictionary<int, List<int>>();
                Manufacturers = new Dictionary<int, List<int>>();
                SortedCategories = new Dictionary<string, List<int>>();
                SortedManufacturers = new Dictionary<string, List<int>>();
                ChildCategories = new Dictionary<int, List<int>>();
                RelatedCategories = new Dictionary<string, int>();
                AutoSuggestPhrases = new Dictionary<int, string>();

                DiscontinuedProducts = new List<int>();
                MissingImagesProducts = new List<int>();

                PopulateManufacturerInfo();
                PopulateProducts();
                PopulateCategories();
                PopulateManufacturers();
                PopulateDiscontinuedProducts();
                PopulateMissingImagesProducts();
                PopulateChildCategories();
                PopulateFeaturedProducts();
                SetProductsForSale();
                PopulateAutoSuggestPhrases();

                // RelatedCategories is not populated here since it would be very
                // time consuming - it is populated along the way as needed

                TimeWhenPopulationCompleted = DateTime.Now;
                result = true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(string.Format("Population exception for {0}:\n{1}", storeKey, Ex.ToString()));

                var ev2 = new WebsiteRequestErrorEvent(string.Format("Failed to populate product data cache for {0}.", storeKey), this, WebsiteEventCode.UnhandledException, Ex);
                ev2.Raise();
            }

            return result;
        }

        protected virtual void PopulateManufacturerInfo()
        {
            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                ManufacturerInfo = dc.Manufacturers.Select(e => new ManufacturerInformation() { ManufacturerID = e.ManufacturerID, Name = e.Name, WebsiteUrl = e.URL, Url = "/m-" + e.ManufacturerID.ToString() + "-" + e.SEName + ".aspx" }).ToDictionary(k => k.ManufacturerID, v => v);

                foreach(var m in ManufacturerInfo.Values)
                {
                    var brand = m.Name;

                    // a few tweaks on the name

                    if (brand.EndsWith(" Fabrics"))
                        brand = brand.Replace(" Fabrics", "");
                    if (brand.EndsWith(" Fabric"))
                        brand = brand.Replace(" Fabric", "");
                    if (brand.EndsWith(" Wallcoverings"))
                        brand = brand.Replace(" Wallcoverings", "");
                    if (brand.EndsWith(" Wallcovering"))
                        brand = brand.Replace(" Wallcovering", "");

                    m.Name = brand;
                }


            }
        }


        protected virtual void PopulateProducts()
        {

            var dtStart = DateTime.Now;

            Products = new Dictionary<int, CacheProduct>();

            // prodcuts are New if added in last 30 days
            var createdAfter = DateTime.Now.AddDays(0 - 90);
            var skippedManufacturers = new List<int>();

            // this will see if any products have a single variant and it's a sample (we do not allow this)
            //select ProductID, (select COUNT(*) from ProductVariant pv where pv.ProductID = p.ProductID) as 'CVar' from Product p
            //where p.ProductID in (select distinct(ProductID) from ProductVariant where Dimensions = 'Sample') order by CVar

            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                dc.CommandTimeout = 10000;  

                skippedManufacturers = dc.Manufacturers.Where(e => e.Published == 0 || e.Deleted == 1).Select(e => e.ManufacturerID).ToList();
                var outletProducts = new HashSet<int>(dc.ProductCategories.Where(e => e.CategoryID == Store.OutletCategoryID).Select(e => e.ProductID).ToList());

                #region Main LINQ Query

                var dtStartMainQuery = DateTime.Now;
                var products = (from p in dc.Products where p.Deleted==0 && p.Published==1

                    join v in dc.ProductVariants on p.ProductID equals v.ProductID
                                where v.IsDefault == 1 && v.Deleted == 0 && v.Published == 1 

                    // && e.Dimensions != "Sample"
                    // IR has shapes in pv.Dimensions, and we don't want to have listing pages show possible samples for the low price
                    // IF and IW have pv.Dimensions as null
                    // IA has actual dimensions in pv.Dimensions

                    // WARNING - must always yield at least one row. If only variant is sample, then there is a problem sine no rows come back.
                    let lowVariantRetailPrice = dc.ProductVariants.Where(e => e.ProductID == p.ProductID && e.Deleted == 0 && e.Published == 1 && (e.Dimensions == null || e.Dimensions != "Sample")).Select(e => e.MSRP.GetValueOrDefault()).Min()
                    let lowVariantOurPrice = dc.ProductVariants.Where(e => e.ProductID == p.ProductID && e.Deleted == 0 && e.Published == 1 && (e.Dimensions == null || e.Dimensions != "Sample")).Select(e => (e.SalePrice ?? e.Price)).Min()

                    let highVariantRetailPrice = dc.ProductVariants.Where(e => e.ProductID == p.ProductID && e.Deleted == 0 && e.Published == 1).Select(e => e.MSRP.GetValueOrDefault()).Max()
                    let highVariantOurPrice = dc.ProductVariants.Where(e => e.ProductID == p.ProductID && e.Deleted == 0 && e.Published == 1).Select(e => (e.SalePrice ?? e.Price)).Max()

                    let manufacturerID = dc.ProductManufacturers.Where(pm => pm.ProductID == p.ProductID).Select(m => m.ManufacturerID).FirstOrDefault()

                    select new SqlProduct
                    {
                        ProductID = p.ProductID,
                        SKU = p.SKU,
                        Name = p.Name,
                        ImageFilename = p.ImageFilenameOverride,
                        SEName = p.SEName,
                        RetailPrice = v.MSRP.GetValueOrDefault(),
                        OurPrice = v.Price,
                        SalePrice = v.SalePrice,
                        LowVariantRetailPrice = lowVariantRetailPrice,
                        LowVariantOurPrice = lowVariantOurPrice,
                        HighVariantRetailPrice = highVariantRetailPrice,
                        HighVariantOurPrice = highVariantOurPrice,
                        ShowBuyButton = p.ShowBuyButton,
                        MiscText = p.MiscText,
                        StockStatus = p.ShowBuyButton == 0 ? "D" : v.Inventory == 0 ? "O" : "I", // discontinued, out of stock, in stock (parsed to enum below)
                        Created = p.CreatedOn,
                        Correlator = p.ManufacturerPartNumber,
                        ProductGroup = p.ProductGroup,
                        ManufacturerID = manufacturerID
                    }).ToList();


                Debug.WriteLine(string.Format("Main query for {0}: {1:N0}     ({2})", Store.StoreKey, products.Count, DateTime.Now - dtStartMainQuery));
                
                #endregion

                #region Secondary Query

                Dictionary<int, SqlProductExtension> productExtensions = null;

                if (Store.IsImageSearchEnabled)
                {
                    var dtStartSecondaryQuery = DateTime.Now;

                    // just grab everything since just a lookup, don't care if any were
                    // deleted since not used unless the product qualifies through the main table

                    productExtensions = (from pf in dc.ProductFeatures
                                         select new SqlProductExtension
                                         {
                                             ProductID = pf.ProductID,
                                             Colors = pf.Colors,
                                             Shapes = pf.Shapes,
                                             ImageDescriptor = pf.ImageDescriptor,
                                             TinyImageDescriptor = pf.TinyImageDescriptor.GetValueOrDefault(),
                                             Similar = pf.Similar,
                                             SimilarColors = pf.SimilarColors,
                                             SimilarStyle = pf.SimilarStyle
                                         }).ToDictionary(k => k.ProductID, v => v);

#if false // temp hack for rugs before had ProductFeatures table.
                    #region Hack for Rugs

                    if (Store.StoreKey == StoreKeys.InsideRugs)
                    {
                        productExtensions = new Dictionary<int, SqlProductExtension>();

                        // grab the data we need from p.Ext4

                        var data = dc.Products.Where(p => p.Published == 1 && p.Deleted == 0)
                                        .Select(p => new
                                        {
                                            ProductID = p.ProductID,
                                            Ext4 = p.ExtensionData4
                                        }).ToList();

                        foreach(var item in data)
                        {
                            var sqlExt = new SqlProductExtension();

                            var extData = ExtensionData4.Deserialize(item.Ext4);

                            if (extData.Data.ContainsKey(ExtensionData4.ProductImageFeatures))
                            {
                                var features = extData.Data[ExtensionData4.ProductImageFeatures] as ImageFeatures;

                                // descriptors

                                sqlExt.TinyImageDescriptor = (int)features.TinyCEDD;
                                sqlExt.ImageDescriptor = features.CEDD;

                                // dominant colors
                                var sbColors = new StringBuilder();

                                bool isFirstColor = true;
                                foreach (var color in features.DominantColors)
                                {
                                    if (!isFirstColor)
                                        sbColors.Append(";");

                                    sbColors.Append(color.Replace("#FF", "#")); // from ARGB to RGB

                                    isFirstColor = false;
                                }

                                sqlExt.Colors = sbColors.ToString();
                            }

                            if (extData.Data.ContainsKey(ExtensionData4.RugShapes))
                            {
                                var dicShapes = extData.Data[ExtensionData4.RugShapes] as Dictionary<string, int>;

                                var sb = new StringBuilder();
                                bool isFirst = true;

                                foreach (var shape in dicShapes)
                                {
                                    if (shape.Value == 0)
                                        continue;

                                    if (!isFirst)
                                        sb.Append(";");

                                    sb.AppendFormat("{0}={1}", shape.Key, shape.Value);
                                    isFirst = false;
                                }

                                sqlExt.Shapes = sb.ToString();
                            }

                            productExtensions[item.ProductID] = sqlExt;
                        }
                    }

                    #endregion // rugs hack
#endif
                    Debug.WriteLine(string.Format("Supplemental Query for {0}: {1:N0}     ({2})", Store.StoreKey, productExtensions.Count, DateTime.Now - dtStartSecondaryQuery));
                }

                #endregion

                #region Big Parallel Loop for Products
                Parallel.ForEach(products, (p) =>
                {
                    try
                    {
                        var productGroup = p.ProductGroup.ToProductGroup();
                        InventoryStatus stockStatus;

                        if (Store.StoreKey == StoreKeys.InsideRugs)
                            stockStatus = p.StockStatus == "D" ? InventoryStatus.Discontinued : InventoryStatus.Unknown;
                        else if (Store.HasAutomatedInventoryTracking)
                            stockStatus = p.StockStatus == "D" ? InventoryStatus.Discontinued : p.StockStatus == "I" ? InventoryStatus.InStock : InventoryStatus.OutOfStock;
                        else
                            stockStatus = p.StockStatus == "D" ? InventoryStatus.Discontinued : InventoryStatus.Unknown;

                        // skip if missing key information
                        if (p.ManufacturerID == 0 || string.IsNullOrWhiteSpace(p.SKU) || string.IsNullOrWhiteSpace(p.Name))
                            return;

                        // do not allow any products from unpublished/deleted manufacturers - which servers as a master override against individual product settings
                        if (skippedManufacturers.Contains(p.ManufacturerID))
                            return;

                        // product group required
                        if (!productGroup.HasValue)
                            return;

                        // must have all non-zero pricing if not otherwise discontinued
                        if (p.ShowBuyButton == 1 && (p.OurPrice == 0M || p.LowVariantOurPrice == 0M || p.LowVariantRetailPrice == 0M || p.HighVariantOurPrice == 0M || p.HighVariantRetailPrice == 0M))
                        {
                            //Debug.WriteLine(string.Format("{0}: Skipping SKU {1} - {2} due to zero prices detected.", Store.StoreKey, p.SKU, p.Name));
                            return;
                        }

                        // inside ave requires images
                        if (Store.StoreKey == StoreKeys.InsideAvenue && string.IsNullOrEmpty(p.ImageFilename))
                            return;

                        // rugs - must be a rug, must have an image
                        if (Store.StoreKey == StoreKeys.InsideRugs && (productGroup != ProductGroup.Rug || string.IsNullOrEmpty(p.ImageFilename)))
                            return;

                        // fabric and wallpaper do not require images because many patterns don't have one - but people still search for them.
                        // with homeware, rugs - this is not really a thing to worry about

                        var isOutlet = outletProducts.Contains(p.ProductID);

                        #region Colors

                        // these get filled in only for image search
                        List<System.Windows.Media.Color> colors = null;
                        List<Lab> labColors = null;
                        uint tinyImageDescriptor = 0;
                        byte[] imageDescriptor = null; // will be byte[144]
                        List<int> similar = null;
                        List<int> similarColors = null;
                        List<int> similarStyle = null;

                        // filled in when needed
                        SqlProductExtension productExtension = null;

                        try
                        {
                            if (Store.IsImageSearchEnabled)
                            {

                                // even for rugs where image search always enabled, will not have a record
                                // here until after images are processed - need for all code to account for such

                                if (productExtensions.TryGetValue(p.ProductID, out productExtension))
                                {
                                    // descriptors

                                    tinyImageDescriptor = (uint)productExtension.TinyImageDescriptor;
                                    imageDescriptor = productExtension.ImageDescriptor;

                                    // dominant colors

                                    if (productExtension.Colors != null)
                                    {
                                        colors = new List<System.Windows.Media.Color>();

                                        var aryColors = productExtension.Colors.Split(';');
                                        foreach (var color in aryColors.Where(e => !string.IsNullOrWhiteSpace(e)))
                                            colors.Add(color.ToColor());

                                        labColors = colors.ToLabColors();
                                    }

                                    // similar products

                                    similar = productExtension.Similar.ParseCommaDelimitedList().Select(e => int.Parse(e)).ToList();
                                    similarColors = productExtension.SimilarColors.ParseCommaDelimitedList().Select(e => int.Parse(e)).ToList();
                                    similarStyle = productExtension.SimilarStyle.ParseCommaDelimitedList().Select(e => int.Parse(e)).ToList();
                                }
                            }
                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine(Ex.Message);
                        }

#if DEBUG
                        if (Store.StoreKey == StoreKeys.InsideFabric && (labColors == null || labColors.Count() == 0))
                            return;
#endif
                        #endregion

                        CacheProduct cacheProduct = null;
                        switch(Store.StoreKey)
                        {
                            case StoreKeys.InsideFabric:
                                cacheProduct = new InsideFabricCacheProduct();
                                break;

                            case StoreKeys.InsideWallpaper:
                                cacheProduct = new InsideWallpaperCacheProduct();
                                break;

                            case StoreKeys.InsideAvenue:
                                cacheProduct = new InsideAvenueCacheProduct();
                                break;

                            case StoreKeys.InsideRugs:
                                cacheProduct = new InsideRugsCacheProduct();
                                break;

                            default:
                                cacheProduct = new CacheProduct();
                                break;
                        }
                        
                        cacheProduct.ProductID = p.ProductID;
                        cacheProduct.SKU = p.SKU;
                        cacheProduct.Name = p.Name;
                        cacheProduct.ImageFilename = p.ImageFilename;
                        cacheProduct.ImageUrl = ImageName(p.ProductID, p.ImageFilename);
                        cacheProduct.ProductUrl = string.Format("/p-{0}-{1}.aspx", p.ProductID, p.SEName).ToLower();
                        cacheProduct.RetailPrice = p.RetailPrice;
                        cacheProduct.OurPrice = p.OurPrice;
                        cacheProduct.SalePrice = p.SalePrice;
                        cacheProduct.IsMissingImage = string.IsNullOrEmpty(p.ImageFilename);
                        cacheProduct.IsDiscontinued = p.ShowBuyButton == 0;
                        cacheProduct.IsNew = !isOutlet && (p.Created >= createdAfter) && (p.ShowBuyButton == 1) && (p.MiscText != "Limited Availability");
                        cacheProduct.StockStatus = stockStatus;
                        cacheProduct.DiscountGroup = "0";
                        cacheProduct.LowVariantRetailPrice = p.LowVariantRetailPrice;
                        cacheProduct.HighVariantRetailPrice = p.HighVariantRetailPrice;
                        cacheProduct.LowVariantOurPrice = p.LowVariantOurPrice;
                        cacheProduct.HighVariantOurPrice = p.HighVariantOurPrice;
                        cacheProduct.Created = p.Created;
                        cacheProduct.ManufacturerID = p.ManufacturerID;
                        cacheProduct.ProductGroup = productGroup;
                        cacheProduct.DisplayPriority = MakeDisplayPriority(p.ProductGroup);
                        cacheProduct.IsOutlet = isOutlet && p.ShowBuyButton == 1;
                        // color information filled in only when image search enabled
                        cacheProduct.ImageDescriptor = imageDescriptor;
                        cacheProduct.TinyImageDescriptor = tinyImageDescriptor;
                        cacheProduct.Colors = colors;
                        cacheProduct.LabColors = labColors;
                        // correlated product information filled in below in a post-processing step; requires correlated products enabled.
                        cacheProduct.Correlator = Store.IsCorrelatedProductsEnabled ? p.Correlator : null;
                        cacheProduct.CorrelatedProducts = null;

                        cacheProduct.SimilarProducts = Store.IsImageSearchEnabled ? similar : null;
                        cacheProduct.SimilarProductsByColor = Store.IsImageSearchEnabled ? similarColors : null;
                        cacheProduct.SimilarProductsByStyle = Store.IsImageSearchEnabled ? similarStyle : null;


                        SetDiscountInfo(cacheProduct);

                        if (cacheProduct is InsideRugsCacheProduct)
                        {
                            // product extensions could be missing when records first created and
                            // not yet fully processed

                            var rugsCacheProduct = cacheProduct as InsideRugsCacheProduct;
                            if (productExtension != null)
                                rugsCacheProduct.Shapes = productExtension.Shapes;
                            else
                                rugsCacheProduct.Shapes = string.Empty;

                            lock (Products)
                            {
                                Products[p.ProductID] = rugsCacheProduct;
                            }
                        }
                        else
                        {
                            lock (Products)
                            {
                                Products[p.ProductID] = cacheProduct;
                            }
                        }

                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine(Ex.ToString());
                    }
                });
                #endregion

                // fill in variant-aware stock status and  primary product category

                if (Store.StoreKey == StoreKeys.InsideAvenue)
                {
                    try
                    {
                        const int CLASS_ROOT_CATEGORYID = 1;

                        var productsWithAnyVariantHavingInventory = new HashSet<int>(dc.ProductVariants.Where(e => e.Inventory > 0).Select(e => e.ProductID).Distinct().ToList());

                        foreach(var p in Products.Values)
                        {
                            if (p.IsDiscontinued)
                                continue;

                            // update status to reflect as InStock if ANY variant has stock (because with above, only reflects default variant).

                            p.StockStatus = productsWithAnyVariantHavingInventory.Contains(p.ProductID) ? InventoryStatus.InStock : InventoryStatus.OutOfStock;
                        }


                        // build up a mapping of our internal catID to SQL catID.
                        Func<List<int>> getPrimaryCategories = () =>
                        {
                            var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                            if (sqlRootNode == null)
                                return new List<int>();

                            // extension data will contain a Dictionary<int, int>

                            if (string.IsNullOrEmpty(sqlRootNode.ExtensionData))
                                return new List<int>();

                            var dicMap = JsonConvert.DeserializeObject<Dictionary<int, int>>(sqlRootNode.ExtensionData);

                            return dicMap.Values.ToList();
                        };

                        // these are the ones we care about - see which one (or first, but should generally be one) the product belongs to, if any
                        var primaryCategories = new HashSet<int>(getPrimaryCategories());

                        var productCategories = (from pc in dc.ProductCategories
                                                 select new { pc.ProductID, pc.CategoryID}
                                                ).ToList();

                        // do manually in memory rather than groupby because seems way faster

                        var dic = dc.ProductCategories.Select(e => e.ProductID).Distinct().ToDictionary(k => k, v => new List<int>());
                        foreach(var item in productCategories)
                            dic[item.ProductID].Add(item.CategoryID);

                        foreach (InsideAvenueCacheProduct product in Products.Values)
                        {
                            // find out which (if any) primary category
                            List<int> cats;
                            if (dic.TryGetValue(product.ProductID, out cats))
                            {
                                foreach (var cat in cats )
                                {
                                    if (cat == CLASS_ROOT_CATEGORYID)
                                        continue;

                                    if (primaryCategories.Contains(cat))
                                    {
                                        product.CategoryID = cat;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    catch(Exception Ex)
                    {
                        Debug.WriteLine(Ex.Message);
                    }
                }


                if (Store.IsFilteredSearchEnabled || Store.IsImageSearchEnabled)
                {
                    // use this specific technique because it's possible to have duplicate SKUs (even though very rare).
                    SkuToProductID = new Dictionary<string, int>();
                    foreach (var product in Products)
                        SkuToProductID[product.Value.SKU] = product.Key;
                }

                // must be done after the parallel for loop completes
                if (Store.IsCorrelatedProductsEnabled)
                    PopulateCorrelations();

                if (Store.IsImageSearchEnabled)
                    PopulateImageSearchHistograms();

                Debug.WriteLine(string.Format("Cached products for {0}: {1:N0}     ({2})", Store.StoreKey, Products.Count, DateTime.Now - dtStart));
            }

        } 

        #endregion

        #region Protected Methods


        protected virtual void PopulateAutoSuggestPhrases()
        {
            // this dic is used as a pkid to phrase string lookup, across all supported lists
            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                AutoSuggestPhrases = dc.AutoSuggestPhrases.ToDictionary(k => k.PhraseID, v => v.Phrase);
            }
        }

        protected virtual void SetProductsForSale()
        {
            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                // fabric does not include variant swatches - and also uses different logic for discontinued

                if (storeKey == StoreKeys.InsideFabric || storeKey == StoreKeys.InsideWallpaper)
                    ProductsForSaleCount = dc.Products.Where(e => e.Published == 1 && e.Deleted == 0 && e.ShowBuyButton == 1).Count();
                else
                    ProductsForSaleCount = dc.ProductVariants.Where(e => e.Published == 1 && e.Deleted == 0).Count();
            }
        }

        protected virtual int MakeDisplayPriority(string productGroup)
        {
            if (string.IsNullOrWhiteSpace(productGroup))
                return 0;

            switch (productGroup.ToLower())
            {
                case "fabric":
                    return 1;

                case "wallcovering":
                    return 2;

                case "trim":
                    return 3;

                // will never be displayed at same time as others
                case "rug":
                    return 1;

                // will never be displayed at same time as others
                case "homeware":
                    return 1;

                default:
                    // should not hit here if all have product groups defined
                    return 4;
            }

        }

        protected virtual string ImageName(int ProductID, string OverrideName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(OverrideName))
                {
                    if (Store.EnableImageDomains)
                    {
                        // the last digit of the productID is used to determine
                        // which imageXX.domain.com is used

                        var lastDigit = ProductID.ToString().Last();

                        // index must be 01, 02 to 10

                        string index;
                        if (lastDigit == '0')
                            index = "10";
                        else
                            index = string.Format("0{0}", lastDigit);

                        string url;
                        if (Store.ImageDomainsUseSSL)
                            url = string.Format("https://image{0}.{1}/{2}/product/icon/{3}", index, "insidestores.com", Store.StoreKey.ToString(), OverrideName).ToLower();
                        else
                            url = string.Format("http://image{0}.{1}/{2}/product/icon/{3}", index, Store.Domain, Store.StoreKey.ToString(), OverrideName).ToLower();
                        return url.ToLower();
                    }
                    else
                    {
                        return string.Format("/images/product/icon/{0}", OverrideName).ToLower();
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }

            return "/images/nopicture.gif";
        }

        protected virtual string MakePrice(decimal price)
        {
            return string.Format("{0:c}", price);
        }

        protected virtual void SetDiscountInfo(CacheProduct p)
        {
            Func<decimal, decimal, int> CalcDiscount = (msrp, ourPrice) =>
            {
                if (msrp == 0M || ourPrice == 0M)
                    return 0;

                return (int)((1.0M - (ourPrice / msrp)) * 100M);
            };

            p.Discount = CalcDiscount(p.RetailPrice, p.OurPrice);

            // available groups are 0, 10, 15, 20, 25, 30, etc. up to 70
            p.DiscountGroup = Math.Min(70, (p.Discount / 5) * 5).ToString();
        }

        protected virtual void PopulateImageSearchHistograms()
        {
            var dtStart = DateTime.Now;

            Parallel.ForEach(Products.Values, product => 
            {
                if (product.ImageDescriptor == null)
                    return;

                product.ColorHistogram = ImageSearch.ColorHistogramTransform(product.ImageDescriptor);
                product.TextureHistogram = ImageSearch.TextureHistogramTransform(product.ImageDescriptor);
            });

            Debug.WriteLine(string.Format("Histograms for {0}: {1})", Store.StoreKey, DateTime.Now - dtStart));
        }

        /// <summary>
        /// Post processing step on Products collection to establish the correlations between products based on teh SQL p.MPN column.
        /// </summary>
        protected virtual void PopulateCorrelations()
        {
            // Products (cachedProduct) is now populated, but CorrelatedProducts is null
            // need to figure out correlations

            var dtCorrelatedStart = DateTime.Now;

            // enumerable collection of {MPN, list CacheProduct}
            var correlations = from c in Products.Values
                               where !string.IsNullOrEmpty(c.Correlator)
                               group c by c.Correlator into g
                               select new
                               {
                                   Correlator = g.Key,
                                   Products = g.Select(p => p).ToList()
                               };

            // we have a List[CacheProduct] associated with each correlator value
            // for each of these lists of correlated products, update the member products to reference
            // that list (so all products in the list link to the same List[CacheProduct]

            Parallel.ForEach(correlations, (correlator) =>
            {
                foreach (var product in correlator.Products)
                    product.CorrelatedProducts = correlator.Products;
            });

            // make sure no null lists
            Parallel.ForEach(Products.Values, (p) =>
            {
                if (p.CorrelatedProducts == null)
                    p.CorrelatedProducts = new List<CacheProduct>();
            });

            Debug.WriteLine(string.Format("Correlated products query for {0}: {1})", Store.StoreKey, DateTime.Now - dtCorrelatedStart));
        }

        protected virtual void PopulateChildCategories()
        {
            // note that this only hits up the top layer of children - and that nested child relationships are
            // figured out and added to the cache along the way as needed

            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                var parentCategories = dc.Categories.Where(e => e.ParentCategoryID == 0).Select(e => e.CategoryID).ToList();

                foreach (var parentCategoryID in parentCategories)
                {
                    var list = dc.Categories.Where(e => e.ParentCategoryID == parentCategoryID).Select(e => e.CategoryID).ToList();
                    ChildCategories.Add(parentCategoryID, list);
                }
            }
        }


        /// <summary>
        /// List of all known discontinued products for the store.
        /// </summary>
        protected virtual void PopulateDiscontinuedProducts()
        {
            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                // this check really only serves to speed up building the collection; which for IA and IR would generally include all products in SQL
                // as opposed to IF which would need to exclude wallcovering

                if (Store.StoreKey == StoreKeys.InsideAvenue || Store.StoreKey == StoreKeys.InsideRugs)
                    DiscontinuedProducts = dc.Products.Where(e => e.ShowBuyButton == 0).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e)).OrderBy(e => e).ToList();
                else // must additionally qualify within the allowed product groups
                    DiscontinuedProducts = dc.Products.Where(e => e.ShowBuyButton == 0).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e) && Store.SupportedProductGroups.Contains(Products[e].ProductGroup.Value)).OrderBy(e => e).ToList();
            }
        }

        /// <summary>
        /// Missing images but not discontinued.
        /// </summary>
        protected virtual void PopulateMissingImagesProducts()
        {
            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                // this check really only serves to speed up building the collection; which for IA and IR would generally include all products in SQL
                // as opposed to IF which would need to exclude wallcovering

                if (Store.StoreKey == StoreKeys.InsideAvenue || Store.StoreKey == StoreKeys.InsideRugs)
                    MissingImagesProducts = dc.Products.Where(e => e.ShowBuyButton == 1 && e.ImageFilenameOverride == null).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e)).OrderBy(e => e).ToList();
                else // must additionally qualify within the allowed product groups
                    MissingImagesProducts = dc.Products.Where(e => e.ShowBuyButton == 1 && e.ImageFilenameOverride == null).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e) && Store.SupportedProductGroups.Contains(Products[e].ProductGroup.Value)).OrderBy(e => e).ToList();
            }
        }

        /// <summary>
        /// Create a dictionary with entries for each category for the store. Each entry will have a list of member productID.
        /// </summary>
        /// <remarks>
        /// Must be live (not discontinued) and have images.
        /// </remarks>
        protected virtual void PopulateCategories()
        {
            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                var uniqueList = dc.ProductCategories.Select(e => e.CategoryID).Distinct().ToList();

                foreach (var cat in uniqueList)
                {
                    // use the ProductCategory table to get a list of qualifying products for the given category

                    List<int> products;

                    if (Store.StoreKey == StoreKeys.InsideAvenue || Store.StoreKey == StoreKeys.InsideRugs)
                    {
                        // don't need to filter on ProductGroup since any group is SQL is supported (unlike IF which would need to exclude wallcovering)

                        products = dc.ProductCategories.Where(e => e.CategoryID == cat).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e) && !Products[e].IsDiscontinued && !Products[e].IsMissingImage).ToList();

                    }
                    else
                    {
                        products = dc.ProductCategories.Where(e => e.CategoryID == cat).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e) && !Products[e].IsDiscontinued && !Products[e].IsMissingImage && Store.SupportedProductGroups.Contains(Products[e].ProductGroup.Value)).ToList();
                    }

                    // sort into an ordered list - which then becomes the "default" sort for website display

                    // already randomized so no need to call RandomizeTopProducts()

                    Categories[cat] = MakeOrderedProductListByManufacturerWeight(products);
                }

            }

            Parallel.ForEach(Categories, (item) =>
            {
                // for each key/set, create ordered sets for default, priceascend, pricedescend
                // No longer supported:  nameascend, namedescend (except for InsideAvenue)
                PopulateSortedList(SortedCategories, item.Key, item.Value);
            });

        }

        protected string MakeSortedListKey(int key, ProductSortOrder orderBy)
        {
            return string.Format("{0}:{1}", key, orderBy.ToString().ToLower());
        }

        protected void PopulateSortedList(Dictionary<string, List<int>> dic, int key, List<int> list)
        {
            Action<string, List<int>> ListAdd = (dicKey, dicValue) =>
            {
                lock (dic)
                {
                    dic[dicKey] = dicValue;
                }
            };

            var productList = new List<CacheProduct>();
            foreach (var productID in list)
            {
                CacheProduct p;
                if (Products.TryGetValue(productID, out p))
                {
                    if (p.IsDiscontinued)
                        continue;

                    productList.Add(p);
                }
            }

            var defaultKey = MakeSortedListKey(key, ProductSortOrder.Default);
            var priceAscendKey = MakeSortedListKey(key, ProductSortOrder.PriceAscend);
            var priceDescendKey = MakeSortedListKey(key, ProductSortOrder.PriceDescend);

            ListAdd(defaultKey, productList.Select(e => e.ProductID).ToList());
            ListAdd(priceAscendKey, productList.OrderBy(e => e.OurPrice).Select(e => e.ProductID).ToList());
            ListAdd(priceDescendKey, productList.OrderByDescending(e => e.OurPrice).Select(e => e.ProductID).ToList());
        }

        /// <summary>
        /// Create a dictionary with entries for each manufacturer for the store. Each entry will have a list of member productID.
        /// </summary>
        /// <remarks>
        /// Must be live (not discontinued) and have images.
        /// </remarks>        
        protected virtual void PopulateManufacturers()
        {
            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                var uniqueList = dc.ProductManufacturers.Select(e => e.ManufacturerID).Distinct().ToList();

                foreach (var man in uniqueList)
                {
                    List<int> products;

                    if (Store.StoreKey == StoreKeys.InsideAvenue)
                    {
                        products = dc.ProductManufacturers.Where(e => e.ManufacturerID == man).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e) && !Products[e].IsDiscontinued).OrderBy(e => Products[e].DisplayPriority).ToList();
                    }
                    else 
                    if (Store.StoreKey == StoreKeys.InsideRugs)
                    {
#if DEBUG
                        // don't care about missing images for debug
                        products = dc.ProductManufacturers.Where(e => e.ManufacturerID == man).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e) && !Products[e].IsDiscontinued).OrderBy(e => Products[e].DisplayPriority).ToList();
#else
                        // notice that Created is not part of the sort, that shuffle is used
                        var allProducts = dc.ProductManufacturers.Where(e => e.ManufacturerID == man).Select(e => e.ProductID).ToList();
                        allProducts.Shuffle();
                        products = allProducts.Where(e => Products.ContainsKey(e) && !Products[e].IsDiscontinued && !Products[e].IsMissingImage).OrderBy(e => Products[e].DisplayPriority).ToList();
#endif
                    }
                    else
                    {
                        // assumes stores have inventory tracking

                        // must have images, not be discontinued; sorted by in stock, then created -- so newest in stock shown first
                        products = dc.ProductManufacturers.Where(e => e.ManufacturerID == man).Select(e => e.ProductID).ToList().Where(e => Products.ContainsKey(e) && Store.SupportedProductGroups.Contains(Products[e].ProductGroup.Value) && !Products[e].IsDiscontinued && !Products[e].IsMissingImage).OrderBy(e => Products[e].StockStatus).ThenBy(e => Products[e].DisplayPriority).ThenByDescending(e => Products[e].Created).ToList();
                    }

                    // the purpose of this tiny randomization is to cause the first page to have some changes on a daily basis for google
                    // to see some freshness.
                    Manufacturers[man] = RandomizeTopProducts(products, 50);
                }
            }

            Parallel.ForEach(Manufacturers, (item) =>
            {
                // for each key/set, create ordered sets for default, priceascend, pricedescend
                // No longer supported:  nameascend, namedescend 
                PopulateSortedList(SortedManufacturers, item.Key, item.Value);
            });

        }

        /// <summary>
        /// Shuffle only the top N, then return the entire list reassembled.
        /// </summary>
        /// <param name="products"></param>
        /// <param name="topN"></param>
        /// <returns></returns>
        protected virtual List<int> RandomizeTopProducts(List<int> products, int topN)
        {
            // get the first bunch and randomize within that group
            var topSlice = products.Take(topN).ToList();
            topSlice.DeterministicShuffle(DateTime.Today.DayOfYear);

            // add the rest back and return the entire list, but now, the top N 
            // have been randomized
            topSlice.AddRange(products.Skip(topN));
            return topSlice;
        }

        protected virtual int GetSeed(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            SHA1Managed hashString = new SHA1Managed();
            int result = 0;

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                result += x;

            return result;
        }

        protected void PopulateFeaturedProducts()
        {
            // presently only using InsideAvenue products.
            if (storeKey != StoreKeys.InsideAvenue)
                FeaturedProducts = new List<int>();

            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                FeaturedProducts = dc.Products.Where(e => e.IsFeatured == 1 && e.ShowBuyButton == 1 && e.Deleted == 0 && e.Published == 1).Select(e => e.ProductID).ToList();
                Debug.WriteLine(string.Format("{0} featured products: {1:N0}", Store.StoreKey.ToString(), FeaturedProducts.Count()));
                
                // make sure we have enough - else fill randomly from loaded up products
                if (FeaturedProducts.Count() < 50 && Products.Count() > 1000)
                {
                    var rnd = new Random();

                    var desiredProductCount = 500;
                    var totalProductCount = Products.Count();

                    var selectedIndexes = new HashSet<int>();

                    int maxPickAttempts = desiredProductCount * 4;
                    int pickAttempts = 0;
                    while (true)
                    {
                        var index = rnd.Next(0, totalProductCount - 1);
                        selectedIndexes.Add(index);

                        if (selectedIndexes.Count() == desiredProductCount || ++pickAttempts > maxPickAttempts)
                            break;
                    }

                    var productIDs = Products.Keys.ToList();
                    FeaturedProducts.Clear();
                    foreach (var idx in selectedIndexes)
                        FeaturedProducts.Add(productIDs[idx]);
                }
            }
        }

        #endregion

        #region MakeOrderedProductListByManufacturerWeight


        /// <summary>
        /// ReOrder list of productID to have an appropriate spread across all vendors
        /// based on weighting factors.
        /// </summary>
        /// <remarks>
        /// Only intended to be used for sorting lists which are comprised of multiple manufacturers.
        /// Called for fixed category memberships, plus a variety of search results.
        /// </remarks>
        /// <param name="products"></param>
        /// <returns></returns>
        public virtual List<int> MakeOrderedProductListByManufacturerWeight(List<int> products)
        {

            // given a list of productIDs, get a list of CacheProduct which qualifies based on the predicate
            Func<Predicate<CacheProduct>, List<CacheProduct>> GetQualifyingProducts = (predicate) =>
                {
                    var list = new List<CacheProduct>();

                    foreach (var productID in products)
                    {
                        CacheProduct product;

                        if (!Products.TryGetValue(productID, out product))
                            continue;

                        if (predicate(product))
                            list.Add(product);
                    }

                    return list;
                };

            // the list we get on input is simply all productIDs which qualify for inclusion in the master set.
            // the goal is to sort them into the order we wish to present on the website.

            // first take all the products which are not discontinued and have good images - sort them with weighting
            // then do same for active products without images

            // then for discontinued with images
            // then for discontinued without images
            // suppose for the discontinued we can just throw them all in at the end and not bother with weighted sort.

            Func<List<CacheProduct>, List<int>> sortRandom = (list) =>
                {
                    // present implementation is completely random - does not using weighting

                    var rand = new Random();

                    var resultList = new List<int>();

                    if (list == null || list.Count() == 0)
                        return resultList;

                    var workingList = list.Select(e => e.ProductID).ToList();

                    while (workingList.Count > 0)
                    {
                        var pickIndex = rand.Next(0, workingList.Count());
                        var pickedProductID = workingList[pickIndex];
                        resultList.Add(pickedProductID);
                        workingList.RemoveAt(pickIndex);
                    }

                    Debug.Assert(resultList.Count() == list.Count());
                    return resultList;
                };

            // called for live with images and in stock - intended to take newness and margins (favored vendors) into account
            Func<List<CacheProduct>, List<int>> sortIntelligent = (list) =>
            {
                var newProductCutofDate = DateTime.Now.AddDays(-90);
                var intelligentList = new List<int>();

                // presently - just segregates new products (by 90 days) and then randomly sorts those two groups

                intelligentList.AddRange(sortRandom(list.Where(e => e.Created >= newProductCutofDate).ToList()));
                intelligentList.AddRange(sortRandom(list.Where(e => e.Created < newProductCutofDate).ToList()));

                return intelligentList;
            };


            var liveWithImages = GetQualifyingProducts(e => !e.IsDiscontinued && !e.IsMissingImage);
            var liveWithoutImages = GetQualifyingProducts(e => !e.IsDiscontinued && e.IsMissingImage);

            // this list of discontinued is sorted simply by putting the ones with images first, and sorting also by created.
            var discontinuedProducts = GetQualifyingProducts(e => e.IsDiscontinued).OrderBy(e => e.IsMissingImage).ThenBy(e => e.Created).Select(e => e.ProductID).ToList();

            var finalList = new List<int>();

            if (Store.StoreKey == StoreKeys.InsideRugs)
            {
                finalList.AddRange(sortRandom(liveWithImages));
            }
            else if (Store.HasAutomatedInventoryTracking)
            {
                // if we know about stock tracking, then put the ones that are in stock up at the top
                finalList.AddRange(sortIntelligent(liveWithImages.Where(e => e.StockStatus == InventoryStatus.InStock).ToList()));
                finalList.AddRange(sortRandom(liveWithImages.Where(e => e.StockStatus == InventoryStatus.OutOfStock).ToList()));
            }
            else
            {
                finalList.AddRange(sortRandom(liveWithImages));
            }

            // anything else is low probability and not sorted

            finalList.AddRange(liveWithoutImages.Select(e => e.ProductID));
            finalList.AddRange(discontinuedProducts);

            Debug.Assert(products.Count() == finalList.Count());
            Debug.Assert(finalList.Distinct().Count() == finalList.Count);

            return finalList;
        }
        
        #endregion
    }

}