using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Products;
using Utilities;

namespace ProductScanner.App.ViewData
{
    /// <summary>
    /// RadGrid plus export data model for New Products commit batch.
    /// </summary>
    [ViewData(CommitBatchType.NewProducts, Viewer = typeof(Controls.BatchNewProductsRadGrid), IsFreezeColumnsSupported = true)]
    public class NewProductViewData : ViewDataBase, IViewData
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public int VariantCount { get; set; } // the number of variants (count)

        [ExcelColumn(ExcelColumnType.Url)]
        public string VendorUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        public decimal Cost { get; set; } // of default variant

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("Retail Price")]
        public decimal RetailPrice { get; set; } // of default variant

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("Our Price")]
        public decimal OurPrice { get; set; } // of default variant

        [Description("Product Group")]
        public string ProductGroup { get; set; }

        [Description("Unit of Measure")]
        public string UnitOfMeasure { get; set; }


        private StoreProduct _storeProduct;
        

        public NewProductViewData()
        {
        }

        public NewProductViewData(StoreProduct storeProduct)
        {
            _storeProduct = storeProduct;

            VendorUrl = storeProduct.GetDetailUrl();

            SKU = storeProduct.SKU;
            Name = storeProduct.Name;
            ProductGroup = storeProduct.ProductGroup.ToString();
            VariantCount = storeProduct.ProductVariants.Count;
            UnitOfMeasure = storeProduct.UnitOfMeasure.ToString();

            var defaultVariant = storeProduct.ProductVariants.SingleOrDefault(x => x.IsDefault);
            if (defaultVariant == null)
            {
                defaultVariant = storeProduct.ProductVariants.First();
                if (defaultVariant == null) return;
            }

            Cost = defaultVariant.Cost;
            RetailPrice = defaultVariant.RetailPrice;
            OurPrice = defaultVariant.OurPrice;
        }


        public override ICommitRecordDetails GetDetails()
        {
            var details = new CommitRecordDetails
            {
                Title = SKU,
                FullName = Name,
                ImageUrl = _storeProduct.ProductImages.Select(e => e.SourceUrl).FirstOrDefault(),
                StoreUrl = null, // not in store, so cannot have a url yet
                VendorUrl = VendorUrl,
                JSON = _storeProduct.ToJSON(),
                ProductImages = _storeProduct.ProductImages,
                ProductVariants = _storeProduct.ProductVariants,
            };

            return details;
        }

        public static Task<List<IViewData>> CreateDataSetAsync(IStoreDatabaseConnector dbStore, StoreType storeKey, byte[] gzipJsonData)
        {
            var tsc = new TaskCompletionSource<List<IViewData>>();

            Task.Run(() =>
            {
                // note that for new products, no need to gather supplemental data from Store SQL.

                var list = new List<IViewData>();

                try
                {
                    var variants = CommitDataList<List<StoreProduct>>(gzipJsonData);
                    foreach (var variant in variants)
                    {
                        var rec = new NewProductViewData(variant);
                        list.Add(rec);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                tsc.SetResult(list);
            });

            return tsc.Task;
        }

        public static new void Export(IEnumerable<IViewData> data, string suggestedName)
        {
            ExportManager.SaveExcelFile(data.Cast<NewProductViewData>(), suggestedName);
        }

    }
}
