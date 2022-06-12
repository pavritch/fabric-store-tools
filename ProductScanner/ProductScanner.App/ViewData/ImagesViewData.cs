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
using ProductScanner.Core.Scanning.Commits;
using Utilities;

namespace ProductScanner.App
{
    /// <summary>
    /// RadGrid plus export data model for found images commit batch.
    /// </summary>
    /// <remarks>
    /// Presently, the Grid does not show the found links. Can add that later if desired.
    /// We now just show the basic ProductID sort of data for any product for which images have been found.
    /// </remarks>
    [ViewData(CommitBatchType.Images, Viewer = typeof(ProductScanner.App.Controls.BatchSimpleProductRadGrid))]
    public class ImagesViewData : SimpleProductViewData, IViewData
    {
        private ProductImageSet _imageSet;

        public ImagesViewData()
        {

        }

        public ImagesViewData(ProductImageSet imageSet, ProductSupplementalData p)
            : base(p)
        {
            _imageSet = imageSet;
        }

        public override ICommitRecordDetails GetDetails()
        {
            var details = new CommitRecordDetails
            {
                Title = SKU,
                FullName = Name,
                ImageUrl = _imageSet.ProductImages.Select(e => e.SourceUrl).FirstOrDefault(),
                StoreUrl = StoreUrl,
                VendorUrl = VendorUrl,
                JSON = _imageSet.ToJSON(),
                ProductImages = _imageSet.ProductImages,
                ProductVariants = null,
            };

            return details;
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
                    var images = CommitDataList<List<ProductScanner.Core.Scanning.Commits.ProductImageSet>>(gzipJsonData);
                    var products = images.Select(e => e.ProductId).ToList();

                    // then select the productIDs and fetch the corresponding supplemental fields
                    var supplementalData = await dbStore.GetProductSupplementalDataAsync(storeKey, products);
                    var dic = supplementalData.ToDictionary(k => k.ProductID, v => v);

                    foreach (var imageSet in images)
                    {
                        ProductSupplementalData product;
                        if (!dic.TryGetValue(imageSet.ProductId, out product))
                            continue;

                        var rec = new ImagesViewData(imageSet, product);
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
            ExportManager.SaveExcelFile(data.Cast<ImagesViewData>(), suggestedName);
        }

    }
}
