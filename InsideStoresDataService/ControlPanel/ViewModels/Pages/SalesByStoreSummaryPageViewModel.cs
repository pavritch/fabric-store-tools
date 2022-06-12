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

    // Yellow #FFF7EF1D

    public class SalesByStoreSummaryPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {

        public class BarChartItem
        {
            public string Label { get; set; }
            public DateTime Date { get; set; }
            public int OrderCount { get; set; }
            public decimal InsideFabric { get; set; }
            public decimal InsideRugs { get; set; }
            public decimal InsideWallpaper { get; set; }
            public decimal InsideAvenue { get; set; }

            public BarChartItem()
            {

            }

            public decimal Total
            {
                get
                {
                    return InsideFabric + InsideRugs + InsideWallpaper + InsideAvenue;
                }
            }

            public bool IsAllZeros
            {
                get
                {
                    return InsideFabric == 0M && InsideRugs == 0M && InsideWallpaper == 0M && InsideAvenue == 0M;
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

        public SalesByStoreSummaryPageViewModel()
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
                decimal insideFabric = 0M;
                decimal insideRugs = 0M;
                decimal insideWallpaper = 0M;
                decimal insideAvenue = 0M;

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
                    totalSales += item.Total;
                    insideFabric += item.InsideFabric;
                    insideRugs += item.InsideRugs;
                    insideWallpaper += item.InsideWallpaper;
                    insideAvenue += item.InsideAvenue;
                    lastIndex++;
                }

                // make so this last index is inclusive
                lastIndex--;

                TotalSales = totalSales.ToString("C");
                OrderCount = orderCount.ToString("N0");

                var metricsData = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("INSIDE FABRIC", insideFabric.ToString("C")),
                new KeyValuePair<string, string>("INSIDE RUGS".ToUpper(), insideRugs.ToString("C")),
                new KeyValuePair<string, string>("INSIDE WALLPAPER".ToUpper(), insideWallpaper.ToString("C")),
                new KeyValuePair<string, string>("INSIDE AVENUE".ToUpper(), insideAvenue.ToString("C")),
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

                int pieInsideRugsPct = calcPctOfSales(insideRugs);
                int pieInsideWallpaperPct = calcPctOfSales(insideWallpaper);
                int pieInsideAvenuePct = calcPctOfSales(insideAvenue);
                int pieInsideFabricPct = 100 - (pieInsideRugsPct + pieInsideWallpaperPct + pieInsideAvenuePct);

                var pieChartData = new List<PieChartItem>
            {
                new PieChartItem("Inside Fabric", pieInsideFabricPct),
                new PieChartItem("Inside Rugs", pieInsideRugsPct),
                new PieChartItem("Inside Wallpaper", pieInsideWallpaperPct),
                new PieChartItem("Inside Avenue", pieInsideAvenuePct)
            };

                var colPieChart = new ObservableCollection<PieChartItem>(pieChartData);
                PieChartItemsSource = colPieChart;

                // bar chart

                // show different number of bars and date labels based on the duration of the selected period

                var salesChart = new List<BarChartItem>();

                var currentDate = startDate;

                string barChartLabel = null;
                decimal barChartInsideFabric = 0M;
                decimal barChartInsideRugs = 0M;
                decimal barChartInsideWallpaper = 0M;
                decimal barChartInsideAvenue = 0M;

                Action insertChartItem = () =>
                {
                    var itm = new BarChartItem()
                    {
                        Label = barChartLabel,
                        InsideFabric = barChartInsideFabric,
                        InsideRugs = barChartInsideRugs,
                        InsideWallpaper = barChartInsideWallpaper,
                        InsideAvenue = barChartInsideAvenue,
                    };

                    salesChart.Add(itm);

                    barChartLabel = null;
                    barChartInsideFabric = 0M;
                    barChartInsideRugs = 0M;
                    barChartInsideWallpaper = 0M;
                    barChartInsideAvenue = 0M;
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
                            InsideFabric = item.InsideFabric,
                            InsideRugs = item.InsideRugs,
                            InsideWallpaper = item.InsideWallpaper,
                            InsideAvenue = item.InsideAvenue,
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
                        barChartInsideFabric += item.InsideFabric;
                        barChartInsideRugs += item.InsideRugs;
                        barChartInsideWallpaper += item.InsideWallpaper;
                        barChartInsideAvenue += item.InsideAvenue;

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
                        barChartInsideFabric += item.InsideFabric;
                        barChartInsideRugs += item.InsideRugs;
                        barChartInsideWallpaper += item.InsideWallpaper;
                        barChartInsideAvenue += item.InsideAvenue;

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

            var combinedList = new List<BarChartItem>();

            foreach (var item in AppSvc.Current.GetSalesSummaryData(StoreKeys.InsideFabric))
            {
                var day = new BarChartItem()
                {
                    Date = item.Date,
                    OrderCount = item.OrderCount,
                    InsideFabric = item.TotalSales,
                };
                combinedList.Add(day);
            }

            // due to the way the data is prepared, each list should have exactly the same set of days
            // which should make combining fairly simple

            var dicInsideRugs = AppSvc.Current.GetSalesSummaryData(StoreKeys.InsideRugs).ToDictionary(k => k.Date, v => v);
            var dicInsideWallpaper = AppSvc.Current.GetSalesSummaryData(StoreKeys.InsideWallpaper).ToDictionary(k => k.Date, v => v);
            var dicInsideAvenue = AppSvc.Current.GetSalesSummaryData(StoreKeys.InsideAvenue).ToDictionary(k => k.Date, v => v);

            foreach (var day in combinedList)
            {
                SalesSummaryMetric m;
                if (dicInsideRugs.TryGetValue(day.Date, out m))
                {
                    day.OrderCount += m.OrderCount;
                    day.InsideRugs = m.TotalSales;
                }

                if (dicInsideWallpaper.TryGetValue(day.Date, out m))
                {
                    day.OrderCount += m.OrderCount;
                    day.InsideWallpaper = m.TotalSales;
                }

                if (dicInsideAvenue.TryGetValue(day.Date, out m))
                {
                    day.OrderCount += m.OrderCount;
                    day.InsideAvenue = m.TotalSales;
                }
            }

            var colTimebarData = new ObservableCollection<BarChartItem>(combinedList);
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
                new PieChartItem("INSIDE FABRIC", 85),
                new PieChartItem("INSIDE RUGS", 7),
                new PieChartItem("INSIDE WALLPAPER", 3),
                new PieChartItem("INSIDE AVENUE", 5),
            };

            var colPieChart = new ObservableCollection<PieChartItem>(pieChartData);
            PieChartItemsSource = colPieChart;


            var metricsData = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("INSIDE FABRIC", "$2,513.13"),
                new KeyValuePair<string, string>("INSIDE RUGS", "$1,034.40"),
                new KeyValuePair<string, string>("INSIDE WALLPAPER", "$423.20"),
                new KeyValuePair<string, string>("INSIDE AVENUE", "19,200.33"),
            };

            var colMetricsData = new ObservableCollection<KeyValuePair<string, string>>(metricsData);
            MetricsItemsSource = colMetricsData;


            TotalSales = 133000.90M.ToString("C");
            OrderCount = 678.ToString("N0");

            var endDate = DateTime.Now.AddDays(1).Date;

            var rnd = new Random();

            var salesChart = new List<BarChartItem>();

            for (var currentDate = DateTime.Now.AddMonths(-1).Date; currentDate < endDate; currentDate = currentDate.AddDays(1))
            {
                var totalSales = (decimal)(rnd.Next(2000, 10000));

                var day = new BarChartItem()
                {
                    Label = string.Format("{0:d}", currentDate),
                    InsideFabric = totalSales * .77M,
                    InsideRugs = totalSales * .15M,
                    InsideWallpaper = totalSales * .04M,
                    InsideAvenue = totalSales * .04M,
                };

                salesChart.Add(day);

            }

            var colSalesChart = new ObservableCollection<BarChartItem>(salesChart);
            SalesChartItemsSource = colSalesChart;

            // time bar sparklines

            var timebarData = new List<BarChartItem>();

            for (var currentDate = DateTime.Now.AddYears(-2).Date; currentDate != endDate; currentDate = currentDate.AddDays(1))
            {
                var totalSales = (decimal)(rnd.Next(2000, 10000));

                var day = new BarChartItem()
                {
                    Date = currentDate,
                    OrderCount = rnd.Next(10, 50),
                    InsideFabric = totalSales * .5M,
                    InsideRugs = totalSales * .1M,
                    InsideWallpaper = totalSales * .1M,
                    InsideAvenue = totalSales * .3M,
                };

                timebarData.Add(day);

            }

            var colTimebarData = new ObservableCollection<BarChartItem>(timebarData);
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


        private ObservableCollection<BarChartItem> timebarItemsSource;

        public ObservableCollection<BarChartItem> TimebarItemsSource
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
