using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using MEFedMVVM.ViewModelLocator;
using Intersoft.Client.Framework;
using Intersoft.Client.Framework.Input;
using Intersoft.Client.UI.Aqua;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using MEFedMVVM.Services.CommonServices;
using System.ServiceModel.DomainServices.Client;
using Intersoft.Client.UI.Navigation;
using System.ComponentModel.Composition;
using Website.Services;
using System.Collections.Generic;
using ControlPanel.Views;
using System.Windows.Threading;
using Website;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;


namespace ControlPanel.ViewModels
{


    public class CombinedSalesSummaryPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {

        public class BarChartItem
        {
            public string Label { get; set; }

            public decimal Voided { get; set; }
            public decimal Refunded { get; set; }
            public decimal Adjustments { get; set; }
            public decimal NetSales { get; set; }

            public BarChartItem()
            {

            }

            public decimal Total
            {
                get
                {
                    return Voided + Refunded + NetSales + Adjustments;
                }
            }

            public bool IsAllZeros
            {
                get
                {
                    return Voided == 0M && Refunded == 0M && NetSales == 0M && Adjustments == 0M;
                }
            }

        }



        public class PieChartItem
        {
            public string Label { get; set; }
            public int Percent { get; set; }

            public PieChartItem()
            {

            }

            public PieChartItem(string label, int pct)
            {
                this.Label = label;
                this.Percent = pct;
            }
        }


        private bool isInitializing;

        public CombinedSalesSummaryPageViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
            }
            else
            {
                SetDesignTimeData();
            }
        }


        private void Recalculate()
        {
            if (isInitializing || TimebarItemsSource == null || timebarItemsSource.Count() == 0 || ISControl.IsInDesignModeStatic)
                return;

            try
            {
                var startDate = SelectedStartDate;
                var endDate = SelectedEndDate.AddDays(-1);

                // metrics

                int orderCount = 0;
                decimal totalSales = 0M;
                decimal voided = 0M;
                decimal refunded = 0M;
                decimal authorized = 0M;
                decimal captured = 0M;
                decimal salesTax = 0M;
                decimal shipping = 0M;
                decimal productSales = 0M;
                decimal adjustments = 0M;

                // assumes the timebar data is ordered by date

                // keep track of indexes used from collection here for fast access below for bar chart

                int startIndex = 0;
                int lastIndex = 0;

                foreach (var item in TimebarItemsSource)
                {
                    if (item.Date < startDate)
                    {
                        startIndex++;
                        lastIndex++;
                        continue;
                    }

                    if (item.Date > endDate)
                        break;

                    orderCount += item.OrderCount;
                    totalSales += item.TotalSales;
                    voided += item.Voided;
                    refunded += item.Refunded;
                    adjustments += item.Adjustments;
                    authorized += item.Authorized;
                    captured += item.Captured;
                    salesTax += item.SalesTax;
                    shipping += item.Shipping;
                    productSales += item.ProductSubtotal;

                    lastIndex++;
                }

                // make so this last index is inclusive
                lastIndex--;

                TotalSales = totalSales.ToString("C");
                NetSales = (totalSales - (voided + refunded + adjustments)).ToString("C");
                OrderCount = orderCount.ToString("N0");
                VoidedSales = voided.ToString("C");
                RefundedSales = refunded.ToString("C");

                var metricsData = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("Order Count".ToUpper(), OrderCount),
                //new KeyValuePair<string, string>("Total Sales", TotalSales),
                new KeyValuePair<string, string>("Voided".ToUpper(), VoidedSales),
                new KeyValuePair<string, string>("Refunded".ToUpper(), RefundedSales),
                new KeyValuePair<string, string>("Adjustments".ToUpper(), adjustments.ToString("C")),
                new KeyValuePair<string, string>("Authorized".ToUpper(), authorized.ToString("C")),
                new KeyValuePair<string, string>("Captured".ToUpper(), captured.ToString("C")),
                new KeyValuePair<string, string>("Net Sales Tax".ToUpper(), salesTax.ToString("C")),
                new KeyValuePair<string, string>("Net Shipping".ToUpper(), shipping.ToString("C")),
                new KeyValuePair<string, string>("Net Product".ToUpper(), productSales.ToString("C")),
            };

                var colMetricsData = new ObservableCollection<KeyValuePair<string, string>>(metricsData);
                MetricsItemsSource = colMetricsData;

                // pie chart, uses metrics just calc'd above

                Func<decimal, int> calcPctOfSales = (amt) =>
                    {
                        if (totalSales == 0M)
                            return 0;

                        return (int)Math.Round((amt / totalSales) * 100M);
                    };

                int pieVoidedPct = calcPctOfSales(voided);
                int pieRefundedPct = calcPctOfSales(refunded);
                int pieAdjustmentsPct = calcPctOfSales(adjustments);
                int pieNetSalesPct = 100 - (pieVoidedPct + pieRefundedPct + pieAdjustmentsPct);

                var pieChartData = new List<PieChartItem>
            {
                new PieChartItem("Net Sales", pieNetSalesPct),
                new PieChartItem("Voided", pieVoidedPct),
                new PieChartItem("Refunded", pieRefundedPct),
                new PieChartItem("Adjustments", pieAdjustmentsPct)
            };

                var colPieChart = new ObservableCollection<PieChartItem>(pieChartData);
                PieChartItemsSource = colPieChart;

                // bar chart

                // show different number of bars and date labels based on the duration of the selected period

                var salesChart = new List<BarChartItem>();

                var currentDate = startDate;

                string barChartLabel = null;
                decimal barChartNetSales = 0M;
                decimal barChartVoided = 0M;
                decimal barChartRefunded = 0M;
                decimal barChartAdjustments = 0M;

                Action insertChartItem = () =>
                {
                    var itm = new BarChartItem()
                    {
                        Label = barChartLabel,
                        NetSales = barChartNetSales,
                        Voided = barChartVoided,
                        Refunded = barChartRefunded,
                        Adjustments = barChartAdjustments,
                    };

                    salesChart.Add(itm);

                    barChartLabel = null;
                    barChartNetSales = 0M;
                    barChartVoided = 0M;
                    barChartRefunded = 0M;
                    barChartAdjustments = 0M;
                };


                if (lastIndex - startIndex <= 31)
                {
                    // show N days

                    for (var i = startIndex; i <= lastIndex; i++)
                    {
                        var item = TimebarItemsSource[i];
                        var day = new BarChartItem()
                        {
                            Label = string.Format("{0:d}", currentDate),
                            NetSales = item.NetSales,
                            Voided = item.Voided,
                            Refunded = item.Refunded,
                            Adjustments = item.Adjustments,
                        };

                        salesChart.Add(day);
                        currentDate = currentDate.AddDays(1);
                    }

                }
                else if (lastIndex - startIndex <= (31 * 24))
                {
                    // show N months

                    Func<DateTime, string> makeMonthLabel = (dt) =>
                        {
                            return dt.ToString("MMM yyyy");
                        };

                    for (var i = startIndex; i <= lastIndex; i++)
                    {
                        var item = TimebarItemsSource[i];

                        var thisMonthLabel = makeMonthLabel(currentDate);
                        if (barChartLabel != thisMonthLabel)
                        {
                            if (barChartLabel != null)
                                insertChartItem();
                            barChartLabel = thisMonthLabel;
                        }
                        barChartNetSales += item.NetSales;
                        barChartVoided += item.Voided;
                        barChartRefunded += item.Refunded;
                        barChartAdjustments += item.Adjustments;

                        currentDate = currentDate.AddDays(1);
                    }

                    insertChartItem();
                }
                else
                {
                    // show years


                    Func<DateTime, string> makeYearLabel = (dt) =>
                    {
                        return dt.ToString("yyyy");
                    };

                    for (var i = startIndex; i <= lastIndex; i++)
                    {
                        var item = TimebarItemsSource[i];

                        var thisYearLabel = makeYearLabel(currentDate);
                        if (barChartLabel != thisYearLabel)
                        {
                            if (barChartLabel != null)
                                insertChartItem();
                            barChartLabel = thisYearLabel;
                        }
                        barChartNetSales += item.NetSales;
                        barChartVoided += item.Voided;
                        barChartRefunded += item.Refunded;
                        barChartAdjustments += item.Adjustments;

                        currentDate = currentDate.AddDays(1);
                    }

                    insertChartItem();


                }

                // if everything is zero, then yield empty collection, else Chart control
                // seems to put up a full set of green bars

                if (salesChart.Where(e => e.IsAllZeros).Count() == salesChart.Count())
                    salesChart.Clear();

                var colSalesChart = new ObservableCollection<BarChartItem>(salesChart);
                SalesChartItemsSource = colSalesChart;
            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }

        }

        private void LoadSalesSummaryData()
        {
            isInitializing = true;

            var combinedList = new List<SalesSummaryMetric>();

            foreach (var item in AppSvc.Current.GetSalesSummaryData(StoreKeys.InsideFabric))
            {
                var day = new SalesSummaryMetric()
                {
                    Date = item.Date,
                    OrderCount = item.OrderCount,
                    TotalSales = item.TotalSales,

                    NetSales = item.NetSales,
                    Voided = item.Voided,
                    Refunded = item.Refunded,
                    Adjustments = item.Adjustments,
                    Authorized = item.Authorized,
                    Captured = item.Captured,

                    SalesTax = item.SalesTax,
                    Shipping = item.Shipping,
                    ProductSubtotal = item.ProductSubtotal,

                };
                combinedList.Add(day);
            }

            // due to the way the data is prepared, each list should have exactly the same set of days
            // which should make combining fairly simple

            foreach (var storekey in new StoreKeys[] { StoreKeys.InsideAvenue, StoreKeys.InsideWallpaper, StoreKeys.InsideRugs })
            {
                var dicStore = AppSvc.Current.GetSalesSummaryData(storekey).ToDictionary(k => k.Date, v => v);
                foreach (var day in combinedList)
                {
                    SalesSummaryMetric m;
                    if (dicStore.TryGetValue(day.Date, out m))
                    {
                        day.OrderCount += m.OrderCount;
                        day.TotalSales += m.TotalSales;

                        day.NetSales += m.NetSales;
                        day.Voided += m.Voided;
                        day.Refunded += m.Refunded;
                        day.Adjustments += m.Adjustments;
                        day.Authorized += m.Authorized;
                        day.Captured += m.Captured;

                        day.SalesTax += m.SalesTax;
                        day.Shipping += m.Shipping;
                        day.ProductSubtotal += m.ProductSubtotal;
                    }

                }
            }


            var colTimebarData = new ObservableCollection<SalesSummaryMetric>(combinedList);
            TimebarItemsSource = colTimebarData;
            InitializeTimebarRanges();
            isInitializing = false;
            Recalculate();
        }


        private void SetDesignTimeData()
        {
            isInitializing = true;

            var pieChartData = new PieChartItem[]
            {
                new PieChartItem("Net Sales", 85),
                new PieChartItem("Voided", 7),
                new PieChartItem("Refunded", 3),
                new PieChartItem("Adjustments", 5),
            };

            var colPieChart = new ObservableCollection<PieChartItem>(pieChartData);
            PieChartItemsSource = colPieChart;


            var metricsData = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("ORDER COUNT", "678"),
                //new KeyValuePair<string, string>("Total Sales", "$133,000.00"),
                new KeyValuePair<string, string>("VOIDED", "$2,513.13"),
                new KeyValuePair<string, string>("REFUNDED", "$1,034.40"),
                new KeyValuePair<string, string>("ADJUSTMENTS", "$423.20"),
                new KeyValuePair<string, string>("AUTHORIZED", "19,200.33"),
                new KeyValuePair<string, string>("CAPTURED", "$17,304.98"),
                new KeyValuePair<string, string>("NET SALES TAX", "$5,011.10"),
                new KeyValuePair<string, string>("NET SHIPPING", "$1,763.15"),
                new KeyValuePair<string, string>("NET PRODUCT", "$40,090.30"),
            };

            var colMetricsData = new ObservableCollection<KeyValuePair<string, string>>(metricsData);
            MetricsItemsSource = colMetricsData;


            TotalSales = 133000.90M.ToString("C");
            NetSales = 123908.88M.ToString("C");
            OrderCount = 678.ToString("N0");
            VoidedSales = 4329.34M.ToString("C");
            RefundedSales = 1038.98M.ToString("C");

            var endDate = DateTime.Now.AddDays(1).Date;

            var rnd = new Random();

            var salesChart = new List<BarChartItem>();

            for (var currentDate = DateTime.Now.AddMonths(-1).Date; currentDate < endDate; currentDate = currentDate.AddDays(1))
            {
                var totalSales = (decimal)(rnd.Next(2000, 10000));

                var day = new BarChartItem()
                {
                    Label = string.Format("{0:d}", currentDate),
                    NetSales = totalSales * .77M,
                    Voided = totalSales * .15M,
                    Refunded = totalSales * .04M,
                    Adjustments = totalSales * .04M,
                };

                salesChart.Add(day);

            }

            var colSalesChart = new ObservableCollection<BarChartItem>(salesChart);
            SalesChartItemsSource = colSalesChart;

            // time bar sparklines

            var timebarData = new List<SalesSummaryMetric>();

            for (var currentDate = DateTime.Now.AddYears(-2).Date; currentDate != endDate; currentDate = currentDate.AddDays(1))
            {
                var totalSales = (decimal)(rnd.Next(2000, 10000));

                var day = new SalesSummaryMetric()
                {
                    Date = currentDate,
                    OrderCount = rnd.Next(10, 50),
                    TotalSales = totalSales,

                    NetSales = totalSales * .77M,
                    Voided = totalSales * .15M,
                    Refunded = totalSales * .04M,
                    Adjustments = totalSales * .02M,
                    Authorized = totalSales * .40M,
                    Captured = totalSales * .60M,

                    SalesTax = totalSales * .08M,
                    Shipping = totalSales * .1M,
                    ProductSubtotal = totalSales * .82M,
                };

                timebarData.Add(day);

            }

            var colTimebarData = new ObservableCollection<SalesSummaryMetric>(timebarData);
            TimebarItemsSource = colTimebarData;

            InitializeTimebarRanges();

            isInitializing = false;

        }

        private void InitializeTimebarRanges()
        {

            try
            {

                if (TimebarItemsSource == null || TimebarItemsSource.Count() == 0)
                {
                    AppService.SetErrorMessage("TimebarItemsSource is empty.");
                    return;
                }

                DateTime endDate = DateTime.Now.AddDays(1).Date;
                DateTime startDate = TimebarItemsSource[0].Date;

                PeriodStart = startDate;
                PeriodEnd = endDate;
                VisiblePeriodStart = endDate.AddMonths(-3).Date;
                VisiblePeriodEnd = endDate.Date;
                SelectedStartDate = endDate.AddMonths(-1).Date;
                SelectedEndDate = endDate.Date;
                MinSelectionRange = TimeSpan.FromDays(1);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }


        private DateTime selectedStartDate;
        public DateTime SelectedStartDate
        {
            get { return this.selectedStartDate; }

            set
            {
                if (this.selectedStartDate != value)
                {
                    this.selectedStartDate = value;
                    RaisePropertyChanged(() => SelectedStartDate);

                    Recalculate();
                }
            }
        }

        private DateTime selectedEndDate;
        public DateTime SelectedEndDate
        {
            get { return this.selectedEndDate; }

            set
            {
                if (this.selectedEndDate != value)
                {
                    this.selectedEndDate = value;
                    RaisePropertyChanged(() => SelectedEndDate);

                    Recalculate();
                }
            }
        }

        private DateTime periodStart;
        public DateTime PeriodStart
        {
            get { return this.periodStart; }

            set
            {
                if (this.periodStart != value)
                {
                    this.periodStart = value;
                    RaisePropertyChanged(() => PeriodStart);
                }
            }
        }

        private DateTime periodEnd;
        public DateTime PeriodEnd
        {
            get { return this.periodEnd; }

            set
            {
                if (this.periodEnd != value)
                {
                    this.periodEnd = value;
                    RaisePropertyChanged(() => PeriodEnd);
                }
            }
        }

        private DateTime visiblePeriodEnd;
        public DateTime VisiblePeriodEnd
        {
            get { return this.visiblePeriodEnd; }

            set
            {
                if (this.visiblePeriodEnd != value)
                {
                    this.visiblePeriodEnd = value;
                    RaisePropertyChanged(() => VisiblePeriodEnd);
                }
            }
        }

        private DateTime visiblePeriodStart;
        public DateTime VisiblePeriodStart
        {
            get { return this.visiblePeriodStart; }

            set
            {
                if (this.visiblePeriodStart != value)
                {
                    this.visiblePeriodStart = value;
                    RaisePropertyChanged(() => VisiblePeriodStart);
                }
            }
        }

        private TimeSpan minSelectionRange;
        public TimeSpan MinSelectionRange
        {
            get { return this.minSelectionRange; }

            set
            {
                if (this.minSelectionRange != value)
                {
                    this.minSelectionRange = value;
                    RaisePropertyChanged(() => MinSelectionRange);
                }
            }
        }


        private ObservableCollection<PieChartItem> pieChartItemsSource;

        public ObservableCollection<PieChartItem> PieChartItemsSource
        {
            get { return pieChartItemsSource; }
            set
            {
                if (pieChartItemsSource != value)
                {
                    pieChartItemsSource = value;
                    RaisePropertyChanged(() => PieChartItemsSource);
                }
            }
        }


        private ObservableCollection<KeyValuePair<string, string>> metricsItemsSource;

        public ObservableCollection<KeyValuePair<string, string>> MetricsItemsSource
        {
            get { return metricsItemsSource; }
            set
            {
                if (metricsItemsSource != value)
                {
                    metricsItemsSource = value;
                    RaisePropertyChanged(() => MetricsItemsSource);
                }
            }
        }



        private ObservableCollection<BarChartItem> salesChartItemsSource;

        public ObservableCollection<BarChartItem> SalesChartItemsSource
        {
            get { return salesChartItemsSource; }
            set
            {
                if (salesChartItemsSource != value)
                {
                    salesChartItemsSource = value;
                    RaisePropertyChanged(() => SalesChartItemsSource);
                }
            }
        }


        private ObservableCollection<SalesSummaryMetric> timebarItemsSource;

        public ObservableCollection<SalesSummaryMetric> TimebarItemsSource
        {
            get { return timebarItemsSource; }
            set
            {
                if (timebarItemsSource != value)
                {
                    timebarItemsSource = value;
                    RaisePropertyChanged(() => TimebarItemsSource);
                }
            }
        }

        private string totalSales;

        public string TotalSales
        {
            get { return totalSales; }
            set
            {
                if (totalSales != value)
                {
                    totalSales = value;
                    RaisePropertyChanged(() => TotalSales);
                }
            }
        }

        private string netSales;

        public string NetSales
        {
            get { return netSales; }
            set
            {
                if (netSales != value)
                {
                    netSales = value;
                    RaisePropertyChanged(() => NetSales);
                }
            }
        }

        private string orderCount;

        public string OrderCount
        {
            get { return orderCount; }
            set
            {
                if (orderCount != value)
                {
                    orderCount = value;
                    RaisePropertyChanged(() => OrderCount);
                }
            }
        }

        private string voidedSales;

        public string VoidedSales
        {
            get { return voidedSales; }
            set
            {
                if (voidedSales != value)
                {
                    voidedSales = value;
                    RaisePropertyChanged(() => VoidedSales);
                }
            }
        }

        private string refundedSales;

        public string RefundedSales
        {
            get { return refundedSales; }
            set
            {
                if (refundedSales != value)
                {
                    refundedSales = value;
                    RaisePropertyChanged(() => RefundedSales);
                }
            }
        }

        


        public void OnNavigatedTo(UXViewPage page, NavigationEventArgs e)
        {
            try
            {
                // jumpstart any data fetch here...
                LoadSalesSummaryData(); // resets is initializing and recalculates
            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

    }
}
