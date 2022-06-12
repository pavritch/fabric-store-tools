using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Greenhouse.Details
{
    public class GreenhouseProductScraper : ProductScraper<GreenhouseVendor>
    {
        public GreenhouseProductScraper(IPageFetcher<GreenhouseVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct discoveredProduct)
        {
            var mpn = discoveredProduct.MPN;
            var url = "https://www.greenhousefabrics.com/fabric/" + mpn;
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);
            if (detailsPage.InnerText.ContainsIgnoreCase("Access Denied")) return new List<ScanData>();
            if (detailsPage.InnerText.ContainsIgnoreCase("Log In"))
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, mpn);
                throw new AuthenticationException();
            }

            var priceField = detailsPage.GetFieldValue(".uc-price");
            var wholesalePrice = priceField != null ? priceField.Replace("$", "") : "0";

            var product = new ScanData();
            product[ScanField.AdditionalInfo] = detailsPage.GetFieldValue(".field-fabric-additional-info");
            product[ScanField.Book] = detailsPage.GetFieldValue(".fabric-nav-area a");
            product[ScanField.Category] = detailsPage.GetFieldValue(".field-fabric-category.inline");
            product[ScanField.Cleaning] = detailsPage.GetFieldValue(".field-fabric-cleaning-code.inline");
            product[ScanField.Color] = detailsPage.GetFieldValue(".field-fabric-color.inline");
            product[ScanField.Country] = detailsPage.GetFieldValue(".field-fabric-country-of-origin.inline");
            product[ScanField.Direction] = detailsPage.GetFieldValue(".field-fabric-direction.inline");
            product[ScanField.Durability] = detailsPage.GetFieldValue(".field-fabric-abrasion.inline");
            product[ScanField.Finish] = detailsPage.GetFieldValue(".field-fabric-finish.inline");
            product[ScanField.FireCode] = detailsPage.GetFieldValue(".field-fabric-fire-code.inline");
            product[ScanField.ManufacturerPartNumber] = discoveredProduct.MPN.SkuTweaks();
            product[ScanField.Note] = detailsPage.GetFieldValue(".call-to-order-message");
            product[ScanField.ProductUse] = detailsPage.GetFieldValue(".field-fabric-usage.inline");
            product[ScanField.Repeat] = detailsPage.GetFieldValue(".field-fabric-repeat.inline");
            product[ScanField.Status] = detailsPage.GetFieldValue(".field-fabric-closeout");
            product[ScanField.StockCount] = detailsPage.GetFieldValue(".field-fabric-inventory");
            product[ScanField.Style] = detailsPage.GetFieldValue(".field-fabric-style.inline");
            product[ScanField.Width] = detailsPage.GetFieldValue(".field-fabric-width.inline");

            product.Cost = wholesalePrice.ToDecimalSafe();
            product.DetailUrl = new Uri(url);

            // content field has <br> tags separating different data
            var contentField = detailsPage.QuerySelector(".field-fabric-content.inline");
            if (contentField != null)
            {
                var contentFieldPieces = contentField.InnerHtml.Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries);
                product[ScanField.Content] = contentFieldPieces.First().UnEscape().Trim();

                if (contentFieldPieces.Length > 1) product[ScanField.Bullet1] = contentFieldPieces[1].UnEscape().Trim();
                if (contentFieldPieces.Length > 2) product[ScanField.Bullet2] = contentFieldPieces[2].UnEscape().Trim();
                if (contentFieldPieces.Length > 3) product[ScanField.Bullet3] = contentFieldPieces[3].UnEscape().Trim();
                if (contentFieldPieces.Length > 4) product[ScanField.Bullet4] = contentFieldPieces[4].UnEscape().Trim();
                if (contentFieldPieces.Length > 5) product[ScanField.Bullet5] = contentFieldPieces[5].UnEscape().Trim();
                if (contentFieldPieces.Length > 6) product[ScanField.Bullet6] = contentFieldPieces[6].UnEscape().Trim();
            }

            var imageUrl = detailsPage.QuerySelector("a.zoom").Attributes["href"].Value;
            product.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

            var priceSuffix = detailsPage.GetFieldValue(".price-suffixes");
            product[ScanField.UnitOfMeasure] = priceSuffix;
            // put this in the builder
            if (priceSuffix.ContainsIgnoreCase("yard"))
                product[ScanField.UnitOfMeasure] = "Yard";
            else if (priceSuffix.ContainsIgnoreCase("square foot"))
                product[ScanField.UnitOfMeasure] = "Square Foot";
            product.RelatedProducts = CaptureCorrelatedPatterns(product[ScanField.ManufacturerPartNumber], detailsPage);
            return new List<ScanData> {product};
        }

        private List<string> CaptureCorrelatedPatterns(string itemNumber, HtmlNode detailsPage)
        {
            var correlatedPatternsHrefs = detailsPage.QuerySelectorAll(".fabric-teaser").Select(x => x.QuerySelector("a").Attributes["href"].Value);
            var patternMpns = correlatedPatternsHrefs.Select(x => x.CaptureWithinMatchedPattern("/fabric/(?<capture>(.*))")).ToList();
            patternMpns.Add(itemNumber);
            return patternMpns;
        }
    }
}