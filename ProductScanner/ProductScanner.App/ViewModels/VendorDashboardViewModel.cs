using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App.ViewModels
{
    public class VendorDashboardViewModel : VendorContentPageViewModel
    {
        #region PieChartSlice Class
        public class PieChartSlice
        {
            public string Label { get; set; }
            public int Value { get; set; }

            public PieChartSlice()
            {

            }

            public PieChartSlice(string label, int value)
            {
                this.Label = label;
                this.Value = value;
            }
        }
        
        #endregion

#if DEBUG
        public VendorDashboardViewModel()
            : this((new DesignVendorModel { Name = "Kravet", VendorId=3 }) as IVendorModel)
        {
            // dev only - normally the navigation system needs to create one of these viewmodels with a reference
            // to the desired store.
        }
#endif


        public VendorDashboardViewModel(IVendorModel vendor)
            : base(vendor)
        {
            PageSubTitle = "Dashboard";
            RequiresToBeCached = true;
            IsNavigationJumpTarget = true;
            PageType = ContentPageTypes.VendorDashboard;

            BreadcrumbTemplate = "{Home}/{Store}/{Vendor}/Dashboard";

            if (IsInDesignMode)
            {
                MakeFakeProductStatistics();
                PopulateRecentCommits(6);
                MakeFakePieChart();
            }
            else
            {
                HookMessages();
                Refresh();
            }
        }

        #region Local Methods

        private void HookMessages()
        {
            MessengerInstance.Register<VendorChangedNotification>(this, (msg) =>
            {
                if (!msg.Vendor.Equals(Vendor))
                    return;

                Refresh();
                ScannerStatus = MakeScannerStatus();
            });

            MessengerInstance.Register<ScanningOperationNotification>(this, (msg) =>
            {
                if (!msg.Vendor.Equals(Vendor))
                    return;

                Refresh();
                ScannerStatus = MakeScannerStatus();
            });

        }

        private void Refresh()
        {
            // handled via direct binding:
            //   productCount, variantProductCount, commitBatchCount, 
            //   website username, password and url, login valid
            //   has warning, warning text

            UpdateScanningStartTime();
            UpdateLastCheckpoint();

            // scanner mode
            ScannerMode = Vendor.Status.ToString();

            // scanner status: Scanning, Idle, Suspended, Successful, Failed
            ScannerStatus = MakeScannerStatus();

            // product stats
            PopulateProductStatistics();

            // pie chart
            PopulatePieChart();

            // recent commits
            PopulateRecentCommits(6);

            VisitWebsite.RaiseCanExecuteChanged();
            VerifyLogin.RaiseCanExecuteChanged();
            ClearWarning.RaiseCanExecuteChanged();
            ShowScanPage.RaiseCanExecuteChanged();
            ShowTestsPage.RaiseCanExecuteChanged();
        }

        protected override void VendorPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ScanningStartTime":
                    UpdateScanningStartTime();
                    break;

                case "LastCheckpointDate":
                    UpdateLastCheckpoint();
                    break;
            }
        }

        private void UpdateScanningStartTime()
        {
            if (Vendor.ScanningStartTime.HasValue)
            {
                ScanStartTime = Vendor.ScanningStartTime.Value.ToString();
            }
            else
            {
                ScanStartTime = "n/a";
            }
        }

        private void UpdateLastCheckpoint()
        {
            if (Vendor.LastCheckpointDate.HasValue)
            {
                LastCheckpointTime = Vendor.LastCheckpointDate.Value.ToString();
            }
            else if (Vendor.IsScanning)
            {
                LastCheckpointTime = "None";
            }
            else
            {
                LastCheckpointTime = "n/a";
            }
        }

        private string MakeScannerStatus()
        {
            if (Vendor.IsScanning)
                return "Scanning";

            if (Vendor.IsSuspended)
                return "Suspended";

            // TODO: IsRecentScanSuccessful
            //if (Vendor.IsRecentScanSuccessful.HasValue)
            //    return Vendor.IsRecentScanSuccessful.Value == true ? "Successful" : "Failed";
            
            return "Idle";
        }

        private void PopulatePieChart()
        {

            var pieSlices = new List<PieChartSlice>();

            int total = Vendor.InStockProductCount + Vendor.OutOfStockProductCount + Vendor.DiscontinuedProductCount;

            if (total > 0)
            {
                int remainingPct = 100;

                Action<int, string> addSlice = (val, lbl) =>
                {
                    int pct = Math.Min(remainingPct, (int)Math.Round(val * 100 / (decimal)total));

                    var label = string.Format("{0} {1}%", lbl, pct);
                    pieSlices.Add(new PieChartSlice(label, pct));
                    remainingPct -= pct; // to ensure always adds up to 100 irrespective of rounding
                };

                // pie charts takes on different look depending on if product centric (like fabric) or variant centric (like rugs)
                if (Vendor.IsVariantCentricInventory)
                {
                    addSlice(Vendor.InStockProductVariantCount, "In Stock");
                    addSlice(Vendor.OutOfStockProductVariantCount, "Out of Stock");
                }
                else
                {
                    addSlice(Vendor.InStockProductCount, "In Stock");
                    addSlice(Vendor.OutOfStockProductCount, "Out of Stock");
                    addSlice(Vendor.DiscontinuedProductCount, "Discontinued");
                }
            }

            PieChartSlices = new ObservableCollection<PieChartSlice>(pieSlices);
        }

        private void PopulateProductStatistics()
        {
            Dictionary<string, string> dic;

            if (Vendor.IsVariantCentricInventory)
            {
                dic = new Dictionary<string, string>()
                {
                    {"ManufacturerID", Vendor.Vendor.Id.ToString()},
                    {"Products", Vendor.ProductCount.ToString("N0")},
                    {"Variants", Vendor.ProductVariantCount.ToString("N0")},
                    {"In Stock Variants", Vendor.InStockProductVariantCount.ToString("N0")},
                    {"Out of Stock Variants", Vendor.OutOfStockProductVariantCount.ToString("N0")},
                    {"Discontinued", Vendor.DiscontinuedProductCount.ToString("N0")},
                    {"Clearance Products", Vendor.ClearanceProductCount.ToString("N0")},
                };
            }
            else
            {
                dic = new Dictionary<string, string>()
                {
                    {"ManufacturerID", Vendor.Vendor.Id.ToString()},
                    {"Products", Vendor.ProductCount.ToString("N0")},
                    {"Variants", Vendor.ProductVariantCount.ToString("N0")},
                    {"In Stock Products", Vendor.InStockProductCount.ToString("N0")},
                    {"Out of Stock Products", Vendor.OutOfStockProductCount.ToString("N0")},
                    {"Discontinued", Vendor.DiscontinuedProductCount.ToString("N0")},
                    {"Clearance Products", Vendor.ClearanceProductCount.ToString("N0")},
                };
            }

            ProductStatistics = new ObservableCollection<dynamic>(dic.ToDynamicNameValueList());
        }

        private void PopulateRecentCommits(int max=6)
        {
            // vendor model must return these in descending order (which is what the IScannerDatabaseConnector will fetch).
            RecentCommits = new ObservableCollection<CommitBatchSummary>(Vendor.RecentCommits.Take(max));
        }

        private void MakeFakeProductStatistics()
        {
            var dic = new Dictionary<string, string>()
            {
                {"ManufacturerID", "5"},
                {"Products", "89,121"},
                {"Variants", "201,223"},
                {"In Stock Variants", "79,900"},
                {"Out of Stock Variants", "34,896"},
                {"Discontinued", "44,892"},
                {"Clearance Products", "11,123"},
            };

            ProductStatistics = new ObservableCollection<dynamic>(dic.ToDynamicNameValueList());
        } 

        private void MakeFakePieChart()
        {

            var pieSlices = new List<PieChartSlice>()
                {
                    new PieChartSlice("In Stock 70%", 70),
                    new PieChartSlice("Out of Stock 20%", 20),
                    new PieChartSlice("Discontinued 10%", 10),
                };
            PieChartSlices = new ObservableCollection<PieChartSlice>(pieSlices);

        }
        #endregion


        #region Public Properties

        private ObservableCollection<CommitBatchSummary> _recentCommits = null;

        /// <summary>
        /// Sets and gets the RecentCommits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<CommitBatchSummary> RecentCommits
        {
            get
            {
                return _recentCommits;
            }
            set
            {
                Set(() => RecentCommits, ref _recentCommits, value);
            }
        }

        private ObservableCollection<dynamic> _productStatistics = null;

        /// <summary>
        /// Sets and gets the StockCheckResults property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<dynamic> ProductStatistics
        {
            get
            {
                return _productStatistics;
            }
            set
            {
                Set(() => ProductStatistics, ref _productStatistics, value);
            }
        }

        private ObservableCollection<PieChartSlice> _pieChartSlices = null;
        public ObservableCollection<PieChartSlice> PieChartSlices
        {
            get
            {
                return _pieChartSlices;
            }

            set
            {
                if (_pieChartSlices == value)
                {
                    return;
                }

                _pieChartSlices = value;
                RaisePropertyChanged(() => PieChartSlices);
            }
        }


        private string _scanStartTime = null;
        public string ScanStartTime
        {
            get
            {
                return _scanStartTime;
            }

            set
            {
                if (_scanStartTime == value)
                {
                    return;
                }

                _scanStartTime = value;
                RaisePropertyChanged(() => ScanStartTime);
            }
        }

        private string _lastCheckpointTime = null;
        public string LastCheckpointTime
        {
            get
            {
                return _lastCheckpointTime;
            }

            set
            {
                if (_lastCheckpointTime == value)
                {
                    return;
                }

                _lastCheckpointTime = value;
                RaisePropertyChanged(() => LastCheckpointTime);
            }
        }

        private string _scannerMode = null;
        public string ScannerMode
        {
            get
            {
                return _scannerMode;
            }

            set
            {
                if (_scannerMode == value)
                {
                    return;
                }

                _scannerMode = value;
                RaisePropertyChanged(() => ScannerMode);
            }
        }


        private string _scannerStatus = null;
        public string ScannerStatus
        {
            get
            {
                return _scannerStatus;
            }

            set
            {
                if (_scannerStatus == value)
                {
                    return;
                }

                _scannerStatus = value;
                RaisePropertyChanged(() => ScannerStatus);
            }
        }


        #endregion

        #region Commands

        private RelayCommand _showCommitsPage;

        /// <summary>
        /// Gets the ShowCommitsPage.
        /// </summary>
        public RelayCommand ShowCommitsPage
        {
            get
            {
                return _showCommitsPage
                    ?? (_showCommitsPage = new RelayCommand(
                    () =>
                    {
                        if (!ShowCommitsPage.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.VendorCommits);
                    },
                    () => true));
            }
        }

        private RelayCommand _showScanPage;

        /// <summary>
        /// Gets the ShowScanPage.
        /// </summary>
        public RelayCommand ShowScanPage
        {
            get
            {
                return _showScanPage
                    ?? (_showScanPage = new RelayCommand(
                    () =>
                    {
                        if (!ShowScanPage.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.VendorScan);

                    },
                    () => Vendor.Status != VendorStatus.Disabled && !Vendor.IsPerformingTests && !Vendor.IsCheckingCredentials));
            }
        }
        private RelayCommand _showStockCheckPage;

        /// <summary>
        /// Gets the ShowStockCheckPage.
        /// </summary>
        public RelayCommand ShowStockCheckPage
        {
            get
            {
                return _showStockCheckPage
                    ?? (_showStockCheckPage = new RelayCommand(
                    () =>
                    {
                        if (!ShowStockCheckPage.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.VendorStockCheck);

                    },
                    () => Vendor.IsTestable));
            }
        }

        private RelayCommand _showTestsPage;

        /// <summary>
        /// Gets the ShowTestsPage.
        /// </summary>
        public RelayCommand ShowTestsPage
        {
            get
            {
                return _showTestsPage
                    ?? (_showTestsPage = new RelayCommand(
                    () =>
                    {
                        if (!ShowTestsPage.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.VendorTests);

                    },
                    () => Vendor.IsTestable));
            }
        }

        private RelayCommand _showPropertiesPage;

        /// <summary>
        /// Gets the ShowPropertiesPage.
        /// </summary>
        public RelayCommand ShowPropertiesPage
        {
            get
            {
                return _showPropertiesPage
                    ?? (_showPropertiesPage = new RelayCommand(
                    () =>
                    {
                        if (!ShowPropertiesPage.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.VendorProperties);

                    },
                    () => true));
            }
        }

        private RelayCommand _visitWebsite;

        /// <summary>
        /// Gets the VisitWebsite.
        /// </summary>
        public RelayCommand VisitWebsite
        {
            get
            {
                return _visitWebsite
                    ?? (_visitWebsite = new RelayCommand(
                    () =>
                    {
                        if (!VisitWebsite.CanExecute(null))
                        {
                            return;
                        }

                        MessengerInstance.Send(new RequestLaunchBrowser(Vendor.VendorWebsiteUrl));
                    },
                    () => !string.IsNullOrWhiteSpace(Vendor.VendorWebsiteUrl)));
            }
        }

        private RelayCommand _verifyLogin;

        /// <summary>
        /// Gets the VerifyLogin.
        /// </summary>
        public RelayCommand VerifyLogin
        {
            get
            {
                return _verifyLogin
                    ?? (_verifyLogin = new RelayCommand(
                    () =>
                    {
                        if (!VerifyLogin.CanExecute(null))
                        {
                            return;
                        }
                        
                        // don't care about result since bindings will 
                        // simply reflect the new status - so no need to await

                        // Vendor model will indicate IsCheckingCredentials, set IsVendorWebsiteLoginValid
                        // and update warning for vendor

                        Vendor.VerifyVendorWebsiteCredentialsAsync();
                        
                    },
                    () => Vendor.IsTestable && !Vendor.IsCheckingCredentials && !Vendor.IsPerformingTests));
            }
        }

        private RelayCommand _clearWarning;

        /// <summary>
        /// Gets the ClearWarning.
        /// </summary>
        public RelayCommand ClearWarning
        {
            get
            {
                return _clearWarning
                    ?? (_clearWarning = new RelayCommand(
                    () =>
                    {
                        if (!ClearWarning.CanExecute(null))
                        {
                            return;
                        }

                        Vendor.ClearWarning();
                        
                    },
                    () => Vendor.HasWarning));
            }
        }
        #endregion

 
    }
}