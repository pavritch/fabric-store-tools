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


    public class ManufacturerCountsPieChartPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {

        ControlPanelDomainContext ctx;


        public ManufacturerCountsPieChartPageViewModel()
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
                            TotalCount = 12000,
                        },

                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Greenhouse Design",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
                            TotalCount = 59000,
                        },

                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Ralph The Man",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
                            TotalCount = 59000,
                        },
                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Peter Joker",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
                            TotalCount = 59000,
                        },
                        new ManufacturerMetric
                        {
                            StoreKey = StoreKeys.InsideFabric,
                            ManufacturerName = "Yellowbird Fabric",
                            AvailableCount = 50000,
                            DiscontinuedCount = 9000,
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
