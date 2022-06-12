using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace RalphLauren.Details
{
    public class RalphLaurenProductScraper : ProductScraper<RalphLaurenVendor>
    {
        public RalphLaurenProductScraper(IPageFetcher<RalphLaurenVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var key = context.ScanData[ScanField.ManufacturerPartNumber];
            var url = context.DetailUrl.AbsoluteUri;
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, key);
            if (page.InnerHtml.ContainsIgnoreCase("Please enter your customer account number")) throw new AuthenticationException();

            var stockCount = page.GetFieldValue("td:contains('Stock Available') + td");
            var category = page.GetFieldValue("td:contains('Product category') + td");

            var product = new ScanData(context.ScanData);
            product[ScanField.PatternName] = page.GetFieldValue("td:contains('Description') + td");
            product[ScanField.ColorName] = page.GetFieldValue("td:contains('Color') + td");
            product[ScanField.HorizontalRepeat] = page.GetFieldValue("td:contains('Horizontal Repeat') + td");
            product[ScanField.VerticalRepeat] = page.GetFieldValue("td:contains('Vertical Repeat') + td");
            product[ScanField.Country] = page.GetFieldValue("td:contains('Country of origin') + td");
            product[ScanField.Content] = page.GetFieldValue("td:contains('Fiber Content') + td");
            product[ScanField.Category] = category;
            product[ScanField.Width] = page.GetFieldValue("td:contains('Width') + td");
            product[ScanField.StockCount] = stockCount;
            product[ScanField.Book] = page.GetFieldValue("td:contains('Book Name') + td");

            product.Cost = page.GetFieldValue("td:contains('Your Cost') + td").Replace("$", "").ToDecimalSafe();

            if (page.InnerText.ContainsIgnoreCase("This item is discontinued"))
            {
                if (stockCount.TakeOnlyFirstIntegerToken() > 0)
                {
                    product.IsLimitedAvailability = true;
                }
                else
                {
                    product.IsDiscontinued = true;
                }
            }

            // determine product group
            // use dic lookup to figure out kind of product
            string productGroup;
            if (dicProductGroups.TryGetValue(category.ToUpper(), out productGroup))
            {
                // some of the lookup values will not be ones we want, so filter on what is valid
                if (productGroup.IsValidProductGroup())
                    product[ScanField.ProductGroup] = productGroup;
                else
                    return new List<ScanData>();
            }

            // see if have enough information to determine unit of measure
            // the problem is when stock is none, it just says None and no hint as to UoM
            string value = null;

            if (stockCount.ContainsIgnoreCase("yards"))
                value = "Yard";
            else if (stockCount.ContainsIgnoreCase("rolls"))
                value = "Roll";
            else if (stockCount.ContainsIgnoreCase("each"))
                value = "Each";
            else if (stockCount.ContainsIgnoreCase("square feet"))
                value = "Square Foot";

            if (value != null)
                product[ScanField.UnitOfMeasure] = value;
            else
            {
                if (product[ScanField.Content].ContainsIgnoreCase("roll"))
                    product[ScanField.UnitOfMeasure] = "Roll";
                else
                {
                    product[ScanField.UnitOfMeasure] = "Yard";
                }
            }
            return new List<ScanData> {product};
        }

        // Lookup table for relating phrases found in Category field to which prdouct group.
        private static readonly Dictionary<string, string> dicProductGroups = new Dictionary<string, string>()
        {
            { "FABRIC WOVENS", "Fabric" },
            { "FABRIC PRINTS", "Fabric" },
            { "LEATHER", "Fabric" },

            { "WALLPAPERS", "Wallcovering" },

            { "BORDERS", "Trim" },
            { "GREIGE", "Trim" },
            { "TRIMMING", "Trim" },
            { "FURNITURE FRINGE", "Trim" },
            { "CHAIR TIE", "Trim" },
            { "TIEBACK", "Trim" },
            { "KEY TASSLE", "Trim" },
            { "TASSLE FRINGE", "Trim" },
            { "GIMP", "Trim" },
            { "CORDS", "Trim" },
            { "BULLION", "Trim" },
            { "RUCHE", "Trim" },

            // not wanted, gets filtered out

            { "NOT KNOWN", "" },
            { "PURCHASE SAMPLES", "" },
            { "MISCELLANEOUS ITEMS", "" },
        };
    }
}