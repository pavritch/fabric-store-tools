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

namespace Website
{
    public partial class SqlSearchMetric
    {
        public int DurationMS
        {
            get
            {
                return (int)this.Duration.TotalMilliseconds;
            }
        }

    }
}

namespace ControlPanel.ViewModels
{


    public class SqlSearchMetricsPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {
        public class BarChartDataPoint
        {
            public string Category { get; set; }
            public int Value1 { get; set; }
            public int Value2 { get; set; }

            public BarChartDataPoint()
            {

            }

            public BarChartDataPoint(string category, int value1 = 0, int value2 = 0 )
            {
                this.Category = category;
                this.Value1 = value1;
                this.Value2 = value2;
            }
        }


        ControlPanelDomainContext ctx;

        private TimeSpan[] modeTimerIntervals = new TimeSpan[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1) };
        private DispatcherTimer timerSearchesBarChart;
        private DispatcherTimer timerAvgSearchDuration;
        private DispatcherTimer timerRecentSearches;

        /// <summary>
        /// tracks highest fetched ID, so new timer ticks fetch IDs greater than this
        /// </summary>
        private int highestRecentSearchID = 0;

        public SqlSearchMetricsPageViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
                //RecentSearchesDesignTime();
            }
            else
            {
                SetDesignTimeData();
            }
            SearchesChartGapLength = .3;
            SearchesChartVerticalAxisMaxValue = Double.PositiveInfinity;
        }

        private void SetCacheHitRatio(double ratio)
        {
            CacheHitRatioText = string.Format("{0:N0}%", ratio);
            CacheHitRatioMeterValue = ratio;
        }

        private void SetAvgSearchDuration(double milliseconds)
        {
            AvgSearchDuration = string.Format("{0:N1}ms", milliseconds);
        }


        private void KillAvgSearchDurationMonitor()
        {
            if (timerAvgSearchDuration != null)
            {
                timerAvgSearchDuration.Stop();
                timerAvgSearchDuration.Tick -= AvgSearchDurationMonitor_Tick;
                timerAvgSearchDuration = null;
            }
        }

        private void KillSearchesBarChartMonitor()
        {
            if (timerSearchesBarChart != null)
            {
                timerSearchesBarChart.Stop();
                timerSearchesBarChart.Tick -= SearchesBarChartMonitor_Tick;
                timerSearchesBarChart = null;
            }
        }

        private void KillRecentSearchesMonitor()
        {
            if (timerRecentSearches != null)
            {
                timerRecentSearches.Stop();
                timerRecentSearches.Tick -= RecentSearchesMonitor_Tick;
                timerRecentSearches = null;
            }
        }


        private void InitAvgSearchDurationMonitor()
        {

            if (ISControl.IsInDesignModeStatic)
                return;

            KillAvgSearchDurationMonitor();
            timerAvgSearchDuration = new DispatcherTimer();
            timerAvgSearchDuration.Interval = TimeSpan.FromSeconds(30);
            timerAvgSearchDuration.Tick += AvgSearchDurationMonitor_Tick;
            timerAvgSearchDuration.Start();
        }


        private void InitSearchesBarChartMonitor()
        {

            if (ISControl.IsInDesignModeStatic)
                return;

            KillSearchesBarChartMonitor();
            timerSearchesBarChart = new DispatcherTimer();
            timerSearchesBarChart.Interval = modeTimerIntervals[(int)SearchesChartTimelineSeries];
            timerSearchesBarChart.Tick += SearchesBarChartMonitor_Tick;
            timerSearchesBarChart.Start();
        }


        private void InitRecentSearchesMonitor()
        {

            if (ISControl.IsInDesignModeStatic)
                return;

            KillRecentSearchesMonitor();
            timerRecentSearches = new DispatcherTimer();
            timerRecentSearches.Interval = TimeSpan.FromSeconds(1);
            timerRecentSearches.Tick += RecentSearchesMonitor_Tick;
            timerRecentSearches.Start();
        }

        void AvgSearchDurationMonitor_Tick(object sender, EventArgs e)
        {
            FetchAvgSearchDuration();
        }


        void RecentSearchesMonitor_Tick(object sender, EventArgs e)
        {
            FetchRecentSearches();
        }


        void SearchesBarChartMonitor_Tick(object sender, EventArgs e)
        {
            FetchSearchBarChartData();
            FetchCacheHitRatioData();
        }

        private IEnumerable<BarChartDataPoint> MakeSearchBarChartDataSeries(IEnumerable<SearchMetric> rawData)
        {
            var data = rawData.ToList();
            var points = new List<BarChartDataPoint>();

            int x = (data.Count() - 1) * -1;

            foreach (var point in data)
                points.Add(new BarChartDataPoint((-1 * x++).ToString(), point.SimpleCount, point.AdvancedCount));

            return points;
        }

        private void FetchRecentSearches()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.GetSqlSearchMetrics(StoreKey, highestRecentSearchID, 1, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        return;
                    }

                    foreach (var item in result.Value)
                    {
                        if (item.ID > highestRecentSearchID)
                            highestRecentSearchID = item.ID;

                        if (RecentSearchesItemsSource.Count >= 100)
                            RecentSearchesItemsSource.RemoveAt(RecentSearchesItemsSource.Count - 1);

                        RecentSearchesItemsSource.Insert(0, item);
                    }

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }


        private void InitialFetchRecentSearches()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.GetLastSqlSearchMetrics(StoreKey, 100, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        return;
                    }
                    
                    // prime the highest known ID so the timer tick method will pick up 
                    // from there

                    if (result.Value.Count() > 0)
                        highestRecentSearchID = result.Value[0].ID;

                    RecentSearchesItemsSource = new ObservableCollection<SqlSearchMetric>(result.Value);

                    InitRecentSearchesMonitor();

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }


        private void FetchSearchBarChartData()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.TotalSearchesChartData(StoreKey, SearchesChartTimelineSeries, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);

                        return;
                    }

                    SearchesChartItemsSource = null;

                    var dataSeries = MakeSearchBarChartDataSeries(result.Value);

                    if (dataSeries.All(e => (e.Value1 + e.Value2) < 10))
                        SearchesChartVerticalAxisMaxValue = 10;
                    else
                        SearchesChartVerticalAxisMaxValue = Double.PositiveInfinity;            

                    SearchesChartItemsSource = dataSeries;

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        private void FetchCacheHitRatioData()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.SqlSearchCacheHitRatio(StoreKey, SearchesChartTimelineSeries, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        SetCacheHitRatio(0.0);
                        return;
                    }

                    SetCacheHitRatio(result.Value);

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        private void FetchAvgSearchDuration()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.SqlSearchAvgDuration(StoreKey, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        SetAvgSearchDuration(0.0);
                        return;
                    }

                    SetAvgSearchDuration(result.Value);

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }


        private void SetDesignTimeData()
        {
            SearchesChartHorizontalAxisTitle = "Horizontal Title";
            SearchesChartVerticalAxisTitle = "Vertical Title";
            SearchesChartSliderIntervalValue = 1;
            SetAvgSearchDuration(119.4);
            SetCacheHitRatio(57.3);

            var searchPoints = new BarChartDataPoint[]
            {
                new BarChartDataPoint("11", 5, 100),
                new BarChartDataPoint("10", 7, 40),
                new BarChartDataPoint("9", 19, 40),
                new BarChartDataPoint("8", 4, 55),
                new BarChartDataPoint("7", 10, 4),
                new BarChartDataPoint("6", 5, 30),
                new BarChartDataPoint("5", 4, 80),
                new BarChartDataPoint("4", 9, 18),
                new BarChartDataPoint("3", 19, 44),
                new BarChartDataPoint("2", 39, 30),
                new BarChartDataPoint("1", 30, 8),
                new BarChartDataPoint("0", 5, 18),
            };


            if (searchPoints.All(e => (e.Value1 + e.Value2) < 10))
                SearchesChartVerticalAxisMaxValue = 10;
            else
                SearchesChartVerticalAxisMaxValue = Double.PositiveInfinity;            
            
            var col2 = new ObservableCollection<BarChartDataPoint>(searchPoints);
            SearchesChartItemsSource = col2;

            RecentSearchesDesignTime();

        }

        private void RecentSearchesDesignTime()
        {
            var recent = new SqlSearchMetric[]
            {
                new SqlSearchMetric
                {
                    ID=1,
                    SearchPhrase = "birds and bees",
                    Duration = TimeSpan.FromMilliseconds(20),
                    Time = DateTime.Now,
                    ResultCount = 300,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=2,
                    SearchPhrase = "pink littlel birds",
                    Duration = TimeSpan.FromMilliseconds(200),
                    Time = DateTime.Now,
                    ResultCount = 12020,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=3,
                    SearchPhrase = "tiny big puppies with wings",
                    Duration = TimeSpan.FromMilliseconds(119),
                    Time = DateTime.Now,
                    ResultCount = 7,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=4,
                    SearchPhrase = "red silk flowers",
                    Duration = TimeSpan.FromMilliseconds(28),
                    Time = DateTime.Now,
                    ResultCount = 1120,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=5,
                    SearchPhrase = "blue satin drapes",
                    Duration = TimeSpan.FromMilliseconds(90),
                    Time = DateTime.Now,
                    ResultCount = 17,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=6,
                    SearchPhrase = "white silk sheets",
                    Duration = TimeSpan.FromMilliseconds(423),
                    Time = DateTime.Now,
                    ResultCount = 158,
                    IsAdvancedSearch = false,
                },

                new SqlSearchMetric
                {
                    ID=7,
                    SearchPhrase = "birds and bees",
                    Duration = TimeSpan.FromMilliseconds(20),
                    Time = DateTime.Now,
                    ResultCount = 300,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=8,
                    SearchPhrase = "pink littlel birds",
                    Duration = TimeSpan.FromMilliseconds(200),
                    Time = DateTime.Now,
                    ResultCount = 12020,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=9,
                    SearchPhrase = "tiny big puppies with wings",
                    Duration = TimeSpan.FromMilliseconds(119),
                    Time = DateTime.Now,
                    ResultCount = 7,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=10,
                    SearchPhrase = "red silk flowers",
                    Duration = TimeSpan.FromMilliseconds(28),
                    Time = DateTime.Now,
                    ResultCount = 1120,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=11,
                    SearchPhrase = "blue satin drapes",
                    Duration = TimeSpan.FromMilliseconds(90),
                    Time = DateTime.Now,
                    ResultCount = 17,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=12,
                    SearchPhrase = "white silk sheets",
                    Duration = TimeSpan.FromMilliseconds(423),
                    Time = DateTime.Now,
                    ResultCount = 158,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=1,
                    SearchPhrase = "birds and bees",
                    Duration = TimeSpan.FromMilliseconds(20),
                    Time = DateTime.Now,
                    ResultCount = 300,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=2,
                    SearchPhrase = "pink littlel birds",
                    Duration = TimeSpan.FromMilliseconds(200),
                    Time = DateTime.Now,
                    ResultCount = 12020,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=3,
                    SearchPhrase = "tiny big puppies with wings",
                    Duration = TimeSpan.FromMilliseconds(119),
                    Time = DateTime.Now,
                    ResultCount = 7,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=4,
                    SearchPhrase = "red silk flowers",
                    Duration = TimeSpan.FromMilliseconds(28),
                    Time = DateTime.Now,
                    ResultCount = 1120,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=5,
                    SearchPhrase = "blue satin drapes",
                    Duration = TimeSpan.FromMilliseconds(90),
                    Time = DateTime.Now,
                    ResultCount = 17,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=6,
                    SearchPhrase = "white silk sheets",
                    Duration = TimeSpan.FromMilliseconds(423),
                    Time = DateTime.Now,
                    ResultCount = 158,
                    IsAdvancedSearch = false,
                },

                new SqlSearchMetric
                {
                    ID=7,
                    SearchPhrase = "birds and bees",
                    Duration = TimeSpan.FromMilliseconds(20),
                    Time = DateTime.Now,
                    ResultCount = 300,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=8,
                    SearchPhrase = "pink littlel birds",
                    Duration = TimeSpan.FromMilliseconds(200),
                    Time = DateTime.Now,
                    ResultCount = 12020,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=9,
                    SearchPhrase = "tiny big puppies with wings",
                    Duration = TimeSpan.FromMilliseconds(119),
                    Time = DateTime.Now,
                    ResultCount = 7,
                    IsAdvancedSearch = false,
                },
                new SqlSearchMetric
                {
                    ID=10,
                    SearchPhrase = "red silk flowers",
                    Duration = TimeSpan.FromMilliseconds(28),
                    Time = DateTime.Now,
                    ResultCount = 1120,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=11,
                    SearchPhrase = "blue satin drapes",
                    Duration = TimeSpan.FromMilliseconds(90),
                    Time = DateTime.Now,
                    ResultCount = 17,
                    IsAdvancedSearch = true,
                },
                new SqlSearchMetric
                {
                    ID=12,
                    SearchPhrase = "white silk sheets",
                    Duration = TimeSpan.FromMilliseconds(423),
                    Time = DateTime.Now,
                    ResultCount = 158,
                    IsAdvancedSearch = false,
                },
            };

            var col3 = new ObservableCollection<SqlSearchMetric>(recent);
            RecentSearchesItemsSource = col3;
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
            "Searches/Second",
            "Searches/Minute",
            "Searches/Hour",
            "Searches/Day",
        };

        private void OnRequestsChartTimelineSeriesUpdated(TimelineSeries series)
        {
            if ((int)series < 0 || (int)series > 3)
                return;

            SearchesChartVerticalAxisTitle = verticalBarChartTitles[(int)series];
            SearchesChartHorizontalAxisTitle = horizontalBarChartTitles[(int)series];
            InitSearchesBarChartMonitor();
            FetchSearchBarChartData();
            FetchCacheHitRatioData();
        }

        private double searchesChartVerticalAxisMaxValue;

        public double SearchesChartVerticalAxisMaxValue
        {
            get { return searchesChartVerticalAxisMaxValue; }
            set
            {
                if (searchesChartVerticalAxisMaxValue != value)
                {
                    searchesChartVerticalAxisMaxValue = value;
                    RaisePropertyChanged(() => SearchesChartVerticalAxisMaxValue);
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

        private ObservableCollection<SqlSearchMetric> recentSearchesItemsSource;

        public ObservableCollection<SqlSearchMetric> RecentSearchesItemsSource
        {
            get { return recentSearchesItemsSource; }
            set
            {
                if (recentSearchesItemsSource != value)
                {
                    recentSearchesItemsSource = value;
                    RaisePropertyChanged(() => RecentSearchesItemsSource);
                }
            }
        }
        
        private double cacheHitRatioMeterValue;

        public double CacheHitRatioMeterValue
        {
            get { return cacheHitRatioMeterValue; }
            set
            {
                if (cacheHitRatioMeterValue != value)
                {
                    cacheHitRatioMeterValue = value;
                    RaisePropertyChanged(() => CacheHitRatioMeterValue);
                }
            }
        }

        private string cacheHitRatioText;

        public string CacheHitRatioText
        {
            get { return cacheHitRatioText; }
            set
            {
                if (cacheHitRatioText != value)
                {
                    cacheHitRatioText = value;
                    RaisePropertyChanged(() => CacheHitRatioText);
                }
            }
        }

        private string avgSearchDuration;

        public string AvgSearchDuration
        {
            get { return avgSearchDuration; }
            set
            {
                if (avgSearchDuration != value)
                {
                    avgSearchDuration = value;
                    RaisePropertyChanged(() => AvgSearchDuration);
                }
            }
        }
        

        private TimelineSeries searchesChartTimelineSeries;

        public TimelineSeries SearchesChartTimelineSeries
        {
            get { return searchesChartTimelineSeries; }
            set
            {
                if (searchesChartTimelineSeries != value)
                {
                    searchesChartTimelineSeries = value;
                    RaisePropertyChanged(() => SearchesChartTimelineSeries);
                    OnRequestsChartTimelineSeriesUpdated(value);
                }
            }
        }

        private IEnumerable<BarChartDataPoint> searchesChartItemsSource;

        public IEnumerable<BarChartDataPoint> SearchesChartItemsSource
        {
            get { return searchesChartItemsSource; }
            set
            {
                if (searchesChartItemsSource != value)
                {
                    searchesChartItemsSource = value;
                    RaisePropertyChanged(() => SearchesChartItemsSource);
                }
            }
        }


        private double searchesChartGapLength;

        public double SearchesChartGapLength
        {
            get { return searchesChartGapLength; }
            set
            {
                if (searchesChartGapLength != value)
                {
                    searchesChartGapLength = value;
                    RaisePropertyChanged(() => SearchesChartGapLength);
                }
            }
        }


        /// <summary>
        /// Value in range 1 to 4 (sec, min, hour, day)
        /// </summary>
        private double searchesChartSliderIntervalValue;

        public double SearchesChartSliderIntervalValue
        {
            get { return searchesChartSliderIntervalValue; }
            set
            {
                searchesChartSliderIntervalValue = value;
                RaisePropertyChanged(() => SearchesChartSliderIntervalValue);

                var intValue = (int)value;
                if (value < 0 || value > 3)
                    return;

                SearchesChartTimelineSeries = (TimelineSeries)intValue;
            }
        }

        private string searchesChartVerticalAxisTitle;

        public string SearchesChartVerticalAxisTitle
        {
            get { return searchesChartVerticalAxisTitle; }
            set
            {
                if (searchesChartVerticalAxisTitle != value)
                {
                    searchesChartVerticalAxisTitle = value;
                    RaisePropertyChanged(() => SearchesChartVerticalAxisTitle);
                }
            }
        }

        private string searchesChartHorizontalAxisTitle;

        public string SearchesChartHorizontalAxisTitle
        {
            get { return searchesChartHorizontalAxisTitle; }
            set
            {
                if (searchesChartHorizontalAxisTitle != value)
                {
                    searchesChartHorizontalAxisTitle = value;
                    RaisePropertyChanged(() => SearchesChartHorizontalAxisTitle);
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

                SearchesChartVerticalAxisMaxValue = Double.PositiveInfinity;
                ctx = new Website.Services.ControlPanelDomainContext();

                // changes to bar chart time series MUST be initiated with the slider value,
                // which in turn updates the enum and that updates the chart data
                SearchesChartSliderIntervalValue = (double)TimelineSeries.Minutes;
                InitAvgSearchDurationMonitor();

                AppService.RunActionWithDelay(1000, () =>
                    {
                        OnRequestsChartTimelineSeriesUpdated(SearchesChartTimelineSeries);
                        FetchAvgSearchDuration();
                        InitialFetchRecentSearches();
                    });
            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            KillSearchesBarChartMonitor();
            KillAvgSearchDurationMonitor();
            KillRecentSearchesMonitor();
            ctx = null;
        }

    }
}
