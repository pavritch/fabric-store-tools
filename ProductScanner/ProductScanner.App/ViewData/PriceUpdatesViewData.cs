using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Commits;
using Utilities;

namespace ProductScanner.App
{

    /// <summary>
    /// RadGrid plus export data model for Price Updates commit batch.
    /// </summary>
    [ViewData(CommitBatchType.PriceUpdate, Viewer = typeof(Controls.BatchPriceUpdatesRadGrid), IsFreezeColumnsSupported=true)]
    public class PriceUpdatesViewData : ViewDataBase, IViewData
    {
        // the reason we don't piggy back on the SimpleVariant data base class is that when exporting
        // the columns come out in the wrong order - so simple solution is just to include the columns here
        // in the order we want

        [Description("Variant ID")]
        public int VariantID { get; set; }
        [Description("Product ID")]
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        [Description("Product Group")]
        public string ProductGroup { get; set; }
        [Description("Unit of Measure")]
        public string UnitOfMeasure { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string StoreUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string VendorUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("New Cost")]
        public decimal NewCost { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("New Retail Price")]
        public decimal NewRetailPrice { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("New Our Price")]
        public decimal NewOurPrice { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("Old Cost")]
        public decimal OldCost { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("Old Retail Price")]
        public decimal OldRetailPrice { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("Old Our Price")]
        public decimal OldOurPrice { get; set; }

        /// <summary>
        /// Null means no change. Non-null means to make the specific change to clearance or non-clearance.
        /// </summary>
        [Description("Is Clearance")]
        public bool? IsClearance { get; set; }


        private VariantPriceChange _priceChange;
        private string _imageUrl;

        public PriceUpdatesViewData()
        {

        }

        public PriceUpdatesViewData(VariantPriceChange priceChange, VariantSupplementalData v)
        {
            _priceChange = priceChange;
            _imageUrl = v.ImageUrl;

            VariantID = v.VariantID;
            ProductID = v.ProductID;
            SKU = v.SKU; // already compbined with product SKU
            Name = v.Name;
            ProductGroup = v.ProductGroup;
            UnitOfMeasure = v.UnitOfMeasure;
            StoreUrl = v.StoreUrl;
            VendorUrl = v.VendorUrl;

            NewCost = priceChange.Cost;
            NewRetailPrice = priceChange.RetailPrice;
            NewOurPrice = priceChange.OurPrice;
            OldCost = priceChange.OldCost;
            OldRetailPrice = priceChange.OldRetailPrice;
            OldOurPrice = priceChange.OldOurPrice;
            IsClearance = priceChange.IsClearance;
        }

        public override ICommitRecordDetails GetDetails()
        {
            var details = new CommitRecordDetails
            {
                Title = SKU,
                FullName = Name,
                ImageUrl = _imageUrl,
                StoreUrl = StoreUrl,
                VendorUrl = VendorUrl,
                JSON = _priceChange.ToJSON(),
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
                    var variants = CommitDataList<List<VariantPriceChange>>(gzipJsonData);
                    var supplementalData = await dbStore.GetVariantSupplementalDataAsync(storeKey, variants.Select(e => e.VariantId).ToList());
                    foreach (var variant in variants)
                    {
                        var rec = new PriceUpdatesViewData(variant, supplementalData.SingleOrDefault(x => x.VariantID == variant.VariantId));
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
            ExportManager.SaveExcelFile(data.Cast<PriceUpdatesViewData>(), suggestedName);
        }
    }
}
