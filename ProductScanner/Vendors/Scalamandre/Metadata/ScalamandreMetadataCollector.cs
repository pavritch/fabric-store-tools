using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Scalamandre.Metadata
{
    /*
    public class ScalamandreMetadataCollector : IMetadataCollector<ScalamandreVendor>
    {
        private const string SearchUrl = "http://www.scalamandre.com/websearch.php";
        private readonly IPageFetcher<ScalamandreVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<ScalamandreVendor> _sessionManager; 

        private static Dictionary<string, string> _collectionsByYear;

        public ScalamandreMetadataCollector(IPageFetcher<ScalamandreVendor> pageFetcher, IVendorScanSessionManager<ScalamandreVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;

            _collectionsByYear = new Dictionary<string, string>();
            for (var year = 1998; year <= 2014; year++)
            {
                foreach (var season in new[] { "Spring", "Fall" })
                {
                    string key = string.Format("{0} {1}", season, year);
                    string value = string.Format("{0}{1}", season.First(), year);

                    _collectionsByYear.Add(key, value);
                }
            }
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            // find category/collection associations
            var productAssociations = await FindAssociations();

            // match up associations
            MatchUpProductsWithAssociations(products, productAssociations);

            // fill in any missing units of measure by matching to another in the same pattern
            FillMissingUnitOfMeasures(products);
            return products;
        }

        protected void FillMissingUnitOfMeasures(List<ScanData> webProducts)
        {
            var dicPatternNumberLookup = webProducts.GroupBy(x => x[ScanField.PatternNumber])
                .ToDictionary(k => k.Key, v => v.ToList());

            // attempt to fill in any missing unit of measure with ones from same pattern number

            foreach (var product in webProducts.Where(e => !e.ContainsKey(ScanField.UnitOfMeasure)))
            {
                // search list of products with same pattern number to see if one might have a unit of measure filled in

                var list = dicPatternNumberLookup[product[ScanField.PatternNumber]];

                var otherProduct = list.FirstOrDefault(e => 
                    e.ContainsKey(ScanField.UnitOfMeasure) && e[ScanField.ProductGroup] == product[ScanField.ProductGroup]);

                if (otherProduct != null)
                    product[ScanField.UnitOfMeasure] = otherProduct[ScanField.UnitOfMeasure];
            }
        }

        // Given a list of products and associations, match up and set vendor properties on the product records.
        protected void MatchUpProductsWithAssociations(List<ScanData> products, Dictionary<string, List<string>> associations)
        {
            foreach (var item in associations)
            {
                var aryQueryParts = item.Key.Split(':');
                var productGroup = aryQueryParts[0];
                var filterKey = aryQueryParts[1];
                var filterValue = aryQueryParts[2];

                // reverse lookups
                var dicCollectionNameFromCode = _collectionsByYear.ToDictionary(k => k.Value, v => v.Key);
                var dicFabricCategoriesFromCode = FabricCategories.ToDictionary(k => k.Value, v => v.Key);
                var dicTrimCategoriesFromCode = TrimCategories.ToDictionary(k => k.Value, v => v.Key);

                switch (filterKey)
                {
                    case "SOI": // year collection
                        var collectionName = dicCollectionNameFromCode[filterValue];
                        foreach (var mpn in item.Value)
                        {
                            var match = products.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == mpn);
                            if (match != null)
                            {
                                match[ScanField.Collection] = collectionName;
                            }
                        }
                        break;

                    case "CO": // contract
                        foreach (var mpn in item.Value)
                        {
                            var match = products.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == mpn);
                            if (match != null)
                            {
                                match[ScanField.Brand] = "Scalamandre Contract";
                            }
                        }
                        break;

                    case "CAT": // category
                        if (productGroup == "Fabric")
                        {
                            var catName = dicFabricCategoriesFromCode[filterValue];
                            foreach (var mpn in item.Value)
                            {
                                var match = products.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == mpn);
                                if (match != null)
                                {
                                    match[ScanField.Group] = catName;
                                }
                            }
                        }
                        else if (productGroup == "Trim")
                        {
                            var catName = dicTrimCategoriesFromCode[filterValue];
                            foreach (var mpn in item.Value)
                            {
                                var match = products.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == mpn);
                                if (match != null)
                                {
                                    match[ScanField.Group] = catName;
                                }
                            }
                        }
                        break;
                }
            }
        }

        protected async Task<Dictionary<string, List<string>>> FindAssociations()
        {
            // build up a set of queries to run
            var queries = new List<string>();

            Action<string, string, string> add = (productGroup, filterKey, filterValue) =>
                {
                    var query = string.Format("{0}:{1}:{2}", productGroup, filterKey, filterValue);
                    queries.Add(query);
                };

            // fabric categories
            foreach (var item in FabricCategories)
                add("Fabric", "CAT", item.Value);

            // fabric collections
            foreach (var item in _collectionsByYear)
                add("Fabric", "SOI", item.Value);

            // fabric contract
            add("Fabric", "CO", "08");

            // wallpaper collections
            foreach (var item in _collectionsByYear)
                add("Wallpaper", "SOI", item.Value);

            // trim categories
            foreach (var item in TrimCategories)
                add("Trim", "CAT", item.Value);

            return await FindAssociations(queries);
        }

        // Given a list of queries to perform, return a dictionary with matching products for each.
        protected async Task<Dictionary<string, List<string>>> FindAssociations(List<string> queries)
        {
            // step 8 - find associations
            // query key, list<MPN>
            var dicAssociations = new Dictionary<string, List<string>>();
            await _sessionManager.ForEachNotifyAsync("Scanning metadata", queries, async query =>
            {
                // ProductGroup:FilterKey:FilterValue
                var aryQueryParts = query.Split(':');

                var productGroup = aryQueryParts[0];
                var filterKey = aryQueryParts[1];
                var filterValue = aryQueryParts[2];

                var dicFilters = new Dictionary<string, string>
                {
                    {filterKey, filterValue}
                };

                var products = await DetectAvailableProductsFromWeb(productGroup, false, dicFilters);

                //// add entry to dictionary for this query and associate all the MPNs found for it
                dicAssociations.Add(query, products.Select(x => x[ScanField.ManufacturerPartNumber]).ToList());
            });
            return dicAssociations;
        }

        private static readonly Dictionary<string, string> FabricCategories = new Dictionary<string, string>()
        {
            { "Brocades", "D" },
            { "Damasks", "H" },
            { "Embroidery", "M" },
            { "Exclusives", "L" },
            { "Horsehairs", "J" },
            { "Leather", "LE" },
            { "Plaids & Checks", "E" },
            { "Plains", "I" },
            { "Prints", "A" },
            { "Sheers & Casements", "F" },
            { "Stripes", "B" },
            { "Tapestries", "K" },
            { "Textures", "G" },
            { "Velvets", "C" },
        };

        private static readonly Dictionary<string, string> TrimCategories = new Dictionary<string, string>()
        {
            { "Braids", "V" },
            { "Bullion Fringe", "FB" },
            { "Cords", "CD" },
            { "Cut Fringe", "FC" },
            { "Loop Fringe", "FL" },
            { "Moss Fringe", "FM" },
            { "Other Fringe", "FX" },
            { "Tassel Fringe", "FT" },
            { "Tie Backs", "T" },
            { "Tufts & Bows", "ST" },
        };

        // Duplicated
        public async Task<List<ScanData>> DetectAvailableProductsFromWeb(string productGroup, bool isClearance, Dictionary<string, string> filters = null)
        {
            var divParam = string.Empty;

            switch (productGroup)
            {
                case "Wallpaper":
                    divParam = "3";
                    break;

                case "Trim":
                    divParam = "2";
                    break;

                case "Fabric":
                    divParam = "1";
                    break;

                default:
                    throw new Exception("Invalid product group.");

            }

            var typeParam = isClearance ? "TTF" : "DES";
            var url = string.Format("{0}?TYPE={1}&DIV={2}&page=1&NEWDISP=20000&REC=0&VEN=&SEARCH=", SearchUrl, typeParam, divParam);
            var cacheKey = string.Format("{0}-{1}", productGroup, typeParam);

            // possible filters to pass in are:
            // CAT - categries
            // SOI- typically for collections by year
            // CO - contract

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    url += string.Format("&{0}={1}", filter.Key, filter.Value);
                    cacheKey += string.Format("-{0}-{1}", filter.Key, filter.Value);
                }
            }

            var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, cacheKey);

            var productNodes = page.QuerySelectorAll("a[href*='&amp;sku=']");

            var products = new List<ScanData>();
            if (productNodes != null)
            {
                foreach (HtmlNode productNode in productNodes)
                {
                    try
                    {

                        string productUrl = productNode.Attributes["href"].Value.Replace("&amp;", "&");
                        string sku = productUrl.CaptureWithinMatchedPattern(@"sku=(?<capture>(.+))$");

                        if (!String.IsNullOrEmpty(sku))
                        {
                            sku = sku.ToUpper();

                            var newProduct = new ScanData();

                            Action<ScanField, string> addProperty = (k, v) =>
                            {
                                if (v == null)
                                    return;

                                var str = v.Trim();

                                if (string.IsNullOrWhiteSpace(str))
                                    return;

                                newProduct[k] = str;
                            };

                            newProduct.DetailUrl = new Uri(productUrl);
                            addProperty(ScanField.ProductGroup, productGroup);
                            addProperty(ScanField.ManufacturerPartNumber, sku);
                            addProperty(ScanField.IsClearance, isClearance.ToString());
                            
                            bool hasImage = !productNode.QuerySelector("img").GetAttributeValue("src", string.Empty).ContainsIgnoreCase("/noimage.jpg");

                            if (hasImage)
                            {
                                // 720x720
                                var imageUrl = string.Format("http://VISUALACCESS.SCALAMANDRE.COM/Scalamandre/images/{0}.jpg", sku);
                                addProperty(ScanField.ImageUrl, imageUrl);

                                // alt smaller - note the "a" in imagesa folder
                                // approx 320x291
                                var altImageUrl = string.Format("http://VISUALACCESS.SCALAMANDRE.COM/Scalamandre/imagesa/{0}.jpg", sku);
                                addProperty(ScanField.AlternateImageUrl, altImageUrl);
                            }

                            //if (newProduct.SetAndValidateKey() && !products.ContainsKey(newProduct.Key))
                            {
                                products.Add(newProduct);
                            }
                        }
                    }
                    catch
                    {
                        //LogErrorEvent("Error fetching information for product.");
                    }
                }
            }
            return products;
        }
    }
    */
}