using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace ArtAndFrameSource
{
    public class ArtAndFrameProductScraper : ProductScraper<ArtAndFrameSourceVendor>
    {
        private readonly Dictionary<string, ScanField> _keys = new Dictionary<string, ScanField>
        {
            { "Frame Style:", ScanField.FrameStyle},
            { "Frame Description:", ScanField.Description},
            { "Color:", ScanField.Color},
            { "Dimensions:", ScanField.Size},
            { "Size:", ScanField.Size},
            { "Size :", ScanField.Size},
            { "Size each:", ScanField.Size},
            { "Artist:", ScanField.Designer},
            { "Media:", ScanField.Material},
        };   
        public ArtAndFrameProductScraper(IPageFetcher<ArtAndFrameSourceVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var documentName = product.DetailUrl.GetDocumentName();
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, documentName);
            if (page.InnerText.ContainsIgnoreCase("Login or create an account"))
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, documentName);
                throw new AuthenticationException();
            }

            if (page.InnerText.ContainsIgnoreCase("504 Gateway Time-out"))
            {
                return new List<ScanData>();
            }

            var scanData = new ScanData();
            scanData[ScanField.ProductName] = page.GetFieldValue(".product-name");
            var price = page.GetFieldValue(".regular-price");
            if (price == null) return new List<ScanData>();

            scanData.Cost = page.GetFieldValue(".regular-price").Replace("$", "").ToDecimalSafe();
            scanData.DetailUrl = product.DetailUrl;
            scanData[ScanField.ManufacturerPartNumber] = documentName.Split('-').First();

            var bullets = page.QuerySelector(".short-description .std").InnerHtml
                .Replace("<ul>", "").Replace("</ul>", "").Replace("</li>", "").Replace("<br>", "")
                .Split(new[] {"<li>"}, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !x.Contains("Molding no longer available"))
                .Where(x => !x.Contains("Frame shown is no longer available"))
                .Where(x => !string.IsNullOrEmpty(x)).ToList();
            foreach (var bullet in bullets)
            {
                var value = bullet.Split(':').Last();
                var match = _keys.Where(x => bullet.Contains(x.Key)).Select(x => new { x.Key, x.Value }).FirstOrDefault();
                if (match == null)
                {
                    scanData[ScanField.Style] = value;
                    continue;
                }
                if (value != string.Empty)
                    scanData[match.Value] = value;
            }

            var imageUrl = page.QuerySelector("#image-main").Attributes["src"].Value;
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

            return new List<ScanData> { scanData };
        }
    }
}
