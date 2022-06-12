using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities;
using Utilities.Extensions;

namespace BlueMountain.Details
{
    public class BlueMountainProductScraper : ProductScraper<BlueMountainVendor>
    {
        private const string StockUrl = "https://extranet.itgeneration.ca/EXTRANET-BMW_BPP/CustomerService/InventoryInquiry/inventory.aspx?part_no={0}";
        private const string DetailUrl = "http://www.designbycolor.net/en/info.aspx?child={0}";
        public BlueMountainProductScraper(IPageFetcher<BlueMountainVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.GetMPN();
            var detailUrl = string.Format(DetailUrl, mpn);
            var detailPage = await PageFetcher.FetchAsync(detailUrl, CacheFolder.Details, mpn);
            var stockPage = await PageFetcher.FetchAsync(string.Format(StockUrl, mpn), CacheFolder.Stock, mpn);
            return new List<ScanData> {CreateProduct(product, detailPage, stockPage, detailUrl)};
        }

        private ScanData CreateProduct(DiscoveredProduct discoveredProduct, HtmlNode detailPage, HtmlNode stockPage, string detailUrl)
        {
            var priceInfo = detailPage.GetFieldValue("#lblPriceSWL");
            var coverage = detailPage.GetFieldValue("#lblCoverage");
            var productType = detailPage.GetFieldValue("#lblProductType");
            var match = detailPage.GetFieldValue("#lblMatch");
            var repeat = detailPage.GetFieldValue("#lblRepeat");
            var cleaning = detailPage.GetFieldValue("#lblCleaning");
            var removal = detailPage.GetFieldValue("#lblRemoval");
            var width = detailPage.GetFieldValue("#lblWidth");
            var length = detailPage.GetFieldValue("#lblLength");
            var paste = detailPage.GetFieldValue("#lblPaste");
            var height = detailPage.GetFieldValue("#lblHeight");

            var unit = stockPage.GetFieldValue("#ContentPlaceHolder1_lbluom");
            var country = stockPage.GetFieldValue("#ContentPlaceHolder1_lblCountry_Desc");
            var price = stockPage.GetFieldValue("#ContentPlaceHolder1_lblPrice").Replace("USD", "").Trim();
            var stock = stockPage.GetFieldValue("#ContentPlaceHolder1_lblInventoryBAvailable");
            var patternName = stockPage.GetFieldValue("#ContentPlaceHolder1_lblItemDescription");
            var skuNo = stockPage.GetFieldValue("#ContentPlaceHolder1_lblSku_no");
            var substrate = stockPage.GetFieldValue("#ContentPlaceHolder1_lblsubstratedescription");
            var stkCode = stockPage.GetFieldValue("#ContentPlaceHolder1_lblStkCodeDescription");
            var weight = stockPage.GetFieldValue("#ContentPlaceHolder1_lblweight_ea");
            var casePack = stockPage.GetFieldValue("#ContentPlaceHolder1_lblcase_pack");
            var upc = stockPage.GetFieldValue("#ContentPlaceHolder1_lblUPC_code");
            var obsolete = stockPage.GetFieldValue("#ContentPlaceHolder1_lblObsolete");

            if (stock.ContainsIgnoreCase("OBS")) stock = "0";
            if (stock.ContainsIgnoreCase("BAK")) stock = "0";

            var product = new ScanData();
            product[ProductPropertyType.ManufacturerPartNumber] = discoveredProduct.GetMPN();
            product[ProductPropertyType.ColorGroup] = discoveredProduct.ScanData[ProductPropertyType.ColorGroup];
            product[ProductPropertyType.Category] = discoveredProduct.ScanData[ProductPropertyType.Category];
            product[ProductPropertyType.ItemNumber] = discoveredProduct.ScanData[ProductPropertyType.ItemNumber];
            product[ProductPropertyType.ProductDetailUrl] = detailUrl;

            product.ProductImages.Add(new ProductImage(ImageVariantType.Primary, discoveredProduct.ScanData[ProductPropertyType.ImageUrl]));

            product[ProductPropertyType.UnitOfMeasure] = unit;
            product[ProductPropertyType.Note] = priceInfo;
            product[ProductPropertyType.Coverage] = coverage;
            product[ProductPropertyType.ProductType] = productType;
            product[ProductPropertyType.Match] = match;
            product[ProductPropertyType.Repeat] = repeat;
            product[ProductPropertyType.Cleaning] = cleaning;
            product[ProductPropertyType.Removal] = removal;
            product[ProductPropertyType.Width] = width;
            product[ProductPropertyType.Length] = length;
            product[ProductPropertyType.Paste] = paste;
            product[ProductPropertyType.Height] = height;

            product[ProductPropertyType.CountryOfOrigin] = country;
            product[ProductPropertyType.WholesalePrice] = price;
            product[ProductPropertyType.StockCount] = stock;
            product[ProductPropertyType.PatternName] = patternName.Replace(discoveredProduct.GetMPN(), "").Trim();
            product[ProductPropertyType.SKU] = skuNo;
            product[ProductPropertyType.Material] = substrate;
            product[ProductPropertyType.TempContent1] = stkCode;
            product[ProductPropertyType.Weight] = weight;
            product[ProductPropertyType.TempContent2] = casePack;
            product[ProductPropertyType.UPC] = upc;
            product[ProductPropertyType.TempContent3] = obsolete;
            return product;
        }
    }
}