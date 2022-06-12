//------------------------------------------------------------------------------
// 
// Class: CacheProduct 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ColorMine.ColorSpaces;

namespace Website
{
    public class CacheProduct
    {
        public virtual IWebStore GetWebStore()
        {
            return null;
        }

        #region Properties
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        //public string AlternateName { get; set; }
        public string ImageUrl { get; set; }
        public string ImageFilename { get; set; }
        public string ProductUrl { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal OurPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public bool IsMissingImage { get; set; }
        public bool IsDiscontinued { get; set; }
        public int ManufacturerID { get; set; }
        public DateTime Created { get; set; }
        public int DisplayPriority { get; set; }
        public InventoryStatus StockStatus { get; set; }
        public bool IsNew { get; set; }
        public bool IsOutlet { get; set; }
        public ProductGroup? ProductGroup { get; set; }

        /// <summary>
        /// Computed integer discount on default variant
        /// </summary>
        public int Discount { get; set; }

        /// <summary>
        /// Nearest discount range by fives - 0, 5, 10, 15, 20, etc. Max 70.
        /// </summary>
        public string DiscountGroup { get; set; }

        public decimal LowVariantRetailPrice { get; set; }
        public decimal HighVariantRetailPrice { get; set; }
        public decimal LowVariantOurPrice { get; set; }
        public decimal HighVariantOurPrice { get; set; }


        // requires correlated products enabled, else null/default
        public string Correlator { get; set; }
        // entire set, including self, all products reference the same collection in memory
        public List<CacheProduct> CorrelatedProducts { get; set; } 

        // below here requires image search to be enabled

        // requires image search enabled, else null/default
        public byte[] ImageDescriptor { get; set; }
        public uint TinyImageDescriptor { get; set; }
        public List<Lab> LabColors { get; set; }
        public List<System.Windows.Media.Color> Colors { get; set; }

        /// <summary>
        /// Histogram computed on the fly for image search by color.
        /// </summary>
        /// <remarks>
        /// See ImageSearch.ColorHistogramTransform().
        /// </remarks>
        public byte[] ColorHistogram { get; set; }

        /// <summary>
        /// Histogram computed on the fly for image search by texture.
        /// </summary>
        /// <remarks>
        /// See ImageSearch.TextureHistogramTransform().
        /// </remarks>
        public byte[] TextureHistogram { get; set; }

        /// <summary>
        /// Ordered list of productID, null if not enabled or empty list if not yet populated.
        /// </summary>
        /// <remarks>
        /// CEDD search by color and texture. Requires image search to be enabled.
        /// </remarks>
        public List<int> SimilarProducts { get; set; }

        /// <summary>
        /// Ordered list of productID, null if not enabled or empty list if not yet populated.
        /// </summary>
        /// <remarks>
        /// CEDD search by color. Requires image search to be enabled.
        /// </remarks>
        public List<int> SimilarProductsByColor { get; set; }

        /// <summary>
        /// Ordered list of productID, null if not enabled or empty list if not yet populated.
        /// </summary>
        /// <remarks>
        /// CEDD search by texture. Requires image search to be enabled.
        /// </remarks>
        public List<int> SimilarProductsByStyle { get; set; } 


        #endregion

        public virtual CacheProduct CopyWithAbsoluteUrls(string domain)
        {
            // adjust based on if CDN URLs are already in place

            var copy = new CacheProduct();
            CopyWithAbsoluteUrls(copy, domain);
            return copy;
        }

        protected void CopyWithAbsoluteUrls(CacheProduct dest, string domain)
        {
            string _imageUrl = this.ImageUrl;

            if (!this.ImageUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase) && !this.ImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                _imageUrl = string.Format("//www.{0}{1}", domain, this.ImageUrl);

            Copy(this, dest);
            dest.ImageUrl = _imageUrl;
            dest.ProductUrl = string.Format("http://www.{0}{1}", domain, this.ProductUrl);
        }

        protected void Copy(CacheProduct src, CacheProduct dest)
        {
            dest.ProductID = src.ProductID;
            dest.SKU = src.SKU;
            dest.Name = src.Name;
            //dest.AlternateName = src.AlternateName;
            dest.ImageUrl = src.ImageUrl;
            dest.ImageFilename = src.ImageFilename;
            dest.ProductUrl = src.ProductUrl;
            dest.RetailPrice = src.RetailPrice;
            dest.OurPrice = src.OurPrice;
            dest.SalePrice = src.SalePrice;
            dest.IsMissingImage = src.IsMissingImage;
            dest.IsDiscontinued = src.IsDiscontinued;
            dest.StockStatus = src.StockStatus;
            dest.Created = src.Created;
            dest.ManufacturerID = src.ManufacturerID;
            dest.Discount = src.Discount;
            dest.DiscountGroup = src.DiscountGroup;
            dest.LowVariantRetailPrice = src.LowVariantRetailPrice;
            dest.HighVariantRetailPrice = src.HighVariantRetailPrice;
            dest.LowVariantOurPrice = src.LowVariantOurPrice;
            dest.HighVariantOurPrice = src.HighVariantOurPrice;
            dest.DisplayPriority = src.DisplayPriority;
            dest.ProductGroup = src.ProductGroup;
            dest.IsNew = src.IsNew;
            dest.IsOutlet = src.IsOutlet;
            dest.Correlator = src.Correlator;
            dest.CorrelatedProducts = src.CorrelatedProducts;

            // all these require image search to be enabled

            dest.Colors = src.Colors;
            dest.LabColors = src.LabColors;
            dest.ImageDescriptor = src.ImageDescriptor;
            dest.TinyImageDescriptor = src.TinyImageDescriptor;
            dest.ColorHistogram = src.ColorHistogram;
            dest.TextureHistogram = src.TextureHistogram;
            dest.SimilarProducts = src.SimilarProducts;
            dest.SimilarProductsByColor = src.SimilarProductsByColor;
            dest.SimilarProductsByStyle = src.SimilarProductsByStyle;
        }
    }


    #region InsideFabricCacheProduct

    public class InsideFabricCacheProduct : CacheProduct
    {
        private static IWebStore _store = null;

        public override IWebStore GetWebStore()
        {
            if (_store == null)
                _store = MvcApplication.Current.GetWebStore(StoreKeys.InsideFabric);

            return _store;
        }
    }

    #endregion

    #region InsideWallpaperCacheProduct

    public class InsideWallpaperCacheProduct : CacheProduct
    {
        private static IWebStore _store = null;

        public override IWebStore GetWebStore()
        {
            if (_store == null)
                _store = MvcApplication.Current.GetWebStore(StoreKeys.InsideWallpaper);

            return _store;
        }
    }

    #endregion

    #region InsideAvenueCacheProduct

    public class InsideAvenueCacheProduct : CacheProduct
    {
        private static IWebStore _store = null;

        /// <summary>
        /// Primary categoryID (from taxonomy) for this product.
        /// </summary>
        public int? CategoryID { get; set; }

        public override IWebStore GetWebStore()
        {
            if (_store == null)
                _store = MvcApplication.Current.GetWebStore(StoreKeys.InsideAvenue);

            return _store;
        }
    }

    #endregion

    #region InsideRugsCacheProduct

    public class InsideRugsCacheProduct : CacheProduct
    {
        private static IWebStore _store = null;

        public override IWebStore GetWebStore()
        {
            if (_store == null)
                _store = MvcApplication.Current.GetWebStore(StoreKeys.InsideRugs);

            return _store;
        }

        /// <summary>
        /// Only difference is we're adding the shapes
        /// </summary>
        public string Shapes { get; set; }

        public InsideRugsCacheProduct()
        {

        }

        public InsideRugsCacheProduct(CacheProduct p)
        {
            Copy(p, this);
        }

        public override CacheProduct CopyWithAbsoluteUrls(string domain)
        {
            var copy = new InsideRugsCacheProduct();
            CopyWithAbsoluteUrls(copy, domain);

            copy.Shapes = this.Shapes;

            return copy;
        }
    } 

    #endregion
}