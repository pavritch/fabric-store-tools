using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using InsideStores.Imaging;
using Irony.Parsing;
using Irony.Ast;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using Gen4.Util.Misc;
using System.Web.Caching;
using System.Configuration;
using System.Threading.Tasks;
using Website.Entities;
using System.Text.RegularExpressions;
using System.IO;
using Algolia.Search;
using Newtonsoft.Json.Linq;
using InsideFabric.Data;

namespace Website
{
    /// <summary>
    /// Part 2 of the WebStoreBase class - query processing.
    /// </summary>
    public partial class WebStoreBase<TProductDataCache>
    {
        private const string MATCH_UPLOADED_PHOTO_GUID = @"^@\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$"; // for image upload identifiers

        private static string[] searchOperators = new string[] { "like:", "color:", "style:", "pattern:", "suggested:" };



        private class SearchOperator
        {
            public string Key { get; set; }
            public Func<string, List<int>, List<int>> Callback { get; set; }

            public SearchOperator(string Key, Func<string, List<int>, List<int>> Callback)
            {
                this.Key = Key;
                this.Callback = Callback;
            }
        }



        private List<string> ConvertAutoSuggestResultsToStringList(List<int> suggestions)
        {
            // take a list of PKID into the universe of phrases and return a list
            // of strings for those phrases in same order

            var list = new List<string>();

            var productContext = productData.Data;
            var dic = productContext.AutoSuggestPhrases;

            foreach (var id in suggestions)
            {
                string s;
                if (dic.TryGetValue(id, out s))
                    list.Add(s);
            }

            return list;
        }

        private List<string> ProcessAlgoliaResults(JObject queryResults)
        {
            var list = new List<string>(); // ordered

            try
            {
                // there are two result sets
                var suggestions = queryResults["results"][0];
                var products = queryResults["results"][1];

                var hash = new HashSet<string>(); // lower case, for deduplication

                foreach (var hit in suggestions["hits"])
                {
                    var phrase = hit["phrase"].ToString();
                    var lowerValue = phrase.ToLower();

                    if (hash.Contains(lowerValue))
                        continue;

                    list.Add(lowerValue);
                    hash.Add(lowerValue);
                }

                Action<JToken> add = (jt) =>
                    {
                        var v = jt["value"].ToString(); // will have the <em> in it
                        if (string.IsNullOrWhiteSpace(v))
                            return;

                        var cleanValue = v.Replace("<em>", "").Replace("</em>", "");
                        var lowerValue = cleanValue.ToLower();
                        if (hash.Contains(lowerValue))
                            return;

                        list.Add(lowerValue);

                        hash.Add(lowerValue);
                    };

                // general priority of fields: brand, categories, properties, name, mpn, sku, upc

                // filter down to just the highlight information so we have a smaller set to iterate over
                var hilitedResults = new List<JToken>();
                foreach (var hit in products["hits"])
                    hilitedResults.Add(hit["_highlightResult"]);


                Action<JToken, string> findPhraseFull = (jt, fieldName) =>
                    {
                        var jtField = jt[fieldName];
                        if (jtField == null)
                            return;
                        var matchLevel = jtField["matchLevel"].ToString();
                        if (matchLevel != "full")
                            return;
                        add(jtField);
                    };

                Action<JToken, string> findPhrasePartial = (jt, fieldName) =>
                    {
                        var jtField = jt[fieldName];
                        if (jtField == null)
                            return;
                        var matchLevel = jtField["matchLevel"].ToString();
                        if (matchLevel != "partial")
                            return;
                        add(jtField);
                    };


                // brand
                foreach(var jt in hilitedResults)
                    findPhraseFull(jt, "brand");

                // categories
                //foreach (var jt in hilitedResults)
                //{
                //    foreach(var jtCat in jt["categories"])
                //    {
                //        var matchLevel = jtCat["matchLevel"].ToString();
                //        if (matchLevel != "full")
                //            continue;
                //        add(jtCat);
                //    }
                //}

                // properties
                foreach (var jt in hilitedResults)
                {
                    if (jt["properties"] != null)
                        foreach (var jtProp in jt["properties"])
                        {
                            var matchLevel = jtProp["matchLevel"].ToString();
                            if (matchLevel != "full")
                                continue;
                            add(jtProp);
                        }
                }

                // name
                foreach (var jt in hilitedResults)
                    findPhraseFull(jt, "name");

                // mpn
                foreach (var jt in hilitedResults)
                    findPhraseFull(jt, "mpn");

                // sku
                foreach (var jt in hilitedResults)
                    findPhraseFull(jt, "sku");

                // upc
                foreach (var jt in hilitedResults)
                    findPhraseFull(jt, "upc");

                // next pass looking at partials

                // brand
                foreach (var jt in hilitedResults)
                    findPhrasePartial(jt, "brand");

                // categories
                //foreach (var jt in hilitedResults)
                //{
                //    foreach (var jtCat in jt["categories"])
                //    {
                //        var matchLevel = jtCat["matchLevel"].ToString();
                //        if (matchLevel != "partial")
                //            continue;
                //        add(jtCat);
                //    }
                //}

                // properties
                foreach (var jt in hilitedResults)
                {
                    if (jt["properties"] != null)
                        foreach (var jtProp in jt["properties"])
                        {
                            var matchLevel = jtProp["matchLevel"].ToString();
                            if (matchLevel != "partial")
                                continue;
                            add(jtProp);
                        }
                }

                // name
                foreach (var jt in hilitedResults)
                    findPhrasePartial(jt, "name");

                // mpn
                foreach (var jt in hilitedResults)
                    findPhrasePartial(jt, "mpn");

                // sku
                foreach (var jt in hilitedResults)
                    findPhrasePartial(jt, "sku");

                // upc
                foreach (var jt in hilitedResults)
                    findPhrasePartial(jt, "upc");


            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }

            return list.Take(100).ToList();
        }

        public void SubmitAutoSuggestQuery(AutoSuggestQuery query)
        {
            var t = new Task(() =>
                {
                    var list = new List<string>();

                    try
                    {
                        if (string.IsNullOrWhiteSpace(query.Query))
                            return;

                        if (IsAlgoliaEnabled)
                        {
                            string myQuery = query.Query;
                            var indexNameProducts = string.Format("{0}Products", storeKey.ToString());
                            var indexNameSuggestions = string.Format("{0}Suggestions", storeKey.ToString());
                            AlgoliaClient algoliaClient = new AlgoliaClient(this.AlgoliaApplicationID, this.AlgoliaSearchOnlyApiKey);

                            //Index indexProducts = algoliaClient.InitIndex(indexNameProducts);
                            //Index indexSuggestions = algoliaClient.InitIndex(indexNameSuggestions);
                            //var searchResults = index.Search(new Query(query.Query));

                            var indexQueries = new List<IndexQuery>();

                            indexQueries.Add(new IndexQuery(indexNameSuggestions, new Query(myQuery)));
                            indexQueries.Add(new IndexQuery(indexNameProducts, new Query(myQuery)));

                            var searchResults = algoliaClient.MultipleQueries(indexQueries, strategy: "none");

                            list.AddRange(ProcessAlgoliaResults(searchResults));
                            return;
                        }
                        else
                        {
                            // only startswith supported
                            if (query.Mode != AutoSuggestMode.StartsWith)
                                return;

                            // only list 0 (default) supported
                            if (query.ListID != 0)
                                return;

                            var cachedResults = CachedAutoSuggestResult.Lookup(query);

                            // if no cache results, get the data from SQL and make a cache entry

                            // note that unlike product search, we do not care about matching up with 
                            // the same product context - as that would cause our results to flush out too often.
                            // phrases are just word lists - no need to get too granular

                            if (cachedResults == null)
                            {
                                using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
                                {
                                    var records = dc.AutoSuggestPhrases.Where(e => e.PhraseListID == query.ListID && e.Phrase.StartsWith(query.Query)).OrderByDescending(e => e.Priority).Select(e => e.PhraseID).Take(query.Take).ToList();
                                    cachedResults = new CachedAutoSuggestResult(query, records);
                                    cachedResults.Insert();
                                }
                            }

                            list = ConvertAutoSuggestResultsToStringList(cachedResults.Suggestions);
                            return;

                        }

                    }
                    catch (Exception)
                    { }
                    finally
                    {
                        if (query.CompletedAction != null)
                            query.CompletedAction(query.Query, list);
                    }
                });
            t.Start();
        }


        public void SubmitAutoSuggestQuery(CollectionsAutoSuggestQuery query)
        {
            var t = new Task(() =>
            {
                var list = new List<string>();

                try
                {
                    if (string.IsNullOrWhiteSpace(query.Query))
                        return;

                    // set list to the result to return

                    List<ProductCollection> collections = null;
                    switch(query.Kind)
                    {
                        case CollectionsKind.Books:
                            collections = GetBooksByManufacturer(query.manufacturerID, query.Query, query.Mode);
                            break;

                        case CollectionsKind.Patterns: 
                            collections = GetPatternsByManufacturer(query.manufacturerID, query.Query, query.Mode);
                            break;

                        case CollectionsKind.Collections:
                            collections = GetCollectionsByManufacturer(query.manufacturerID, query.Query, query.Mode);
                            break;

                        default:
                            throw new Exception("SubmitAutoSuggestQuery: Invalid operation.");
                    }

                    // we now have a list of matched collections, just need to return the short names, unless across all manufacturers

                    if (query.manufacturerID == 0)
                        list = collections.Select(e => e.Name).ToList();
                    else
                        list = collections.Select(e => e.ShortName).ToList();

                }
                catch (Exception)
                { }
                finally
                {
                    if (query.CompletedAction != null)
                        query.CompletedAction(query.Kind, query.manufacturerID, query.Query, list);
                }
            });
            t.Start();
        }


        public void SubmitAutoSuggestQuery(ProductCollectionAutoSuggestQuery query)
        {
            var t = new Task(() =>
            {
                var list = new List<string>();

                try
                {
                    if (string.IsNullOrWhiteSpace(query.Query))
                        return;

                    var productContext = productData.Data;
                    if (productContext == null)
                        return;

                    // this is the list of qualifying productID - now just need to get their names
                    var resultSet = GetProductListByProductCollection(productContext, query.CollectionID, query.Query, query.Mode);

                    list = resultSet.Select(p => productContext.Products[p].Name).ToList();
                }
                catch (Exception)
                { }
                finally
                {
                    if (query.CompletedAction != null)
                        query.CompletedAction(query.Query, list);
                }
            });
            t.Start();
        }


        public void SubmitAutoSuggestQuery(NewProductsByManufacturerAutoSuggestQuery query)
        {
            var t = new Task(() =>
            {
                var list = new List<string>();

                try
                {
                    if (string.IsNullOrWhiteSpace(query.Query))
                        return;

                    var productContext = productData.Data;
                    if (productContext == null)
                        return;


                    var resultSet = GetNewProductListByManufacturer(productContext, query.ManufacturerID, query.Days, query.Query, query.Mode);

                    list = resultSet.Select(p => productContext.Products[p].Name).ToList();
                }
                catch (Exception)
                { }
                finally
                {
                    if (query.CompletedAction != null)
                        query.CompletedAction(query.Query, list);
                }
            });
            t.Start();
        }


        public void SubmitProductQuery(IQueryRequest query)
        {
            // put the query into the thread queue and return

            var workItem = new QueryRequestWorkItem(query);
            threadPool.QueueWorkItem(new Amib.Threading.Func<QueryRequestWorkItem, bool>(QueryRequestWorkerThread), workItem);
            Performance.TotalApiRequests.Bump();
        }

        /// <summary>
        /// Main query processor which processes the task queue.
        /// </summary>
        /// <param name="reqItem"></param>
        /// <returns></returns>
        private bool QueryRequestWorkerThread(QueryRequestWorkItem reqItem)
        {
            var query = reqItem.QueryRequest;

            // branch based on which core group of processing is to be called -list of products, or list of collections

            if (query is IProductCollectionQuery)
                return ProductCollectionQueryRequest(reqItem);

            return ProductQueryRequest(reqItem);
        }

        private bool ProductCollectionQueryRequest(QueryRequestWorkItem reqItem)
        {
            var query = reqItem.QueryRequest as IProductCollectionQuery;

            try
            {

                List<ProductCollection> completeResultSet = null;
                int pageCount = 0;
                List<ProductCollection> collectionList = new List<ProductCollection>();

                switch (query.QueryMethod)
                {
                    case QueryRequestMethods.ListBooksByManufacturer:
                        var booksCollectionQuery = query as BooksCollectionQuery;

                        completeResultSet = GetBooksByManufacturer(booksCollectionQuery.ManufacturerID, booksCollectionQuery.Filter);
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;
                            collectionList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();
                        }
                        break;

                    case QueryRequestMethods.ListCollectionsByManufacturer:
                        var collectionsCollectionQuery = query as CollectionsCollectionQuery;

                        completeResultSet = GetCollectionsByManufacturer(collectionsCollectionQuery.ManufacturerID, collectionsCollectionQuery.Filter);
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;
                            collectionList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();
                        }
                        break;

                    case QueryRequestMethods.ListPatternsByManufacturer:
                        var patternsCollectionQuery = query as PatternsCollectionQuery;

                        completeResultSet = GetPatternsByManufacturer(patternsCollectionQuery.ManufacturerID, patternsCollectionQuery.Filter);
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;
                            collectionList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();
                        }
                        break;

                    default:
                        pageCount = 0;
                        collectionList = new List<ProductCollection>();
                        break;
                }

                // note that this call could be on a different thread than original ASP.NET thread,
                // so the original caller must take care (as needed) to sync to the correct thread.

                if (query.CompletedAction != null)
                    query.CompletedAction(collectionList, pageCount);

                return true;
            }
            catch (Exception Ex)
            {
                var msg = string.Format("Exception in ProductQueryWorkerThread for {0}.\n{1}", storeKey, Dump.ToDump(query, "Query"));
                Debug.WriteLine(msg);
                var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                ev2.Raise();

                if (query.CompletedAction != null)
                    query.CompletedAction(new List<ProductCollection>(), 0);

                return false;
            }
        }

        private bool ProductQueryRequest(QueryRequestWorkItem reqItem)
        {
            var query = reqItem.QueryRequest as IProductQuery;

            try
            {
                TProductDataCache productContext = null;

                // guard code to make sure we don't try to access the cached product
                // data unless it is guaranteed to be present - else return empty result set

                if (!IsPopulated || productData == null)
                {
                    query.CompletedAction(new List<CacheProduct>(), 0);
                    return false;
                }

                // capture the current state of product data and use this for all query operations
                // which guarantees we're working with atomic data for a single transaction

                productContext = productData.Data;
                if (productContext == null)
                {
                    query.CompletedAction(new List<CacheProduct>(), 0);
                    return false;
                }

                List<int> completeResultSet = null;
                int pageCount = 0;
                List<CacheProduct> productList = new List<CacheProduct>();

                switch (query.QueryMethod)
                {
                    case QueryRequestMethods.ListByCategory:
                        var categoryQuery = query as CategoryProductQuery;

                        // if is filtered, turn into an advanced search

                        if (categoryQuery.IsFiltered)
                        {
                            var criteria = new SearchCriteria
                            {
                                Keywords = null,
                                PartNumber = null,
                                Collection = null,
                                ColorName = null,
                                ManufacturerList = categoryQuery.FilterByBrandList,
                                TypeList = categoryQuery.FilterByTypeList,
                                PatternList = categoryQuery.FilterByPatternList,
                                ColorList = categoryQuery.FilterByColorList,
                                PriceRangeList = new List<int>(),
                            };

                            productList = Search(productContext, out pageCount, criteria, query.PageNo - 1, query.PageSize, query.OrderBy);
                        }
                        else
                        {
                            completeResultSet = GetListByCategory(productContext, categoryQuery.CategoryID, query.OrderBy);
                            if (completeResultSet != null && completeResultSet.Count() > 0)
                            {
                                pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;

                                var pagedProductList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
                                productList = pagedProductList.Select(p => productContext.Products[p]).ToList();
                            }
                        }
                        break;

                    case QueryRequestMethods.ListByManufacturer:

                        var manufacturerQuery = query as ManufacturerProductQuery;

                        // if is filtered, turn into an advanced search

                        if (manufacturerQuery.IsFiltered)
                        {
                            var criteria = new SearchCriteria
                            {
                                Keywords = null,
                                PartNumber = null,
                                Collection = null,
                                ColorName = null,
                                ManufacturerList = new List<int>() { manufacturerQuery.ManufacturerID },
                                TypeList = manufacturerQuery.FilterByTypeList,
                                PatternList = manufacturerQuery.FilterByPatternList,
                                ColorList = manufacturerQuery.FilterByColorList,
                                PriceRangeList = new List<int>(),
                            };

                            productList = Search(productContext, out pageCount, criteria, query.PageNo - 1, query.PageSize, query.OrderBy);
                        }
                        else
                        {
                            completeResultSet = GetListByManufacturer(productContext, manufacturerQuery.ManufacturerID, query.OrderBy);
                            if (completeResultSet != null && completeResultSet.Count() > 0)
                            {
                                pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;

                                var pagedProductList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
                                productList = pagedProductList.Select(p => productContext.Products[p]).ToList();
                            }
                        }
                        break;

                    case QueryRequestMethods.ListByPatternCorrelator:
                        var patternQuery = query as PatternCorrelatorProductQuery;

                        completeResultSet = GetListByPatternCorrelator(productContext, patternQuery.Pattern, patternQuery.SkipMissingImages, patternQuery.ExcludeProductID);
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;

                            var pagedProductList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
                            productList = pagedProductList.Select(p => productContext.Products[p]).ToList();
                        }
                        break;


                    case QueryRequestMethods.ListByProductCollection:
                        var productCollectionQuery = query as ProductCollectionProductQuery;

                        completeResultSet = GetProductListByProductCollection(productContext, productCollectionQuery.CollectionID, productCollectionQuery.Filter);
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;

                            var pagedProductList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
                            productList = pagedProductList.Select(p => productContext.Products[p]).ToList();
                        }
                        break;


                    case QueryRequestMethods.ListNewProductsByManufacturer:
                        var newProductsByManufacturerQuery = query as NewProductsByManufacturerProductQuery;

                        completeResultSet = GetNewProductListByManufacturer(productContext, newProductsByManufacturerQuery.ManufacturerID, newProductsByManufacturerQuery.Days, newProductsByManufacturerQuery.Filter);
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;

                            var pagedProductList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
                            productList = pagedProductList.Select(p => productContext.Products[p]).ToList();
                        }
                        break;
                    
                    
                    case QueryRequestMethods.ListByLabelValueWithinManufacturer:
                        var queryParms = query as LabelProdutQuery;
                        productList = ListByLabelValueWithinManufacturer(productContext, out pageCount, queryParms.ManufacturerID, queryParms.Label, queryParms.Value, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    case QueryRequestMethods.Search:
                        productList = Search(productContext, out pageCount, (query as SearchProductQuery).SearchPhrase, query.PageNo - 1, query.PageSize, query.OrderBy, (query as SearchProductQuery).RecentlyViewed);
                        break;

                    case QueryRequestMethods.AdvancedSearch:
                        productList = Search(productContext, out pageCount, (query as AdvSearchProductQuery).Criteria, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    case QueryRequestMethods.FacetSearch:
                        productList = Search(productContext, out pageCount, (query as FacetSearchProductQuery).Criteria, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    case QueryRequestMethods.ListRelatedProducts:
                        var relatedQuery = query as RelatedProductQuery;
                        var productID = relatedQuery.ProductID;
                        var parentCategoryID = relatedQuery.ParentCategoryID;
                        completeResultSet = GetRelatedProductsList(productContext, productID, parentCategoryID, query.PageSize);
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = 1;
                            productList = completeResultSet.Select(p => productContext.Products[p]).ToList();
                        }
                        break;

                    case QueryRequestMethods.ProductSet:
                        {
                            var productSet = (query as ProductSetProductQuery).ProductsSet;
                            foreach (var id in productSet)
                            {
                                CacheProduct p;
                                if (productContext.Products.TryGetValue(id, out p))
                                    productList.Add(p);
                            }
                            pageCount = 1;
                        }
                        break;

                    case QueryRequestMethods.CrossMarketingProducts:
                        var crossMarketingQuery = query as CrossMarketingProductQuery;
                        productList = GetCrossMarketingProducts(productContext, crossMarketingQuery.ReferenceIdentifier, crossMarketingQuery.AllowResultsFromSelf, crossMarketingQuery.PageSize);
                        pageCount = 1;
                        break;


                    case QueryRequestMethods.ListDiscontinuedProducts:
                        
                        var discontinuedQuery = query as DiscontinuedProductQuery;

                        if (discontinuedQuery.ManufacturerID.HasValue)
                            completeResultSet = ProductData.DiscontinuedProducts.Where(e =>   ProductData.Products.ContainsKey(e) && ProductData.Products[e].ManufacturerID==discontinuedQuery.ManufacturerID.Value).ToList();
                        else
                            completeResultSet = ProductData.DiscontinuedProducts;                                
                        
                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;

                            var pagedProductList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
                            productList = pagedProductList.Select(p => productContext.Products[p]).ToList();
                        }

                        break;

                    case QueryRequestMethods.ListProductsMissingImages:

                        var missingImagesQuery = query as MissingImagesProductQuery;

                        if (missingImagesQuery.ManufacturerID.HasValue)
                            completeResultSet = ProductData.MissingImagesProducts.Where(e => ProductData.Products.ContainsKey(e) && ProductData.Products[e].ManufacturerID == missingImagesQuery.ManufacturerID.Value).ToList();
                        else
                            completeResultSet = ProductData.MissingImagesProducts;

                        if (completeResultSet != null && completeResultSet.Count() > 0)
                        {
                            pageCount = (completeResultSet.Count() + (query.PageSize - 1)) / query.PageSize;

                            var pagedProductList = completeResultSet.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize);
                            productList = pagedProductList.Select(p => productContext.Products[p]).ToList();
                        }

                        break;

                    case QueryRequestMethods.FindSimilarProducts:
                        productList = FindSimilarProducts(productContext, out pageCount, (query as FindSimilarProductsQuery).ProductID, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    case QueryRequestMethods.FindSimilarProductsByColor:
                        productList = FindSimilarProductsByColor(productContext, out pageCount, (query as FindSimilarProductsQuery).ProductID, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    case QueryRequestMethods.FindSimilarProductsByTexture:
                        productList = FindSimilarProductsByTexture(productContext, out pageCount, (query as FindSimilarProductsQuery).ProductID, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    case QueryRequestMethods.FindByTopDominantColor:
                        productList = FindByTopDominantColor(productContext, out pageCount, (query as FindProductsByDominantColorQuery).Color, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    case QueryRequestMethods.FindByAnyDominantColor:
                        productList = FindByAnyDominantColor(productContext, out pageCount, (query as FindProductsByDominantColorQuery).Color, query.PageNo - 1, query.PageSize, query.OrderBy);
                        break;

                    default:
                        pageCount = 0;
                        productList = new List<CacheProduct>();
                        break;
                }

                // note that this call could be on a different thread than original ASP.NET thread,
                // so the original caller must take care (as needed) to sync to the correct thread.

                if (query.CompletedAction != null)
                    query.CompletedAction(productList, pageCount);

                return true;
            }
            catch (Exception Ex)
            {
                var msg = string.Format("Exception in ProductQueryWorkerThread for {0}.\n{1}", storeKey, Dump.ToDump(query, "Query"));
                Debug.WriteLine(msg);
                var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                ev2.Raise();

                if (query.CompletedAction != null)
                    query.CompletedAction(new List<CacheProduct>(), 0);

                return false;
            }

        }

        public string MakeSortedListKey(int key, ProductSortOrder orderBy)
        {
            return string.Format("{0}:{1}", key, orderBy.ToString().ToLower());
        }

        protected int GetSeed(string text)
        {
            return text.ToSeed();
        }

        /// <summary>
        /// Get child list for parent category. Cache results until next repopulation.
        /// </summary>
        /// <param name="parentCategoryID"></param>
        /// <returns></returns>
        private List<int> GetChildCategoriesByParent(TProductDataCache productContext, int parentCategoryID)
        {
            lock (productContext.ChildCategories)
            {
                List<int> list;
                if (!productContext.ChildCategories.TryGetValue(parentCategoryID, out list))
                {
                    using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
                    {
                        list = dc.Categories.Where(e => e.ParentCategoryID == parentCategoryID).Select(e => e.CategoryID).ToList();
                        productContext.ChildCategories.Add(parentCategoryID, list);
                    }
                }
                return list;
            }
        }

        private int? GetRelatedCategoryID(TProductDataCache productContext, int ProductID, int ParentCategoryID)
        {
            var key = string.Format("{0}:{1}", ParentCategoryID, ProductID);
            lock (productContext.RelatedCategories)
            {
                int catID;
                if (productContext.RelatedCategories.TryGetValue(key, out catID))
                    return catID;
            }

            // not found, need to figure out, then put into cache

            var childCategories = GetChildCategoriesByParent(productContext, ParentCategoryID);

            int? relatedCatID = null;

            // find the first category which has this product as a member

            var startTime = DateTime.Now;
#if false
            using(var dc = new AspStoreDataContextReadOnly(ConnectionString))
            {
                relatedCatID = dc.ProductCategories.Where(e => e.ProductID == ProductID && childCategories.Contains(e.CategoryID)).Select(e => e.CategoryID).FirstOrDefault();
                if (relatedCatID == 0)
                    relatedCatID = null;
            }
#else
            // in memory approach
            foreach (var catID in childCategories)
            {
                List<int> productList;

                if (productContext.Categories.TryGetValue(catID, out productList))
                {
                    if (productList.Contains(ProductID))
                    {
                        relatedCatID = catID;
                        break;
                    }
                }
            }
#endif

            if (!relatedCatID.HasValue)
                return null;

            lock (productContext.RelatedCategories)
            {
                if (!productContext.RelatedCategories.ContainsKey(key))
                {
                    productContext.RelatedCategories.Add(key, relatedCatID.Value);
                }
            }

            return relatedCatID;
        }

        /// <summary>
        /// Get list of products related to the specified product.
        /// </summary>
        /// <remarks>
        /// The determination of which products are related is based on the parent categoryID.
        /// We get the list of all categories with that parent, and then spin through them looking
        /// for the first one with this product as a member. Then with that list, we spin off a 
        /// subset (size = indicated count) using a deterministic randomization scheme.
        /// Do not include the initially specified product in the return sequence.
        /// </remarks>
        /// <param name="ProductID"></param>
        /// <param name="ParentCategoryID"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public List<int> GetRelatedProductsList(TProductDataCache productContext, int ProductID, int ParentCategoryID, int Count)
        {
            var relatedCategoryID = GetRelatedCategoryID(productContext, ProductID, ParentCategoryID);

            if (!relatedCategoryID.HasValue)
                return null;

            var hashKey = string.Format("{0}:{1}", ProductID, ParentCategoryID);
            var rnd = new Random(GetSeed(hashKey));

            var allRelatedProducts = productContext.Categories[relatedCategoryID.Value];

            var resultSet = new Dictionary<int, int>();

            if (allRelatedProducts.Count() <= Count * 2)
            {
                int listCount = 0;
                foreach (var id in allRelatedProducts)
                {
                    if (id != ProductID)
                    {
                        resultSet.Add(id, id);
                        listCount++;
                    }

                    if (listCount == Count)
                        break;
                }
            }
            else
            {
                var maxIndex = allRelatedProducts.Count() - 1;
                int missCount = 0;

                while (resultSet.Count() < Count)
                {
                    var index = rnd.Next(0, maxIndex);

                    var id = allRelatedProducts[index];
                    if (!resultSet.ContainsKey(id) && id != ProductID)
                    {
                        resultSet.Add(id, id);
                    }
                    else
                    {
                        missCount++;
                        if (missCount > 50)
                        {
                            // having trouble picking randomly, revert to the simple way

                            int listCount = 0;
                            foreach (var id2 in allRelatedProducts)
                            {
                                if (!resultSet.ContainsKey(id2) && id2 != ProductID)
                                {
                                    resultSet.Add(id2, id2);
                                    listCount++;
                                }

                                if (listCount == Count)
                                    break;
                            }

                            break;
                        }
                    }
                }
            }

            return resultSet.Select(e => e.Value).ToList(); ;

        }


        public List<int> GetListByPatternCorrelator(TProductDataCache productContext, string pattern, bool skipMissingImages, int? excludeProductID)
        {
            // this feature intended to be used where we show a bunch of similar patterns (less self) under the big photos on the product details page

            // cached for 10 minutes

            var cachedResults = CachedListByPatternCorrelatorResult.Lookup(pattern, skipMissingImages);
            if (cachedResults == null)
            {
                using (var dc = new AspStoreDataContextReadOnly(connectionString))
                {
                    // MPN is the pattern correlator, case insensitive
                    var query = dc.Products.Where(e => e.ManufacturerPartNumber == pattern && e.Published == 1 && e.Deleted == 0 && e.ShowBuyButton==1);

                    if (skipMissingImages)
                        query = query.Where(e => e.ImageFilenameOverride != null);

                    // don't do here so can cache
                    //if (excludeProductID.HasValue)
                    //    query = query.Where(e => e.ProductID != excludeProductID.Value);

                    var list = query.OrderBy(e => e.ProductID).Select(e => e.ProductID).ToList();

                    list = list.Where(e => this.ProductData.Products.ContainsKey(e) && SupportedProductGroups.Contains(ProductData.Products[e].ProductGroup.Value)).ToList();

                    cachedResults = new CachedListByPatternCorrelatorResult(pattern, skipMissingImages, list);
                    cachedResults.Insert();
                }
            }

            if (!excludeProductID.HasValue)
                return cachedResults.Products;

            // need a private copy of the list so can remove without harm

            var prunedResult = cachedResults.Products.ToList();
            prunedResult.RemoveAll(e => e == excludeProductID.Value);

            return prunedResult;
        }


        public List<int> GetNewProductListByManufacturer(TProductDataCache productContext, int? manufacturerID, int days, string filter = null, AutoSuggestMode mode = AutoSuggestMode.Contains)
        {
            // filtered to supported product groups

            // cached for 20 minutes

            var cachedResults = CachedNewProductsByManufacturerResult.Lookup(this.StoreKey, manufacturerID, days);
            if (cachedResults == null)
            {
                var supportedGroups = SupportedProductGroups.Select(e => e.DescriptionAttr()).ToList();

                using (var dc = new AspStoreDataContextReadOnly(connectionString))
                {
                    List<int> list = new List<int>();

                    // if manufacturerID is null, then across all manufacturers

                    var createdAfter = DateTime.Now.AddDays(0 - days);
                    var daysDelta = days;
                    // summarily exclude trim and products without images - simply because marketing is more important than technical 

                    if (manufacturerID.GetValueOrDefault() != 0)
                    {
                        for (var attempt = 1; attempt < 9; attempt++)
                        {
                            list = (from e in dc.Products
                                    where supportedGroups.Contains(e.ProductGroup) && e.CreatedOn >= createdAfter && e.ImageFilenameOverride != null && e.ProductGroup != "Trim" && e.Published == 1 && e.Deleted == 0 && e.ShowBuyButton == 1 && dc.ProductManufacturers.Where(pm => pm.ManufacturerID == manufacturerID.Value).Select(pm => pm.ProductID).Contains(e.ProductID)
                                    orderby e.CreatedOn descending
                                    select e.ProductID
                                    ).Take(2000).ToList();

                            if (list.Count() > 20)
                                break;

                            // we seem to be having trouble finding products
                            daysDelta *= 2; // double the look back each time.
                            createdAfter = DateTime.Now.AddDays(-daysDelta);
                        }

                        list.Shuffle(); // to mix up the patterns which may be adjacent

                        // don't qualify rugs using inventory status because that status reflects only the default SKU, and could be other skus are in stock
                        if (HasAutomatedInventoryTracking && StoreKey != StoreKeys.InsideRugs)
                            list = list.Where(p => ProductData.Products.ContainsKey(p)  && ProductData.Products[p].StockStatus == InventoryStatus.InStock).ToList();
                        else
                            list = list.Where(p => ProductData.Products.ContainsKey(p)).ToList();
                    }
                    else
                    {
                        // need a fair distribution amongst manufacturers

                        // first build a dic by manufacturer of all their new products with images (not trim)

                        var dic = new Dictionary<int, List<int>>();

                        var manufacturers = dc.Manufacturers.Where(e => e.Deleted == 0 && e.Published == 1).Select(e => e.ManufacturerID).ToList();
                        foreach(var thisManufacturerID in manufacturers)
                        {
                            var manufacturersProducts = (from e in dc.Products
                                                         where supportedGroups.Contains(e.ProductGroup) && e.CreatedOn >= createdAfter && e.ImageFilenameOverride != null && e.ProductGroup != "Trim" && e.Published == 1 && e.Deleted == 0 && e.ShowBuyButton == 1 && dc.ProductManufacturers.Where(pm => pm.ManufacturerID == thisManufacturerID).Select(pm => pm.ProductID).Contains(e.ProductID)
                                                         orderby e.CreatedOn descending
                                                         select e.ProductID
                                    ).Take(2000).ToList();

                            // don't qualify rugs using inventory status because that status reflects only the default SKU, and could be other skus are in stock

                            if (HasAutomatedInventoryTracking && StoreKey != StoreKeys.InsideRugs)
                                manufacturersProducts = manufacturersProducts.Where(e => this.ProductData.Products.ContainsKey(e) && ProductData.Products[e].IsNew && ProductData.Products[e].StockStatus == InventoryStatus.InStock).ToList();
                            else
                                manufacturersProducts = manufacturersProducts.Where(e => this.ProductData.Products.ContainsKey(e) && ProductData.Products[e].IsNew).ToList();

                            manufacturersProducts.Shuffle(); // to mix up the patterns which may be adjacent

                            dic[thisManufacturerID] = manufacturersProducts;
                        }

                        // perform several rounds of picks and randomization to really get a fair distribution

                        var selectedProducts = new List<int>();

                        for (int i = 1; i <= 5; i++)
                        {
                            var additionalProducts = new List<int>();

                            foreach (var thisManufacturerID in manufacturers)
                            {
                                var picked = dic[thisManufacturerID].Take(100 * i).ToList();
                                additionalProducts.AddRange(picked);
                                if (picked.Count() > 0)
                                    dic[thisManufacturerID].RemoveRange(0, picked.Count());
                            }

                            additionalProducts.Shuffle();
                            selectedProducts.AddRange(additionalProducts);
                        }

                        list = selectedProducts.Take(2500).ToList();
                    }

                    cachedResults = new CachedNewProductsByManufacturerResult(this.StoreKey, manufacturerID, days, list);
                    cachedResults.Insert();
                }
            }

            if (string.IsNullOrWhiteSpace(filter))
                return cachedResults.Products;

            // full record for all products in cached list
            var productList = cachedResults.Products.Select(p => productContext.Products[p]).ToList();

            List<int> filteredList;

            if (mode == AutoSuggestMode.Contains)
                filteredList = productList.Where(e => e.Name.ContainsIgnoreCase(filter)).Select(e => e.ProductID).ToList();
            else
                filteredList = productList.Where(e => e.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).Select(e => e.ProductID).ToList();

            return filteredList;
        }


        public List<int> GetProductListByProductCollection(TProductDataCache productContext, int collectionID, string filter = null, AutoSuggestMode mode = AutoSuggestMode.Contains)
        {
            // cached for 10 minutes

            var cachedResults = CachedProductListByProductCollectionResult.Lookup(collectionID);
            if (cachedResults == null)
            {
                var supportedGroups = SupportedProductGroups.Select(e => e.DescriptionAttr()).ToList();

                using (var dc = new AspStoreDataContextReadOnly(connectionString))
                {
                    List<int> list;

                    var collectionInfo = dc.ProductCollections
                        .Where(e => e.ProductCollectionID == collectionID)
                        .FirstOrDefault();

                    if (collectionInfo == null)
                        return new List<int>();

                    // the kind of query depends on the kind of collection

                    if (collectionInfo.Kind == 0 || collectionInfo.Kind == 2)
                    {
                        // books | collections

                        // select productID from ProductLabels where Label=@queryLabel and Value=@queryValue and ProductID in 
                        // (select ProductID from ProductManufacturer where ManufacturerID = @manufacturerID) order by ProductID

                        // ordered by:
                        //   ones for sale
                        //     with photos
                        //       newest first

                        list = dc.Products.Where(e => supportedGroups.Contains(e.ProductGroup))
                            .Where(e => dc.ProductManufacturers.Where(pm => pm.ManufacturerID == collectionInfo.ManufacturerID).Select(pm => pm.ProductID).Contains(e.ProductID) && e.Published == 1 && e.Deleted == 0)
                            .Where(e => dc.ProductLabels.Where(pc => pc.Label == collectionInfo.PropName && pc.Value == collectionInfo.PropValue).Select(pc => pc.ProductID).Contains(e.ProductID))
                            .OrderByDescending(e => e.ShowBuyButton)
                            .ThenBy(e => e.ImageFilenameOverride == null)
                            .ThenByDescending(e => e.CreatedOn)
                            .Select(e => e.ProductID)
                            .ToList();
                    }
                    else if (collectionInfo.Kind == 1)
                    {
                        // patterns, use pattern correlator

                        // ordered by:
                        //   ones for sale
                        //     with photos
                        //       newest first

                        list = dc.Products
                            .Where(e => e.ManufacturerPartNumber == collectionInfo.PatternCorrelator && e.Published == 1 && e.Deleted == 0 && supportedGroups.Contains(e.ProductGroup))
                            .OrderByDescending(e => e.ShowBuyButton)
                            .ThenBy(e => e.ImageFilenameOverride == null)
                            .ThenByDescending(e => e.CreatedOn)
                            .Select(e => e.ProductID)
                            .ToList();
                    }
                    else // invalid
                        return new List<int>();

                    cachedResults = new CachedProductListByProductCollectionResult(collectionID, list);
                    cachedResults.Insert();
                }
            }

            if (string.IsNullOrWhiteSpace(filter))
                return cachedResults.Products;

            // full record for all products in cached list
            var productList = cachedResults.Products.Select(p => productContext.Products[p]).ToList();

            List<int> filteredList;

            if (mode == AutoSuggestMode.Contains)
                filteredList = productList.Where(e => e.Name.ContainsIgnoreCase(filter)).Select(e => e.ProductID).ToList();
            else
                filteredList = productList.Where(e => e.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).Select(e => e.ProductID).ToList();

            return filteredList;
        }


        public List<ProductCollection> GetBooksByManufacturer(int manufacturerID, string filter = null, AutoSuggestMode mode = AutoSuggestMode.Contains)
        {
            // manufacturer can be 0 for any

            var cachedResults = CachedBooksByManufacturerResult.Lookup(this.StoreKey, manufacturerID);
            if (cachedResults == null)
            {
                var supportedGroups = SupportedProductGroups.Select(e => e.DescriptionAttr()).ToList();
                using (var dc = new AspStoreDataContextReadOnly(connectionString))
                {
                    List<ProductCollection> list = null;
                    var createdAfter = DateTime.Now.AddDays(0 - 90);

                    if (manufacturerID == 0)
                    {
                        //  any manufacturer

                        var dic = new Dictionary<int, List<ProductCollection>>();
                        var manufacturers = dc.Manufacturers.Where(e => e.Deleted == 0 && e.Published == 1).Select(e => e.ManufacturerID).ToList();
                        foreach (var thisManufacturerID in manufacturers)
                        {
                            var manufacturersCollection = dc.ProductCollections.Where(e => e.ManufacturerID == thisManufacturerID  && e.Kind == 0 && e.Published == 1 && supportedGroups.Contains(e.ProductGroup)  && e.ImageFilenameOverride != null && e.CreatedOn >= createdAfter).OrderByDescending(e => e.CreatedOn).Take(1000).ToList();
                            manufacturersCollection.Shuffle(); 
                            dic[thisManufacturerID] = manufacturersCollection;
                        }

                        var selectedCollections = new List<ProductCollection>();

                        for (int i = 1; i <= 5; i++)
                        {
                            var additionalCollections = new List<ProductCollection>();

                            foreach (var thisManufacturerID in manufacturers)
                            {
                                var picked = dic[thisManufacturerID].Take(100 * i).ToList();
                                additionalCollections.AddRange(picked);
                                if (picked.Count() > 0)
                                    dic[thisManufacturerID].RemoveRange(0, picked.Count());
                            }

                            additionalCollections.Shuffle();
                            selectedCollections.AddRange(additionalCollections);
                        }

                        list = selectedCollections.Take(1000).ToList();
                    }
                    else
                    {
                        // specific manufacturer
                        list = dc.ProductCollections.Where(e => e.ManufacturerID == manufacturerID && e.Kind == 0 && e.Published == 1 && supportedGroups.Contains(e.ProductGroup)).OrderBy(e => e.ImageFilenameOverride == null).ThenByDescending(e => e.CreatedOn).ToList();
                    }
                    cachedResults = new CachedBooksByManufacturerResult(this.StoreKey, manufacturerID, list);
                    cachedResults.Insert();
                }
            }

            if(string.IsNullOrWhiteSpace(filter))
                return cachedResults.Collections;

            List<ProductCollection> filteredList;

            if (manufacturerID == 0)
            {
                if (mode == AutoSuggestMode.Contains)
                    filteredList = cachedResults.Collections.Where(e => e.Name.ContainsIgnoreCase(filter)).ToList();
                else
                    filteredList = cachedResults.Collections.Where(e => e.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                if (mode == AutoSuggestMode.Contains)
                    filteredList = cachedResults.Collections.Where(e => e.ShortName.ContainsIgnoreCase(filter)).ToList();
                else
                    filteredList = cachedResults.Collections.Where(e => e.ShortName.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return filteredList;
        }

        public List<ProductCollection> GetCollectionsByManufacturer(int manufacturerID, string filter = null, AutoSuggestMode mode = AutoSuggestMode.Contains)
        {
            // manufacturer can be 0 for any

            var cachedResults = CachedCollectionsByManufacturerResult.Lookup(this.StoreKey, manufacturerID);
            if (cachedResults == null)
            {
                var supportedGroups = SupportedProductGroups.Select(e => e.DescriptionAttr()).ToList();

                using (var dc = new AspStoreDataContextReadOnly(connectionString))
                {
                    List<ProductCollection> list = null;
                    var createdAfter = DateTime.Now.AddDays(0 - 90);

                    if (manufacturerID == 0)
                    {
                        // any manufacturer
                        var dic = new Dictionary<int, List<ProductCollection>>();
                        var manufacturers = dc.Manufacturers.Where(e => e.Deleted == 0 && e.Published == 1).Select(e => e.ManufacturerID).ToList();
                        foreach (var thisManufacturerID in manufacturers)
                        {
                            var manufacturersCollection = dc.ProductCollections.Where(e => e.ManufacturerID == thisManufacturerID && e.Kind == 2 && e.Published == 1 && supportedGroups.Contains(e.ProductGroup) && e.ImageFilenameOverride != null && e.CreatedOn >= createdAfter).OrderByDescending(e => e.CreatedOn).Take(1000).ToList();
                            manufacturersCollection.Shuffle();
                            dic[thisManufacturerID] = manufacturersCollection;
                        }

                        var selectedCollections = new List<ProductCollection>();

                        for (int i = 1; i <= 5; i++)
                        {
                            var additionalCollections = new List<ProductCollection>();

                            foreach (var thisManufacturerID in manufacturers)
                            {
                                var picked = dic[thisManufacturerID].Take(100 * i).ToList();
                                additionalCollections.AddRange(picked);
                                if (picked.Count() > 0)
                                    dic[thisManufacturerID].RemoveRange(0, picked.Count());
                            }

                            additionalCollections.Shuffle();
                            selectedCollections.AddRange(additionalCollections);
                        }

                        list = selectedCollections.Take(1000).ToList();
                    }
                    else
                    {
                        // specific manufacturer
                        list = dc.ProductCollections.Where(e => e.ManufacturerID == manufacturerID && e.Kind == 2 && e.Published == 1 && supportedGroups.Contains(e.ProductGroup)).OrderBy(e => e.ImageFilenameOverride == null).ThenByDescending(e => e.CreatedOn).ToList();
                    }
                    cachedResults = new CachedCollectionsByManufacturerResult(this.StoreKey, manufacturerID, list);
                    cachedResults.Insert();
                }
            }

            if (string.IsNullOrWhiteSpace(filter)) 
                return cachedResults.Collections;

            List<ProductCollection> filteredList;

            if (manufacturerID == 0)
            {
                if (mode == AutoSuggestMode.Contains)
                    filteredList = cachedResults.Collections.Where(e => e.Name.ContainsIgnoreCase(filter)).ToList();
                else
                    filteredList = cachedResults.Collections.Where(e => e.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                if (mode == AutoSuggestMode.Contains)
                    filteredList = cachedResults.Collections.Where(e => e.ShortName.ContainsIgnoreCase(filter)).ToList();
                else
                    filteredList = cachedResults.Collections.Where(e => e.ShortName.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }


            return filteredList;
        }


        public List<ProductCollection> GetPatternsByManufacturer(int manufacturerID, string filter = null, AutoSuggestMode mode=AutoSuggestMode.Contains)
        {
            // manufacturer can be 0 for any

            var cachedResults = CachedPatternsByManufacturerResult.Lookup(this.StoreKey, manufacturerID);
            if (cachedResults == null)
            {
                var supportedGroups = SupportedProductGroups.Select(e => e.DescriptionAttr()).ToList();

                using (var dc = new AspStoreDataContextReadOnly(connectionString))
                {
                    List<ProductCollection> list = null;
                    var createdAfter = DateTime.Now.AddDays(0 - 90);

                    if (manufacturerID == 0)
                    {
#if true
                        var dic = new Dictionary<int, List<ProductCollection>>();
                        var manufacturers = dc.Manufacturers.Where(e => e.Deleted == 0 && e.Published == 1).Select(e => e.ManufacturerID).ToList();
                        foreach (var thisManufacturerID in manufacturers)
                        {
                            var manufacturersCollection = dc.ProductCollections.Where(e => e.ManufacturerID == thisManufacturerID && e.Kind == 1 && e.Published == 1 && supportedGroups.Contains(e.ProductGroup) && e.ImageFilenameOverride != null && e.CreatedOn >= createdAfter).OrderByDescending(e => e.CreatedOn).Take(1000).ToList();
                            manufacturersCollection.Shuffle();
                            dic[thisManufacturerID] = manufacturersCollection;
                        }

                        var selectedCollections = new List<ProductCollection>();

                        for (int i = 1; i <= 5; i++)
                        {
                            var additionalCollections = new List<ProductCollection>();

                            foreach (var thisManufacturerID in manufacturers)
                            {
                                var picked = dic[thisManufacturerID].Take(100 * i).ToList();
                                additionalCollections.AddRange(picked);
                                if (picked.Count() > 0)
                                    dic[thisManufacturerID].RemoveRange(0, picked.Count());
                            }

                            additionalCollections.Shuffle();
                            selectedCollections.AddRange(additionalCollections);
                        }

                        list = selectedCollections.Take(1000).ToList();
#else
                        list = dc.ProductCollections.Where(e => e.Kind == 1 && e.Published == 1 && e.ImageFilenameOverride != null && e.CreatedOn >= createdAfter).OrderByDescending(e => e.CreatedOn).Take(1000).ToList();
#endif
                    }
                    else
                    {
                        list = dc.ProductCollections.Where(e => e.ManufacturerID == manufacturerID && e.Kind == 1 && e.Published == 1 && supportedGroups.Contains(e.ProductGroup)).OrderBy(e => e.ImageFilenameOverride == null).ThenByDescending(e => e.CreatedOn).ToList();
                    }
                    cachedResults = new CachedPatternsByManufacturerResult(this.StoreKey, manufacturerID, list);
                    cachedResults.Insert();
                }
            }

            if (string.IsNullOrWhiteSpace(filter))
                return cachedResults.Collections;

            List<ProductCollection> filteredList;

            if (manufacturerID == 0)
            {
                if (mode == AutoSuggestMode.Contains)
                    filteredList = cachedResults.Collections.Where(e => e.Name.ContainsIgnoreCase(filter)).ToList();
                else
                    filteredList = cachedResults.Collections.Where(e => e.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                if (mode == AutoSuggestMode.Contains)
                    filteredList = cachedResults.Collections.Where(e => e.ShortName.ContainsIgnoreCase(filter)).ToList();
                else
                    filteredList = cachedResults.Collections.Where(e => e.ShortName.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return filteredList;

        }

        public List<int> GetListByCategory(TProductDataCache productContext, int categoryID, ProductSortOrder orderBy)
        {
            List<int> list = null;
            var listKey = MakeSortedListKey(categoryID, orderBy);

            productContext.SortedCategories.TryGetValue(listKey, out list);

            return list;
        }

        public List<int> GetListByManufacturer(TProductDataCache productContext, int manufacturerID, ProductSortOrder orderBy)
        {
            List<int> list = null;
            var listKey = MakeSortedListKey(manufacturerID, orderBy);

            productContext.SortedManufacturers.TryGetValue(listKey, out list);

            return list;
        }


        protected virtual DataTable SearchBySku(string query)
        {

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sSQL = "select productID from Product where SKU=@query and published=1 and deleted=0";

                SqlDataAdapter da = null;
                DataTable dtResults = null;

                try
                {
                    da = new SqlDataAdapter(sSQL, con);
                    dtResults = new DataTable();
                    da.SelectCommand.Parameters.Add("@query", SqlDbType.VarChar, 4000).Value = query;

                    da.Fill(dtResults);
                    da.Dispose();
                }
                catch (Exception ex)
                {
                    if (da != null)
                        da.Dispose();
                    if (dtResults != null)
                        dtResults.Dispose();
                    throw (ex);
                }
                return dtResults;

            }
        }


        private DataTable FullTextSearch(string query)
        {
            var ftsQuery = MakeContainsQuery(query) ?? string.Empty;

            // return null if nothing to query

            if (string.IsNullOrWhiteSpace(ftsQuery))
                return null;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                string sSQL = "select productID from Product where CONTAINS(*, @fts) and Published=1 and deleted=0";

                SqlDataAdapter da = null;
                DataTable dtResults = null;

                try
                {
                    da = new SqlDataAdapter(sSQL, con);
                    dtResults = new DataTable();
                    da.SelectCommand.Parameters.Add("@fts", SqlDbType.VarChar, 4000).Value = ftsQuery;

                    da.Fill(dtResults);
                    da.Dispose();
                }
                catch (Exception ex)
                {
                    if (da != null)
                        da.Dispose();
                    if (dtResults != null)
                        dtResults.Dispose();
                    throw (ex);
                }
                return dtResults;

            }
        }

        protected string MakeAdjustedQuery(string query)
        {
            string adjQuery = query; // has been prescreened to not be null

            if (adjQuery.Contains("/"))
                adjQuery = string.Format("\"{0}\"", query);

            return adjQuery;
        }


        #region Search Operators for Image Search

        private const int DEFAULT_IMAGESEARCH_CEDD_TOLERANCE = 60;
        private const int DEFAULT_IMAGESEARCH_COLOR_TOLERANCE = 15;

        private bool isQueryParameterSKU(string parameter)
        {
            return (ProductData.LookupProduct(parameter) != null);
        }

        private bool isQueryParameterProductID(string parameter)
        {
            int productID;
            if (int.TryParse(parameter, out productID))
                return (ProductData.LookupProduct(productID) != null);

            return false;
        }

        private bool isQueryParameterRGB(string parameter)
        {
            return Regex.IsMatch(parameter, @"^\#[a-fA-F0-9]{6}$");
        }

        private bool isQueryParameterUrl(string parameter)
        {
            // http, https, ftp

            return (parameter.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || parameter.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || parameter.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase));
        }


        private string FindUploadedPhotoFilepath(string guid)
        {
            // full path to found file, else null

            var extensions = new string[] { ".jpg", ".png" };

            foreach (var ext in extensions)
            {
                var tryPath = Path.Combine(UploadedPhotosFolder, guid + ext);
                if (File.Exists(tryPath))
                    return tryPath;
            }

            return null;
        }


        private List<int> ExecuteLikeSearchOperator(string queryParameter, List<int> recentlyViewed)
        {
            // sku, productID, url

            // recentlyViewed will always be null for now

            if (isQueryParameterSKU(queryParameter))
            {
                // find CEDD matches
                var productID = ProductData.LookupProductIDFromSKU(queryParameter);
                return this.ImageSearch.FindSimilarProducts(productID.Value, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else if (isQueryParameterProductID(queryParameter))
            {
                // find CEDD matches
                return this.ImageSearch.FindSimilarProducts(int.Parse(queryParameter), DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else if (Regex.IsMatch(queryParameter, MATCH_UPLOADED_PHOTO_GUID))
            {
                // like:@4e50881d-f596-ae17-581f-b90bad543cc0
                var guid = queryParameter.Substring(1);
                var filepath = FindUploadedPhotoFilepath(guid);

                if (filepath == null)
                    return new List<int>();

                var cedd = Website.ImageSearch.GetDescriptorFromImageFile(filepath);

                if (cedd == null || cedd.Length != 144)
                    return new List<int>();

                if (cedd.Sum(e => e) == 0)
                    Debug.WriteLine("empty descriptor");

                return this.ImageSearch.FindSimilarProducts(null, cedd, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            
            }
            else if (isQueryParameterUrl(queryParameter))
            {
                var cedd = Website.ImageSearch.GetDescriptorFromImageUrl(queryParameter);
                if (cedd == null || cedd.Length != 144)
                    return new List<int>();

                if (cedd.Sum(e => e) == 0)
                    Debug.WriteLine("empty descriptor");

                return this.ImageSearch.FindSimilarProducts(null, cedd, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else 
                return new List<int>();
        }

        private List<int> ExecuteColorSearchOperator(string queryParameter, List<int> recentlyViewed)
        {
            // #rgb, sku, productID, url

            // if RGB, then look for products having that dominant color

            // if SKU or ProductID, then use CEDD descriptors to match the color
            // histogram

            // if URL, retrieve image, get CEDD and do histogram search

            // recentlyViewed will always be null for now

            if (isQueryParameterRGB(queryParameter))
            {
                // then is a dominant color, find products with this top color
                var color = InsideStores.Imaging.ExtensionMethods.ToColor(queryParameter);
                return this.ImageSearch.FindProductsHavingTopDominantColor(color, DEFAULT_IMAGESEARCH_COLOR_TOLERANCE, 1000);
            }
            else if (isQueryParameterSKU(queryParameter))
            {
                // find prodcuts with CEDD colors
                var productID = ProductData.LookupProductIDFromSKU(queryParameter);
                return this.ImageSearch.FindSimilarProductsByColor(productID.Value, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else if (isQueryParameterProductID(queryParameter))
            {
                // find prodcuts with CEDD colors
                return this.ImageSearch.FindSimilarProductsByColor(int.Parse(queryParameter), DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else if (Regex.IsMatch(queryParameter, MATCH_UPLOADED_PHOTO_GUID))
            {
                // like:@4e50881d-f596-ae17-581f-b90bad543cc0
                var guid = queryParameter.Substring(1);
                var filepath = FindUploadedPhotoFilepath(guid);

                if (filepath == null)
                    return new List<int>();

                var cedd = Website.ImageSearch.GetDescriptorFromImageFile(filepath);

                if (cedd == null || cedd.Length != 144)
                    return new List<int>();

                if (cedd.Sum(e => e) == 0)
                    Debug.WriteLine("empty descriptor");

                return this.ImageSearch.FindSimilarProductsByColor(null, cedd, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);

            }
            else if (isQueryParameterUrl(queryParameter))
            {
                var cedd = Website.ImageSearch.GetDescriptorFromImageUrl(queryParameter);
                if (cedd == null || cedd.Length != 144)
                    return new List<int>();

                return this.ImageSearch.FindSimilarProductsByColor(null, cedd, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else
                return new List<int>();
        }

        private List<int> ExecuteStyleSearchOperator(string queryParameter, List<int> recentlyViewed)
        {
            // sku, productID, url

            // recentlyViewed will always be null for now

            if (isQueryParameterSKU(queryParameter))
            {
                // find prodcuts with CEDD textures
                var productID = ProductData.LookupProductIDFromSKU(queryParameter);
                return this.ImageSearch.FindSimilarProductsByTexture(productID.Value, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else if (isQueryParameterProductID(queryParameter))
            {
                // find prodcuts with CEDD textures
                return this.ImageSearch.FindSimilarProductsByTexture(int.Parse(queryParameter), DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else if (Regex.IsMatch(queryParameter, MATCH_UPLOADED_PHOTO_GUID))
            {
                // like:@4e50881d-f596-ae17-581f-b90bad543cc0
                var guid = queryParameter.Substring(1);
                var filepath = FindUploadedPhotoFilepath(guid);

                if (filepath == null)
                    return new List<int>();

                var cedd = Website.ImageSearch.GetDescriptorFromImageFile(filepath);

                if (cedd == null || cedd.Length != 144)
                    return new List<int>();

                if (cedd.Sum(e => e) == 0)
                    Debug.WriteLine("empty descriptor");

                return this.ImageSearch.FindSimilarProductsByTexture(null, cedd, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else if (isQueryParameterUrl(queryParameter))
            {
                var cedd = Website.ImageSearch.GetDescriptorFromImageUrl(queryParameter);
                if (cedd == null || cedd.Length != 144)
                    return new List<int>();

                return this.ImageSearch.FindSimilarProductsByTexture(null, cedd, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 1000);
            }
            else
                return new List<int>();
        }

        private List<int> ExecuteFindCorrelatedProducts(CacheProduct product, List<int> recentlyViewed)
        {
            using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
            {
                var products = dc.Products.Where(e => e.ManufacturerPartNumber == product.Correlator 
                    && e.ProductGroup == product.ProductGroup.Value.ToString() && e.Deleted==0 && e.Published==1).Select(e => e.ProductID).ToList();
                return products;
            }
        }

        private List<int> ExecutePatternSearchOperator(string queryParameter, List<int> recentlyViewed)
        {
            // must refer to an existing product
            // sku, productID

            // recentlyViewed will always be null for now

            if (isQueryParameterSKU(queryParameter))
            {
                var product = ProductData.LookupProduct(queryParameter);
                return ExecuteFindCorrelatedProducts(product, recentlyViewed);
            
            }
            else if (isQueryParameterProductID(queryParameter))
            {
                var product = ProductData.LookupProduct(int.Parse(queryParameter));
                return ExecuteFindCorrelatedProducts(product, recentlyViewed);
            }
            else
                return new List<int>();

        }

        private List<int> ExecuteSuggestedSearchOperator(string queryParameter, List<int> recentlyViewed)
        {
            // must refer to an existing product
            // sku, productID

            // recentlyViewed is optional, will contain a list of recently viewed product IDs.

            if (recentlyViewed != null && recentlyViewed.Count() > 1)
            {
                // we have a list of recently viewed
                return this.ImageSearch.FindSimilarProducts(recentlyViewed, DEFAULT_IMAGESEARCH_CEDD_TOLERANCE, 300);
            }
            else
            {
                // just a single product to work with

                // for a quick solution - pick randomly from LIKE results
                var results = ExecuteLikeSearchOperator(queryParameter, null).Skip(75).Take(120).ToList();
                results.Shuffle();
                return results;
            }
        }
        
        #endregion

        private List<int> SearchProducts(string query, List<int> recentlyViewed=null)
        {

            var ftsSearchOperators = new List<SearchOperator>()
            { 
                new SearchOperator("like:", ExecuteLikeSearchOperator),
                new SearchOperator("color:", ExecuteColorSearchOperator),
                new SearchOperator("style:", ExecuteStyleSearchOperator),
                new SearchOperator("pattern:", ExecutePatternSearchOperator),
                new SearchOperator("suggested:", ExecuteSuggestedSearchOperator)
            };

            // if nothing to search, return empty results

            if (string.IsNullOrWhiteSpace(query))
                return new List<int>();

            Performance.TotalApiSearchRequests.Bump();
            var dtStart = DateTime.Now;

            List<int> products = new List<int>();

            if (IsImageSearchEnabled)
            {
                foreach(var op in ftsSearchOperators)
                {
                    if (query.StartsWith(op.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var parsedQuery = query.Substring(op.Key.Length).Trim();
                            if (string.IsNullOrWhiteSpace(parsedQuery))
                                return new List<int>();
                            products = op.Callback(parsedQuery, recentlyViewed);
                            // don't clutter up the data
                            //Performance.AddSearchMetric(query, dtStart, products.Count(), isAdvanced: false);
                            return products;
                        }
                        catch(Exception Ex)
                        {
                            Debug.WriteLine(Ex.Message);
                            return new List<int>();
                        }
                    }
                }
            }

            var dt = SearchBySku(MakeAdjustedQuery(query));

            if (dt != null && dt.Rows.Count > 0)
            {
                Performance.AddSearchMetric(query, dtStart, dt.Rows.Count, isAdvanced: false);
                foreach (DataRow row in dt.Rows)
                {
                    products.Add((int)row[0]);
                }
                return products;
            }

            dt = FullTextSearch(MakeAdjustedQuery(query));

            if (dt == null)
                return new List<int>();

            Performance.AddSearchMetric(query, dtStart, dt.Rows.Count, isAdvanced: false);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    products.Add((int)row[0]);
                }
            }

            return products;
        }

        private string MakeStringList(List<int> list)
        {
            var ary = new string[list.Count];
            for (int i = 0; i < ary.Length; i++)
                ary[i] = list[i].ToString();

            return string.Join(",", ary);
        }


        protected string MakeContainsQuery(string phrase)
        {
            string sql = null;
            var grammar = new SearchGrammar();
            var parser = new Parser(grammar);

            try
            {

                var parseTree = parser.Parse(phrase.ToLower());

                if (parseTree == null || parseTree.HasErrors())
                    return null;

                sql = SearchGrammar.ConvertQuery(parseTree.Root);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }
            finally
            {
                // they seem to linger...
                grammar = null;
                parser = null;
            }

            return sql;
        }

        private DataTable FullTextSearch(SearchCriteria searchCriteria)
        {
            bool bFTS = false;
            string ftsQuery = null;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                var sbQuery = new StringBuilder(4000);
                int whereCount = 0;

                // if using FTS, then must set bFTS true and fill in ftsQuery
                if (searchCriteria.Keywords != null)
                {
                    ftsQuery = MakeContainsQuery(searchCriteria.Keywords);
                    if (ftsQuery != null)
                        bFTS = true;
                }

                sbQuery.Append("select ProductID from Product where");

                if (bFTS)
                {
                    if (whereCount > 0)
                        sbQuery.Append(" and");

                    sbQuery.Append(" CONTAINS(*, @ftsQuery)");
                    whereCount++;
                }

                if (searchCriteria.ManufacturerList.Count > 0)
                {
                    if (whereCount > 0)
                        sbQuery.Append(" and");

                    sbQuery.AppendFormat(" ProductID in (select ProductID from ProductManufacturer where ManufacturerID in ({0}))", MakeStringList(searchCriteria.ManufacturerList));
                    whereCount++;
                }

                if (searchCriteria.TypeList.Count > 0)
                {
                    if (whereCount > 0)
                        sbQuery.Append(" and");

                    sbQuery.AppendFormat(" ProductID in (select ProductID from ProductCategory where CategoryID in ({0}))", MakeStringList(searchCriteria.TypeList));
                    whereCount++;
                }

                if (searchCriteria.PatternList.Count > 0)
                {
                    if (whereCount > 0)
                        sbQuery.Append(" and");

                    sbQuery.AppendFormat(" ProductID in (select ProductID from ProductCategory where CategoryID in ({0}))", MakeStringList(searchCriteria.PatternList));
                    whereCount++;
                }

                if (searchCriteria.ColorList.Count > 0)
                {
                    if (whereCount > 0)
                        sbQuery.Append(" and");

                    sbQuery.AppendFormat(" ProductID in (select ProductID from ProductCategory where CategoryID in ({0}))", MakeStringList(searchCriteria.ColorList));
                    whereCount++;
                }

                if (searchCriteria.PriceRangeList.Count > 0)
                {
                    if (whereCount > 0)
                        sbQuery.Append(" and");

                    sbQuery.AppendFormat(" ProductID in (select ProductID from ProductCategory where CategoryID in ({0}))", MakeStringList(searchCriteria.PriceRangeList));
                    whereCount++;
                }

                if (whereCount > 0)
                    sbQuery.Append(" and");

                sbQuery.AppendFormat(" Deleted=0 and Published=1");
                whereCount++;


                var sQuery = sbQuery.ToString();

                SqlDataAdapter da = null;
                DataTable dtResults = null;
                // Debug.WriteLine(string.Format("SQL Query: {0}", sQuery));

                try
                {
                    da = new SqlDataAdapter(sQuery, con);
                    dtResults = new DataTable();

                    // if needed, add in the FTS query parameter

                    if (bFTS)
                        da.SelectCommand.Parameters.Add("@ftsQuery", SqlDbType.NVarChar, 4000).Value = ftsQuery;

                    da.Fill(dtResults);
                    da.Dispose();
                }
                catch (Exception ex)
                {
                    if (da != null)
                        da.Dispose();
                    if (dtResults != null)
                        dtResults.Dispose();
                    throw (ex);
                }
                return dtResults;
            }

        }

        private DataTable FullTextSearch(FacetSearchCriteria searchCriteria)
        {
            // note that json is deserializing empty lists to null, so must guard against that

            var ftsSearchOperators = new List<SearchOperator>()
            { 
                new SearchOperator("like:", ExecuteLikeSearchOperator),
                new SearchOperator("color:", ExecuteColorSearchOperator),
                new SearchOperator("style:", ExecuteStyleSearchOperator),
                new SearchOperator("pattern:", ExecutePatternSearchOperator),
                new SearchOperator("suggested:", ExecuteSuggestedSearchOperator)
            };


            bool bFTS = false;
            bool bImageSearch = false;
            string ftsQuery = null;

            // if nothing to search, return empty results

            if (string.IsNullOrWhiteSpace(searchCriteria.SearchPhrase) && searchCriteria.Facets.Count() == 0)
                return null;

            List<int> products = new List<int>();

            var searchPhrase = searchCriteria.SearchPhrase;
            if (!string.IsNullOrWhiteSpace(searchPhrase) && IsImageSearchEnabled)
            {
                foreach (var op in ftsSearchOperators)
                {
                    if (searchPhrase.StartsWith(op.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        bImageSearch = true;
                        try
                        {
                            var parsedQuery = searchPhrase.Substring(op.Key.Length).Trim();
                            if (string.IsNullOrWhiteSpace(parsedQuery))
                                break;

                            products = op.Callback(parsedQuery, searchCriteria.RecentlyViewed ?? new List<int>());
                            break;
                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine(Ex.Message);
                        }
                    }
                }
            }

            // products variable could now potentially contain a list of results from image search
            // if any kind of image search, regardless if anything found, then bImageSearch is true,
            // which is mutually exclusive to bFTS

            var sbQuery = new StringBuilder(bImageSearch ? 15000 : 4000);
            int whereCount = 0;

            // if using FTS, then must set bFTS true and fill in ftsQuery
            if (!bImageSearch && !string.IsNullOrWhiteSpace(searchPhrase))
            {
                ftsQuery = MakeContainsQuery(searchPhrase);
                if (ftsQuery != null)
                    bFTS = true;
            }

            sbQuery.Append("select top 10000 ProductID from Product where");

            Action And = () =>
                {
                    if (whereCount > 0)
                        sbQuery.Append(" and");

                    whereCount++;
                };

            if (bFTS)
            {
                And();
                sbQuery.Append(" CONTAINS(*, @ftsQuery)");
            }


            foreach(var facet in searchCriteria.Facets ?? new List<FacetItem>())
            {
                if (facet.Members == null || facet.Members.Count == 0)
                    continue;

                switch(facet.FacetKey)
                {
                    case "Manufacturer":
                        And();
                        sbQuery.AppendFormat(" ProductID in (select ProductID from ProductManufacturer where ManufacturerID in ({0}))", MakeStringList(facet.Members));
                        break;

                    // category - we actually don't care which...only that they are in segmented groups
                    default:
                        And();
                        sbQuery.AppendFormat(" ProductID in (select ProductID from ProductCategory where CategoryID in ({0}))", MakeStringList(facet.Members));
                        break;
                }
            }

            // special case for IF to separate fabric from trim
            // trim and fabric are mutually exclusive
            if (StoreKey == StoreKeys.InsideFabric)
            {
                var isTrim = searchCriteria.Facets != null && searchCriteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 0;

                And();
                sbQuery.AppendFormat(" ProductGroup='{0}'", isTrim ? "Trim" : "Fabric");
            }

            // merge in the image search results if any
            if (products.Count > 0)
            {
                And();
                sbQuery.AppendFormat(" ProductID in ({0})", MakeStringList(products));
            }

            And();
            sbQuery.AppendFormat(" Deleted=0 and Published=1 and ShowBuyButton=1 and ImageFilenameOverride is not null");


            var sQuery = sbQuery.ToString();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlDataAdapter da = null;
                DataTable dtResults = null;
                //Debug.WriteLine(string.Format("SQL Query: {0}", sQuery));

                try
                {
                    da = new SqlDataAdapter(sQuery, con);
                    dtResults = new DataTable();

                    // if needed, add in the FTS query parameter

                    if (bFTS)
                        da.SelectCommand.Parameters.Add("@ftsQuery", SqlDbType.NVarChar, 4000).Value = ftsQuery;

                    da.Fill(dtResults);
                    da.Dispose();
                }
                catch (Exception ex)
                {
                    if (da != null)
                        da.Dispose();
                    if (dtResults != null)
                        dtResults.Dispose();
                    throw (ex);
                }
                return dtResults;
            }

        }

        private List<int> SearchProductsByLabelValueWithinManufacturer(int manufacturerID, string label, string value)
        {
            // if nothing to search, return empty results

            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value) || manufacturerID == 0)
                return new List<int>();

            List<int> products = new List<int>();

            Func<DataTable> performQuery = () =>
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string sSQL = "select productID from ProductLabels where Label=@queryLabel and Value=@queryValue and ProductID in (select ProductID from ProductManufacturer where ManufacturerID = @manufacturerID) order by ProductID";

                    SqlDataAdapter da = null;
                    DataTable dtResults = null;

                    try
                    {
                        da = new SqlDataAdapter(sSQL, con);
                        dtResults = new DataTable();
                        da.SelectCommand.Parameters.Add("@queryLabel", SqlDbType.VarChar, 256).Value = label;
                        da.SelectCommand.Parameters.Add("@queryValue", SqlDbType.VarChar, 256).Value = value;
                        da.SelectCommand.Parameters.Add("@manufacturerID", SqlDbType.Int, 4000).Value = manufacturerID;

                        da.Fill(dtResults);
                        da.Dispose();
                    }
                    catch (Exception ex)
                    {
                        if (da != null)
                            da.Dispose();
                        if (dtResults != null)
                            dtResults.Dispose();
                        throw (ex);
                    }
                    return dtResults;

                }
            };

            var dt = performQuery();

            if (dt == null)
                return new List<int>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    products.Add((int)row[0]);
                }
            }

            return products;
        }


        /// <summary>
        /// Create a simple string reprentation of some of the criteria to show in reporting.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private string SearchCriteriaToMetrics(SearchCriteria searchCriteria)
        {
                var sb = new StringBuilder(100);

                int items = 0;

                sb.Append("{");
                if (searchCriteria.ManufacturerList != null && searchCriteria.ManufacturerList.Count() > 0)
                {
                    sb.AppendFormat("Manufacturers={0:N0}", searchCriteria.ManufacturerList.Count());
                    items++;
                }

                if (searchCriteria.TypeList != null && searchCriteria.TypeList.Count() > 0)
                {
                    if (items > 0)
                        sb.Append(", ");

                    sb.AppendFormat("Types={0:N0}", searchCriteria.TypeList.Count());
                    items++;
                }

                if (searchCriteria.PatternList != null && searchCriteria.PatternList.Count() > 0)
                {
                    if (items > 0)
                        sb.Append(", ");

                    sb.AppendFormat("Patterns={0:N0}", searchCriteria.PatternList.Count());
                    items++;
                }

                if (searchCriteria.ColorList != null && searchCriteria.ColorList.Count() > 0)
                {
                    if (items > 0)
                        sb.Append(", ");

                    sb.AppendFormat("Colors={0:N0}", searchCriteria.ColorList.Count());
                    items++;
                }

                if (searchCriteria.PriceRangeList != null && searchCriteria.PriceRangeList.Count() > 0)
                {
                    if (items > 0)
                        sb.Append(", ");

                    sb.AppendFormat("PriceRanges={0:N0}", searchCriteria.PriceRangeList.Count());
                    items++;
                }

                sb.Append("}");

                if (items > 0)
                    return sb.ToString();

                return null;
        }

        /// <summary>
        /// Create a simple string reprentation of some of the criteria to show in reporting.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private string SearchCriteriaToMetrics(FacetSearchCriteria searchCriteria)
        {
            var sb = new StringBuilder(100);

            int items = 0;

            sb.Append("{");

            if (searchCriteria.Facets != null)
            {
                foreach(var item in searchCriteria.Facets)
                {
                    if (items > 0)
                        sb.Append(", ");

                    if (item.Members != null)
                        sb.AppendFormat("{0}={1:N0}", item.FacetKey, item.Members.Count());
                    items++;
                }
            }

            sb.Append("}");

            if (items > 0)
                return sb.ToString();

            return null;
        }

        /// <summary>
        /// Find out approximate result count for a given facet search.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int FacetSearchProductsResultCount(FacetSearchCriteria searchCriteria)
        {
            DataTable dt = null;

            dt = FullTextSearch(searchCriteria);
            if (dt == null)
                return 0;

            return dt.Rows.Count;
        }

        public List<int> FacetSearchProductsResultSet(FacetSearchCriteria searchCriteria)
        {
            var list = new List<int>();
            DataTable dt = null;

            dt = FullTextSearch(searchCriteria);
            if (dt == null)
                return list;

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add((int)row[0]);
                }
            }

            return list;
        }

        /// <summary>
        /// Facet search
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<int> FacetSearchProducts(FacetSearchCriteria searchCriteria)
        {
            Performance.TotalApiAdvSearchRequests.Bump();
            var dtStart = DateTime.Now;

            List<int> products = new List<int>();

            DataTable dt = null;

            dt = FullTextSearch(searchCriteria);
            if (dt == null)
                return products;

            var sbSearchPhrase = new StringBuilder(200);

            bool bHasKeywords = false;

            if (!string.IsNullOrWhiteSpace(searchCriteria.SearchPhrase))
            {
                sbSearchPhrase.Append(searchCriteria.SearchPhrase);
                bHasKeywords = true;
            }

            var criteriaMetrics = SearchCriteriaToMetrics(searchCriteria);
            if (criteriaMetrics != null)
            {
                if (bHasKeywords)
                    sbSearchPhrase.Append("  ");

                sbSearchPhrase.Append(criteriaMetrics);
            }

            Performance.AddSearchMetric(sbSearchPhrase.ToString(), dtStart, dt.Rows.Count, isAdvanced: true);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    products.Add((int)row[0]);
                }
            }

            if (ProductData != null && HasAutomatedInventoryTracking)
            {
                // move in stock products to the top
                var orderedList = products.OrderBy(e => ProductData.Products[e].StockStatus).ToList(); 
                return orderedList;
            }
            else
            {
                return products;
            }
        }

        /// <summary>
        /// Advaanced search
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private List<int> AdvancedSearchProducts(SearchCriteria searchCriteria)
        {
            Performance.TotalApiAdvSearchRequests.Bump();
            var dtStart = DateTime.Now;

            List<int> products = new List<int>();

            DataTable dt = null;

            if (!string.IsNullOrWhiteSpace(searchCriteria.Keywords))
            {
                dt = SearchBySku(searchCriteria.Keywords);

                if (dt != null && dt.Rows.Count > 0)
                {
                    Performance.AddSearchMetric(searchCriteria.Keywords, dtStart, dt.Rows.Count, isAdvanced: true);
                    foreach (DataRow row in dt.Rows)
                    {
                        products.Add((int)row[0]);
                    }
                    return products;
                }
            }


            dt = FullTextSearch(searchCriteria);

            var sbSearchPhrase = new StringBuilder(200);

            bool bHasKeywords = false;

            if (!string.IsNullOrWhiteSpace(searchCriteria.Keywords))
            {
                sbSearchPhrase.Append(searchCriteria.Keywords);
                bHasKeywords = true;
            }

            var criteriaMetrics = SearchCriteriaToMetrics(searchCriteria);
            if (criteriaMetrics != null)
            {
                if (bHasKeywords)
                    sbSearchPhrase.Append("  ");

                sbSearchPhrase.Append(criteriaMetrics);
            }

            Performance.AddSearchMetric(sbSearchPhrase.ToString(), dtStart, dt.Rows.Count, isAdvanced: true);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    products.Add((int)row[0]);
                }
            }
            return products;
        }

        private List<int> GetOrderedSearchResults(TProductDataCache productContext, ProductSortOrder orderBy, CachedSearchResult cache)
        {
            Debug.Assert(cache.Products != null);

            List<int> srchResults;
            srchResults = cache.GetSortedList(orderBy);
            if (srchResults == null)
            {
                // if we did not have a cached list (will always be the case when caching not enabled), then create
                // the new sort and stuff away for next time

                switch (orderBy)
                {
                    case ProductSortOrder.PriceAscend:
                        if (storeKey == StoreKeys.InsideFabric || storeKey == StoreKeys.InsideWallpaper)
                            srchResults = cache.Products.OrderBy(e => productContext.Products[e].OurPrice).ToList();
                        else
                            srchResults = cache.Products.OrderBy(e => productContext.Products[e].LowVariantOurPrice).ToList();
                        break;

                    case ProductSortOrder.PriceDescend:
                        if (storeKey == StoreKeys.InsideFabric || storeKey == StoreKeys.InsideWallpaper)
                            srchResults = cache.Products.OrderByDescending(e => productContext.Products[e].OurPrice).ToList();
                        else
                            srchResults = cache.Products.OrderByDescending(e => productContext.Products[e].HighVariantOurPrice).ToList();
                        break;

                    default:
                        // should never get here since the default sort is always in the cache object
                        Debug.Assert(orderBy != ProductSortOrder.Default);
                        break;
                }

                if (EnableSearchCaching && orderBy != ProductSortOrder.Default)
                    cache.AddSortedList(orderBy, srchResults);
            }

            return srchResults;
        }

        public List<CacheProduct> FindSimilarProducts(TProductDataCache productContext, out int PageCount, int ProductID, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var cacheKey = string.Format("FindSimilarProducts:{0}", ProductID);
            return ProductImageSearch(cacheKey, () =>
            {
                return ImageSearch.FindSimilarProducts(ProductID, 50, 1000);
            }, productContext, out PageCount, PageNo, PageSize, orderBy);
        }

        public List<CacheProduct> FindSimilarProductsByColor(TProductDataCache productContext, out int PageCount, int ProductID, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var cacheKey = string.Format("FindSimilarProductsByColor:{0}", ProductID);
            return ProductImageSearch(cacheKey, () =>
            {
                return ImageSearch.FindSimilarProductsByColor(ProductID, 50, 1000);
            }, productContext, out PageCount, PageNo, PageSize, orderBy);
        }

        public List<CacheProduct> FindSimilarProductsByTexture(TProductDataCache productContext, out int PageCount, int ProductID, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var cacheKey = string.Format("FindSimilarProductsByTexture:{0}", ProductID);
            return ProductImageSearch(cacheKey, () =>
            {
                return ImageSearch.FindSimilarProductsByTexture(ProductID, 50, 1000);
            }, productContext, out PageCount, PageNo, PageSize, orderBy);
        }


        public List<CacheProduct> FindByTopDominantColor(TProductDataCache productContext, out int PageCount, System.Windows.Media.Color color, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var cacheKey = string.Format("FindByTopDominantColor:{0}", color.ToRGBColorsString());
            return ProductImageSearch(cacheKey, () =>
            {
                return ImageSearch.FindProductsHavingTopDominantColor(color, 15, 1000);
            }, productContext, out PageCount, PageNo, PageSize, orderBy);
        }

        public List<CacheProduct> FindByAnyDominantColor(TProductDataCache productContext, out int PageCount, System.Windows.Media.Color color, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var cacheKey = string.Format("FindByAnyDominantColor:{0}", color.ToRGBColorsString());
            return ProductImageSearch(cacheKey, () =>
            {
                return ImageSearch.FindProductsHavingAnyDominantColor(color, 15, 1000);
            }, productContext, out PageCount, PageNo, PageSize, orderBy);
        }

        protected List<CacheProduct> ProductImageSearch(string cacheKey, Func<List<int>> callback, TProductDataCache productContext, out int PageCount, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            // pageNo is 0-rel

            CachedSearchResult cache;

            if (EnableSearchCaching)
            {
                // attempt to get results from cache - else create and cache for next time.

                cache = CachedSearchResult.Lookup(productContext, cacheKey);
                if (cache == null)
                {
                    var orderedList = callback();
                    cache = new CachedSearchResult(productContext, cacheKey, orderedList);
                    cache.Insert();
                }
            }
            else
            {
                var orderedList = callback();
                cache = new CachedSearchResult(productContext, cacheKey, orderedList);
            }

            List<int> srchResults = cache.Products; // GetOrderedSearchResults(productContext, orderBy, cache);

            if (srchResults.Count > 0)
                PageCount = (srchResults.Count + (PageSize - 1)) / PageSize;
            else
                PageCount = 0;

            var records = srchResults.Skip(PageNo * PageSize).Take(PageSize).Select(p => productContext.Products[p]).ToList();

            return records;
        }




        public List<CacheProduct> Search(TProductDataCache productContext, out int PageCount, string Query, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default, List<int> recentlyViewed=null)
        {
            // if the query is an image search operator, then already in the order we want, do not 
            // use any special fuzzy ordering logic

            bool isSearchOperator = searchOperators.Any(x => Query.StartsWith(x));

            // pageNo is 0-rel

            CachedSearchResult cache;

            if (EnableSearchCaching && recentlyViewed == null)
            {
                // attempt to get results from cache - else create and cache for next time.

                cache = CachedSearchResult.Lookup(productContext, Query);
                if (cache == null)
                {
                    var unorderedList = SearchProducts(Query).Where(e => productContext.Products.ContainsKey(e)).ToList();
                    // order them appropriately
                    var orderedSearchResults = isSearchOperator ? unorderedList : productContext.MakeOrderedProductListByManufacturerWeight(unorderedList);
                    cache = new CachedSearchResult(productContext, Query, orderedSearchResults);
                    cache.Insert();
                }
                else
                    Performance.TotalApiSearchCacheHits.Bump();
            }
            else
            {
                // even though we're not caching, we use the same structure since makes coding easier
                var unorderedList = SearchProducts(Query, recentlyViewed);
                var orderedSearchResults = isSearchOperator ? unorderedList : productContext.MakeOrderedProductListByManufacturerWeight(unorderedList);
                cache = new CachedSearchResult(productContext, Query, orderedSearchResults);
            }

            // just product IDs
            List<int> srchResults = GetOrderedSearchResults(productContext, orderBy, cache);

            if (srchResults.Count > 0)
                PageCount = (srchResults.Count + (PageSize - 1)) / PageSize;
            else
                PageCount = 0;

            var records = srchResults.Skip(PageNo * PageSize).Take(PageSize).Select(p => productContext.Products[p]).ToList();

            return records;
        }

        public List<CacheProduct> Search(TProductDataCache productContext, out int PageCount, SearchCriteria searchCriteria, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {

            //log the query
            //var msg = string.Format("Advanced search for {0}.\n{1}", storeKey, Dump.ToDump(searchCriteria, "Query"));
            //new WebsiteApplicationLifetimeEvent(msg, this, WebsiteEventCode.Notification).Raise();

            CachedSearchResult cache;

            if (EnableSearchCaching)
            {
                // attempt to get results from cache - else create and cache for next time.

                cache = CachedSearchResult.Lookup(productContext, searchCriteria);
                if (cache == null)
                {
                    var unorderedList = AdvancedSearchProducts(searchCriteria).Where(e => productContext.Products.ContainsKey(e)).ToList();
                    // order them appropriately
                    var orderedSearchResults = productContext.MakeOrderedProductListByManufacturerWeight(unorderedList);
                    cache = new CachedSearchResult(productContext, searchCriteria, orderedSearchResults);
                    cache.Insert();
                }
                else
                    Performance.TotalApiSearchCacheHits.Bump();
            }
            else
            {
                // even though we're not caching, we use the same structure since makes coding easier
                var unorderedList = AdvancedSearchProducts(searchCriteria);
                var orderedSearchResults = productContext.MakeOrderedProductListByManufacturerWeight(unorderedList);
                cache = new CachedSearchResult(productContext, searchCriteria, orderedSearchResults);
            }

            // just product IDs
            List<int> srchResults = GetOrderedSearchResults(productContext, orderBy, cache);

            if (srchResults.Count > 0)
                PageCount = (srchResults.Count + (PageSize - 1)) / PageSize;
            else
                PageCount = 0;

            var records = srchResults.Skip(PageNo * PageSize).Take(PageSize).Select(p => productContext.Products[p]).ToList();

            return records;
        }

        public List<CacheProduct> Search(TProductDataCache productContext, out int PageCount, FacetSearchCriteria searchCriteria, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            CachedSearchResult cache;

            if (EnableSearchCaching)
            {
                // attempt to get results from cache - else create and cache for next time.

                cache = CachedSearchResult.Lookup(productContext, searchCriteria);
                if (cache == null)
                {
                    var unorderedList = FacetSearchProducts(searchCriteria).Where(e => productContext.Products.ContainsKey(e)).ToList();
                    // order them appropriately
                    var orderedSearchResults = productContext.MakeOrderedProductListByManufacturerWeight(unorderedList);
                    cache = new CachedSearchResult(productContext, searchCriteria, orderedSearchResults);
                    cache.Insert();
                }
                else
                    Performance.TotalApiSearchCacheHits.Bump();
            }
            else
            {
                // even though we're not caching, we use the same structure since makes coding easier
                var unorderedList = FacetSearchProducts(searchCriteria);
                var orderedSearchResults = productContext.MakeOrderedProductListByManufacturerWeight(unorderedList);
                cache = new CachedSearchResult(productContext, searchCriteria, orderedSearchResults);
            }

            // just product IDs
            List<int> srchResults = GetOrderedSearchResults(productContext, orderBy, cache);

            if (srchResults.Count > 0)
                PageCount = (srchResults.Count + (PageSize - 1)) / PageSize;
            else
                PageCount = 0;

            var records = srchResults.Skip(PageNo * PageSize).Take(PageSize).Select(p => productContext.Products[p]).ToList();

            return records;
        }



        private List<CacheProduct> ListByLabelValueWithinManufacturer(TProductDataCache productContext, out int PageCount, int manufacturerID, string label, string value, int PageNo, int PageSize, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            // pageNo is 0-rel

            // ordered by ProductID, does not take discontinued or images into account for order
            var listProductID = SearchProductsByLabelValueWithinManufacturer(manufacturerID, label, value);

            // we want to adjust the order in a deterministic way so that the order will be different from get products for collections, so 
            // google won't have a problem with duplicate content.

            listProductID.DeterministicShuffle(830983); // the value is random...no meaning to it, just needs to be constant so the random() gen returns same numeric stream

            if (listProductID.Count > 0)
                PageCount = (listProductID.Count + (PageSize - 1)) / PageSize;
            else
                PageCount = 0;

            var records = listProductID.Skip(PageNo * PageSize).Take(PageSize).Select(p => productContext.Products[p]).ToList();

            return records;

        }


        /// <summary>
        /// Used for temporary access for cross marketing computations.
        /// </summary>
        private class StoreData
        {
            public StoreKeys StoreKey { get; set; }
            public IProductDataCache ProductData { get; set; }
            public int FeaturedProductCount { get; set; }
            public string Domain { get; set; }
            public int Offset { get; set; }
        }

#if false
        public List<CacheProduct> GetCrossMarketingProducts(TProductDataCache productContext, string referenceIdentifier, bool allowResultsFromSelf, int productCount)
        {
            try
            {
                var hashKey = string.Format("{0}:{1}:{2}:{3}:{4}", storeKey.ToString(), referenceIdentifier.ToLower(), productCount, allowResultsFromSelf, DateTime.Now.DayOfYear);
                var cacheKey = string.Format("CrossMarketingResult:{0}", hashKey.GetMD5HashCode());

                // see if we have a cached copy
                List<CacheProduct> selectedProducts = HttpRuntime.Cache[cacheKey] as List<CacheProduct>;

                if (selectedProducts != null)
                    return selectedProducts;

                // create an atomic data collection for this operation - so we have hooks to the entire set of cached
                // data so we don't need to worry about in-memory ASP.NET caches expiring

                var atomicStoreData = (from store in MvcApplication.Current.WebStores.Values
                                       orderby store.StoreKey
                                       select new StoreData
                                       {
                                           StoreKey = store.StoreKey,
                                           ProductData = store.ProductData,
                                           FeaturedProductCount = store.ProductData.FeaturedProducts.Count(),
                                           Domain = store.Domain,
                                           Offset = 0,
                                       }).ToList();

                // leave only the stores which will contribute to the final result set

                if (!allowResultsFromSelf)
                    atomicStoreData.RemoveAll(e => e.StoreKey == storeKey);

                // compute relative offsets for each store into the master (virtual) array of products in the pool -
                // the virtual array is simply the imaginary concatination of the store featured products

                var currentOffset = 0;
                foreach (var store in atomicStoreData)
                {
                    store.Offset = currentOffset;
                    currentOffset += store.FeaturedProductCount;
                }

                // create a random number generator using a deterministic seed - so for a given scenario,
                // the number sequence will be consistent - yet every scenario would have a different
                // seed and thus a different outcome. Notice that each day the list will be different,
                // but the same page on website will yield the same products for that given day.

                var rnd = new Random(GetSeed(hashKey));

                var totalProductCount = atomicStoreData.Sum(e => e.FeaturedProductCount);

                // build up array of indexes into the virtual featured product list for the products
                // chosen to be included in the result set

                var selectedIndexes = new HashSet<int>();

                int maxPickAttempts = Math.Min(200, productCount * 4);
                int pickAttempts = 0;
                while (true)
                {
                    var index = rnd.Next(0, totalProductCount - 1);
                    selectedIndexes.Add(index);

                    if (selectedIndexes.Count() == productCount || ++pickAttempts > maxPickAttempts)
                        break;
                }

                selectedProducts = new List<CacheProduct>();

                foreach (var index in selectedIndexes)
                {
                    try
                    {
                        // figure out which store it comes from
                        foreach (var store in atomicStoreData)
                        {
                            if (index >= store.Offset + store.FeaturedProductCount)
                                continue;

                            var productID = store.ProductData.FeaturedProducts[index - store.Offset];

                            if (!store.ProductData.Products.ContainsKey(productID))
                                break;

                            var product = store.ProductData.Products[productID];

                            // if the product is on the same store as the one making the request, then
                            // no need for absolute URL on links and images
                            if (this.StoreKey == store.StoreKey)
                                selectedProducts.Add(product);
                            else
                                selectedProducts.Add(product.CopyWithAbsoluteUrls(store.Domain));
                        }
                    }
                    catch (Exception Ex)
                    {
                        // any lookup problems are ignored - just return what we can 
                        Debug.WriteLine(string.Format("Exception choosing cross marketing products. \n{0}", Ex.ToString()));
                    }
                }

                // save to cache for next time

                HttpRuntime.Cache.Insert(cacheKey, selectedProducts, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(60 * 10), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);

                return selectedProducts;
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                return new List<CacheProduct>();
            }
        }
#else
        public List<CacheProduct> GetCrossMarketingProducts(TProductDataCache productContext, string referenceIdentifier, bool allowResultsFromSelf, int productCount)
        {
            // this version ONLY uses InsideAvenue as the source of features products
            try
            {
                var hashKey = string.Format("{0}:{1}:{2}:{3}:{4}", storeKey.ToString(), referenceIdentifier.ToLower(), productCount, allowResultsFromSelf, DateTime.Now.DayOfYear);
                var cacheKey = string.Format("CrossMarketingResult:{0}", hashKey.GetMD5HashCode());

                // see if we have a cached copy
                List<CacheProduct> selectedProducts = HttpRuntime.Cache[cacheKey] as List<CacheProduct>;

                if (selectedProducts != null)
                    return selectedProducts;

                // create an atomic data collection for this operation - so we have hooks to the entire set of cached
                // data so we don't need to worry about in-memory ASP.NET caches expiring

                var atomicStoreData = (from store in MvcApplication.Current.WebStores.Values
                                       where store.StoreKey == StoreKeys.InsideAvenue
                                       select new StoreData
                                       {
                                           StoreKey = store.StoreKey,
                                           ProductData = store.ProductData,
                                           FeaturedProductCount = store.ProductData.FeaturedProducts.Count(),
                                           Domain = store.Domain,
                                           Offset = 0,
                                       }).Single();


                var rnd = new Random(GetSeed(hashKey));

                var totalProductCount = atomicStoreData.FeaturedProductCount;

                // build up array of indexes into the virtual featured product list for the products
                // chosen to be included in the result set

                var selectedIndexes = new HashSet<int>();

                int maxPickAttempts = Math.Min(200, productCount * 4);
                int pickAttempts = 0;
                while (true)
                {
                    var index = rnd.Next(0, totalProductCount - 1);
                    selectedIndexes.Add(index);

                    if (selectedIndexes.Count() == productCount || ++pickAttempts > maxPickAttempts)
                        break;
                }

                selectedProducts = new List<CacheProduct>();

                foreach (var index in selectedIndexes)
                {
                    try
                    {
                        var productID = atomicStoreData.ProductData.FeaturedProducts[index];

                        if (!atomicStoreData.ProductData.Products.ContainsKey(productID))
                            continue;

                        var product = atomicStoreData.ProductData.Products[productID];

                        if (this.StoreKey == StoreKeys.InsideAvenue)
                            selectedProducts.Add(product);
                        else
                            selectedProducts.Add(product.CopyWithAbsoluteUrls(atomicStoreData.Domain));

                    }
                    catch (Exception Ex)
                    {
                        // any lookup problems are ignored - just return what we can 
                        Debug.WriteLine(string.Format("Exception choosing cross marketing products. \n{0}", Ex.ToString()));
                    }
                }

                // save to cache for next time

                HttpRuntime.Cache.Insert(cacheKey, selectedProducts, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(60 * 10), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);

                return selectedProducts;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                return new List<CacheProduct>();
            }
        }
#endif

    }
}