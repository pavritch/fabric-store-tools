using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities;
using Utilities.Extensions;

namespace JamesHare.Discovery
{
    // Note: James Hare doesn't have any concept of checkpoints because I'm scanning recursively
    // so I have no idea where I'm at in the process
    // Fortunately the scan is pretty short
    public class JamesHareProductDiscoverer : IProductDiscoverer<JamesHareVendor>
    {
        private List<ScanData> _products = new List<ScanData>();
        private readonly IPageFetcher<JamesHareVendor> PageFetcher;
        public JamesHareProductDiscoverer(IPageFetcher<JamesHareVendor> pageFetcher)
        {
            PageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            await ScanByCollection("http://www.james-hare.com/519-view-by-collection.html", new Dictionary<ProductPropertyType, string>(), null);
            await ScanByCollection("http://www.james-hare.com/1365-view-by-colour.html", new Dictionary<ProductPropertyType, string>(), null);
            await ScanByCollection("http://www.james-hare.com/2294-view-by-fabric.html", new Dictionary<ProductPropertyType, string>(), null);
            return _products.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task ScanByCollection(string baseUrl, Dictionary<ProductPropertyType, string> properties, ProductProperty newProp)
        {
            var props = new Dictionary<ProductPropertyType, string>(properties);
            if (newProp != null) props.Add(newProp.ProductPropertyType, newProp.Value);

            var cachePath = baseUrl.Replace("http://www.james-hare.com/", "").Replace(".html", "").UrlEncode();
            var page = await PageFetcher.FetchAsync(baseUrl, CacheFolder.Search, cachePath);
            // some of the pages have no content?
            if (page.InnerText == string.Empty) return;
            if (page.QuerySelector("#breadcrumbs").InnerText.Contains("Fashion Fabrics")) return;
            if (page.InnerText.Contains("encountered problems")) return;
            if (page.InnerText.Contains("Fabric Information"))
            {
                CreateProduct(page, props);
                return;
            }

            var links = FindLinkNodes(page);
            foreach (var element in links)
            {
                var newProperty = GetNewProperty(element, page);
                await ScanByCollection(element.Attributes["href"].Value, props, newProperty);
            }
        }

        private ProductProperty GetNewProperty(HtmlNode linkElement, HtmlNode page)
        {
            var img = linkElement.QuerySelector("img[title]");
            if (img != null)
            {
                var titleText = img.Attributes["title"].Value;
                var isSku = Regex.IsMatch(titleText, @"\d+\/\d+");
                if (isSku) return new ProductProperty(ProductPropertyType.ProductName, titleText);
                return new ProductProperty(ProductPropertyType.ColorGroup, titleText);
            }
            var header = linkElement.ParentNode.QuerySelector(".header");
            if (header != null)
            {
                var isSku = Regex.IsMatch(header.InnerText, @"\d+\/\d+");
                if (isSku)
                    return new ProductProperty(ProductPropertyType.ProductName, header.InnerText);
                if (page.InnerHtml.Contains("<h1>View by Collection</h1>"))
                {
                    var values = header.InnerText.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var collection = values.First();
                    return new ProductProperty(ProductPropertyType.Collection, collection);
                }
                if (page.InnerHtml.Contains("View By Fabric"))
                {
                    var values = header.InnerText.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var type = values.First();
                    return new ProductProperty(ProductPropertyType.ProductType, type);
                }
                return new ProductProperty(ProductPropertyType.Design, header.InnerText);
            }
            return new ProductProperty(ProductPropertyType.Category, linkElement.InnerText);
        }

        private void CreateProduct(HtmlNode page, Dictionary<ProductPropertyType, string> properties)
        {
            var propertyTable = new Dictionary<string, ProductPropertyType>
            {
                { "Description", ProductPropertyType.PatternName },
                { "Fabric Code", ProductPropertyType.ManufacturerPartNumber },
                { "Colour", ProductPropertyType.Color },
                { "Detail", ProductPropertyType.Cleaning },
                { "Width", ProductPropertyType.Width },
                { "Composition", ProductPropertyType.Content },
                { "Repeat", ProductPropertyType.Repeat },
                { "Weight", ProductPropertyType.Weight },
            };

            var headerCells = page.QuerySelectorAll(".fabric-info th").ToList();
            var dataCells = page.QuerySelectorAll(".fabric-info td").ToList();
            var productNumber = dataCells[0].InnerText;

            var existingProduct = _products.SingleOrDefault(x => x[ProductPropertyType.ManufacturerPartNumber] == productNumber);
            if (existingProduct == null)
            {
                existingProduct = new ScanData();
                _products.Add(existingProduct);
            }

            for (int i = 0; i < headerCells.Count; i++)
            {
                var header = headerCells[i].InnerText;
                var data = dataCells[i].InnerText;
                var property = propertyTable[header];
                existingProduct[property] = data;
            }

            var descriptionNode = page.QuerySelector("#the-text p");
            if (descriptionNode != null) 
                existingProduct[ProductPropertyType.Description] = descriptionNode.InnerText;

            SetProperties(properties, existingProduct);
            existingProduct[ProductPropertyType.ImageUrl] = FindImageUrl(page);
            existingProduct[ProductPropertyType.ManufacturerPartNumber] = productNumber;
            existingProduct[ProductPropertyType.ProductGroup] = "Fabric";
            existingProduct[ProductPropertyType.UnitOfMeasure] = "Yard";
        }

        private string FindImageUrl(HtmlNode page)
        {
            var imageNode = page.QuerySelector("#mainimg");
            if (imageNode != null)
                return imageNode.Attributes["href"].Value;

            imageNode = page.QuerySelector("#productdetail-image img");
            if (imageNode != null)
                return imageNode.Attributes["src"].Value;

            return string.Empty;
        }

        private void SetProperties(Dictionary<ProductPropertyType, string> properties, ScanData data)
        {
            foreach (var kvp in properties)
            {
                data[kvp.Key] = kvp.Value;
            }
        }

        private IEnumerable<HtmlNode> FindLinkNodes(HtmlNode page)
        {
            var links = GetLinkNodes(page, "children-group-wide");
            if (!links.Any()) links = GetLinkNodes(page, "children-group");
            if (!links.Any()) links = GetLinkNodes(page, "children-group-4");
            return links;
        }

        private List<HtmlNode> GetLinkNodes(HtmlNode page, string name)
        {
            var lis = page.QuerySelectorAll(string.Format("#{0} li", name));
            return lis.Select(x => x.QuerySelector("a")).ToList();
        }
    }
}