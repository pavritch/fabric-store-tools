using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;

namespace ProductScanner.App
{
    /// <summary>
    /// RadGrid plus export data model for Discontinued products commit batch.
    /// </summary>
    [ViewData(CommitBatchType.Discontinued, Viewer = typeof(Controls.BatchSimpleProductRadGrid))]
    public class DiscontinuedViewData : SimpleProductViewData
    {
        public DiscontinuedViewData()
        {

        }

        public DiscontinuedViewData(ProductSupplementalData p)
            : base(p)
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
                    var products = CommitDataList<List<int>>(gzipJsonData);

                    var supplementalData = await dbStore.GetProductSupplementalDataAsync(storeKey, products);

                    foreach (var product in supplementalData)
                    {
                        var rec = new DiscontinuedViewData(product);
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
            ExportManager.SaveExcelFile(data.Cast<DiscontinuedViewData>(), suggestedName);
        }

    }
}
