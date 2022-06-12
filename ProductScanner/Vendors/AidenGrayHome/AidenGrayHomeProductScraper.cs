using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace AidanGrayHome
{
    public class AidenGrayHomeProductScraper : ProductScraper<AidanGrayHomeVendor>
    {
        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Additional Measurements", ScanField.Ignore },
            { "Additional Measurments", ScanField.Ignore },
            { "Addittional Measurements", ScanField.Ignore },
            { "Addittional Measurments", ScanField.Ignore },
            { "Array", ScanField.Ignore },
            { "Assembly Instruction", ScanField.AssemblyInstruction },
            { "Base of Shade (in inches)", ScanField.Ignore },
            { "Between Shelves", ScanField.Ignore },
            { "Bottom Clearence", ScanField.Ignore },
            { "Bottom Depth of Live Space", ScanField.Ignore },
            { "Bottom Width of Live Space", ScanField.Ignore },
            { "Box Weight", ScanField.Ignore },
            { "Bulb", ScanField.Bulbs },
            { "Bulb(s)", ScanField.Bulbs },
            { "C3 Related Technique", ScanField.Ignore },
            { "Collection", ScanField.Collection },
            { "Color", ScanField.ColorGroup },
            { "Color/Finish", ScanField.Color },
            { "Cord Cover", ScanField.Ignore },
            { "Cord Lenght", ScanField.CordLength },
            { "Cord Length", ScanField.CordLength },
            { "Crystall Small", ScanField.Ignore },
            { "Crystal Large", ScanField.Ignore },
            { "Cushion", ScanField.Ignore },
            { "Cushion Type", ScanField.Ignore },
            { "Depth", ScanField.Depth },
            { "Depth of Base", ScanField.Ignore },
            { "Depth Of Base (in inches)", ScanField.Ignore },
            { "Depth of Seat", ScanField.Ignore },
            { "Depth of Shade (in inches)", ScanField.Ignore },
            { "Dimensions 4", ScanField.Ignore },
            { "Dimension 5", ScanField.Ignore },
            { "Estimated Yardage", ScanField.Ignore },
            { "Fabric", ScanField.Ignore },
            { "Fabric Secondary", ScanField.Ignore },
            { "Fabric Type", ScanField.Ignore },
            { "Feature ID", ScanField.Ignore },
            { "Floor to Arm Rest", ScanField.Ignore },
            { "Floor to top of Seat", ScanField.Ignore },
            { "Hardware Color", ScanField.Ignore },
            { "Harp Size (in inches)", ScanField.Ignore },
            { "Height", ScanField.Height },
            { "Height of Back Rest", ScanField.Ignore },
            { "Height of Base", ScanField.Ignore },
            { "Height Of Base (in inches)", ScanField.Ignore },
            { "Height Of Live Space", ScanField.Ignore },
            { "Height of Mirror", ScanField.Ignore },
            { "Height Of Shade (in inches)", ScanField.Ignore },
            { "Item Size", ScanField.Ignore },
            { "Lamp Hardware", ScanField.Ignore },
            { "Large", ScanField.Ignore },
            { "Material 1", ScanField.Material },
            { "Material 2", ScanField.Material },
            { "Material 3", ScanField.Material },
            { "Material 4", ScanField.Material },
            { "Medium", ScanField.Ignore },
            { "Product Dimensions 7(Length of longest arm)", ScanField.Ignore },
            { "Product Dimensions 8 (Max diametar of candle)", ScanField.Ignore },
            { "Product Volume (CBM)", ScanField.Ignore },
            { "Product Weight", ScanField.Weight },
            { "Secondary Fabric Type", ScanField.Ignore },
            { "Shade Color 1", ScanField.Ignore },
            { "Shade Color 2", ScanField.Ignore },
            { "Shade Type", ScanField.Ignore },
            { "Shipping Box Width", ScanField.PackageWidth },
            { "Shipping Box Height", ScanField.PackageHeight },
            { "Shipping Box Depth", ScanField.Ignore },
            { "Shipping oversize item Cost", ScanField.Ignore },
            { "Shipping Weight", ScanField.ShippingWeight },
            { "Small", ScanField.Ignore },
            { "SKU", ScanField.Ignore }, // same as MPN
            { "Switch Type", ScanField.Ignore },
            { "Technical Specification", ScanField.Ignore },
            { "Technical Specifcation", ScanField.Ignore },
            { "Top of Shade (in inches)", ScanField.Ignore },
            { "Top Depth of Live Space", ScanField.Ignore },
            { "Top Width of Live space", ScanField.Ignore },
            { "Ups Oversize", ScanField.Ignore },
            { "UPC Code", ScanField.UPC },
            { "Wattage", ScanField.Wattage },
            { "Width", ScanField.Width },
            { "Width of Back Rest", ScanField.Ignore },
            { "Width of Base", ScanField.BaseWidth },
            { "Width Of Base(in inches)", ScanField.BaseWidth },
            { "Width of Mirror", ScanField.Ignore },
            { "Width of Seat", ScanField.Ignore },
            { "Xlarge", ScanField.Ignore },
            { "Xsmall", ScanField.Ignore },
            { "Depth Of Base", ScanField.Ignore },
            { "Harp Size", ScanField.Ignore },
            { "Top of Shade", ScanField.Ignore },
            { "Width Of Base", ScanField.Ignore },
            { "Base of Shade", ScanField.Ignore },
            { "Depth of Shade", ScanField.Ignore },
            { "Height Of Base", ScanField.Ignore },
            { "Height Of Shade", ScanField.Ignore },
        }; 

        public AidenGrayHomeProductScraper(IPageFetcher<AidanGrayHomeVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var name = product.DetailUrl.GetDocumentName();
            var detailPage = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details, name);
            var nameField = detailPage.GetFieldValue(".product-name");
            var split = nameField.Split(new[] {'#'});

            var scanData = new ScanData(product.ScanData);
            scanData[ScanField.Category] = string.Join(", ", detailPage.QuerySelectorAll(".breadcrumbs a").Select(x => x.InnerText).Skip(1).ToList());
            //scanData[ScanField.TempContent1] = detailPage.GetFieldValue(".product");
            scanData[ScanField.ProductName] = split.First();
            scanData[ScanField.ManufacturerPartNumber] = split.Last();
            scanData[ScanField.RetailPrice] = detailPage.GetFieldValue(".special-price .price");
            if (string.IsNullOrWhiteSpace(scanData[ScanField.RetailPrice]))
                scanData[ScanField.RetailPrice] = detailPage.GetFieldValue(".regular-price");
            scanData[ScanField.Description] = detailPage.GetFieldValue(".collateral-box .std");
            scanData[ScanField.StockCount] = detailPage.GetFieldValue(".stock_status");
            scanData[ScanField.Note] = detailPage.GetFieldValue(".short-description .std");

            scanData.Cost = scanData[ScanField.RetailPrice].Replace("$", "").Replace(",", "").ToDecimalSafe();
            scanData.DetailUrl = product.DetailUrl;

            var image = detailPage.QuerySelector("#zoom_img_media");
            if (image == null)
            {
                image = detailPage.QuerySelectorAll("img[itemprop='image']").SingleOrDefault(x => x.Attributes.Contains("src"));
                if (image == null) return new List<ScanData>();
            }
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Attributes["src"].Value));

            var specs = detailPage.QuerySelectorAll("#product-attribute-specs-table tr").ToList();
            foreach (var spec in specs)
            {
                var cells = spec.QuerySelectorAll("td").ToList();
                if (!cells.Any()) continue;

                var label = cells.First().InnerText;
                var value = cells.Last().InnerText;
                if (value == "No") continue;
                if (value == "N/A") continue;

                var field = _fields[label];
                if (scanData.ContainsKey(field))
                    scanData[field] += "," + value;
                else
                    scanData[field] = value;
            }
            return new List<ScanData> { scanData };
        }
    }
}