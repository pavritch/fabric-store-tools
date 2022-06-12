using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Products;
using Utilities;

namespace ProductScanner.App.ViewData
{
    /// <summary>
    /// RadGrid plus export data model for New Variant commit batch.
    /// </summary>
    /// <remarks>
    /// When a variant is added to an already-existing product.
    /// </remarks>
    [ViewData(CommitBatchType.NewVariants, Viewer = typeof(Controls.BatchNewSwatchesRadGrid), IsFreezeColumnsSupported=true)]
    public class NewVariantsViewData : ViewDataBase, IViewData
    {
        [Description("Product ID")]
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        [Description("Product Group")]
        public string ProductGroup { get; set; }
        [Description("Unit of Measure")]
        public string UnitOfMeasure { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        public decimal Cost { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        public decimal RetailPrice { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        public decimal OurPrice { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string StoreUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string VendorUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string ImageUrl { get; set; }

        private StoreProductVariant _storeProductVariant;

        public NewVariantsViewData()
        {

        }

        public NewVariantsViewData(ProductScanner.Core.Scanning.Commits.NewVariant variant, ProductSupplementalData p)
        {
            _storeProductVariant = variant.StoreProductVariant;

            ProductID = p.ProductID;
            SKU = p.SKU + _storeProductVariant.SKUSuffix;
            Name = p.Name;
            ProductGroup = p.ProductGroup;
            UnitOfMeasure = variant.StoreProductVariant.UnitOfMeasure.ToString();
            StoreUrl = p.StoreUrl;
            VendorUrl = p.VendorUrl;
            ImageUrl = p.ImageUrl;
            Cost = variant.StoreProductVariant.Cost;
            RetailPrice = variant.StoreProductVariant.RetailPrice;
            OurPrice = variant.StoreProductVariant.OurPrice;
        }

        public override ICommitRecordDetails GetDetails()
        {
            var details = new CommitRecordDetails
            {
                Title = SKU,
                FullName = Name,
                ImageUrl = ImageUrl,
                StoreUrl = StoreUrl,
                VendorUrl = VendorUrl,
                JSON = _storeProductVariant.ToJSON(),
                ProductImages = null,
                ProductVariants = null,
            };

            return details;
        }

        public static Task<List<IViewData>> CreateDataSetAsync(IStoreDatabaseConnector dbStore, StoreType storeKey, byte[] gzipJsonData)
        {
            var tsc = new TaskCompletionSource<List<IViewData>>();
            Task.Run(async () =>
            {
                var list = new List<IViewData>();

                try
                {
                    var variants = CommitDataList<List<ProductScanner.Core.Scanning.Commits.NewVariant>>(gzipJsonData);

                    var supplementalData = await dbStore.GetProductSupplementalDataAsync(storeKey, variants.Select(e => e.ProductId).ToList());
                    var dic = supplementalData.ToDictionary(k => k.ProductID, v => v);

                    foreach (var variant in variants)
                    {
                        if (!dic.ContainsKey(variant.ProductId))
                            continue;

                        var suppData = dic[variant.ProductId];
                        var rec = new NewVariantsViewData(variant, suppData);
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
            ExportManager.SaveExcelFile(data.Cast<NewVariantsViewData>(), suggestedName);
        }

    }
}
