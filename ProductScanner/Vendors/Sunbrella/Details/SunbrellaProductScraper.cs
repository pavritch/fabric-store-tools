using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Sunbrella.Details
{
    public class SunbrellaProductScraper : ProductScraper<SunbrellaVendor>
    {
        private const string SearchUrl = "https://www.sunbrella.com/en-us/fabrics/search?keywords={0}+{1}+{2}";

        public SunbrellaProductScraper(IPageFetcher<SunbrellaVendor> pageFetcher)
            : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var mpn = context.MPN;
            // if the MPN from the file does not have -XXXX then append -0000
            if (!Regex.IsMatch(mpn, @"-\d{4,4}"))
                mpn += "-0000";

            var pattern = context.ScanData[ScanField.PatternName];
            var color = context.ScanData[ScanField.ColorName];
            var searchUrl = string.Format(SearchUrl, mpn, pattern, color);
            var searchResults = await PageFetcher.FetchAsync(searchUrl, CacheFolder.Search, mpn + "-search");
            var products = searchResults.QuerySelectorAll(".img-overlay-wrapper").ToList();

            if (products.Count == 1)
            {
                var productUrl = "https://www.sunbrella.com" + products.First().QuerySelector("a").Attributes["href"].Value;
                var productDetails = await PageFetcher.FetchAsync(productUrl, CacheFolder.Details, mpn);

                var product = new ScanData(context.ScanData);

                var details = productDetails.QuerySelector(".detail-wrapper").QuerySelectorAll("p").Select(x => x.InnerText).ToList();
                if (details.Count > 0) product[ScanField.Bullet1] = details[0];
                if (details.Count > 1) product[ScanField.Bullet2] = details[1];
                if (details.Count > 2) product[ScanField.Bullet3] = details[2];
                if (details.Count > 3) product[ScanField.Bullet4] = details[3];

                var collection = productDetails.QuerySelector("div.detail-title:contains('Collections')");
                product[ScanField.Collection] = collection.NextSibling.InnerText;

                product.DetailUrl = new Uri(productUrl);

                var imageUrl = "http://cdn.glenraven.net/sunbrella/_img/showroom/fabrics/Swatch/" + mpn + ".jpg";
                product.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

                var imageUrl2 = "https://cdn.glenraven.net/_img/_rasterize.php?src=/sunbrella/_img/showroom/fabrics/Swatch/{0}.jpg&w=950";
                product.AddImage(new ScannedImage(ImageVariantType.Primary, string.Format(imageUrl2, mpn)));
                product[ScanField.ManufacturerPartNumber] = mpn;
                return new List<ScanData> { product };
            }
            return new List<ScanData>();
        }

        private ScanData CreateProduct(string mpn, XElement xml, ScanData discoveredProperties)
        {
            var newProduct = new ScanData();
            if (GetXmlProperty(xml, "number") != mpn)
                throw new Exception(string.Format("XML for product does not match expected MPN: {0}", mpn));

            var imageUrl = "http://cdn.glenraven.net/sunbrella/_img/showroom/fabrics/Swatch/" + mpn + ".jpg";
            newProduct.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

            newProduct[ScanField.ManufacturerPartNumber] = mpn;
            newProduct[ScanField.Collection] = GetXmlProperty(xml, "collection");
            newProduct[ScanField.Style] = GetXmlProperty(xml, "pattern");
            newProduct[ScanField.Width] = GetXmlProperty(xml, "width");

            newProduct[ScanField.Bullet2] = GetXmlProperty(xml, "primary-color");
            newProduct[ScanField.Bullet3] = GetXmlProperty(xml, "secondary-color");
            newProduct[ScanField.Bullet4] = GetXmlProperty(xml, "accent-color");

            newProduct[ScanField.Repeat] = GetXmlProperty(xml, "repeat");
            newProduct[ScanField.Direction] = GetXmlProperty(xml, "direction");
            newProduct[ScanField.Content] = GetXmlProperty(xml, "fiber-content");

            newProduct[ScanField.Use] = GetProductUses(xml);

            newProduct[ScanField.PatternName] = discoveredProperties[ScanField.PatternName];
            newProduct[ScanField.ColorName] = discoveredProperties[ScanField.ColorName];
            newProduct[ScanField.Cost] = discoveredProperties[ScanField.Cost];
            newProduct[ScanField.MAP] = discoveredProperties[ScanField.MAP];
            return newProduct;
        }

        // get the named property from the XML document
        private string GetXmlProperty(XElement xml, string name)
        {
            var details = xml.Descendants("details").FirstOrDefault();
            if (details == null)
                return null;

            var el = details.Descendants(name).FirstOrDefault();

            if (el == null)
                return null;

            return el.Value.TrimToNull();
        }

        private string GetProductUses(XElement xml)
        {
            // create a list of product uses from the xml doc
            var applications = xml.Descendants("fabric-applications").FirstOrDefault();

            if (applications == null)
                return null;

            var uses = applications.Descendants("application").Select(e => e.Value).ToList();
            return uses.ToCommaDelimitedList();
        }
    }
}