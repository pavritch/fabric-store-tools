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


    public class SalesByManufacturerPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {

        public class BarChartItem
        {
            public string Label { get; set; }

            public decimal ProductSales { get; set; }
            public decimal SwatchSales { get; set; }

            public BarChartItem()
            {

            }

            public decimal Total
            {
                get
                {
                    return ProductSales + SwatchSales;
                }
            }

            public bool IsAllZeros
            {
                get
                {
                    return ProductSales == 0M && SwatchSales == 0M;
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


        private ControlPanelDomainContext ctx;
        private bool isInitializing;

        public SalesByManufacturerPageViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
                ExportOrdersCommand = new DelegateCommand(ExecuteExportOrdersCommand, CanExecuteExportOrdersCommand);
            }
            else
            {
                SetDesignTimeData();
            }
        }

        public DelegateCommand ExportOrdersCommand { get; private set; }


        private void Recalculate()
        {
            if (isInitializing || SalesByManufacturerItemsSource == null || SalesByManufacturerItemsSource.Count() == 0 || ISControl.IsInDesignModeStatic)
                return;

            try
            {
                IsExportSuccessful = false;

                var startDate = SelectedStartDate;
                var endDate = SelectedEndDate.AddDays(-1);

                // metrics

                int productOrders = 0;
                int swatchOrders = 0;

                int productYards = 0;

                decimal swatchSales = 0M;
                decimal productSales = 0M;
                decimal totalSales = 0M;

                // assumes the timebar data is ordered by date

                // keep track of indexes used from collection here for fast access below for bar chart

                int startIndex = 0;
                int lastIndex = 0;

                foreach (var item in SalesByManufacturerItemsSource)
                {
                    if (item.Date < startDate)
                    {
                        startIndex++;
                        lastIndex++;
                        continue;
                    }

                    if (item.Date > endDate)
                        break;

                    productOrders += item.ProductOrders;
                    swatchOrders += item.SwatchOrders;
                    
                    productYards += item.ProductYards;

                    productSales += item.ProductSales;
                    swatchSales += item.SwatchSales;
                    totalSales += item.TotalSales;

                    lastIndex++;
                }

                // make so this last index is inclusive
                lastIndex--;

                TotalSales = totalSales.ToString("C");
                TotalOrders = (productOrders + swatchOrders).ToString("N0"); 

                var metricsData = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("PRODUCT ORDERS", productOrders.ToString("N0")),
                    new KeyValuePair<string, string>("SWATCH ORDERS", swatchOrders.ToString("N0")),
                    new KeyValuePair<string, string>("PRODUCT YARDS", productYards.ToString("N0")),
                    new KeyValuePair<string, string>("PRODUCT SALES", productSales.ToString("C")),
                    new KeyValuePair<string, string>("SWATCH SALES", swatchSales.ToString("C")),
                    new KeyValuePair<string, string>("AVG. PRODUCT SALE", (productOrders > 0) ? (productSales / productOrders).ToString("C") : "N/A"),
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

                int pieProductSalesPct = calcPctOfSales(productSales);
                int pieSwatchSalesPct = calcPctOfSales(swatchSales);

                var pieChartData = new List<PieChartItem>
                {
                    new PieChartItem("Products", pieProductSalesPct),
                    new PieChartItem("Swatches", pieSwatchSalesPct),
                };

                var colPieChart = new ObservableCollection<PieChartItem>(pieChartData);
                PieChartItemsSource = colPieChart;

                // bar chart

                // show different number of bars and date labels based on the duration of the selected period

                var salesChart = new List<BarChartItem>();

                var currentDate = startDate;

                string barChartLabel = null;
                decimal barChartProductSales = 0M;
                decimal barChartSwatchSales = 0M;

                Action insertChartItem = () =>
                {
                    var itm = new BarChartItem()
                    {
                        Label = barChartLabel,
                        ProductSales = barChartProductSales,
                        SwatchSales = barChartSwatchSales,
                    };

                    salesChart.Add(itm);

                    barChartLabel = null;
                    barChartProductSales = 0M;
                    barChartSwatchSales = 0M;
                };


                if (lastIndex - startIndex <= 31)
                {
                    // show N days

                    for (var i = startIndex; i <= lastIndex; i++)
                    {
                        var item = SalesByManufacturerItemsSource[i];
                        var day = new BarChartItem()
                        {
                            Label = string.Format("{0:d}", currentDate),
                            ProductSales = item.ProductSales,
                            SwatchSales = item.SwatchSales,
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
                        var item = SalesByManufacturerItemsSource[i];

                        var thisMonthLabel = makeMonthLabel(currentDate);
                        if (barChartLabel != thisMonthLabel)
                        {
                            if (barChartLabel != null)
                                insertChartItem();
                            barChartLabel = thisMonthLabel;
                        }
                        barChartProductSales += item.ProductSales;
                        barChartSwatchSales += item.SwatchSales;

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
                        var item = SalesByManufacturerItemsSource[i];

                        var thisYearLabel = makeYearLabel(currentDate);
                        if (barChartLabel != thisYearLabel)
                        {
                            if (barChartLabel != null)
                                insertChartItem();
                            barChartLabel = thisYearLabel;
                        }
                        barChartProductSales += item.ProductSales;
                        barChartSwatchSales += item.SwatchSales;

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

        private void SelectedManufacturerUpdated(int ?manufacturerID)
        {
            if (ISControl.IsInDesignModeStatic || isInitializing)
                return;

            ctx.GetSalesByManufacturerMetrics(StoreKey, manufacturerID, (result) =>
            {
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    return;
                }

                SalesByManufacturerItemsSource = new ObservableCollection<SalesByManufacturerMetric>(result.Value);

                Recalculate();

            }, null);

        }
        private void LoadSalesByManufacturerData()
        {
            isInitializing = true;
            IsExportSuccessful = false;
            ManufacturersItemsSource = new ObservableCollection<ManufacturerIdentity>(AppSvc.Current.GetManufacturers(StoreKey));
            ManufacturersItemsSource.Insert(0, new ManufacturerIdentity { ManufacturerID=0, ManufacturerName = "All Manufacturers", StoreKey = StoreKey });

            // timebar uses sales summary to keep it simple
            var colTimebarData = new ObservableCollection<SalesSummaryMetric>(AppSvc.Current.GetSalesSummaryData(StoreKey));
            TimebarItemsSource = colTimebarData;
            InitializeTimebarRanges();

            SelectedManufacturer = ManufacturersItemsSource[0];

            ctx.GetSalesByManufacturerMetrics(StoreKey, SelectedManufacturerID, (result) =>
            {
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    return;
                }

                SalesByManufacturerItemsSource = new ObservableCollection<SalesByManufacturerMetric>(result.Value);

                isInitializing = false;
                Recalculate();

            }, null);
        }


        private void SetDesignTimeData()
        {
            isInitializing = true;

            IsExportSuccessful = true;

            var manufacturers = new List<ManufacturerIdentity>()
            {
                new ManufacturerIdentity
                {
                    ManufacturerID = 0,
                    ManufacturerName = "All Manufacturers",
                    StoreKey = StoreKeys.InsideFabric,
                },

                new ManufacturerIdentity
                {
                    ManufacturerID = 1,
                    ManufacturerName = "Kravet Fabric",
                    StoreKey = StoreKeys.InsideFabric,
                },

                new ManufacturerIdentity
                {
                    ManufacturerID = 2,
                    ManufacturerName = "Lee Joffa Fabric",
                    StoreKey = StoreKeys.InsideFabric,
                },

                new ManufacturerIdentity
                {
                    ManufacturerID = 3,
                    ManufacturerName = "Duralee Fabric",
                    StoreKey = StoreKeys.InsideFabric,
                },
                new ManufacturerIdentity
                {
                    ManufacturerID = 4,
                    ManufacturerName = "Fabricut Fabric",
                    StoreKey = StoreKeys.InsideFabric,
                },

                new ManufacturerIdentity
                {
                    ManufacturerID = 5,
                    ManufacturerName = "Pindler Fabric",
                    StoreKey = StoreKeys.InsideFabric,
                },

            };

            ManufacturersItemsSource = new ObservableCollection<ManufacturerIdentity>(manufacturers);
            SelectedManufacturer = ManufacturersItemsSource[0];

            var pieChartData = new PieChartItem[]
            {
                new PieChartItem("Products", 85),
                new PieChartItem("Swatches", 7),
            };

            var colPieChart = new ObservableCollection<PieChartItem>(pieChartData);
            PieChartItemsSource = colPieChart;


            var metricsData = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("PRODUCT ORDERS", "98"),
                new KeyValuePair<string, string>("SWATCH ORDERS", "53"),
                new KeyValuePair<string, string>("PRODUCT YARDS", "6,090"),
                new KeyValuePair<string, string>("PRODUCT SALES", "$17,304.98"),
                new KeyValuePair<string, string>("SWATCH SALES", "$5,011.10"),
                new KeyValuePair<string, string>("AVG. PRODUCT SALE", "$1,763.15"),
            };

            var colMetricsData = new ObservableCollection<KeyValuePair<string, string>>(metricsData);
            MetricsItemsSource = colMetricsData;

            TotalSales = 1256789.90M.ToString("C");
            TotalOrders = 10023.ToString("N0");

            var endDate = DateTime.Now.AddDays(1).Date;

            var rnd = new Random();

            var salesChart = new List<BarChartItem>();

            for (var currentDate = DateTime.Now.AddMonths(-1).Date; currentDate < endDate; currentDate = currentDate.AddDays(1))
            {
                var totalSales = (decimal)(rnd.Next(2000, 10000));

                var day = new BarChartItem()
                {
                    Label = string.Format("{0:d}", currentDate),
                    ProductSales = totalSales * .77M,
                    SwatchSales = totalSales * .15M,
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
                    // only need these properties to populate the timebar here
                    Date = currentDate,
                    TotalSales = totalSales,
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

        private ManufacturerIdentity selectedManufacturer;
        public ManufacturerIdentity SelectedManufacturer
        {
            get { return this.selectedManufacturer; }

            set
            {
                if (this.selectedManufacturer != value)
                {
                    this.selectedManufacturer = value;
                    RaisePropertyChanged(() => SelectedManufacturer);
                    if (!ISControl.IsInDesignModeStatic)
                    {
                        SelectedManufacturerID = (value.ManufacturerID == 0) ? (int?)null : value.ManufacturerID;
                    }
                }
            }
        }


        private int? selectedManufacturerID;
        public int? SelectedManufacturerID
        {
            get { return this.selectedManufacturerID; }

            set
            {
                if (this.selectedManufacturerID != value)
                {
                    this.selectedManufacturerID = value;
                    RaisePropertyChanged(() => SelectedManufacturerID);
                    SelectedManufacturerUpdated(selectedManufacturerID);
                }
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


        private ObservableCollection<ManufacturerIdentity> manufacturersItemsSource;

        public ObservableCollection<ManufacturerIdentity> ManufacturersItemsSource
        {
            get { return manufacturersItemsSource; }
            set
            {
                if (manufacturersItemsSource != value)
                {
                    manufacturersItemsSource = value;
                    RaisePropertyChanged(() => ManufacturersItemsSource);
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

        private ObservableCollection<SalesByManufacturerMetric> salesbyManufacturerItemsSource;

        public ObservableCollection<SalesByManufacturerMetric> SalesByManufacturerItemsSource
        {
            get { return salesbyManufacturerItemsSource; }
            set
            {
                if (salesbyManufacturerItemsSource != value)
                {
                    salesbyManufacturerItemsSource = value;
                    RaisePropertyChanged(() => SalesByManufacturerItemsSource);
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

        private string totalOrders;

        public string TotalOrders
        {
            get { return totalOrders; }
            set
            {
                if (totalOrders != value)
                {
                    totalOrders = value;
                    RaisePropertyChanged(() => TotalOrders);
                }
            }
        }



        private bool isSavingFile;

        public bool IsSavingFile
        {
            get { return isSavingFile; }
            set
            {
                if (isSavingFile != value)
                {
                    isSavingFile = value;
                    RaisePropertyChanged(() => IsSavingFile);
                    ExportOrdersCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool isExportSuccessful;

        public bool IsExportSuccessful
        {
            get { return isExportSuccessful; }
            set
            {
                if (isExportSuccessful != value)
                {
                    isExportSuccessful = value;
                    RaisePropertyChanged(() => IsExportSuccessful);
                }
            }
        }
        
        #region Command Actions

        private bool CanExecuteExportOrdersCommand(object parameter)
        {
            return !IsSavingFile;
        }

        private void ExecuteExportOrdersCommand(object parameter)
        {
            IsExportSuccessful = false;

            var dialog = new SaveFileDialog()
            {
                DefaultFileName = "OrderDetail.xlsx",
                DefaultExt = ".xlsx",
                Filter = "Excel Files|*.xlsx|All Files|*.*",
                FilterIndex = 2,
            };

            var startDate = SelectedStartDate;
            var endDate = SelectedEndDate;

            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult != true)
                return;

            IsSavingFile = true;

            ctx.ExportSalesOrderDetail(StoreKey, startDate, endDate, SelectedManufacturerID, (result) =>
            {

                if (result.HasError)
                {
                    IsSavingFile = false;
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage("Processing error.");
                    return;
                }

                var fileData = result.Value;

                try
                {
                    using (var fs = dialog.OpenFile())
                    {
                        fs.Write(fileData, 0, fileData.Length);
                        fs.Flush();
                    }

                    IsExportSuccessful = true;
                }
                catch
                {
                    AppSvc.Current.RunActionWithDelay(1, () => AppSvc.Current.SetErrorMessage("Error saving file."));
                }

                IsSavingFile = false;


            }, null);

        }

        #endregion


        public void OnNavigatedTo(UXViewPage page, NavigationEventArgs e)
        {
            try
            {
                var key = (StoreKeys?)e.ExtraData;
                StoreKey = key.Value;

                ctx = new Website.Services.ControlPanelDomainContext();
                // jumpstart any data fetch here...
                LoadSalesByManufacturerData(); // resets is initializing and recalculates
            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ctx = null;
        }

    }
}
