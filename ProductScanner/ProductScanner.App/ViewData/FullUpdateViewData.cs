using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Products;
using Utilities;

namespace ProductScanner.App.ViewData
{
    /// <summary>
    /// RadGrid plus export data model for New Products commit batch.
    /// </summary>
    [ViewData(CommitBatchType.FullUpdate, Viewer = typeof(Controls.BatchFullUpdatesRadGrid), IsFreezeColumnsSupported = true)]
    public class FullUpdateViewData : ViewDataBase, IViewData
    {
        [Description("Product ID")]
        public int ProductID { get; set; }

        public string SKU { get; set; }
        public string Name { get; set; }
        public int VariantCount { get; set; } // the number of variants (count)

        [ExcelColumn(ExcelColumnType.Url)]
        public string StoreUrl { get; set; }

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

        public FullUpdateViewData()
        {
        }

        public FullUpdateViewData(StoreProduct storeProduct, ProductSupplementalData p)
        {
            _storeProduct = storeProduct;

            ProductID = storeProduct.ProductID ?? 0;
            if (p != null)
                StoreUrl = p.StoreUrl;

            VendorUrl = storeProduct.GetDetailUrl();
            SKU = storeProduct.SKU;
            Name = storeProduct.Name;
            ProductGroup = storeProduct.ProductGroup.ToString();
            VariantCount = storeProduct.ProductVariants.Count;
            UnitOfMeasure = storeProduct.UnitOfMeasure.ToString();

            var defaultVariant = storeProduct.ProductVariants.SingleOrDefault(x => x.IsDefault);
            if (defaultVariant == null) return;

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
                StoreUrl = StoreUrl,
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
            Task.Run( async () =>
            {

                var list = new List<IViewData>();

                try
                {
                    var products = CommitDataList<List<StoreProduct>>(gzipJsonData);
                    var supplementalData = await dbStore.GetProductSupplementalDataAsync(storeKey, products.Select(e => e.ProductID ?? 0).ToList());
                    foreach (var product in products)
                    {
                        var suppData = supplementalData.SingleOrDefault(x => x.ProductID == product.ProductID);
                        var rec = new FullUpdateViewData(product, suppData);
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
            ExportManager.SaveExcelFile(data.Cast<FullUpdateViewData>(), suggestedName);
        }

    }
}
