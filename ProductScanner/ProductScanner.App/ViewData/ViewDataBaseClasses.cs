using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProductScanner.Core.DataInterfaces;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.App
{
    public abstract class ViewDataBase
    {
        /// <summary>
        /// Convert gzipped JSON data back into some typesafe object model; typically a List of something.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gzipJsonData"></param>
        /// <returns></returns>
        static protected T CommitDataList<T>(byte[] gzipJsonData)
        {
            var commitJson = gzipJsonData.UnGZipMemoryToString();
            var list = commitJson.JSONtoList<T>();
            return list;
        }

        /// <summary>
        /// This is identical to how it's done in core.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static protected byte[] MakeGZippedJsonDataForTesting(object obj)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None }; // TODO: this was "All" but had trouble
            var json = obj.ToJSON(settings);
            var zipped = json.GZipMemory();
            return zipped;
        }

        public static Task<List<IViewData>> CreateDataSetAsync(IStoreDatabaseConnector dbStore, string storeKey, byte[] gzipJsonData)
        {
            throw new NotImplementedException("Derrived class must implment this.");
        }

        public static void Export(IEnumerable<IViewData> data, string suggestedName)
        {
            throw new NotImplementedException("Derrived class must implment this.");
        }

        public abstract ICommitRecordDetails GetDetails();

#if DEBUG
        /// <summary>
        /// Generate a simple list of int for testing. Returned json gzipped.
        /// </summary>
        /// <returns></returns>
        protected static byte[] GenerateDummyIntList(int count)
        {
            var list = new List<int>();
            for (int i = 0; i < count; i++)
                list.Add(1000 + i);

            var bytes = MakeGZippedJsonDataForTesting(list);
            return bytes;
        }
#endif

    }

    /// <summary>
    /// Common base for view data classes which work with products (as opposed to variants).
    /// </summary>
    public class SimpleProductViewData : ViewDataBase, IViewData
    {
        [Description("Product ID")]
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }

        [Description("Product Group")]
        public string ProductGroup { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string StoreUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string VendorUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        public string ImageUrl { get; set; }

        public SimpleProductViewData()
        {

        }

        public SimpleProductViewData(ProductSupplementalData p)
        {
            ProductID = p.ProductID;
            SKU = p.SKU;
            Name = p.Name;
            ProductGroup = p.ProductGroup;
            StoreUrl = p.StoreUrl;
            VendorUrl = p.VendorUrl;
            ImageUrl = p.ImageUrl;
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
                JSON = ProductID.ToJSON(),
                ProductImages = null,
                ProductVariants = null,
            };

            return details;
        }
    }

    /// <summary>
    /// Common base for view data classes which work with variants (as opposed to products).
    /// </summary>
    public class SimpleVariantViewData : ViewDataBase, IViewData
    {
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

        [ExcelColumn(ExcelColumnType.Url)]
        public string ImageUrl { get; set; }

        public SimpleVariantViewData()
        {

        }

        public SimpleVariantViewData(VariantSupplementalData v)
        {
            VariantID = v.VariantID;
            ProductID = v.ProductID;
            SKU = v.SKU; // this is already combined with product SKU
            Name = v.Name;
            ProductGroup = v.ProductGroup;
            UnitOfMeasure = v.UnitOfMeasure;
            StoreUrl = v.StoreUrl;
            VendorUrl = v.VendorUrl;
            ImageUrl = v.ImageUrl;
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
                JSON = VariantID.ToJSON(),
                ProductImages = null,
                ProductVariants = null,
            };

            return details;
        }

    }

}
