using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using System.Diagnostics;
using System.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Threading;
using System.IO;
using System.Threading.Tasks;


namespace Website.Services
{

    // TODO: Create methods containing your application logic.
    [EnableClientAccess()]
    public class ControlPanelDomainService : DomainServiceBase
    {
        #region Locals

        #endregion

        #region Properties

        #endregion

        #region ctors

        static ControlPanelDomainService()
        {

        }

        #endregion

        #region About
        [Invoke]
        public string About()
        {
            try
            {
                return "Stores Data Service 1.0";
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }
        #endregion

        #region Authentication
        [Invoke]
        public bool IsAuthenticated(string Password)
        {
            try
            {
                var configPassword = ConfigurationManager.AppSettings["ControlPanelPassword"] ?? string.Empty;

                var result = configPassword == Password;

                if (result == true)
                {
                    // if a good login, initiate a fetch of these counts
                    // because seems to take a while 
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            GetManufacturerCounts(null);
                        }
                        catch { }
                    });
                }

                return result;
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        #endregion

        #region Manufacturers
        [Invoke]
        public List<ManufacturerMetric> GetManufacturerCounts(StoreKeys? storeKey)
        {
            try
            {
                var data = new List<ManufacturerMetric>();

                if (storeKey.HasValue)
                {
                    var store = MvcApplication.Current.GetWebStore(storeKey.Value);
                    data.AddRange(ManufacturerMetric.GetCounts(store));
                }
                else
                {
                    foreach (var store in MvcApplication.Current.WebStores.Values)
                        data.AddRange(ManufacturerMetric.GetCounts(store));
                }

                return data;

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        [Invoke]
        public List<ManufacturerIdentity> GetManufacturerNames(StoreKeys storeKey)
        {
            try
            {
                var data = new List<ManufacturerIdentity>();

                var store = MvcApplication.Current.GetWebStore(storeKey);

                using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                {
                    data = (from m in dc.Manufacturers 
                            where m.Deleted == 0 && m.Published==1 
                            orderby m.Name
                           select new ManufacturerIdentity
                           {
                               ManufacturerID = m.ManufacturerID,
                               ManufacturerName = m.Name,
                               StoreKey = storeKey
                           }).ToList();
                }

                return data;

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        #endregion

        #region Store Information
        [Invoke]
        public WebStoreInformation GetWebStoreInformation(StoreKeys storeKey)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                var info = new WebStoreInformation(store);
                return info;
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }
        #endregion

        #region Requests per Second
        [Invoke]
        public double CombinedRequestsPerSecond()
        {
            try
            {
#if false
                var rand = new Random();
                return rand.Next(0, 20);
#else
                double countRequests = 0;

                foreach (var store in MvcApplication.Current.WebStores.Select(e => e.Value))
                {
                    var timeline = store.Performance.TotalApiRequests.SecondsTimeline;
                    countRequests += timeline.Reverse<int>().Skip(1).Take(5).Sum(e => e);
                }

                return countRequests / 5;
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }



        [Invoke]
        public double CombinedRequestsPerMinute()
        {
            try
            {
#if false
                var rand = new Random();
                return rand.Next(0, 20);
#else
                double countRequests = 0;

                foreach (var store in MvcApplication.Current.WebStores.Select(e => e.Value))
                {
                    var timeline = store.Performance.TotalApiRequests.MinutesTimeline;
                    countRequests += timeline.Reverse<int>().Skip(1).Take(5).Sum(e => e);
                }

                return countRequests / 5;
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }



        [Invoke]
        public double RequestsPerMinute(StoreKeys storeKey)
        {
            try
            {
#if false
                var rand = new Random();
                return rand.Next(0, 20);
#else
                var store = MvcApplication.Current.GetWebStore(storeKey);

                var timeline = store.Performance.TotalApiRequests.MinutesTimeline;

                if (timeline == null)
                    return 0;

                var count = timeline.Reverse<int>().Skip(1).Take(5).Average(e => e);

                return count;
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        [Invoke]
        public double RequestsPerSecond(StoreKeys storeKey)
        {
            try
            {
#if false
                var rand = new Random();
                return rand.Next(0, 20);
#else
                var store = MvcApplication.Current.GetWebStore(storeKey);

                var timeline = store.Performance.TotalApiRequests.SecondsTimeline;

                if (timeline == null)
                    return 0;

                var count = timeline.Reverse<int>().Skip(1).Take(5).Average(e => e);

                return count;
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        #endregion

        #region Repopulate, Rebuild, etc.
        [Invoke]
        public bool RepopulateDataCache(StoreKeys storeKey)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.RepopulateProducts(10 * 1000);
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        [Invoke]
        public bool RebuildCategoriesTable(StoreKeys storeKey)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.RebuildProductCategoryTable(30 * 1000);
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        [Invoke]
        public bool RebuildSearchExtensionData(StoreKeys storeKey)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.RebuildProductSearchExtensionData(30 * 1000);
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }
        #endregion

        #region Total Requests Chart Data
        [Invoke]
        public List<int> TotalRequestsChartData(StoreKeys storeKey, TimelineSeries series)
        {
            try
            {
#if false
                var dataPoints = new List<int>();
                int max;
                int min;
                switch (series)
                {
                    case TimelineSeries.Seconds:
                        max = 20;
                        min = 0;
                        break;

                    case TimelineSeries.Minutes:
                        max = 200;
                        min = 10;
                        break;

                    case TimelineSeries.Hours:
                        max = 10000;
                        min = 100;
                        break;

                    case TimelineSeries.Days:
                        min = 20000;
                        max = 1500000;
                        break;


                    default:
                        min = 0;
                        max = 100;
                        break;
                }

                var rand = new Random();
                for (int i = 0; i < 30; i++)
                    dataPoints.Add(rand.Next(min, max));

                return dataPoints;
#else
                List<int> arySeries;

                var store = MvcApplication.Current.GetWebStore(storeKey);

                var performance = store.Performance;

                switch (series)
                {
                    case TimelineSeries.Seconds:
                        arySeries = performance.TotalApiRequests.SecondsTimeline;
                        break;

                    case TimelineSeries.Minutes:
                        arySeries = performance.TotalApiRequests.MinutesTimeline;
                        break;

                    case TimelineSeries.Hours:
                        arySeries = performance.TotalApiRequests.HoursTimeline;
                        break;

                    case TimelineSeries.Days:
                        arySeries = performance.TotalApiRequests.DaysTimeline;
                        break;

                    default:
                        throw new Exception("Invalid series parameter.");
                }

                return arySeries;
#endif
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        [Invoke]
        public List<CombinedRequestsMetric> CombinedTotalRequestsChartData(TimelineSeries series)
        {
            try
            {

                var arySeries = new List<CombinedRequestsMetric>();
                var dic = new Dictionary<StoreKeys, List<int>>();

                foreach (var store in MvcApplication.Current.WebStores.Select(e => e.Value))
                {
                    var performance = store.Performance;

                    switch (series)
                    {
                        case TimelineSeries.Seconds:
                            dic.Add(store.StoreKey, performance.TotalApiRequests.SecondsTimeline);
                            break;

                        case TimelineSeries.Minutes:
                            dic.Add(store.StoreKey, performance.TotalApiRequests.MinutesTimeline);
                            break;

                        case TimelineSeries.Hours:
                            dic.Add(store.StoreKey, performance.TotalApiRequests.HoursTimeline);
                            break;

                        case TimelineSeries.Days:
                            dic.Add(store.StoreKey, performance.TotalApiRequests.DaysTimeline);
                            break;

                        default:
                            throw new Exception("Invalid series parameter.");
                    }

                }

                var indexCount = dic.First().Value.Count();

                for (int i = 0; i < indexCount; i++)
                {
                    var m = new CombinedRequestsMetric()
                    {
                        InsideFabric = dic[StoreKeys.InsideFabric][i],
                        InsideRugs = dic[StoreKeys.InsideRugs][i],
                        InsideWallpaper = dic[StoreKeys.InsideWallpaper][i],
                        InsideAvenue = dic[StoreKeys.InsideAvenue][i]
                    };

                    arySeries.Add(m);
                }

                return arySeries;
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        #endregion

        #region Total Searches Chart Data
        [Invoke]
        public List<SearchMetric> TotalSearchesChartData(StoreKeys storeKey, TimelineSeries series)
        {
            try
            {
#if false
                var dataPoints = new List<SearchMetric>();
                int max;
                int min;
                switch (series)
                {
                    case TimelineSeries.Seconds:
                        max = 20;
                        min = 0;
                        break;

                    case TimelineSeries.Minutes:
                        max = 200;
                        min = 10;
                        break;

                    case TimelineSeries.Hours:
                        max = 200;
                        min = 10;
                        break;

                    case TimelineSeries.Days:
                        min = 20000;
                        max = 1500000;
                        break;


                    default:
                        min = 0;
                        max = 100;
                        break;
                }

                var rand = new Random();
                for (int i = 0; i < 61; i++)
                    dataPoints.Add(new SearchMetric(rand.Next(min, max), rand.Next(min, max)));

                return dataPoints;
#else

                List<int> arySearchesSeries;
                List<int> aryAdvSearchesSeries;

                var store = MvcApplication.Current.GetWebStore(storeKey);
                var performance = store.Performance;

                switch (series)
                {
                    case TimelineSeries.Seconds:
                        arySearchesSeries = performance.TotalApiSearchRequests.SecondsTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.SecondsTimeline;
                        break;

                    case TimelineSeries.Minutes:
                        arySearchesSeries = performance.TotalApiSearchRequests.MinutesTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.MinutesTimeline;
                        break;

                    case TimelineSeries.Hours:
                        arySearchesSeries = performance.TotalApiSearchRequests.HoursTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.HoursTimeline;
                        break;

                    case TimelineSeries.Days:
                        arySearchesSeries = performance.TotalApiSearchRequests.DaysTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.DaysTimeline;
                        break;

                    default:
                        throw new Exception("Invalid series parameter.");
                }

                // combine the series and return

                // based on the assumption that the raw series array indexes refer to the same
                // time element (both series started at the same time)

                // expect these to all start within milliseconds of each other, so for now not going
                // to worry about any inconsequential timeline differences

                List<SearchMetric> arySeries = new List<SearchMetric>();

                var maxCommonCount = Math.Min(arySearchesSeries.Count(), aryAdvSearchesSeries.Count());

                for (int i = 0; i < maxCommonCount; i++)
                    arySeries.Add(new SearchMetric(arySearchesSeries[i], aryAdvSearchesSeries[i]));

                return arySeries;
#endif
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }
        #endregion

        #region Page Views Chart Data

        [Invoke]
        public double CombinedPageViewsPerSecond()
        {
            try
            {
#if false
                var rand = new Random();
                return rand.Next(0, 20);
#else
                double countRequests = 0;

                foreach (var store in MvcApplication.Current.WebStores.Select(e => e.Value))
                {
                    var timeline = store.Performance.PageViews.SecondsTimeline;
                    countRequests += timeline.Reverse<PageViewStats>().Skip(1).Take(5).Sum(e => e.Total);
                }

                return countRequests / 5;
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }



        [Invoke]
        public RequestsMetric PageViewsPerSecond(StoreKeys storeKey)
        {
            try
            {
#if false
                var rand = new Random();
                return new RequestsMetric(rand.Next(0, 20));
#else
                var store = MvcApplication.Current.GetWebStore(storeKey);

                var timeline = store.Performance.PageViews.SecondsTimeline;

                if (timeline == null)
                    return new RequestsMetric(0);

                var count = timeline.Reverse<PageViewStats>().Skip(1).Take(5).Average(e => e.Total);

                var responseStats = store.Performance.PageViews.MostRecentCompletedMinute;

                // if no requests in last completed minute, revert to seconds so we can show something
                // at the earliest possible time - find the most recent second with somethin not 0

                if (responseStats.ResponseTimeAvg == 0 && timeline.Count() >= 2)
                {
                    for (var index = timeline.Count() - 2; index >= 0; index--)
                    {
                        responseStats = timeline[index];
                        if (responseStats.ResponseTimeAvg > 0)
                            break;
                    }
                }
                return new RequestsMetric(count)
                {
                    ResponseTimeAvg = responseStats.ResponseTimeAvg,
                    ResponseTimeHigh = responseStats.ResponseTimeHigh,
                    ResponseTimeLow = responseStats.ResponseTimeLow,
                    ResponseTimeMedian = responseStats.ResponseTimeMedian
                };
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }



        [Invoke]
        public List<PageViewStats> PageViewsChartData(StoreKeys storeKey, TimelineSeries series)
        {
            try
            {
#if false // TEST ONLY
                var dataPoints = new List<PageViewStats>();
                int max;
                int min;
                switch (series)
                {
                    case TimelineSeries.Seconds:
                        max = 20;
                        min = 0;
                        break;

                    case TimelineSeries.Minutes:
                        max = 200;
                        min = 10;
                        break;

                    case TimelineSeries.Hours:
                        max = 10000;
                        min = 100;
                        break;

                    case TimelineSeries.Days:
                        min = 20000;
                        max = 1500000;
                        break;


                    default:
                        min = 0;
                        max = 100;
                        break;
                }

                var rand = new Random();
                for (int i = 0; i < 30; i++)
                {
                    var views = rand.Next(min, max);

                    int home = (int)Math.Round(views * .25M); views -= home;
                    int manufacturer = (int)Math.Round(views * .30M); views -= manufacturer;
                    int category = (int)Math.Round(views * .30M); views -= category;
                    int product = (int)Math.Round(views * .40M); views -= product; 
                    int other =(int)Math.Round(views * .40M); views -= other; 
                    int bot = views;

                    var stats = new PageViewStats()
                    {
                        Home = home,
                        Manufacturer = manufacturer,
                        Category = category,
                        Product = product,
                        Other = other,
                        Bot = bot,
                    };

                    dataPoints.Add(stats);
                }

                return dataPoints;
#else
                List<PageViewStats> arySeries;

                var store = MvcApplication.Current.GetWebStore(storeKey);

                var performance = store.Performance;

                switch (series)
                {
                    case TimelineSeries.Seconds:
                        arySeries = performance.PageViews.SecondsTimeline;
                        break;

                    case TimelineSeries.Minutes:
                        arySeries = performance.PageViews.MinutesTimeline;
                        break;

                    case TimelineSeries.Hours:
                        arySeries = performance.PageViews.HoursTimeline;
                        break;

                    case TimelineSeries.Days:
                        arySeries = performance.PageViews.DaysTimeline;
                        break;

                    default:
                        throw new Exception("Invalid series parameter.");
                }

                return arySeries;
#endif
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        [Invoke]
        public List<CombinedRequestsMetric> CombinedPageViewsChartData(TimelineSeries series)
        {
            try
            {

                var arySeries = new List<CombinedRequestsMetric>();
                var dic = new Dictionary<StoreKeys, List<PageViewStats>>();

                foreach (var store in MvcApplication.Current.WebStores.Select(e => e.Value))
                {
                    var performance = store.Performance;

                    switch (series)
                    {
                        case TimelineSeries.Seconds:
                            dic.Add(store.StoreKey, performance.PageViews.SecondsTimeline);
                            break;

                        case TimelineSeries.Minutes:
                            dic.Add(store.StoreKey, performance.PageViews.MinutesTimeline);
                            break;

                        case TimelineSeries.Hours:
                            dic.Add(store.StoreKey, performance.PageViews.HoursTimeline);
                            break;

                        case TimelineSeries.Days:
                            dic.Add(store.StoreKey, performance.PageViews.DaysTimeline);
                            break;

                        default:
                            throw new Exception("Invalid series parameter.");
                    }

                }

                var indexCount = dic.First().Value.Count();

                for (int i = 0; i < indexCount; i++)
                {
                    var m = new CombinedRequestsMetric()
                    {
                        InsideFabric = dic[StoreKeys.InsideFabric][i].Total,
                        InsideRugs = dic[StoreKeys.InsideRugs][i].Total,
                        InsideWallpaper = dic[StoreKeys.InsideWallpaper][i].Total,
                        InsideAvenue = dic[StoreKeys.InsideAvenue][i].Total
                    };

                    arySeries.Add(m);
                }

                return arySeries;
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        #endregion

        #region SqlSearchMetrics

        [Invoke]
        public List<SqlSearchMetric> GetSqlSearchMetrics(StoreKeys storeKey, int startingAfterID, int takeCount)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.Performance.GetSqlSearchMetrics(startingAfterID, takeCount);
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        public List<SqlSearchMetric> GetLastSqlSearchMetrics(StoreKeys storeKey, int maxCount)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.Performance.GetLastSqlSearchMetrics(maxCount);
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        public List<SqlSearchMetric> GetAllSqlSearchMetrics(StoreKeys storeKey)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.Performance.GetAllSqlSearchMetrics();
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        /// <summary>
        /// Returns the ration of cache hits on sql searches. Simple and advanced combined.
        /// </summary>
        /// <remarks>
        /// Value returned is in range 0 to 100 percent.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        public double SqlSearchCacheHitRatio(StoreKeys storeKey, TimelineSeries series)
        {
            try
            {
#if false
                var rand = new Random();
                return rand.Next(20, 60);
#else
                double cacheHitRatio = 0;

                var store = MvcApplication.Current.GetWebStore(storeKey);

                List<int> arySearchesSeries;
                List<int> aryAdvSearchesSeries;
                List<int> aryCacheHits;

                var performance = store.Performance;

                switch (series)
                {
                    case TimelineSeries.Seconds:
                        arySearchesSeries = performance.TotalApiSearchRequests.SecondsTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.SecondsTimeline;
                        aryCacheHits = performance.TotalApiSearchCacheHits.SecondsTimeline;
                        break;

                    case TimelineSeries.Minutes:
                        arySearchesSeries = performance.TotalApiSearchRequests.MinutesTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.MinutesTimeline;
                        aryCacheHits = performance.TotalApiSearchCacheHits.MinutesTimeline;
                        break;

                    case TimelineSeries.Hours:
                        arySearchesSeries = performance.TotalApiSearchRequests.HoursTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.HoursTimeline;
                        aryCacheHits = performance.TotalApiSearchCacheHits.HoursTimeline;
                        break;

                    case TimelineSeries.Days:
                        arySearchesSeries = performance.TotalApiSearchRequests.DaysTimeline;
                        aryAdvSearchesSeries = performance.TotalApiAdvSearchRequests.DaysTimeline;
                        aryCacheHits = performance.TotalApiSearchCacheHits.DaysTimeline;
                        break;

                    default:
                        throw new Exception("Invalid series parameter.");
                }


                // ensure we do not blow array bounds - find the highest length common to all three arrays
                var maxCommonCount = Math.Min(arySearchesSeries.Count(), aryAdvSearchesSeries.Count());
                maxCommonCount = Math.Min(maxCommonCount, aryCacheHits.Count());

                // add up all the searches and cache hits for the series

                double accumulatedSearches = 0;
                double accumulatedCacheHits = 0;

                for (int i = 0; i < maxCommonCount; i++)
                {
                    // total searches is the sum of simple + advanced + cache hits for a given time elemement
                    accumulatedSearches += arySearchesSeries[i] + aryAdvSearchesSeries[i] + aryCacheHits[i];

                    accumulatedCacheHits += aryCacheHits[i];
                }

                // guard again div by zero

                if (accumulatedSearches > 0)
                    cacheHitRatio = (accumulatedCacheHits / accumulatedSearches) * 100;

                return cacheHitRatio;
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }





        /// <summary>
        /// Returns the time taken by the avg search. Milliseconds.
        /// </summary>
        /// <remarks>
        /// Computed using all sql search records in memory.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        [Invoke]
        public double SqlSearchAvgDuration(StoreKeys storeKey)
        {
            try
            {
#if false
                var rand = new Random();
                return rand.Next(20, 200);
#else
                var store = MvcApplication.Current.GetWebStore(storeKey);

                var performance = store.Performance;

                var data = performance.GetAllSqlSearchMetrics();

                if (data.Count() == 0)
                    return 0.0;

                double sumDuration = data.Select(e => e.Duration.Milliseconds).Sum(e => e);

                return sumDuration / data.Count();
#endif

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        #endregion

        #region Manufacturer Sales

        public List<SalesByManufacturerMetric> GetSalesByManufacturerMetrics(StoreKeys storeKey, int? manufacturerID)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.Performance.GetSalesByManufacturerMetrics(manufacturerID).Result;
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        /// <summary>
        /// Set of sales metrics for all manufacturers. Each has own record.
        /// </summary>
        /// <remarks>
        /// For pie charts.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<SalesByManufacturerMetric> GetComparisonSalesByManufacturerMetrics(StoreKeys storeKey, DateTime startDate, DateTime endDate)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.Performance.GetComparisonSalesByManufacturerMetrics(startDate, endDate).Result;
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        /// <summary>
        /// Create a spreadsheet for order line items for the period.
        /// </summary>
        /// <remarks>
        /// If manufactuererID is null, then all, else only for that specific manufacturer.
        /// Creates two worksheets - second for ones where some information is missing, like
        /// for when a product was deleted.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        [Invoke]
        public byte[] ExportSalesOrderDetail(StoreKeys storeKey, DateTime startDate, DateTime endDate, int? manufacturerID)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                {
                    var orders = from sc in dc.Orders_ShoppingCarts
                                  join o in dc.Orders on sc.OrderNumber equals o.OrderNumber
                                  where o.OrderDate >= startDate && o.OrderDate < endDate && o.Deleted == 0
                                  join pm in dc.ProductManufacturers on sc.ProductID equals pm.ProductID
                                  join m in dc.Manufacturers on pm.ManufacturerID equals m.ManufacturerID
                                  join pv in dc.ProductVariants on sc.VariantID equals pv.VariantID
                                  orderby sc.ShoppingCartRecID
                                  select new
                                  {
                                      ID = sc.ShoppingCartRecID,
                                      o.OrderNumber,
                                      o.OrderDate,
                                      o.TransactionState,
                                      o.CustomerID,
                                      Customer = o.FirstName + " " + o.LastName,
                                      CustomerEmail = o.Email,

                                      ManufacturerID = pm.ManufacturerID,
                                      ManufacturerName = m.Name,

                                      SKU = sc.OrderedProductSKU,
                                      ProductID = sc.ProductID,
                                      VariantID = sc.VariantID,
                                      ProductName = sc.OrderedProductName,
                                      VariantName = sc.OrderedProductVariantName,
                                      ManufacturerPartNumber = sc.OrderedProductManufacturerPartNumber,

                                      Quantity = sc.Quantity,
                                      Price = (sc.OrderedProductPrice / sc.Quantity), 
                                      ExtPrice = sc.OrderedProductPrice, 

                                      // best estimate of margin calc using now-current cost data

                                      // the issue with this calc is that a sale from a long time ago might have
                                      // been before a price change, and the cost data included now is potentially
                                      // off since we're using now cost with a prior price point. However, if this
                                      // is run on a very current month - the figures will be pretty accurate.

                                      Cost = pv.Cost,
                                      ExtCost = pv.Cost * sc.Quantity,
                                      Margin = sc.OrderedProductPrice - (pv.Cost * sc.Quantity),
                                      
                                  };

                    if (manufacturerID.HasValue)
                        orders = orders.Where(e => e.ManufacturerID == manufacturerID.Value);

                    var ordersList = orders.ToList();


                    // without joins, so will include line items with deleted manufacturers, products, etc.
                    // and we set these columns to empty or such

                    var allOrders = (from sc in dc.Orders_ShoppingCarts
                                 join o in dc.Orders on sc.OrderNumber equals o.OrderNumber
                                 where o.OrderDate >= startDate && o.OrderDate < endDate && o.Deleted == 0
                                 orderby o.OrderNumber
                                  select new
                                  {
                                      ID = sc.ShoppingCartRecID,
                                      o.OrderNumber,
                                      o.OrderDate,
                                      o.TransactionState,
                                      o.CustomerID,
                                      Customer = o.FirstName + " " + o.LastName,
                                      CustomerEmail = o.Email,

                                      ManufacturerID = "",
                                      ManufacturerName = sc.OrderedProductSKU.Substring(0, 2),

                                      SKU = sc.OrderedProductSKU,
                                      ProductID = "",
                                      VariantID = "",
                                      ProductName = sc.OrderedProductName,
                                      VariantName = sc.OrderedProductVariantName,
                                      ManufacturerPartNumber = sc.OrderedProductManufacturerPartNumber,

                                      Quantity = sc.Quantity,
                                      Price = (sc.OrderedProductPrice / sc.Quantity),
                                      ExtPrice = sc.OrderedProductPrice,
                                  }).ToDictionary(k => k.ID, v => v);


                    // build up a list of ones which are missing

                    var hashOrders = new HashSet<int>(ordersList.Select(e => e.ID));

                    foreach(var item in allOrders.Keys.ToList())
                    {
                        if (hashOrders.Contains(item))
                            allOrders.Remove(item);                            
                    }

                    using (var pck = new ExcelPackage())
                    {

                        //Create the worksheet
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Order Details");

                        ws.Cells["A1"].LoadFromCollection(ordersList, true);

                        //Format the header
                        using (ExcelRange rng = ws.Cells["A1:U1"])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }

                        if (ordersList.Count() > 0)
                        {
                            // date field
                            using (var col = ws.Cells.Offset(1, 2, orders.Count(), 1))
                            {
                                col.Style.Numberformat.Format = "mm/dd/yyyy";
                                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }

                            // money fields
                            using (var col = ws.Cells.Offset(1, 16, orders.Count(), 5))
                            {
                                col.Style.Numberformat.Format = "#,##0.00";
                                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                        }

                        ws.Cells.AutoFitColumns();
                        ws.View.FreezePanes(2, 1);

                        // second sheet for missing

                        var missingOrders = allOrders.Values.OrderBy(e => e.ID).ToList();

                        ExcelWorksheet ws2 = pck.Workbook.Worksheets.Add("Incomplete Order Details");

                        ws2.Cells["A1"].LoadFromCollection(missingOrders, true);

                        //Format the header
                        using (ExcelRange rng = ws2.Cells["A1:R1"])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 217, 53, 41));  
                            rng.Style.Font.Color.SetColor(Color.White);
                        }

                        if (missingOrders.Count() > 0)
                        {
                            // date field
                            using (var col = ws2.Cells.Offset(1, 2, missingOrders.Count(), 1))
                            {
                                col.Style.Numberformat.Format = "mm/dd/yyyy";
                                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }

                            // money fields
                            using (var col = ws2.Cells.Offset(1, 16, missingOrders.Count(), 2))
                            {
                                col.Style.Numberformat.Format = "#,##0.00";
                                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                        }

                        ws2.Cells.AutoFitColumns();
                        ws2.View.FreezePanes(2, 1);


                        return pck.GetAsByteArray();
                    }

                }

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }

        }


        /// <summary>
        /// Create a spreadsheet showing the comparison of sales between manufacturers.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [Invoke]
        public byte[] ExportSalesComparisonByManufacturer(StoreKeys storeKey, DateTime startDate, DateTime endDate)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                var data = (from d in GetComparisonSalesByManufacturerMetrics(storeKey, startDate, endDate)
                            orderby d.ManufacturerID
                            select new
                            {
                                d.ManufacturerID,
                                d.ManufacturerName,
                                d.ProductOrders,
                                d.SwatchOrders,
                                d.ProductYards,
                                d.ProductSales,
                                d.SwatchSales,
                                d.TotalSales,
                                d.Margin
                            }).ToList();


                using (var pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet 1");

                    ws.Cells["A1"].LoadFromCollection(data, true);

                    //Format the header
                    using (ExcelRange rng = ws.Cells["A1:I1"])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(Color.White);
                    }

                    if (data.Count() > 0)
                    {
                        using (var col = ws.Cells.Offset(1, 2, data.Count(), 3))
                        {
                            col.Style.Numberformat.Format = "#,##0";
                            col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }

                        // money fields
                        using (var col = ws.Cells.Offset(1, 5, data.Count(), 4))
                        {
                            col.Style.Numberformat.Format = "#,##0.00";
                            col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                    }

                    ws.Cells.AutoFitColumns();
                    ws.View.FreezePanes(2, 1);
                    return pck.GetAsByteArray();
                }


            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }

        }

        #endregion

        #region Manufacturer Counts


        public List<ProductCountsByManufacturerMetric> GetProductCountsByManufacturerMetrics(StoreKeys storeKey)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.GetProductCountsByManufacturerMetrics();
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }


        /// <summary>
        /// Create a spreadsheet showing the comparison of sales between manufacturers.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [Invoke]
        public byte[] ExportManufacturerProductCounts(StoreKeys storeKey)
        {
            // DUMMY

            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                var data = GetProductCountsByManufacturerMetrics(storeKey);

                using (var pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet 1");

                    ws.Cells["A1"].LoadFromCollection(data, true);

                    //Format the header
                    using (ExcelRange rng = ws.Cells["A1:V1"])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(Color.White);
                    }

                    if (data.Count() > 0)
                    {
                        using (var col = ws.Cells.Offset(1, 2, data.Count(), 4))
                        {
                            col.Style.Numberformat.Format = "#,##0";
                            col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                    }

                    ws.Cells.AutoFitColumns();
                    ws.View.FreezePanes(2, 1);
                    return pck.GetAsByteArray();
                }


            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }

        }
        #endregion

        #region Sales Summary

        public List<SalesSummaryMetric> GetAllSalesSummaryMetrics(StoreKeys storeKey)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                return store.Performance.GetAllSalesSummaryMetrics().Result;
            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
        }

        [Invoke]
        public byte[] ExportSalesOrders(StoreKeys storeKey, DateTime  startDate, DateTime endDate)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                Func<DateTime?, string> fmtDate = (dt) =>
                    {
                        return dt.HasValue ? dt.Value.ToString("d") : string.Empty;
                    };

                using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                {
                    var orders = (from o in dc.Orders
                        where o.OrderDate >= startDate && o.OrderDate < endDate && o.Deleted == 0
                        orderby o.OrderNumber
                        select new
                        {
                            o.OrderNumber,
                            OrderDate = string.Format("{0:d}", o.OrderDate),
                            o.CustomerID,
                            o.FirstName,
                            o.LastName,
                            o.Email,
                            o.ShippingState,
                            o.OrderSubtotal,
                            o.OrderShippingCosts,
                            o.OrderTax,
                            o.OrderTotal,
                            o.CouponCode,
                            o.CouponDiscountAmount,
                            o.CouponDiscountPercent,
                            o.TransactionState, // CAPTURED, VOIDED, AUTHORIZED, REFUNDED
                            o.PaymentGateway,
                            o.AuthorizationCode,
                            AuthorizedOn = fmtDate(o.AuthorizedOn),
                            RefundedOn = fmtDate(o.RefundedOn),
                            VoidedOn = fmtDate(o.VoidedOn),
                        }).ToList();


                    using (var pck = new ExcelPackage())
                    {

                        //Create the worksheet
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Orders");

                        ws.Cells["A1"].LoadFromCollection(orders, true);

                        //Format the header
                        using (ExcelRange rng = ws.Cells["A1:T1"])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }

                        if (orders.Count() > 0)
                        {
                            using (var col = ws.Cells.Offset(1, 7, orders.Count(), 4))
                            {
                                col.Style.Numberformat.Format = "#,##0.00";
                                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                        }

                        ws.Cells.AutoFitColumns();

                        return pck.GetAsByteArray();
                    }                    
                    
                }

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }

        }



#if DEBUG
        [Invoke]
        public byte[] ExportSalesOrdersHack(StoreKeys storeKey, DateTime startDate, DateTime endDate)
        {
            // this is a hacked version of the above method to filter on CA orders from Fabricut brands.
            // left here to use as a template in case needed in the future.

            try
            {
                startDate = DateTime.Parse("1/1/2013");
                endDate = DateTime.Now;

                var store = MvcApplication.Current.GetWebStore(storeKey);

                Func<DateTime?, string> fmtDate = (dt) =>
                {
                    return dt.HasValue ? dt.Value.ToString("d") : string.Empty;
                };

                using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                {
                    var orders = (from o in dc.Orders
                                  where o.OrderDate >= startDate && o.OrderDate < endDate && o.Deleted == 0 && o.ShippingState == "CA"
                                  orderby o.OrderNumber
                                  select new
                                  {
                                      o.OrderNumber,
                                      OrderDate = string.Format("{0:d}", o.OrderDate),
                                      o.CustomerID,
                                      o.FirstName,
                                      o.LastName,
                                      o.Email,
                                      o.ShippingState,
                                      o.OrderSubtotal,
                                      o.OrderShippingCosts,
                                      o.OrderTax,
                                      o.OrderTotal,
                                      o.CouponCode,
                                      o.CouponDiscountAmount,
                                      o.CouponDiscountPercent,
                                      o.TransactionState, // CAPTURED, VOIDED, AUTHORIZED, REFUNDED
                                      o.PaymentGateway,
                                      o.AuthorizationCode,
                                      AuthorizedOn = fmtDate(o.AuthorizedOn),
                                      RefundedOn = fmtDate(o.RefundedOn),
                                      VoidedOn = fmtDate(o.VoidedOn),
                                  }).ToList();

                    var fabricutOrders = new HashSet<int>(dc.Orders_ShoppingCarts
                                        .Where(oi => oi.CreatedOn >= startDate && oi.CreatedOn < endDate.AddDays(1))
                                        .Where(oi => oi.OrderedProductSKU.StartsWith("FC-") || oi.OrderedProductSKU.StartsWith("TR-") || oi.OrderedProductSKU.StartsWith("VV-") || oi.OrderedProductSKU.StartsWith("HA-") || oi.OrderedProductSKU.StartsWith("SH-"))
                                        .Select(oi => oi.OrderNumber)
                                        .ToList());

                    orders.RemoveAll(x => !fabricutOrders.Contains(x.OrderNumber));

                    using (var pck = new ExcelPackage())
                    {

                        //Create the worksheet
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Orders");

                        ws.Cells["A1"].LoadFromCollection(orders, true);

                        //Format the header
                        using (ExcelRange rng = ws.Cells["A1:T1"])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }

                        if (orders.Count() > 0)
                        {
                            using (var col = ws.Cells.Offset(1, 7, orders.Count(), 4))
                            {
                                col.Style.Numberformat.Format = "#,##0.00";
                                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                        }

                        ws.Cells.AutoFitColumns();

                        return pck.GetAsByteArray();
                    }

                }

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }

        }
#endif

        #endregion

        #region Product Upload

        /// <summary>
        /// Given name of already uploaded file
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [Invoke]
        public string ProcessProductUpload(StoreKeys storeKey, string filename)
        {
#if true
            throw new NotImplementedException("ProcessProductUpload() obsolete due to automation of InsideAvenue.");
            // ProductImageUnzipManager.cs has been excluded from the project -- all this was working fine, just no longer needed
#else
            string filepath = null;

            try
            {
                if (string.IsNullOrWhiteSpace(filename))
                    throw new Exception("Missing filename.");

                var store = MvcApplication.Current.GetWebStore(storeKey);

                if (string.IsNullOrWhiteSpace(store.UploadsFolder))
                    throw new Exception("Service UploadsFolder not configured.");

                filepath = Path.Combine(store.UploadsFolder, filename);

                if (!File.Exists(filepath))
                    throw new Exception(string.Format("File does not exist: {0}", filepath));

                var mgr = new ProductImageUnzipManager();

                var result = mgr.UnzipAndResize(store, filepath);
                return result;

            }
            catch (Exception Ex)
            {
                ProcessException(Ex);
                throw;
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(filepath) && File.Exists(filepath))
                        File.Delete(filepath);
                }
                catch
                {
                }
            }
#endif
        }
        #endregion

    }
}


