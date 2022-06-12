using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace PhillipJeffries
{
    public class Subcategory
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public int SubcategoryId { get; set; }
    }

    public class Collection
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class PhillipJeffriesMetadataCollector : IMetadataCollector<PhillipJeffriesVendor>
    {
        private readonly IPageFetcher<PhillipJeffriesVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<PhillipJeffriesVendor> _sessionManager;

        private const string MaterialUrl = "https://www.phillipjeffries.com/shop/categories";
        private const string BooksUrl = "https://www.phillipjeffries.com/api/products/collections.json?limit=500&offset=0";

        public PhillipJeffriesMetadataCollector(IPageFetcher<PhillipJeffriesVendor> pageFetcher, IVendorScanSessionManager<PhillipJeffriesVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var categories = await FindCategories();
            await SetMaterials(categories, products, "category", ScanField.Material);

            var materials = await FindMaterials();
            await SetMaterials(materials, products, "sub_category", ScanField.Material2);

            var collections = await FindCollections();
            await SetCollections(collections, products);

            var searchPage = await _pageFetcher.FetchAsync("https://www.phillipjeffries.com/shop/wallcoverings", CacheFolder.Search, "main");
            var bookNames = searchPage.QuerySelectorAll("label[for*='filter-binder']").Select(x => x.Attributes["for"].Value).ToList();
            bookNames = bookNames.Select(x => x.Replace("filter-binder-", "")).ToList();

            await _sessionManager.ForEachNotifyAsync("Loading Books", bookNames, async bookName =>
            {
                // https://www.phillipjeffries.com/api/products/skews.json?binder=PJBINDER-TW&limit=50&offset=0
                var bookPage = await _pageFetcher.FetchAsync(string.Format("https://www.phillipjeffries.com/api/products/skews.json?binder={0}&limit=5000&offset=0", bookName), 
                    CacheFolder.Search, "book-" + bookName);
                var productIds = GetProductIds(bookPage);
                var matches = products.Where(x => productIds.Contains(x[ScanField.ManufacturerPartNumber]));
                matches.ForEach(x => x[ScanField.Book] = bookName);
            });

            return products;
        }

        private async Task SetCollections(List<Collection> collections, List<ScanData> products)
        {
            await _sessionManager.ForEachNotifyAsync("Loading Collections", collections, async collection =>
            {
                var url = string.Format("https://www.phillipjeffries.com/api/products/collections/{0}/skews.json", collection.Code);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, collection.Code);

                var productIds = GetProductIdsForCollection(page);
                var matches = products.Where(x => productIds.Contains(x[ScanField.ManufacturerPartNumber]));
                matches.ForEach(x => x[ScanField.Collection] = collection.Name);
            });
        }

        private async Task SetMaterials(List<Subcategory> materials, List<ScanData> products, string fieldName, ScanField field)
        {
            // https://www.phillipjeffries.com/api/products/skews.json?sub_category=216&limit=50&offset=0
            await _sessionManager.ForEachNotifyAsync("Loading Materials", materials, async material =>
            {
                var url = string.Format("https://www.phillipjeffries.com/api/products/skews.json?{0}={1}&limit=5000&offset=0", fieldName, material.SubcategoryId);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, fieldName + "-" + material.SubcategoryId);

                var productIds = GetProductIds(page);
                var matches = products.Where(x => productIds.Contains(x[ScanField.ManufacturerPartNumber]));
                matches.ForEach(x => x[field] = material.Name);
            });
        }

        private List<string> GetProductIds(HtmlNode page)
        {
            dynamic data = JObject.Parse(page.OuterHtml);
            var productIds = new List<string>();
            foreach (var item in data.items)
            {
                productIds.Add(item.id.Value);
            }
            return productIds;
        }

        private List<string> GetProductIdsForCollection(HtmlNode page)
        {
            dynamic data = JObject.Parse(page.OuterHtml);
            if (data.status == 500) return new List<string>();
            var productIds = new List<string>();
            foreach (var item in data.data.items)
            {
                productIds.Add(item.id.Value);
            }
            return productIds;
        }

        private async Task<List<Collection>> FindCollections()
        {
            var collections = new List<Collection>();
            var collectionsPage = await _pageFetcher.FetchAsync(BooksUrl, CacheFolder.Search, "collections");
            dynamic data = JObject.Parse(collectionsPage.OuterHtml);
            foreach (var item in data.items)
            {
                collections.Add(new Collection {Code = item.id, Name = item.name});
            }
            return collections;
        }

        private async Task<List<Subcategory>> FindCategories()
        {
            var materialPage = await _pageFetcher.FetchAsync(MaterialUrl, CacheFolder.Search, "materials");
            var categories = materialPage.QuerySelectorAll(".product-listing--grid__item").ToList();
            return categories.Select(x => new Subcategory
            {
                SubcategoryId = x.QuerySelector(".product-listing--grid__image-link").Attributes["href"].Value
                    .Replace("/shop/categories/", "").Trim('/').ToIntegerSafe(),
                Name = x.GetFieldValue(".product-listing--grid__info")
            }).ToList();
        }

        private async Task<List<Subcategory>> FindMaterials()
        {
            var subcategories = new List<Subcategory>();

            var materialPage = await _pageFetcher.FetchAsync(MaterialUrl, CacheFolder.Search, "materials");
            var categories = materialPage.QuerySelectorAll(".product-listing--grid__item").ToList();

            await _sessionManager.ForEachNotifyAsync("Scanning Categories", categories, async category =>
            {
                var categoryName = category.GetFieldValue(".product-listing--grid__info");
                var url = category.QuerySelector(".product-listing--grid__image-link").Attributes["href"].Value;

                var categoryPage = await _pageFetcher.FetchAsync("https://www.phillipjeffries.com" + url, CacheFolder.Search, categoryName);
                var materialItems = categoryPage.QuerySelectorAll(".product-listing--grid__item").ToList();

                foreach (var item in materialItems)
                {
                    var type = item.GetFieldValue(".product-listing--grid__info");
                    var listUrl = item.QuerySelector(".product-listing--grid__image-link").Attributes["href"].Value;

                    subcategories.Add(new Subcategory()
                    {
                        SubcategoryId = listUrl.Replace("/shop/wallcoverings?sub_category=", "").ToIntegerSafe(),
                        Name = type,
                        Category = categoryName
                    });
                }
            });

            return subcategories.Where(x => x.SubcategoryId != 0).ToList();
        }
    }
}