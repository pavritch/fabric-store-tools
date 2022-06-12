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


namespace ControlPanel.ViewModels
{


    public class PageViewsPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {
        public class BarChartDataPoint
        {
            public string Category { get; set; }
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public int Value3 { get; set; }
            public int Value4 { get; set; }
            public int Value5 { get; set; }
            public int Value6 { get; set; }

            public BarChartDataPoint()
            {

            }

            public BarChartDataPoint(string category, int value1 = 0, int value2 = 0, int value3 = 0, int value4 = 0, int value5 = 0, int value6 = 0)
            {
                this.Category = category;
                this.Value1 = value1;
                this.Value2 = value2;
                this.Value3 = value3;
                this.Value4 = value4;
                this.Value5 = value5;
                this.Value6 = value6;
            }
        }

        public class ChartLegendItem
        {
            public Brush FillColor { get; set; }
            public string Name { get; set; }

            public ChartLegendItem(string name, string fill)
            {
                this.Name = name;
                this.FillColor = new SolidColorBrush(fill.ToColor());
            }
        }


        ControlPanelDomainContext ctx;
        private DispatcherTimer timerRequestsPerSecond;

        private TimeSpan[] modeTimerIntervals = new TimeSpan[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1) };
        private DispatcherTimer timerRequestsBarChart;


        public PageViewsPageViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
            }
            else
            {
                SetDesignTimeData();
            }
            RequestsPerSecondMeterMaxValue = 25;
            RequestsChartGapLength = .3;
            RequestsChartVerticalAxisMaxValue = Double.PositiveInfinity;
            InitChartLegend();

        }


        private void InitChartLegend()
        {
            var data = new List<ChartLegendItem>()
            {
                new ChartLegendItem("Home Page", "#FF8EC441"),
                new ChartLegendItem("Manufacturer", "#FF1B9DDE"),
                new ChartLegendItem("Category", "#FFF59700"),
                new ChartLegendItem("Product", "#FFF7EF1D"),
                new ChartLegendItem("Other", "#FFc55e8b"),
                new ChartLegendItem("Bot", "#20808080"),
            };

            ChartLegendItemsSource = data;
        }

        private void KillRequestsBarChartMonitor()
        {
            if (timerRequestsBarChart != null)
            {
                timerRequestsBarChart.Stop();
                timerRequestsBarChart.Tick -= RequestsBarChartMonitor_Tick;
                timerRequestsBarChart = null;
            }
        }


        private void InitRequestsBarChartMonitor()
        {

            if (ISControl.IsInDesignModeStatic)
                return;

            KillRequestsBarChartMonitor();
            timerRequestsBarChart = new DispatcherTimer();
            timerRequestsBarChart.Interval = modeTimerIntervals[(int)RequestsChartTimelineSeries];
            timerRequestsBarChart.Tick += RequestsBarChartMonitor_Tick;
            timerRequestsBarChart.Start();
        }


        void RequestsBarChartMonitor_Tick(object sender, EventArgs e)
        {
           FetchRequestsBarChartData();
        }

        private IEnumerable<BarChartDataPoint> MakeRequestsBarChartDataSeries(IEnumerable<PageViewStats> rawData)
        {
            var data = rawData.ToList();
            var points = new List<BarChartDataPoint>();

            int x = (data.Count() - 1) * -1;

            foreach (var y in data)
                points.Add(new BarChartDataPoint((-1 * x++).ToString(), y.Home, y.Manufacturer, y.Category, y.Product, y.Other, y.Bot));

            return points;
        }


        private void FetchRequestsBarChartData()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.PageViewsChartData(StoreKey, RequestsChartTimelineSeries, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        RequestsChartItemsSource = null;
                        return;
                    }

                    var dataSeries = MakeRequestsBarChartDataSeries(result.Value);

                    RequestsChartItemsSource = null;

                    bool bForce10 = true;

                    foreach (var m in dataSeries)
                    {
                        var count = m.Value1 + m.Value2 + m.Value3 + m.Value4 + m.Value5 + m.Value6;
                        if (count > 10)
                        {
                            bForce10 = false;
                            break;
                        }
                    }

                    
                    if (bForce10)
                        RequestsChartVerticalAxisMaxValue = 10;
                    else
                        RequestsChartVerticalAxisMaxValue = Double.PositiveInfinity;

                    RequestsChartItemsSource = dataSeries;

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }


        private void KillRequestsPerSecondMonitor()
        {
            if (timerRequestsPerSecond != null)
            {
                timerRequestsPerSecond.Stop();
                timerRequestsPerSecond.Tick -= RequestsPerSecondMonitor_Tick;
                timerRequestsPerSecond = null;
            }
        }

        private void InitRequestsPerSecondMonitor()
        {
            if (ISControl.IsInDesignModeStatic)
                return;

            ResponseTimeAvg = "N/A";
            ResponseTimeMedian = "N/A";
            ResponseTimeHigh = "N/A";
            ResponseTimeLow = "N/A";

            KillRequestsPerSecondMonitor();
            timerRequestsPerSecond = new DispatcherTimer();
            timerRequestsPerSecond.Interval = TimeSpan.FromSeconds(3);
            timerRequestsPerSecond.Tick += RequestsPerSecondMonitor_Tick;
            timerRequestsPerSecond.Start();
        }

        private static double lastMeterMaxValue = 0;
        void RequestsPerSecondMonitor_Tick(object sender, EventArgs e)
        {
            Action<int> adjustFor = (v) =>
            {
                if (v == lastMeterMaxValue)
                    return;

                RequestsPerSecond = 0;
                lastMeterMaxValue = v;
                RequestsPerSecondMeterMaxValue = v;
            };
    

            if (ctx == null)
                return;

            ctx.PageViewsPerSecond(StoreKey, (result) =>
            {
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    RequestsPerSecond = 0;
                    return;
                }

                var metrics = result.Value;

                var requestsCount = metrics.RequestCount;

                if (requestsCount <= 25)
                    adjustFor(25);
                else if (requestsCount <=50)
                    adjustFor(50);
                else if (requestsCount <= 100)
                    adjustFor(100);
                else if (requestsCount <= 500)
                    adjustFor(500);
                else if (requestsCount <= 1000)
                    adjustFor(1000);
                else if (requestsCount <= 10000)
                    adjustFor(10000);
                else
                    adjustFor(25000);

                RequestsPerSecond = requestsCount;

                Func<int, string> fmt = (t) =>
                    {
                        return t > 9999 ? ">10s" : string.Format("{0:N0}ms", t);
                    };

                ResponseTimeAvg = fmt(metrics.ResponseTimeAvg);
                ResponseTimeMedian = fmt(metrics.ResponseTimeMedian);
                ResponseTimeHigh = fmt(metrics.ResponseTimeHigh); 
                ResponseTimeLow  = fmt(metrics.ResponseTimeLow);

            }, null);            
        }

        private void SetDesignTimeData()
        {
            ResponseTimeAvg = string.Format("{0:N0}ms", 9087);
            ResponseTimeHigh = string.Format("{0:N0}ms", 8769);
            ResponseTimeLow = string.Format("{0:N0}ms", 42);
            ResponseTimeMedian = string.Format("{0:N0}ms", 5023);

            RequestsChartHorizontalAxisTitle = "Horizontal Title";
            RequestsChartVerticalAxisTitle = "Vertical Title";
            RequestsChartSliderIntervalValue = 1;

            RequestsPerSecond = 5;


            var points = new BarChartDataPoint[]
            {
                new BarChartDataPoint("1", 5, 10, 25, 50, 10, 6),
                new BarChartDataPoint("2", 7, 10, 22, 19, 10, 6),
                new BarChartDataPoint("3", 19, 4, 2, 10, 10, 6),
                new BarChartDataPoint("4", 4, 5, 7, 4, 10, 6),
                new BarChartDataPoint("5", 10, 30,2,4, 10, 6),
                new BarChartDataPoint("6", 5, 3, 5, 10, 10, 6),
                new BarChartDataPoint("7", 0, 10, 10, 10, 10, 6),
                new BarChartDataPoint("8", 2, 5, 3, 12, 10, 6),
                new BarChartDataPoint("9", 7, 9, 10, 33, 10, 6),
                new BarChartDataPoint("10", 9, 30, 10, 30, 10, 6),
                new BarChartDataPoint("11", 5, 10, 25, 50, 10, 6),
                new BarChartDataPoint("12", 7, 10, 22, 19, 10, 6),
                new BarChartDataPoint("13", 19, 4, 2, 10, 10, 6),
                new BarChartDataPoint("14", 4, 5, 7, 4, 10, 6),
                new BarChartDataPoint("15", 10, 30,2,4, 10, 6),
                new BarChartDataPoint("16", 5, 3, 5, 10, 10, 6),
                new BarChartDataPoint("17", 0, 10, 10, 10, 10, 6),
                new BarChartDataPoint("18", 2, 5, 3, 12, 10, 6),
                new BarChartDataPoint("19", 7, 9, 10, 33, 10, 6),
                new BarChartDataPoint("20", 9, 30, 10, 30, 10, 6),
            };

            var col = new ObservableCollection<BarChartDataPoint>(points);
            RequestsChartItemsSource = col;

        }

        static string[] horizontalBarChartTitles = new string[]
        {
            "Seconds Timeline",
            "Minutes Timeline",
            "Hours Timeline",
            "Days Timeline",
        };

        static string[] verticalBarChartTitles = new string[]
        {
            "Requests/Second",
            "Requests/Minute",
            "Requests/Hour",
            "Requests/Day",
        };

        private void OnRequestsChartTimelineSeriesUpdated(TimelineSeries series)
        {
            if ((int)series < 0 || (int)series > 3)
                return;

            RequestsChartVerticalAxisTitle = verticalBarChartTitles[(int)series];
            RequestsChartHorizontalAxisTitle = horizontalBarChartTitles[(int)series];
            InitRequestsBarChartMonitor();
            FetchRequestsBarChartData();
        }


        private double requestsChartVerticalAxisMaxValue;

        public double RequestsChartVerticalAxisMaxValue
        {
            get { return requestsChartVerticalAxisMaxValue; }
            set
            {
                if (requestsChartVerticalAxisMaxValue != value)
                {
                    requestsChartVerticalAxisMaxValue = value;
                    RaisePropertyChanged(() => RequestsChartVerticalAxisMaxValue);
                }
            }
        }



        private Website.StoreKeys storeKey;

        public Website.StoreKeys StoreKey
        {
            get { return storeKey; }
            set
            {
                if (storeKey != value)
                {
                    storeKey = value;
                    RaisePropertyChanged(() => StoreKey);
                }
            }
        }



        private IEnumerable<ChartLegendItem> chartLegendItemsSource;

        public IEnumerable<ChartLegendItem> ChartLegendItemsSource
        {
            get { return chartLegendItemsSource; }
            set
            {
                if (chartLegendItemsSource != value)
                {
                    chartLegendItemsSource = value;
                    RaisePropertyChanged(() => ChartLegendItemsSource);
                }
            }
        }
        
        private double requestsPerSecondMeterMaxValue;

        public double RequestsPerSecondMeterMaxValue
        {
            get { return requestsPerSecondMeterMaxValue; }
            set
            {
                if (requestsPerSecondMeterMaxValue != value)
                {
                    requestsPerSecondMeterMaxValue = value;
                    RaisePropertyChanged(() => RequestsPerSecondMeterMaxValue);
                }
            }
        }

        private string responseTimeMedian;

        public string ResponseTimeMedian
        {
            get { return responseTimeMedian; }
            set
            {
                if (responseTimeMedian != value)
                {
                    responseTimeMedian = value;
                    RaisePropertyChanged(() => ResponseTimeMedian);
                }
            }
        }
        
        private string responseTimeAvg;

        public string ResponseTimeAvg
        {
            get { return responseTimeAvg; }
            set
            {
                if (responseTimeAvg != value)
                {
                    responseTimeAvg = value;
                    RaisePropertyChanged(() => ResponseTimeAvg);
                }
            }
        }

        private string responseTimeHigh;

        public string ResponseTimeHigh
        {
            get { return responseTimeHigh; }
            set
            {
                if (responseTimeHigh != value)
                {
                    responseTimeHigh = value;
                    RaisePropertyChanged(() => ResponseTimeHigh);
                }
            }
        }

        private string responseTimeLow;

        public string ResponseTimeLow
        {
            get { return responseTimeLow; }
            set
            {
                if (responseTimeLow != value)
                {
                    responseTimeLow = value;
                    RaisePropertyChanged(() => ResponseTimeLow);
                }
            }
        }
        
        
        private TimelineSeries requestsChartTimelineSeries;

        public TimelineSeries RequestsChartTimelineSeries
        {
            get { return requestsChartTimelineSeries; }
            set
            {
                if (requestsChartTimelineSeries != value)
                {
                    requestsChartTimelineSeries = value;
                    RaisePropertyChanged(() => RequestsChartTimelineSeries);
                    OnRequestsChartTimelineSeriesUpdated(value);
                }
            }
        }
        
        private  IEnumerable<BarChartDataPoint> requestsChartItemsSource;

        public IEnumerable<BarChartDataPoint> RequestsChartItemsSource
        {
            get { return requestsChartItemsSource; }
            set
            {
                if (requestsChartItemsSource != value)
                {
                    requestsChartItemsSource = value;
                    RaisePropertyChanged(() => RequestsChartItemsSource);
                }
            }
        }


        
        private double requestsChartGapLength;

        public double RequestsChartGapLength
        {
            get { return requestsChartGapLength; }
            set
            {
                if (requestsChartGapLength != value)
                {
                    requestsChartGapLength = value;
                    RaisePropertyChanged(() => RequestsChartGapLength);
                }
            }
        }
        

        /// <summary>
        /// Value in range 1 to 4 (sec, min, hour, day)
        /// </summary>
        private double requestsChartSliderIntervalValue;

        public double RequestsChartSliderIntervalValue
        {
            get { return requestsChartSliderIntervalValue; }
            set
            {
                requestsChartSliderIntervalValue = value;
                RaisePropertyChanged(() => RequestsChartSliderIntervalValue);

                var intValue = (int)value;
                if(value < 0 || value > 3)
                    return;

                RequestsChartTimelineSeries = (TimelineSeries)intValue;
            }
        }

        private string requestsChartVerticalAxisTitle;

        public string RequestsChartVerticalAxisTitle
        {
            get { return requestsChartVerticalAxisTitle; }
            set
            {
                if (requestsChartVerticalAxisTitle != value)
                {
                    requestsChartVerticalAxisTitle = value;
                    RaisePropertyChanged(() => RequestsChartVerticalAxisTitle);
                }
            }
        }

        private string requestsChartHorizontalAxisTitle;

        public string RequestsChartHorizontalAxisTitle
        {
            get { return requestsChartHorizontalAxisTitle; }
            set
            {
                if (requestsChartHorizontalAxisTitle != value)
                {
                    requestsChartHorizontalAxisTitle = value;
                    RaisePropertyChanged(() => RequestsChartHorizontalAxisTitle);
                }
            }
        }
        

        
        private double requestsPerSecond;

        public double RequestsPerSecond
        {
            get { return requestsPerSecond; }
            set
            {
                if (requestsPerSecond != value)
                {
                    requestsPerSecond = value;
                    RaisePropertyChanged(() => RequestsPerSecond);
                }
            }
        }


        #region Command Actions


        #endregion



        public void OnNavigatedTo(UXViewPage page, NavigationEventArgs e)
        {
            try
            {
                var key = (StoreKeys?)e.ExtraData;
                StoreKey = key.Value;

                ctx = new Website.Services.ControlPanelDomainContext();

                RequestsChartVerticalAxisMaxValue = Double.PositiveInfinity;
                InitRequestsPerSecondMonitor();
                InitRequestsBarChartMonitor();

                // changes to bar chart time series MUST be initiated with the slider value,
                // which in turn updates the enum and that updates the chart data
                RequestsChartSliderIntervalValue = (double)TimelineSeries.Minutes;

                OnRequestsChartTimelineSeriesUpdated(RequestsChartTimelineSeries);
            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            KillRequestsPerSecondMonitor();
            KillRequestsBarChartMonitor();
            ctx = null;
        }

    }
}
