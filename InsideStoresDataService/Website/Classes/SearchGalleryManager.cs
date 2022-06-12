using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InsideFabric.Data;
using Website.Entities;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Website
{
    public enum SearchGalleryProcessingResult
    {
        Success, // has been inserted
        NotEnoughResults, // not inserted because not enough results found
        Failed
    }

    public class SearchGalleryManager
    {
        private const int MINIMUM_SEARCH_RESULTS = 6;

        protected IWebStore store;

        public SearchGalleryManager(IWebStore store)
        {
            this.store = store;
        }

        /// <summary>
        /// Add single query to the queue of permutations to spin up full records.
        /// </summary>
        /// <remarks>
        /// This populates the working queue in advance of spinning final records.
        /// </remarks>
        /// <param name="criteria"></param>
        protected void AddQueryPermutationToQueue(FacetSearchCriteria criteria)
        {
            using (var dc = new AspStoreDataContext(store.ConnectionString))
            {
                var q = new SearchGalleryQuery()
                {
                    Query = criteria.ToJSON(SerializerSettings)
                };

                dc.SearchGalleryQueries.InsertOnSubmit(q);
                dc.SubmitChanges();
            }
        }

        protected void AddQueryPermutationToQueue(IEnumerable<FacetSearchCriteria> items)
        {
            using (var dc = new AspStoreDataContext(store.ConnectionString))
            {
                var records = new List<SearchGalleryQuery>();
                foreach (var item in items)
                {
                    var q = new SearchGalleryQuery()
                    {
                        Query = item.ToJSON(SerializerSettings)
                    };

                    records.Add(q);
                    dc.SearchGalleryQueries.InsertOnSubmit(q);
                }
                dc.SubmitChanges();
            }
        }

        protected virtual List<int> GetCategoryList(int parentID)
        {
            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                var list = dc.Categories.Where(e => e.ParentCategoryID == parentID && e.Published == 1 && e.Deleted==0).Select(e => e.CategoryID).ToList();
                return list;
            }
        }

        protected virtual List<int> GetManufacturerList()
        {
            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                var list = dc.Manufacturers.Where(e => e.Published == 1 && e.Deleted==0).Select(e => e.ManufacturerID).ToList();
                return list;
            }
        }

        /// <summary>
        /// Entry point to populate queue of permutations.
        /// </summary>
        /// <remarks>
        /// Expect that each store will have its own logic.
        /// </remarks>
        virtual public void SpinQueryPermutations()
        {

        }


        protected virtual void BeginSpinProcessing()
        {

        }

        protected virtual void EndSpinProcessing()
        {

        }

        /// <summary>
        /// Receive a single criteria (query) and turn into a populated SQL record in
        /// SQL SearchGalleries table.
        /// </summary>
        /// <remarks>
        /// All information needed should be able to be figured out through the provided criteria.
        /// Be careful to throttle calls so as not to overload search engine
        /// </remarks>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public SearchGalleryProcessingResult ProcessSingleQuery(FacetSearchCriteria criteria)
        {

            try
            {

                Func<int?> getManufacturerID = () =>
                    {
                        var facet = criteria.Facets.Where(e => e.FacetKey == "Manufacturer" && e.Members.Count() > 0).FirstOrDefault();
                        return (facet == null) ? (int?)null : facet.Members.First();
                    };

                    Func<int?> getColorCategoryID = () =>
                    {
                        var facet = criteria.Facets.Where(e => e.FacetKey == "Color" && e.Members.Count() > 0).FirstOrDefault();
                        return (facet == null) ? (int?)null : facet.Members.First();
                    };


                // perform a search and make sure we have enough to care about, else bail

                var searchResults = store.FacetSearchProductsResultSet(criteria);
                if (searchResults.Count < MINIMUM_SEARCH_RESULTS)
                    return SearchGalleryProcessingResult.NotEnoughResults;

                Func<string> getProducts = () =>
                    {
                        if (searchResults.Count == 0)
                            return null;

                        // field is 512, so must stay way under for comma list
                        var strList = searchResults.Take(25).ToCommaDelimitedList(noSpaces: true);

                        if (strList == string.Empty)
                            strList = null;

                        return strList;
                    };

                var now = DateTime.Now;
                var record = new SearchGallery()
                {
                    CreatedOn = now,
                    Manual = 0,
                    Published = 1,
                    LastReviewed = now,
                    UpdatedOn = now,
                    Query = criteria.ToJSON(SerializerSettings),
                    // specific spinning
                    SEName = SpinSEName(criteria),
                    SETitle = SpinSETitle(criteria),
                    SEDescription = SpinSEDescription(criteria),
                    SEKeywords = SpinSEKeywords(criteria),
                    Name = SpinName(criteria),
                    Description = SpinDescription(criteria),
                    AnchorTextList = SpinAnchorTextList(criteria),
                    Tags = SpinTags(criteria),
                    HitCount = searchResults.Count,
                    ManufacturerID = getManufacturerID(), // if there is a manufacturer
                    ColorCategoryID = getColorCategoryID(), // if there is a color
                    Products = getProducts()
                };

                // insert into SQL

                using (var dc = new AspStoreDataContext(store.ConnectionString))
                {
                    dc.SearchGalleries.InsertOnSubmit(record);
                    dc.SubmitChanges();
                }

                return SearchGalleryProcessingResult.Success;

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);

                return SearchGalleryProcessingResult.Failed;
            }
        }


        // SEName
        protected virtual string SpinSEName(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }

        // SETitle
        protected virtual string SpinSETitle(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }

        // SEDescription
        protected virtual string SpinSEDescription(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }

        // SEKeywords
        protected virtual string SpinSEKeywords(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }

        // Name
        protected virtual string SpinName(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }

        // Description
        protected virtual string SpinDescription(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }


        // AnchorTextList
        protected virtual string SpinAnchorTextList(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }

        // Tags
        protected virtual string SpinTags(FacetSearchCriteria criteria)
        {
            return string.Empty;
        }


        protected void doSpinQueries()
        {
            var shouldSpinQueries = false;

            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                shouldSpinQueries = dc.SearchGalleryQueries.Count() == 0;
            }

            if (shouldSpinQueries)
            {
                Task.Factory.StartNew(() =>
                {
#if !DEBUG
                    var rand = new Random();
                    System.Threading.Thread.Sleep(1000 * 60 * rand.Next(5,10));
#endif
                    Debug.WriteLine("Spinning queries - begin");
                    SpinQueryPermutations();
                    Debug.WriteLine("Spinning queries - done");
                });
            }
        }

        protected void doSpinFullRecordsDelta()
        {

            Task.Factory.StartNew(() =>
            {
                // wait until product caches populated
                while (!store.IsPopulated)
                    System.Threading.Thread.Sleep(5000);

                Debug.WriteLine("********Spinning full delta records - begin");

                try
                {

                    BeginSpinProcessing();

                    List<string> queryStrings;
                    HashSet<string> existingRecords = new HashSet<string>();

                    using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                    {
                        queryStrings = dc.SearchGalleryQueries.Select(e => e.Query).ToList();

                        foreach (var rec in dc.SearchGalleries.Select(e => e.Query))
                            existingRecords.Add(rec);
                    }

                    var missingQueries = new List<string>();
                    foreach (var q in queryStrings)
                        if (!existingRecords.Contains(q))
                            missingQueries.Add(q);

                    Debug.WriteLine(string.Format("******* {0:N0} delta records", missingQueries.Count));

                    Parallel.ForEach(missingQueries, (item) =>
                    {
                        var criteria = item.FromJSON<FacetSearchCriteria>();
                        ProcessSingleQuery(criteria);
                    });

                    EndSpinProcessing();
                    Debug.WriteLine("*******Spinning full records - done");
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.ToString());
                }

            });
        }



        protected void doSpinFullRecords()
        {
            var shouldSpin = false;

            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                shouldSpin = dc.SearchGalleries.Count() < 10;
            }

            if (shouldSpin)
            {
                Task.Factory.StartNew(() =>
                {
                    // wait until product caches populated
                    while (!store.IsPopulated)
                        System.Threading.Thread.Sleep(5000);

                    Debug.WriteLine("********Spinning full records - begin");

                    try
                    {

                        BeginSpinProcessing();

                        List<string> list;
                        using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                        {
                            list = dc.SearchGalleryQueries.Select(e => e.Query).ToList();
                        }

                        Parallel.ForEach(list, (item) =>
                        {
                            var criteria = item.FromJSON<FacetSearchCriteria>();
                            ProcessSingleQuery(criteria);
                        });

                        EndSpinProcessing();
                        Debug.WriteLine("*******Spinning full records - done");
                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine(Ex.ToString());
                    }

                });
            }
        }

        public void PopulateMissingSearchGalleries(CancellationToken cancelToken, Action<int> reportProgress = null)
        {
            int totalCount;

            List<string> queryStrings;
            HashSet<string> existingRecords = new HashSet<string>();

            if (reportProgress != null)
                reportProgress(0);

            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                queryStrings = dc.SearchGalleryQueries.Select(e => e.Query).ToList();

                foreach (var rec in dc.SearchGalleries.Select(e => e.Query))
                    existingRecords.Add(rec);
            }

            var missingQueries = new List<string>();
            foreach (var q in queryStrings)
                if (!existingRecords.Contains(q))
                    missingQueries.Add(q);

            totalCount = missingQueries.Count;

            int countCompleted = 0;
            int lastReportedProgressPct = -1;

            Action<int> notifyProgress = (completed) =>
            {
                var pct = (completed * 100) / totalCount;

                if (pct == lastReportedProgressPct)
                    return;

                lastReportedProgressPct = pct;
                if (reportProgress != null)
                    reportProgress(lastReportedProgressPct);
            };

            notifyProgress(0);

            BeginSpinProcessing();

            foreach (var query in missingQueries)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                var criteria = query.FromJSON<FacetSearchCriteria>();
                ProcessSingleQuery(criteria);

                countCompleted++;
                notifyProgress(countCompleted);
            }

            EndSpinProcessing();
        }


        public void ReviewSearchGalleries(CancellationToken cancelToken, Action<int> reportProgress = null)
        {
            int totalCount;

            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                totalCount = dc.SearchGalleries.Count();
            }

            int skip = 0;
            int remaining = totalCount;
            int lastReportedProgressPct = -1;


            Action<int> notifyProgress = (completed) =>
            {
                var pct = (completed * 100) / totalCount;

                if (pct == lastReportedProgressPct)
                    return;

                lastReportedProgressPct = pct;
                if (reportProgress != null)
                    reportProgress(lastReportedProgressPct);
            };

            notifyProgress(0);

            // read in 50 records at a time, do the queries, update SQL with new counts

            while (remaining > 0 && !cancelToken.IsCancellationRequested)
            {
                int take = Math.Min(remaining, 50);

                using (var dc = new AspStoreDataContext(store.ConnectionString))
                {
                    var records = dc.SearchGalleries.OrderBy(e => e.SearchGalleryID).Skip(skip).Take(take).ToList();

                    // don't care about if published, because need to check and could toggle either direction
                    // based on current hit count

                    foreach(var record in records)
                    {
                        var criteria = record.Query.FromJSON<FacetSearchCriteria>();

                        // perform a search and make sure we have enough to care about, else bail

                        var searchResults = store.FacetSearchProductsResultSet(criteria);

                        Func<string> getProducts = () =>
                        {
                            if (searchResults.Count == 0)
                                return null;

                            // field is 512, so must stay way under for comma list
                            var strList = searchResults.Take(25).ToCommaDelimitedList(noSpaces: true);

                            if (strList == string.Empty)
                                strList = null;

                            return strList;
                        };

                        var now = DateTime.Now;
                        record.UpdatedOn = now;
                        record.LastReviewed = now;
                        record.HitCount = searchResults.Count;
                        record.Products = getProducts();
                        record.Published = Convert.ToInt32(searchResults.Count >= MINIMUM_SEARCH_RESULTS);
                    }

                    dc.SubmitChanges();

                    remaining -= take;
                    skip += take;
                    notifyProgress(skip);
                }

            }
        }


        public void UnpublishSearchGalleryManufacaturer(int manufacturerID, CancellationToken cancelToken, Action<int> reportProgress = null)
        {

            using (var dc = new AspStoreDataContext(store.ConnectionString))
            {
                var totalCount = dc.SearchGalleries.Count();

                if (totalCount == 0)
                    return;

                int skip = 0;
                int remaining = totalCount;
                int lastReportedProgressPct = -1;

                Action<int> notifyProgress = (completed) =>
                {
                   var pct = (completed * 100) / totalCount;

                   if (pct == lastReportedProgressPct)
                       return;

                   lastReportedProgressPct = pct;
                   if (reportProgress != null)
                        reportProgress(lastReportedProgressPct);
                };

                notifyProgress(0);

                while (remaining > 0 && !cancelToken.IsCancellationRequested)
                {
                    int take = Math.Min(remaining, 2000);
                    var records = dc.SearchGalleries.OrderBy(e => e.SearchGalleryID).Skip(skip).Take(take).Select(e => new { e.SearchGalleryID, e.Query, e.Published }).ToList();

                    var matchedRecords = new List<int>();

                    // spin through all and keep a list of which ones reference the target manufacturer

                    foreach(var record in records)
                    {
                        if (record.Published == 0)
                            continue;

                        var criteria = record.Query.FromJSON<FacetSearchCriteria>();

                        // not all records have manufacture associations

                        var manufactureres = criteria.Facets.Where(e => e.FacetKey == "Manufacturer").FirstOrDefault();
                        if (manufactureres == null)
                            continue;

                        if (manufactureres.Members.Contains(manufacturerID))
                            matchedRecords.Add(record.SearchGalleryID);
                    }

                    // set unpublished in SQL, 100 at a time
                    // don't want to delete as we're still reading list

                    while (matchedRecords.Count() > 0 && !cancelToken.IsCancellationRequested)
                    {
                        var slice = new HashSet<int>(matchedRecords.Take(100));
                        dc.SearchGalleries.MarkUnpublished(slice);
                        matchedRecords.RemoveAll(e => slice.Contains(e));
                    }

                    remaining -= take;
                    skip += take;
                    notifyProgress(skip);
                }
            }

            throw new NotImplementedException();
        }

        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, Formatting = Formatting.None };

    }
}