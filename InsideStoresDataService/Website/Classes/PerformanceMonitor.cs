using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Website
{

    public class PerformanceMonitor
    {
        private IWebStore store;

        private int searchMetricNextIdentifier;

        private const int SearchMetricCapacity = 1000;

        protected LinkedList<SqlSearchMetric> SearchMetrics { get; private set; }


        private DateTime GlobalStartDate()
        {
            // common start date for showing metrics in control panel sales data
            // this is the single place to change the timebar settings in the control panel.

            // all the other DateTime.Now.AddYears(-2).Date in the code are only design time settings

            return  DateTime.Now.AddYears(-3).Date;
        }

        /// <summary>
        /// Total API requests through the system - of any kind.
        /// </summary>
        public MultiTimeline TotalApiRequests { get; private set; }

        /// <summary>
        /// Actual full text search - simple (not advanced search page)
        /// </summary>
        /// <remarks>
        /// This is a query actually submitted to SQL.
        /// </remarks>
        public MultiTimeline TotalApiSearchRequests { get; private set; }

        /// <summary>
        /// Actual full text search from advanced search.
        /// </summary>
        /// <remarks>
        /// This is a query actually submitted to SQL.
        /// </remarks>
        public MultiTimeline TotalApiAdvSearchRequests { get; private set; }

        /// <summary>
        /// Number of times a search requests come in which is found in cache
        /// and not submitted to SQL.
        /// </summary>
        public MultiTimeline TotalApiSearchCacheHits { get; private set; }


        /// <summary>
        /// Number of page views for store website.
        /// </summary>
        public PageViewsMultiTimeline PageViews { get; private set; }


        public PerformanceMonitor(IWebStore store)
        {
            this.store = store;
            TotalApiRequests = new MultiTimeline();
            TotalApiSearchRequests = new MultiTimeline();
            TotalApiAdvSearchRequests = new MultiTimeline();
            TotalApiSearchCacheHits = new MultiTimeline();
            PageViews = new PageViewsMultiTimeline();
            searchMetricNextIdentifier = 1;
            SearchMetrics = new LinkedList<SqlSearchMetric>();
        }

        /// <summary>
        /// Return a collection of SQL search metrics.
        /// </summary>
        /// <remarks>
        /// Last item in search linked list appears first in the returned collection.
        /// </remarks>
        /// <param name="startingAfterID"></param>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        public List<SqlSearchMetric> GetSqlSearchMetrics(int startingAfterID, int takeCount=int.MaxValue)
        {
            // search starting from the end...because assumes we are looking to pick up the last
            // items added since a specific recent point in time

            lock (SearchMetrics)
            {
                var data = new List<SqlSearchMetric>();

                var currentNode = SearchMetrics.Last;
                while (currentNode != null)
                {
                    if (currentNode.Value.ID <= startingAfterID)
                        break;

                    data.Add(currentNode.Value);
                    currentNode = currentNode.Previous;
                }

                return data;
            }
        }

        /// <summary>
        /// Return a collection of SQL search metrics.
        /// </summary>
        /// <remarks>
        /// Last item in search linked list appears first in the returned collection.
        /// </remarks>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public List<SqlSearchMetric> GetLastSqlSearchMetrics(int maxCount)
        {
            lock (SearchMetrics)
            {
                if (maxCount == 0)
                    maxCount = int.MaxValue;

                var data = new List<SqlSearchMetric>();

                int count = 0;
                var currentNode = SearchMetrics.Last;
                while (currentNode != null)
                {
                    data.Add(currentNode.Value);

                    if (++count == maxCount)
                        break;
                    currentNode = currentNode.Previous;
                }

                return data;
            }
        }

        /// <summary>
        /// Return a collection of all SQL search metrics.
        /// </summary>
        /// <remarks>
        /// First item in linked list is first item in returned collection.
        /// </remarks>
        /// <returns></returns>
        public List<SqlSearchMetric> GetAllSqlSearchMetrics()
        {
            lock (SearchMetrics)
            {
                return SearchMetrics.ToList();
            }
        }


        public void AddSearchMetric(string phrase, DateTime startTime, int resultCount, bool isAdvanced = false)
        {
            // note that search operators (like, color, style, pattern, suggested) are not passed through to this method
            // so we don't clutter our results.

            var metric = new SqlSearchMetric()
            {
                SearchPhrase = string.IsNullOrWhiteSpace(phrase) ? "{None}" : phrase,
                Time = startTime,
                Duration = DateTime.Now - startTime,
                ResultCount = resultCount,
                IsAdvancedSearch = isAdvanced,
            };


            lock (SearchMetrics)
            {
                metric.ID = searchMetricNextIdentifier++;

                // if at capacity, pull the oldest one out
                if (SearchMetrics.Count >= SearchMetricCapacity)
                    SearchMetrics.RemoveFirst();

                SearchMetrics.AddLast(metric);
            }

        }

        #region Sales by Manufacturer

        private Task<List<SalesByManufacturerMetric>> CalculateSalesByManufacturerMetrics(int? manufacturerID)
        {
            var t = Task<List<SalesByManufacturerMetric>>.Factory.StartNew(() =>
            {
                try
                {
                    var taskStartTime = DateTime.Now;

                    var startDate = GlobalStartDate();

                    using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                    {
                        var orders = from sc in dc.Orders_ShoppingCarts
                                      join o in dc.Orders on sc.OrderNumber equals o.OrderNumber
                                      where o.OrderDate >= startDate && o.Deleted == 0
                                      join pm in dc.ProductManufacturers on sc.ProductID equals pm.ProductID
                                     orderby o.OrderNumber
                                     select new
                                      {
                                          o.OrderNumber,
                                          o.OrderDate,
                                          ManufacturerID = pm.ManufacturerID,
                                          IsSwatchOrder = sc.OrderedProductSKU.Contains("-Swatch"),
                                          Quantity = sc.Quantity,
                                          ExtPrice = sc.OrderedProductPrice.GetValueOrDefault(),
                                      };


                        if (manufacturerID.HasValue)
                            orders = orders.Where(e => e.ManufacturerID == manufacturerID.Value);

                        var ordersList = orders.ToList();

                        // aggregate per day

                        var list = new List<SalesByManufacturerMetric>();

                        if (ordersList.Count() == 0)
                            return list;

                        Func<DateTime, SalesByManufacturerMetric> makeNewDay = (dt) =>
                        {
                            var thisNewDay = new SalesByManufacturerMetric()
                            {
                                Date = dt,
                                ProductOrders = 0,
                                SwatchOrders = 0,

                                ProductYards = 0,

                                ProductSales = 0M,
                                SwatchSales = 0M,
                                TotalSales = 0M,
                            };

                            return thisNewDay;

                        };

                        var day = makeNewDay(ordersList[0].OrderDate.Date);

                        // note that it's important to cover all days - fill in missing days with zero totals

                        DateTime nextDay;

                        foreach (var order in ordersList)
                        {
                            if (order.OrderDate.Date != day.Date)
                            {
                                list.Add(day);
                                nextDay = day.Date.AddDays(1).Date;
                                day = makeNewDay(nextDay);

                                // fill in missing days
                                while (nextDay != order.OrderDate.Date)
                                {
                                    list.Add(day);
                                    nextDay = day.Date.AddDays(1).Date;
                                    day = makeNewDay(nextDay);
                                }
                            }

                            // populate each of the fields in the metric object

                            if (order.IsSwatchOrder)
                            {
                                day.SwatchOrders++;
                                day.SwatchSales += order.ExtPrice;
                            }
                            else
                            {
                                day.ProductOrders++;
                                day.ProductYards += order.Quantity;
                                day.ProductSales += order.ExtPrice;
                            }

                            day.TotalSales += order.ExtPrice;

                        }

                        list.Add(day);
                        nextDay = day.Date.AddDays(1).Date;
                        day = makeNewDay(nextDay);

                        var today = DateTime.Now.Date;

                        // fill in missing days up through today
                        while (nextDay <= today)
                        {
                            list.Add(day);
                            nextDay = day.Date.AddDays(1).Date;
                            day = makeNewDay(nextDay);
                        }

                        Debug.WriteLine(string.Format("Time to populate sales by manufacturer data for {0}: {1}", store.FriendlyName, DateTime.Now - taskStartTime));

                        return list;
                    }

                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.Message);

                    var ev = new WebsiteRequestErrorEvent("Unhandled Exception. CalculateSalesByManufacturerMetrics().", this, WebsiteEventCode.UnhandledException, Ex);
                    ev.Raise();

                    return new List<SalesByManufacturerMetric>();
                }
            });

            return t;
        }


        private Task<List<SalesByManufacturerMetric>> CalculateComparisonSalesByManufacturerMetrics(DateTime startDate, DateTime endDate)
        {
            var t = Task<List<SalesByManufacturerMetric>>.Factory.StartNew(() =>
            {
                try
                {
                    using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                    {

                        // dic to hold one metric object for each manufacturer, to be hydrated along the way

                        var manufacturers = (from m in dc.Manufacturers
                                where m.Deleted == 0 && m.Published == 1
                                orderby m.Name
                                select new SalesByManufacturerMetric
                                {
                                    ManufacturerID = m.ManufacturerID,
                                    ManufacturerName = m.Name,
                                }).ToDictionary(k => k.ManufacturerID, v => v);

                        // orders (line items) within the stated date range

                        var ordersList = (from sc in dc.Orders_ShoppingCarts
                                     join o in dc.Orders on sc.OrderNumber equals o.OrderNumber
                                     where o.OrderDate >= startDate && o.OrderDate < endDate && o.Deleted == 0
                                     join pm in dc.ProductManufacturers on sc.ProductID equals pm.ProductID
                                     join pv in dc.ProductVariants on sc.VariantID equals pv.VariantID
                                     orderby sc.ShoppingCartRecID
                                     select new
                                     {
                                         sc.ShoppingCartRecID,
                                         o.OrderNumber,
                                         o.OrderDate,
                                         ManufacturerID = pm.ManufacturerID,
                                         IsSwatchOrder = sc.OrderedProductSKU.Contains("-Swatch"),
                                         Quantity = sc.Quantity,
                                         ExtPrice = sc.OrderedProductPrice.GetValueOrDefault(),
                                         Margin = sc.OrderedProductPrice.GetValueOrDefault() - (sc.Quantity * pv.Cost.GetValueOrDefault())
                                     }).ToList();



                        foreach (var order in ordersList)
                        {
                            SalesByManufacturerMetric manufacturerMetric;

                            if (!manufacturers.TryGetValue(order.ManufacturerID, out manufacturerMetric))
                                continue;

                            if (order.IsSwatchOrder)
                            {
                                manufacturerMetric.SwatchOrders++;
                                manufacturerMetric.SwatchSales += order.ExtPrice;
                            }
                            else
                            {
                                manufacturerMetric.ProductOrders++;
                                manufacturerMetric.ProductYards += order.Quantity;
                                manufacturerMetric.ProductSales += order.ExtPrice;
                            }

                            manufacturerMetric.TotalSales += order.ExtPrice;
                            manufacturerMetric.Margin += order.Margin;
                        }

                        return manufacturers.Values.OrderBy(e => e.ManufacturerName).ToList();
                    }

                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.Message);

                    var ev = new WebsiteRequestErrorEvent("Unhandled Exception. CalculateSalesByManufacturerMetrics().", this, WebsiteEventCode.UnhandledException, Ex);
                    ev.Raise();

                    return new List<SalesByManufacturerMetric>();
                }
            });

            return t;
        }


        /// <summary>
        /// Return a collection of all sales by manufacturer metrics for daily sales performance.
        /// </summary>
        /// <returns></returns>
        public async Task<List<SalesByManufacturerMetric>> GetSalesByManufacturerMetrics(int? manufacturerID)
        {
            var list = await CalculateSalesByManufacturerMetrics(manufacturerID);
            return list;
        }


        public async Task<List<SalesByManufacturerMetric>> GetComparisonSalesByManufacturerMetrics(DateTime startDate, DateTime endDate)
        {
            var list = await CalculateComparisonSalesByManufacturerMetrics(startDate, endDate);
            return list;
        }
        #endregion

        #region Sales Summary

        private Task<List<SalesSummaryMetric>> CalculateSalesSummmaryMetrics()
        {
            var t = Task<List<SalesSummaryMetric>>.Factory.StartNew(() =>
                {
                    try
                    {
                        var taskStartTime = DateTime.Now;

                        var startDate = GlobalStartDate();

                        using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                        {
                            var orders = (from o in dc.Orders
                                          where o.OrderDate >= startDate && o.Deleted == 0
                                          orderby o.OrderNumber
                                          select new
                                          {
                                              o.OrderNumber,
                                              o.OrderDate,
                                              o.OrderSubtotal,
                                              o.OrderTax,
                                              o.OrderShippingCosts,
                                              o.OrderTotal,
                                              o.TransactionType,
                                              o.AuthorizationCode,
                                              o.TransactionState, // CAPTURED, VOIDED, AUTHORIZED, REFUNDED
                                              o.AuthorizedOn,
                                              o.RefundedOn,
                                              o.VoidedOn,
                                              o.CouponCode,
                                              o.CouponDiscountAmount,
                                              o.CouponDiscountPercent
                                          }).ToList();

                            // aggregate per day

                            var list = new List<SalesSummaryMetric>();

                            if (orders.Count() == 0)
                                return list;

                            Func<DateTime, SalesSummaryMetric> makeNewDay = (dt) =>
                                {
                                    var thisNewDay = new SalesSummaryMetric()
                                    {
                                        Date = dt,
                                        OrderCount = 0,
                                        TotalSales = 0M,

                                        NetSales = 0M,
                                        Voided = 0M,
                                        Refunded = 0M,

                                        Authorized = 0M,
                                        Captured = 0M,
                                        Adjustments = 0M,

                                        SalesTax = 0M,
                                        Shipping = 0M,
                                        ProductSubtotal = 0M,
                                    };

                                    return thisNewDay;

                                };

                            var day = makeNewDay(orders[0].OrderDate.Date);

                            // note that it's important to cover all days - fill in missing days with zero totals

                            DateTime nextDay;

                            foreach (var order in orders)
                            {
                                if (order.OrderDate.Date != day.Date)
                                {
                                    list.Add(day);
                                    nextDay = day.Date.AddDays(1).Date; 
                                    day = makeNewDay(nextDay);

                                    // fill in missing days
                                    while (nextDay != order.OrderDate.Date)
                                    {
                                        list.Add(day);
                                        nextDay = day.Date.AddDays(1).Date;
                                        day = makeNewDay(nextDay);
                                    }
                                }

                                // ordersubtotal already takes coupons into consideration...provided done online; but not when customer calls and asks for discount after the fact

                                // sanity check
                                //if (store.StoreKey == StoreKeys.InsideFabric &&  (order.TransactionState == "AUTHORIZED" || order.TransactionState == "CAPTURED") && order.OrderDate > DateTime.Parse("1/1/2013"))
                                //{
                                //    var parts = order.OrderSubtotal + order.OrderTax + order.OrderShippingCosts;
                                //    if (parts - order.OrderTotal == 20 && order.OrderSubtotal >= 200)
                                //        Debug.WriteLine(string.Format("Order {0}: {1}   {2}   Coupon: {3}", order.OrderNumber, order.OrderSubtotal, order.OrderTotal, string.IsNullOrEmpty(order.CouponCode) ? "None" : order.CouponCode));
                                //}

                                // OrderTotal can be less than OrderSubtotal (and tax + shipping) under the following scenarios:
                                //    - customer calls up and says forgot to use coupon, so capture is put in for less, OrderTotal gets adjusted, subtotal not changed
                                //    - item is not available for partial order, so only capture what is available, OrderTotal gets adjusted, subtotal stays as it was

                                var voided = order.TransactionState == "VOIDED" ? order.OrderTotal : 0M;
                                var refunded = order.TransactionState == "REFUNDED" ? order.OrderTotal : 0M;
                                var totalSales = order.OrderSubtotal + order.OrderTax + order.OrderShippingCosts;
                                var adjustments = order.TransactionState == "CAPTURED" ? totalSales - order.OrderTotal : 0M;

                                day.OrderCount++;
                                day.TotalSales += totalSales; // done this way to not get tripped up when capture is less than original total
                                day.Voided += voided;
                                day.Refunded += refunded;
                                day.Authorized += order.TransactionState == "AUTHORIZED" ? order.OrderTotal : 0M;
                                day.Captured += order.TransactionState == "CAPTURED" ? order.OrderTotal : 0M; // correctly deals with actual captured amounts if capture happens to be less than original sale
                                day.Adjustments += adjustments;

                                // correctly deals with actual captured amounts
                                // however, partial voids/refunds reflected in a reduced capture will not reflect in the reported void/refunds, and will simply
                                // show as a reduced net; same goes for offline coupons
                                day.NetSales += (totalSales - (voided + refunded + adjustments));

                                // these are calculated as Net values

                                switch (order.TransactionState)
                                {
                                    case "VOIDED":
                                        // voided transactions do not contribute to Net values
                                        break;

                                    case "REFUNDED":
                                        // refunded transactions do not contribute to Net values
                                        break;

                                    default:
                                        day.SalesTax += order.OrderTax;
                                        day.Shipping += order.OrderShippingCosts;

                                        // do not use order.OrderSubtotal here because may have been captured at less than original transaction
                                        // due to discontinued product and coupon discounts applied after the fact
                                        day.ProductSubtotal += order.OrderTotal - (order.OrderTax + order.OrderShippingCosts);
                                        break;
                                }

                            }

                            list.Add(day);
                            nextDay = day.Date.AddDays(1).Date;
                            day = makeNewDay(nextDay);

                            var today = DateTime.Now.Date;

                            // fill in missing days up through today
                            while (nextDay <= today)
                            {
                                list.Add(day);
                                nextDay = day.Date.AddDays(1).Date;
                                day = makeNewDay(nextDay);
                            }

                            Debug.WriteLine(string.Format("Time to populate sales summary data for {0}: {1}", store.FriendlyName, DateTime.Now - taskStartTime));

                            return list;
                        }


                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine(Ex.Message);

                        var ev = new WebsiteRequestErrorEvent("Unhandled Exception. CalculateSalesSummmaryMetrics().", this, WebsiteEventCode.UnhandledException, Ex);
                        ev.Raise();

                        return new List<SalesSummaryMetric>();
                    }
                });

            return t;
        }

        /// <summary>
        /// Return a collection of all sales summary metrics for daily sales performance.
        /// </summary>
        /// <returns></returns>
        public async Task<List<SalesSummaryMetric>> GetAllSalesSummaryMetrics()
        {
            var list = await CalculateSalesSummmaryMetrics();
            return list;
        }



        #endregion
    }
}