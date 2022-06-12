using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Extensions;

namespace ProductScanner.App
{
    /// <summary>
    /// Object which is populated to export a spreadsheet. One for each vendor.
    /// </summary>
    public class VendorExportRecord
    {

        public int VendorID { get; set; }

        public string Name { get; set; }

        [Description("Fully Implemented")]
        public bool IsFullyImplemented { get; set; }

        [Description("Ready for Live")]
        public bool ReadyForLive { get; set; }

        public string Store { get; set; }

        [Description("SKU Prefix")]
        public string SKUPrefix { get; set; }

        [Description("Module Filename")]
        public string VendorModuleFilename { get; set; }

        [Description("Product Groups")]
        public string ProductGroups { get; set; }

        [Description("Capabilities")]
        public string VendorCapabilities { get; set; }

        [Description("Is Stock Checker Functional")]
        public bool IsStockCheckerFunctional { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("Total Products")]
        public int ProductCount { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("Discontinued Products")]
        public int DiscontinuedCount { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("In Stock Products")]
        public int InStockProductCount { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("Out of Stock Products")]
        public int OutOfStockProductCount { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("Clearance Products")]
        public int ClearanceProductsCount { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("Total Variants")]
        public int VariantCount { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("In Stock Variants")]
        public int InStockVariantCount { get; set; }

        [ExcelColumn(ExcelColumnType.Number)]
        [Description("Out of Stock Variants")]
        public int OutOfStockVariantCount { get; set; }

        [Description("Minimum Price")]
        public string MinimumPrice { get; set; }

        [Description("Minimum Cost")]
        public string MinimumCost { get; set; }

        [Description("Uses IMAP Pricing")]
        public bool UsesIMAP { get; set; }

        [Description("Our Markup")]
        public string OurPriceMarkup { get; set; }

        [Description("Retail Markup")]
        public string RetailPriceMarkup { get; set; }

        [Description("Dev Comments")]
        public string DeveloperComments { get; set; }

        [Description("Discovery Notes")]
        public string DiscoveryNotes { get; set; }

        [Description("Clearance Supported")]
        public bool IsClearanceSupported { get; set; }

        [Description("Has Swatches")]
        public bool HasSwatches { get; set; }

        [Description("Static Files")]
        public bool UsesStaticFiles { get; set; }

        [Description("Static File Version (DLL)")]
        public int StaticFileVersionDLL { get; set; }

        [Description("Static File Version (version.txt)")]
        public int StaticFileVersionTxt { get; set; }

        [Description("Newest Static File")]
        public string NewestStaticFile { get; set; }

        [Description("Stock Check API")]
        public bool HasStockCheckApi { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("Swatch Cost")]
        public decimal? SwatchCost { get; set; }

        [ExcelColumn(ExcelColumnType.Money)]
        [Description("Swatch Price")]
        public decimal? SwatchPrice { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        [Description("Login Url")]
        public string LoginUrl { get; set; }

        [ExcelColumn(ExcelColumnType.Url)]
        [Description("Public Url")]
        public string PublicUrl { get; set; }

        [Description("Website Login")]
        public string WebsiteUsername { get; set; }

        [Description("Website Password")]
        public string WebsitePassword { get; set; }


        public VendorExportRecord()
        {

        }

        /// <summary>
        /// Fill out this object based on the provided vendor.
        /// </summary>
        /// <param name="vendor"></param>
        public VendorExportRecord(IVendorModel v)
        {
            Name = v.Name;
            Store = v.ParentStore.Key.ToString();
            WebsiteUsername = v.VendorWebsiteUsername;
            WebsitePassword = v.VendorWebsitePassword;

            var vendor = v.Vendor;

            LoginUrl = vendor.LoginUrl;

            VendorID = vendor.Id;
            IsFullyImplemented = vendor.IsFullyImplemented;

            PublicUrl = v.VendorWebsiteUrl;

            SKUPrefix = vendor.SkuPrefix;
            VendorModuleFilename = vendor.GetVendorModuleFilename();
            ProductGroups = string.Join(", ", vendor.ProductGroups.Select(x => x.ToString()));
            VendorCapabilities = vendor.StockCapabilities.ToString();

            MinimumPrice = vendor.MinimumPrice.ToString();
            MinimumCost = vendor.MinimumCost.ToString();
            OurPriceMarkup = vendor.GetOurPriceMarkupDescription();
            RetailPriceMarkup = vendor.GetRetailPriceMarkupDescription();
            UsesIMAP = vendor.UsesIMAP;

            DeveloperComments = vendor.DeveloperComments;
            DiscoveryNotes = vendor.DiscoveryNotes;
            IsClearanceSupported = vendor.IsClearanceSupported;
            HasSwatches = vendor.SwatchesEnabled;
            UsesStaticFiles = vendor.UsesStaticFiles;
            StaticFileVersionDLL = vendor.StaticFileVersion;

            HasStockCheckApi = vendor.HasStockCheckApi;

            SwatchCost = vendor.SwatchCost;
            SwatchPrice = vendor.SwatchPrice;

            if (!vendor.IsFullyImplemented)
                return;

            ProductCount = v.ProductCount;
            DiscontinuedCount = v.DiscontinuedProductCount;
            InStockProductCount = v.InStockProductCount;
            OutOfStockProductCount = v.OutOfStockProductCount;
            ClearanceProductsCount = v.ClearanceProductCount;
            VariantCount = v.ProductVariantCount;
            InStockVariantCount = v.InStockProductVariantCount;
            OutOfStockVariantCount = v.OutOfStockProductVariantCount;
            StaticFileVersionTxt = v.StaticFileVersionTxt;

            // not the best way of doing this - await is better
            var metrics = Task.Run(() => v.GetStaticFileStorageMetricsAsync(false)).Result;
            NewestStaticFile = metrics.Newest == null ? string.Empty : metrics.Newest.Value.ToShortDateString();

            IsStockCheckerFunctional = vendor.IsStockCheckerFunctional;
            ReadyForLive = vendor.ReadyForLive;
        }

        public List<KeyValuePair<string, string>> GetProperties()
        {
            var list = new List<KeyValuePair<string, string>>();

            foreach (var memberInfo in typeof(VendorExportRecord).MemberProperties())
            {
                var title = memberInfo.Name;
                if (memberInfo.HasAttribute(typeof(DescriptionAttribute)))
                    title = memberInfo.GetAttributes<DescriptionAttribute>(false).First().Description;

                string value = string.Empty;

                try
                {
                    var objValue = this.GetPropertyValue(memberInfo.Name);
                    if (objValue != null)
                        value = objValue.ToString();
                }
                catch (Exception Ex)
                {
                    string.Format("Problem with {0}: {1}", memberInfo.Name, Ex.Message);
                }

                var kv = new KeyValuePair<string, string>(title, value);
                list.Add(kv);
            }

            return list;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var item in GetProperties())
                sb.AppendFormat("{0}: {1}\n", item.Key, item.Value);

            return sb.ToString();            
        }
    }
}
