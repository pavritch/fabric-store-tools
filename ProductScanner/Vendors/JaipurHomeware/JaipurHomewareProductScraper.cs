using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace JaipurHomeware
{
    public class JaipurHomewareProductScraper : ProductScraper<JaipurHomewareVendor>
    {
        private const string ThrowProductUrl = "http://www.jaipurrugs.com/throwsshowproductdescription.aspx?code={0}";
        private const string PillowProductUrl = "http://www.jaipurrugs.com/pillowshowproductdescription.aspx?code={0}";
        private const string PoufProductUrl = "http://www.jaipurrugs.com/poufsshowproductdescription.aspx?code={0}";

        private Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Catalog Code", ScanField.SKU },
            { "Design", ScanField.Design },
            { "Color", ScanField.Color },
            { "Construction", ScanField.Construction },
            { "Backing", ScanField.Backing },
            { "Closure", ScanField.Closure },
            { "Style", ScanField.Style },
            { "Content", ScanField.Content },
            { "Origin", ScanField.Country },
            { "Pantone TPG", ScanField.Ignore },
            { "Shape", ScanField.Ignore },
            { "Product Height", ScanField.Height },
        }; 

        public JaipurHomewareProductScraper(IPageFetcher<JaipurHomewareVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var detailsPage = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri.Replace("&#39;", "'").Replace("&#232;", "e").Replace("&#160;", " "), CacheFolder.Details,
                product.DetailUrl.AbsoluteUri.Replace("https://www.jaipurliving.com/pillows/", "").Replace("&#39;", "'"));

            var scanData = new ScanData();
            var details = detailsPage.QuerySelectorAll("#ProductDetails tr");
            foreach (var detail in details)
            {
                var cols = detail.QuerySelectorAll("td").ToList();
                var label = cols[0].InnerText.Trim().Trim(':');
                var value = cols[1].InnerText.Trim();
                var field = _fields[label];
                scanData[field] = value;
            }

            var mpn = detailsPage.QuerySelector("#ProductId").Attributes["value"].Value;
            scanData[ScanField.ManufacturerPartNumber] = mpn;

            var imageUrl = detailsPage.QuerySelector("#picture-frame img").Attributes["src"].Value;
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

            var table4 = detailsPage.QuerySelector("#Table4");

            var stockPriceTable = detailsPage.QuerySelector("#Table3 table tbody");
            if (stockPriceTable == null) stockPriceTable = detailsPage.QuerySelector("#Table1 table tbody");
            var headers = stockPriceTable.QuerySelector("tr").QuerySelectorAll("th").Select(x => Regex.Replace(x.InnerText.Trim(), @"\s+", " ", RegexOptions.Multiline)).ToList();
            var productRows = stockPriceTable.QuerySelectorAll("tr").Where(x => x.ParentNode == stockPriceTable).ToList();

            foreach (var row in productRows)
            {
                var columns = row.QuerySelectorAll("td").ToList();
                if (!columns.Any()) continue;

                for (var i = 0; i < headers.Count; i++)
                {
                    var header = headers[i];
                    if (header == "Size (Feet)") scanData[ScanField.Size] = columns[i].InnerText.Replace("&quot;", "\"").ToLower();
                    if (header == "Shape") scanData[ScanField.Shape] = columns[i].InnerText;
                    if (header == "Quantity In-Stock") scanData[ScanField.StockCount] = columns[i].InnerText;

                    if (table4 == null)
                    {
                        if (header == "Fill") scanData[ScanField.Type] = columns[i].InnerText;
                        if (header == "Unit Price") scanData.Cost = columns[i].InnerText.ToDecimalSafe();
                        if (header == "MSRP") scanData[ScanField.RetailPrice] = columns[i].InnerText;
                    }
                }
            }

            if (table4 != null)
            {
                var variants = new List<ScanData>();
                var rows = table4.QuerySelectorAll("tr").Skip(1).ToList();
                // Select, Fill, Qty, Unit Price, Price
                foreach (var row in rows)
                {
                    var cells = row.QuerySelectorAll("td").ToList();
                    var cost = cells[3].InnerText.Trim();
                    var msrp = cells[4].InnerText.Trim();
                    var size = scanData[ScanField.Size];
                    var type = cells[1].InnerText.Trim();

                    var variant = new ScanData();
                    variant[ScanField.Shape] = scanData[ScanField.Shape];
                    variant[ScanField.Size] = size;
                    variant[ScanField.StockCount] = scanData[ScanField.StockCount];
                    variant[ScanField.ManufacturerPartNumber] = scanData[ScanField.SKU] + " " + size + " " + type;
                    variant[ScanField.RetailPrice] = msrp;
                    variant[ScanField.ProductName] = size + " " + type;
                    variant[ScanField.SKU] = "-" + variant[ScanField.ProductName].Replace("\"", "").Replace("x", "").Replace(" Down Fill", "D").Replace(" Polly Fill", "P");
                    variant.Cost = cost.Replace("$", "").ToDecimalSafe();
                    variants.Add(variant);
                }
                scanData.Variants = variants;
            }

            scanData[ScanField.ProductName] = detailsPage.GetFieldValue("h1.text-orange").Replace("New", "");

            // SKU
            // productname
            // description

            return new List<ScanData> { scanData };
        }

        private async Task<ScanData> ScrapePoufData(string mpn)
        {
            var url = string.Format(PoufProductUrl, mpn);
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);
            if (!detailsPage.InnerText.Contains("My Account")) throw new AuthenticationException();

            var scanData = new ScanData();
            scanData[ScanField.ProductName] = detailsPage.GetFieldValue(".product-title");
            scanData[ScanField.Description] = detailsPage.GetFieldValue(".ProductNameRugs");
            scanData.DetailUrl = new Uri(url);

            scanData[ScanField.SKU] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_PoufsDescriptionMain_lblSKU");
            scanData[ScanField.ManufacturerPartNumber] = mpn;
            scanData[ScanField.Design] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_PoufsDescriptionMain_lblDesign");
            scanData[ScanField.Color] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_PoufsDescriptionMain_lblColor");
            scanData[ScanField.Construction] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_PoufsDescriptionMain_lblConstruction");
            scanData[ScanField.Style] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_PoufsDescriptionMain_lblstyle");
            scanData[ScanField.Content] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_PoufsDescriptionMain_lblFiber");
            scanData[ScanField.Country] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_PoufsDescriptionMain_lblOrigin");
            scanData[ScanField.Cleaning] = detailsPage.GetFieldValue("#tab-2 .tab-body");

            var stockRow = detailsPage.QuerySelectorAll("#ContentPlaceHolder1_uc_PoufsDescriptionMain_GVAddCart tr").Skip(1).First();
            var columns = stockRow.QuerySelectorAll("td").ToList();
            var size = columns[1].InnerText.Trim();
            var shape = columns[2].QuerySelector("img").Attributes["title"].Value;
            var inventory = columns[3].InnerText.Trim();
            var price = columns[8].InnerText.Trim();
            scanData[ScanField.Shape] = shape;
            scanData[ScanField.Dimensions] = size;
            scanData[ScanField.StockCount] = inventory;
            scanData.Cost = Convert.ToDecimal(price.TakeOnlyLastIntegerToken());

            var imageUrl = detailsPage.QuerySelector("#ContentPlaceHolder1_uc_PoufsDescriptionMain_hlkLarge").Attributes["href"].Value;
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));
            return scanData;
        }
    }
}