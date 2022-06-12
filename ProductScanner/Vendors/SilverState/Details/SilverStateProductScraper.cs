using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace SilverState.Details
{
    public class SilverStateProductScraper : ProductScraper<SilverStateVendor>
    {
        private readonly Dictionary<string, ScanField> _labels = new Dictionary<string, ScanField>
        {
            { "Pattern:", ScanField.PatternName },
            { "Color:", ScanField.Color },
            { "Brand:", ScanField.Brand },
            { "Application:", ScanField.ProductUse },
            { "Width:", ScanField.Width },
            { "Contents:", ScanField.Content },
            { "Repeat:", ScanField.Repeat },
            { "Finish:", ScanField.Finish },
            { "Cleaning Code:", ScanField.Cleaning },
            { "Abrasion:", ScanField.Durability },
            { "Fire Retardant Specs:", ScanField.FireCode },
            { "Railroaded:", ScanField.Railroaded },
            { "Country of Origin:", ScanField.Country },
            { "Collection:", ScanField.Collection },
            { "Use-ACT Standards:", ScanField.Use },
            { "Color Match:", ScanField.Ignore },
        }; 

        public SilverStateProductScraper(IPageFetcher<SilverStateVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct discoveredProduct)
        {
            var webNumber = discoveredProduct.ScanData[ScanField.WebItemNumber];
            var url = string.Format("https://www.silverstatetextiles.com/storefrontCommerce/itemLookup.jsp?itemNum={0}&p_filters_search=Product+Name+or+ID", webNumber);
            webNumber = webNumber.Replace("%2A", "");
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, webNumber);
            if (page.InnerText.ContainsIgnoreCase("No products to display")) return new List<ScanData>();

            var product = new ScanData();

            var dataFields = page.QuerySelectorAll(".resource_search_item_data_field").ToList();
            foreach (var field in dataFields)
            {
                var labelElement = field.QuerySelector(".item_detail_spec_label");
                if (labelElement == null) continue;

                var label = labelElement.InnerText;
                var value = field.QuerySelector(".item_detail_spec_detail").InnerText;

                var property = _labels[label];
                product[property] = value;
            }

            var price = page.GetFieldValue(".item_price");
            if (price != null) price = price.Replace("$", "");

            var detailForm = page.QuerySelector("form[name='itemDetailForm']");
            if (detailForm == null) return new List<ScanData>();

            var coordinates = page.QuerySelector(".resource_search_item_images_colors_title:contains('Coordinates')");
            var images = coordinates.ParentNode.QuerySelectorAll("img").ToList();
            var mpns = string.Join(", ", images.Select(x => FindMPN(x.Attributes["title"].Value)).ToList());
            product[ScanField.Coordinates] = mpns;

            var unit = page.GetFieldValue(".item_price_label");
            var itemIdElement = page.QuerySelector("input[name='itm_id']");
            if (itemIdElement == null) return new List<ScanData>();

            // this seems like it shouldn't be here, but not sure how else to do it
            var colorName = Regex.Replace(product[ScanField.Color], @"[0-9]", "").Trim();
            var mpn = string.Format("{0}-{1}", FormatPatternName(product[ScanField.PatternName]), colorName).ToUpper();
            product[ScanField.ManufacturerPartNumber] = mpn.SkuTweaks().Replace("*LIMITEDSTOCK*", "").Replace("*", "");
            product.Cost = price.ToDecimalSafe();
            product.DetailUrl = new Uri(url);

            product[ScanField.Status] = page.GetFieldValue(".resource_search_item_project_title");
            product.IsClearance = page.InnerText.ContainsIgnoreCase("Outlet Price");
            product.AddImage(new ScannedImage(ImageVariantType.Primary, 
                "https://www.silverstatetextiles.com" + page.QuerySelector("#popup_image img").Attributes["src"].Value));

            if (product.ContainsKey(ScanField.Collection))
            {
                var collection = product[ScanField.Collection];
                if (collection.ContainsIgnoreCase("trim"))
                    product[ScanField.ProductGroup] = "Trim";
                else
                {
                    product[ScanField.ProductGroup] = "Fabric";
                }
            }

            if (unit.ContainsIgnoreCase("Yard")) product[ScanField.UnitOfMeasure] = "Yard";
            else 
                product[ScanField.UnitOfMeasure] = "Other";
            return new List<ScanData> {product};
        }

        private string FindMPN(string coordMpn)
        {
            return string.Join("-", coordMpn.Split(new[] {' '}).Where(x => !x.IsInteger())).ToUpper();
        }

        private string FormatPatternName(string patternName)
        {
            if (!patternName.ContainsDigit()) return patternName.TitleCase();

            var numberAndName = patternName.Split(new[] { ' ' });
            return numberAndName.Last().TitleCase();
        }
    }
}