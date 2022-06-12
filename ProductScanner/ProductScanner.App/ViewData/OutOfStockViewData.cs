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
    /// RadGrid plus export data model for Out of Stock commit batch.
    /// </summary>
    [ViewData(CommitBatchType.OutOfStock, Viewer = typeof(ProductScanner.App.Controls.BatchSimpleVariantRadGrid))]
    public class OutOfStockViewData : SimpleVariantViewData, IViewData
    {
        public OutOfStockViewData()
        {

        }

        public OutOfStockViewData(VariantSupplementalData v)
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

                    foreach (var variant in supplementalData)
                    {
                        var rec = new OutOfStockViewData(variant);
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
            ExportManager.SaveExcelFile(data.Cast<OutOfStockViewData>(), suggestedName);
        }


    }


}
