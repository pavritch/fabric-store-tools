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


    public class StoreDashboardPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {
        public class BarChartDataPoint
        {
            public int Value1 { get; set; }
            public string Category { get; set; }

            public int Value2 { get; set; }

            public BarChartDataPoint()
            {

            }

            public BarChartDataPoint(string category, int value1)
            {
                this.Category = category;
                this.Value1 = value1;
            }

            public BarChartDataPoint(string category, int value1, int value2)
            {
                this.Category = category;
                this.Value1 = value1;
                this.Value2 = value2;
            }
        }


        private ControlPanelDomainContext ctx; 

        private DispatcherTimer timerRequestsPerSecond;

        private TimeSpan[] modeTimerIntervals = new TimeSpan[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1) };
        private DispatcherTimer timerRequestsBarChart;
        private DispatcherTimer timerSearchBarChart;


        public StoreDashboardPageViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
                RepopulateCacheCommand = new DelegateCommand(ExecuteRepopulateCacheCommand, CanExecuteRepopulateCacheCommand);
                RebuildSearchExtensionDataCommand = new DelegateCommand(ExecuteRebuildSearchExtensionDataCommand, CanExecuteRebuildSearchExtensionDataCommand);
                RebuildCategoriesTableCommand = new DelegateCommand(ExecuteRebuildCategoriesTableCommand, CanExecuteRebuildCategoriesTableCommand);
            }
            else
            {
                SetDesignTimeData();
            }
            RequestsPerSecondMeterMaxValue = 25;
            RequestsChartGapLength = .3;
            RequestsChartVerticalAxisMaxValue = Double.PositiveInfinity;
            SearchChartVerticalAxisMaxValue = Double.PositiveInfinity;
        }


        private void KillSearchBarChartMonitor()
        {
            if (timerSearchBarChart != null)
            {
                timerSearchBarChart.Stop();
                timerSearchBarChart.Tick -= SearchBarChartMonitor_Tick;
                timerSearchBarChart = null;
            }
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


        private void InitSearchBarChartMonitor()
        {

            if (ISControl.IsInDesignModeStatic)
                return;

            KillSearchBarChartMonitor();
            timerSearchBarChart = new DispatcherTimer();
            timerSearchBarChart.Interval = TimeSpan.FromSeconds(34); // arbitrary
            timerSearchBarChart.Tick += SearchBarChartMonitor_Tick;
            timerSearchBarChart.Start();
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

        void SearchBarChartMonitor_Tick(object sender, EventArgs e)
        {
            FetchSearchBarChartData();
        }


        void RequestsBarChartMonitor_Tick(object sender, EventArgs e)
        {
           FetchRequestsBarChartData();
        }

        private IEnumerable<BarChartDataPoint> MakeRequestsBarChartDataSeries(IEnumerable<int> rawData)
        {
            var data = rawData.ToList();
            var points = new List<BarChartDataPoint>();

            int x = (data.Count() - 1) * -1;

            foreach (var y in data)
                points.Add(new BarChartDataPoint((-1 * x++).ToString(), y));

            return points;
        }


        private IEnumerable<BarChartDataPoint> MakeSearchBarChartDataSeries(IEnumerable<SearchMetric> rawData)
        {
            var data = rawData.ToList();
            var points = new List<BarChartDataPoint>();

            int x = (data.Count() - 1) * -1;

            foreach (var point in data)
                points.Add(new BarChartDataPoint((-1 * x++).ToString(), point.SimpleCount, point.AdvancedCount));

            // for now only showing most recent 12 hrs

            int skipCount = 0;

            if (points.Count() > 12)
                skipCount = points.Count() - 12;

            return points.Skip(skipCount);
        }


        private void FetchSearchBarChartData()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.TotalSearchesChartData(StoreKey, TimelineSeries.Hours, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        SearchChartItemsSource = null;
                        return;
                    }

                    var dataSeries = MakeSearchBarChartDataSeries(result.Value);

                    SearchChartItemsSource = null;

                    if (dataSeries.All(e => (e.Value1 + e.Value2) < 10))
                        SearchChartVerticalAxisMaxValue = 10;
                    else
                        SearchChartVerticalAxisMaxValue = Double.PositiveInfinity;  

                    SearchChartItemsSource = dataSeries;

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        private void FetchRequestsBarChartData()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.TotalRequestsChartData(StoreKey, RequestsChartTimelineSeries, (result) =>
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

                    if (dataSeries.All(e => e.Value1 < 10))
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

            ctx.RequestsPerSecond(StoreKey, (result) =>
            {
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    RequestsPerSecond = 0;
                    return;
                }

                var thisValue = result.Value;

                if (thisValue <= 25)
                    adjustFor(25);
                else if (thisValue <=50)
                    adjustFor(50);
                else if (thisValue <= 100)
                    adjustFor(100);
                else if (thisValue <= 500)
                    adjustFor(500);
                else if (thisValue <= 1000)
                    adjustFor(1000);
                else if (thisValue <= 10000)
                    adjustFor(10000);
                else
                    adjustFor(25000);

                RequestsPerSecond = thisValue;

            }, null);            
        }


        private void SetDesignTimeData()
        {
            RequestsChartHorizontalAxisTitle = "Horizontal Title";
            RequestsChartVerticalAxisTitle = "Vertical Title";
            RequestsChartSliderIntervalValue = 1;

            IsExecutingRepopulateCache = true;
            IsExecutingRebuildSearchExtensionData = true;
            IsExecutingRebuildCategoriesTable = true;
            RequestsPerSecond = 5;


            var points = new BarChartDataPoint[]
            {
                new BarChartDataPoint("1", 5),
                new BarChartDataPoint("2", 7),
                new BarChartDataPoint("3", 19),
                new BarChartDataPoint("4", 4),
                new BarChartDataPoint("5", 10),
                new BarChartDataPoint("6", 5),
                new BarChartDataPoint("7", 0),
                new BarChartDataPoint("8", 0),
                new BarChartDataPoint("9", 0),
                new BarChartDataPoint("10", 0),
                new BarChartDataPoint("11", 0),
                new BarChartDataPoint("12", 0),
                new BarChartDataPoint("13", 19),
                new BarChartDataPoint("14", 4),
                new BarChartDataPoint("15", 10),
                new BarChartDataPoint("16", 5),
                new BarChartDataPoint("17", 7),
                new BarChartDataPoint("18", 19),
                new BarChartDataPoint("19", 4),
                new BarChartDataPoint("20", 10),
                new BarChartDataPoint("21", 5),
                new BarChartDataPoint("22", 7),
                new BarChartDataPoint("23", 19),
                new BarChartDataPoint("24", 4),
                new BarChartDataPoint("25", 10),
                new BarChartDataPoint("26", 5),
                new BarChartDataPoint("27", 7),
                new BarChartDataPoint("28", 19),
                new BarChartDataPoint("29", 4),
                new BarChartDataPoint("30", 4),

                new BarChartDataPoint("31", 5),
                new BarChartDataPoint("32", 7),
                new BarChartDataPoint("33", 19),
                new BarChartDataPoint("34", 4),
                new BarChartDataPoint("35", 10),
                new BarChartDataPoint("36", 5),
                new BarChartDataPoint("37", 7),
                new BarChartDataPoint("38", 19),
                new BarChartDataPoint("39", 4),
                new BarChartDataPoint("40", 10),


                new BarChartDataPoint("41", 5),
                new BarChartDataPoint("42", 7),
                new BarChartDataPoint("43", 19),
                new BarChartDataPoint("44", 4),
                new BarChartDataPoint("45", 10),
                new BarChartDataPoint("46", 5),
                new BarChartDataPoint("47", 7),
                new BarChartDataPoint("48", 19),
                new BarChartDataPoint("49", 4),
                new BarChartDataPoint("50", 10),


                new BarChartDataPoint("51", 5),
                new BarChartDataPoint("52", 7),
                new BarChartDataPoint("53", 19),
                new BarChartDataPoint("54", 4),
                new BarChartDataPoint("55", 10),
                new BarChartDataPoint("56", 5),
                new BarChartDataPoint("57", 7),
                new BarChartDataPoint("58", 19),
                new BarChartDataPoint("59", 4),
                new BarChartDataPoint("60", 10),

            };

            var col = new ObservableCollection<BarChartDataPoint>(points);
            RequestsChartItemsSource = col;


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

            var col2 = new ObservableCollection<BarChartDataPoint>(searchPoints);
            SearchChartItemsSource = col2;


            var info = new Website.WebStoreInformation()
            {
                StoreKey = Website.StoreKeys.InsideFabric,
                FriendlyName  = "Inside Fabric",
                Domain = "insidefabric.com",
                ProductCount = 134500,
                FeaturedProductCount = 120,
                CategoryCount = 90,
                ManufacturerCount = 20,
                TimeWhenPopulationCompleted = DateTime.Now,
                TimeToPopulate = TimeSpan.FromSeconds(10),
            };

            StoreInformation = info;
        }

        private void GetStoreInformation()
        {
            if (ctx == null)
                return;

            ctx.GetWebStoreInformation(StoreKey, (result) =>
            {
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    StoreInformation = null;
                    return;
                }

                StoreInformation = result.Value;     
           
            }, null);
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

        private void UpdateActionCommandState()
        {
            RepopulateCacheCommand.RaiseCanExecuteChanged();
            RebuildSearchExtensionDataCommand.RaiseCanExecuteChanged();
            RebuildCategoriesTableCommand.RaiseCanExecuteChanged();
        }


        public DelegateCommand RepopulateCacheCommand { get; private set; }
        public DelegateCommand RebuildSearchExtensionDataCommand { get; private set; }
        public DelegateCommand RebuildCategoriesTableCommand { get; private set; }


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


        private double searchChartVerticalAxisMaxValue;

        public double SearchChartVerticalAxisMaxValue
        {
            get { return searchChartVerticalAxisMaxValue; }
            set
            {
                if (searchChartVerticalAxisMaxValue != value)
                {
                    searchChartVerticalAxisMaxValue = value;
                    RaisePropertyChanged(() => SearchChartVerticalAxisMaxValue);
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


        private IEnumerable<BarChartDataPoint> searchChartItemsSource;

        public IEnumerable<BarChartDataPoint> SearchChartItemsSource
        {
            get { return searchChartItemsSource; }
            set
            {
                if (searchChartItemsSource != value)
                {
                    searchChartItemsSource = value;
                    RaisePropertyChanged(() => SearchChartItemsSource);
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
        

        private ActionStates actionStateRepopulateCache;

        public ActionStates ActionStateRepopulateCache
        {
            get { return actionStateRepopulateCache; }
            set
            {
                if (actionStateRepopulateCache != value)
                {
                    actionStateRepopulateCache = value;
                    RaisePropertyChanged(() => ActionStateRepopulateCache);
                }
            }
        }

        private ActionStates actionStateRebuildCategories;

        public ActionStates ActionStateRebuildCategories
        {
            get { return actionStateRebuildCategories; }
            set
            {
                if (actionStateRebuildCategories != value)
                {
                    actionStateRebuildCategories = value;
                    RaisePropertyChanged(() => ActionStateRebuildCategories);
                }
            }
        }

        private ActionStates actionStateRebuildSearchData;

        public ActionStates ActionStateRebuildSearchData
        {
            get { return actionStateRebuildSearchData; }
            set
            {
                if (actionStateRebuildSearchData != value)
                {
                    actionStateRebuildSearchData = value;
                    RaisePropertyChanged(() => ActionStateRebuildSearchData);
                }
            }
        }
        
        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
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


        private Website.WebStoreInformation storeInformation;

        public Website.WebStoreInformation StoreInformation
        {
            get { return storeInformation; }
            set
            {
                if (storeInformation != value)
                {
                    storeInformation = value;
                    RaisePropertyChanged(() => StoreInformation);
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


        private bool isExecutingRepopulateCache;

        public bool IsExecutingRepopulateCache
        {
            get { return isExecutingRepopulateCache; }
            set
            {
                if (isExecutingRepopulateCache != value)
                {
                    isExecutingRepopulateCache = value;
                    RaisePropertyChanged(() => IsExecutingRepopulateCache);
                }
            }
        }


        private bool isExecutingRebuildCategoriesTable;

        public bool IsExecutingRebuildCategoriesTable
        {
            get { return isExecutingRebuildCategoriesTable; }
            set
            {
                if (isExecutingRebuildCategoriesTable != value)
                {
                    isExecutingRebuildCategoriesTable = value;
                    RaisePropertyChanged(() => IsExecutingRebuildCategoriesTable);
                }
            }
        }

        private bool isExecutingRebuildSearchExtensionData;

        public bool IsExecutingRebuildSearchExtensionData
        {
            get { return isExecutingRebuildSearchExtensionData; }
            set
            {
                if (isExecutingRebuildSearchExtensionData != value)
                {
                    isExecutingRebuildSearchExtensionData = value;
                    RaisePropertyChanged(() => IsExecutingRebuildSearchExtensionData);
                }
            }
        }

        private void SetDefaultActionStateAll()
        {
            ActionStateRepopulateCache = ActionStates.Default;
            ActionStateRebuildCategories = ActionStates.Default;
            ActionStateRebuildSearchData = ActionStates.Default;
        }

        #region Command Actions

        private bool CanExecuteRepopulateCacheCommand(object parameter)
        {
            return ctx != null && !IsBusy;
        }


        private void ExecuteRepopulateCacheCommand(object parameter)
        {
            IsBusy = true;
            UpdateActionCommandState();
            AppService.SetErrorMessage(string.Empty);
            ActionStateRepopulateCache = ActionStates.Executing;
            ctx.RepopulateDataCache(StoreKey, (result) =>
            {
                IsBusy = false;
                UpdateActionCommandState();
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    ActionStateRepopulateCache = ActionStates.Warning;
                    return;
                }

                GetStoreInformation();
                ActionStateRepopulateCache = result.Value ? ActionStates.Success : ActionStates.Warning;

            }, null);
        }


        private bool CanExecuteRebuildCategoriesTableCommand(object parameter)
        {
            return ctx != null && !IsBusy;
        }


        private void ExecuteRebuildCategoriesTableCommand(object parameter)
        {
            IsBusy = true;
            UpdateActionCommandState();
            AppService.SetErrorMessage(string.Empty);
            ActionStateRebuildCategories = ActionStates.Executing;
            ctx.RebuildCategoriesTable(StoreKey, (result) =>
            {
                IsBusy = false;
                UpdateActionCommandState();
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    ActionStateRebuildCategories = ActionStates.Warning;
                    return;
                }
                GetStoreInformation();
                ActionStateRebuildCategories = result.Value ? ActionStates.Success : ActionStates.Warning;

            }, null);
        }



        private bool CanExecuteRebuildSearchExtensionDataCommand(object parameter)
        {
            return ctx != null && !IsBusy;
        }


        private void ExecuteRebuildSearchExtensionDataCommand(object parameter)
        {
            IsBusy = true;
            UpdateActionCommandState();
            AppService.SetErrorMessage(string.Empty);
            ActionStateRebuildSearchData = ActionStates.Executing;
            ctx.RebuildSearchExtensionData(StoreKey, (result) =>
            {
                IsBusy = false;
                UpdateActionCommandState();
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    ActionStateRebuildSearchData = ActionStates.Warning;
                    return;
                }
                GetStoreInformation();
                ActionStateRebuildSearchData = result.Value ? ActionStates.Success : ActionStates.Warning;

            }, null);
        }

        #endregion


        public void OnNavigatedTo(UXViewPage page, NavigationEventArgs e)
        {
            try
            {
                var key = (StoreKeys?)e.ExtraData;
                StoreKey = key.Value;

                SetDefaultActionStateAll();
                RequestsChartVerticalAxisMaxValue = Double.PositiveInfinity;
                ctx = new Website.Services.ControlPanelDomainContext();
                GetStoreInformation();
                InitRequestsPerSecondMonitor();
                InitSearchBarChartMonitor();
                InitRequestsBarChartMonitor();

                // changes to bar chart time series MUST be initiated with the slider value,
                // which in turn updates the enum and that updates the chart data
                RequestsChartSliderIntervalValue = (double)TimelineSeries.Minutes;

                OnRequestsChartTimelineSeriesUpdated(RequestsChartTimelineSeries);
                FetchSearchBarChartData();
            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (IsBusy)
            {
                e.Cancel = true;
                return;
            }
            KillRequestsPerSecondMonitor();
            KillRequestsBarChartMonitor();
            KillSearchBarChartMonitor();
            ctx = null;
        }

    }
}
