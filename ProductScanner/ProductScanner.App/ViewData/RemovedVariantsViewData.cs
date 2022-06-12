using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;

namespace ProductScanner.App
{

    /// <summary>
    /// RadGrid plus export data model for Removed Variant commit batch.
    /// </summary>
    /// <remarks>
    /// When a variant is to be removed from an existing product.
    /// </remarks>
    [ViewData(CommitBatchType.RemovedVariants, Viewer = typeof(ProductScanner.App.Controls.BatchSimpleVariantRadGrid))]
    public class RemovedVariantsViewData : SimpleVariantViewData, IViewData
    {
        public RemovedVariantsViewData()
        {

        }

        public RemovedVariantsViewData(VariantSupplementalData v)
            : base(v)
        {

        }

        public static Task<List<IViewData>> CreateDataSetAsync(IStoreDatabaseConnector dbStore, StoreType storeKey, byte[] gzipJsonData)
        {
#if DEBUG
            if (gzipJsonData == null)
                gzipJsonData = GenerateDummyIntList(300);
#endif

            var tsc = new TaskCompletionSource<List<IViewData>>();

            Task.Run(async () =>
            {
                var list = new List<IViewData>();

                try
                {
                    var variants = CommitDataList<List<int>>(gzipJsonData);

                    var supplementalData = await dbStore.GetVariantSupplementalDataAsync(storeKey, variants);
                    var dic = supplementalData.ToDictionary(k => k.VariantID, v => v);

                    foreach (var variant in variants)
                    {
                        VariantSupplementalData variantData;
                        if (dic.TryGetValue(variant, out variantData))
                        {
                            var rec = new RemovedVariantsViewData(variantData);
                            list.Add(rec);
                        }
                        else
                        {
                            // has already been removed - so fake out a record
                            var removedVariantData = new VariantSupplementalData()
                            {
                                VariantID = variant,
                                ProductID = 0,
                                SKU = "??-??????",
                                Name = "Variant already removed. No data.",
                                UnitOfMeasure = "",
                                ProductGroup = "",
                                ImageUrl = null,
                                StoreUrl = null,
                                VendorUrl = null,
                            };

                            var rec = new RemovedVariantsViewData(removedVariantData);
                            list.Add(rec);
                        }
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
            ExportManager.SaveExcelFile(data.Cast<RemovedVariantsViewData>(), suggestedName);
        }

    }

}
