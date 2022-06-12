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


    public class ManufacturerCountsPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {

        ControlPanelDomainContext ctx;


        public ManufacturerCountsPageViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
                ExportProductCountsCommand = new DelegateCommand(ExecuteExportProductCountsCommand, CanExecuteExportProductCountsCommand);

            }
            else
            {
                SetDesignTimeData();
            }
        }

        public DelegateCommand ExportProductCountsCommand { get; private set; }

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
                    ExportProductCountsCommand.RaiseCanExecuteChanged();
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


        private void FetchManufacturerCounts()
        {
            if (ctx == null)
                return;

            try
            {
                ctx.GetManufacturerCounts(StoreKey, (result) =>
                {
                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        AppService.SetErrorMessage(result.Error.Message);
                        return;
                    }

                    var col = new ObservableCollection<ManufacturerMetric>(result.Value);
                    ManufacturesItemsSource = col;
                }, null);

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }



        private void SetDesignTimeData()
        {

            var points = new ManufacturerMetric[]
                    {
                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Pindler & Pindler",
                            AvailableCount = 10000,
                            DiscontinuedCount = 2000,
                            InStockCount=9000,
                            OutOfStockCount=1000,
                            TotalCount = 12000,
                        },

                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Greenhouse Design",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
                            InStockCount=25000,
                            OutOfStockCount=25000,
                            TotalCount = 59000,
                        },

                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Ralph The Man",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
                            InStockCount=40000,
                            OutOfStockCount=10000,
                            TotalCount = 59000,
                        },
                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Peter Joker",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
                            InStockCount=40000,
                            OutOfStockCount=10000,
                            TotalCount = 59000,
                        },
                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Yellowbird Fabric",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
                            InStockCount=47000,
                            OutOfStockCount=3000,
                            TotalCount = 59000,
                        },
                    
                    };

            var col = new ObservableCollection<ManufacturerMetric>(points);
            ManufacturesItemsSource = col;

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


        private ObservableCollection<ManufacturerMetric> manufacturesItemsSource;

        public ObservableCollection<ManufacturerMetric> ManufacturesItemsSource
        {
            get { return manufacturesItemsSource; }
            set
            {
                if (manufacturesItemsSource != value)
                {
                    manufacturesItemsSource = value;
                    RaisePropertyChanged(() => ManufacturesItemsSource);
                }
            }
        }

        #region Command Actions

        private bool CanExecuteExportProductCountsCommand(object parameter)
        {
            return !IsSavingFile;
        }

        private void ExecuteExportProductCountsCommand(object parameter)
        {
            IsExportSuccessful = false;

            var dialog = new SaveFileDialog()
            {
                DefaultFileName = "ManufacturerProductCounts.xlsx",
                DefaultExt = ".xlsx",
                Filter = "Excel Files|*.xlsx|All Files|*.*",
                FilterIndex = 2,
            };


            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult != true)
                return;

            IsSavingFile = true;


            ctx.ExportManufacturerProductCounts(StoreKey, (result) =>
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
                FetchManufacturerCounts();

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
