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


    public class SalesComparisonByManufacturerPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {


        private ControlPanelDomainContext ctx;
        private bool isInitializing;

        public SalesComparisonByManufacturerPageViewModel()
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
            if (isInitializing || ISControl.IsInDesignModeStatic)
                return;

            try
            {
                IsExportSuccessful = false;

                var startDate = SelectedStartDate;
                var endDate = SelectedEndDate.AddDays(-1);

                ctx.GetComparisonSalesByManufacturerMetrics(StoreKey, startDate, endDate, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        return;
                    }

                    SalesByManufacturerItemsSource = new ObservableCollection<SalesByManufacturerMetric>(result.Value);

                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }

        }


        private void LoadSalesByManufacturerData()
        {
            isInitializing = true;
            IsExportSuccessful = false;

            // timebar uses sales summary to keep it simple
            var colTimebarData = new ObservableCollection<SalesSummaryMetric>(AppSvc.Current.GetSalesSummaryData(StoreKey));
            TimebarItemsSource = colTimebarData;
            InitializeTimebarRanges();

            isInitializing = false;
            Recalculate();

        }


        private void SetDesignTimeData()
        {
            isInitializing = true;

            IsExportSuccessful = true;

            var manufacturers = new List<SalesByManufacturerMetric>()
            {
                new SalesByManufacturerMetric
                {
                    ManufacturerID = 1,
                    ManufacturerName = "Kravet Fabric",

                    ProductOrders = 100,
                    SwatchOrders = 200,
                    ProductYards = 123,
                    ProductSales = 1234M,
                    SwatchSales = 345M,
                    TotalSales = 1023M, 
                },

                new SalesByManufacturerMetric
                {
                    ManufacturerID = 2,
                    ManufacturerName = "Lee Joffa Fabric",

                    ProductOrders = 100,
                    SwatchOrders = 200,
                    ProductYards = 123,
                    ProductSales = 1234M,
                    SwatchSales = 345M,
                    TotalSales = 1023M, 
                },

                new SalesByManufacturerMetric
                {
                    ManufacturerID = 3,
                    ManufacturerName = "Duralee Fabric",

                    ProductOrders = 100,
                    SwatchOrders = 200,
                    ProductYards = 123,
                    ProductSales = 1234M,
                    SwatchSales = 345M,
                    TotalSales = 1023M, 
                },
                new SalesByManufacturerMetric
                {
                    ManufacturerID = 4,
                    ManufacturerName = "Fabricut Fabric",

                    ProductOrders = 100,
                    SwatchOrders = 200,
                    ProductYards = 123,
                    ProductSales = 1234M,
                    SwatchSales = 345M,
                    TotalSales = 1023M, 
                },

                new SalesByManufacturerMetric
                {
                    ManufacturerID = 5,
                    ManufacturerName = "Pindler Fabric",

                    ProductOrders = 100,
                    SwatchOrders = 200,
                    ProductYards = 123,
                    ProductSales = 1234M,
                    SwatchSales = 345M,
                    TotalSales = 1023M, 
                },

            };

            for (int i = 1; i < 43; i++)
            {
                var sm = new SalesByManufacturerMetric
                {
                    ManufacturerID = 100 + i,
                    ManufacturerName = string.Format("TestCompany{0}", 100 + i),

                    ProductOrders = 100,
                    SwatchOrders = 200,
                    ProductYards = 123,
                    ProductSales = 1234M,
                    SwatchSales = 345M,
                    TotalSales = 1023M,
                };

                manufacturers.Add(sm);
            }
                SalesByManufacturerItemsSource = new ObservableCollection<SalesByManufacturerMetric>(manufacturers);
            var endDate = DateTime.Now.AddDays(1).Date;

            var rnd = new Random();

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
                DefaultFileName = "ManufacturerSalesComparison.xlsx",
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

            
            ctx.ExportSalesComparisonByManufacturer(StoreKey, startDate, endDate, (result) =>
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
