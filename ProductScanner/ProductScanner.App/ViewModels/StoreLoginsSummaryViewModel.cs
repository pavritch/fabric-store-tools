using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ProductScanner.Core;

namespace ProductScanner.App.ViewModels
{
    public class StoreLoginsSummaryViewModel : StoreContentPageViewModel
    {

        #region ExportRecord Class
		public class ExportRecord
        {
            public string Vendor { get; set; }
            [ExcelColumn(ExcelColumnType.Url)]
            public string Website { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Status { get; set; }
        }

	    #endregion

        #region TestRecord Class

        public class TestRecord : ObservableObject
        {
            #region Properties

            private IVendorModel _vendor = null;
            public IVendorModel Vendor
            {
                get
                {
                    return _vendor;
                }
                set
                {
                    Set(() => Vendor, ref _vendor, value);
                }
            }


            private TestStatus _status = TestStatus.Unknown;
            public TestStatus Status
            {
                get
                {
                    return _status;
                }

                set
                {
                    if (_status == value)
                    {
                        return;
                    }

                    _status = value;
                    RaisePropertyChanged(() => Status);
                }
            }
            #endregion

            public TestRecord()
            {

            }

            public TestRecord(IVendorModel v)
            {
                this.Vendor = v;

                // initial test setting is from current vendor state

                if (v.IsVendorWebsiteLoginValid.HasValue)
                    this.Status =  v.IsVendorWebsiteLoginValid.Value ? TestStatus.Successful : TestStatus.Failed;
                else
                    this.Status = TestStatus.Unknown;
            }
        }

        #endregion

        private CancellationTokenSource cancellationTokenSource;

#if DEBUG
        public StoreLoginsSummaryViewModel()
            : this((new DesignStoreModel { Name = "InsideFabric", Key = StoreType.InsideFabric}) as IStoreModel)
        {

        }
#endif

        public StoreLoginsSummaryViewModel(IStoreModel store)
            : base(store)
        {
            PageType = ContentPageTypes.StoreLoginsSummary;
            PageSubTitle = "Logins Summary";
            BreadcrumbTemplate = "{Home}/{Store}/Logins";
            IsNavigationJumpTarget = true;
            IsTesting = false;

            if (!IsInDesignMode)
            {
                HookMessages();
                Reset();
            }
            else
                DesignSettings();
        }

        #region Local Methods

        private void DesignSettings()
        {
            TestingStatus = "Running";

            TotalTestsCount = 8;
            PassedTestsCount = 5;
            FailedTestsCount = 3;

            var testList = new List<TestRecord>()
            {
                new TestRecord(Store.Vendors.Skip(0).Take(1).First()),
                new TestRecord(Store.Vendors.Skip(1).Take(1).First()),
                new TestRecord(Store.Vendors.Skip(2).Take(1).First()),
                new TestRecord(Store.Vendors.Skip(3).Take(1).First()),
                new TestRecord(Store.Vendors.Skip(4).Take(1).First()),
            };

            Tests = new ObservableCollection<TestRecord>(testList);
        }

        private void HookMessages()
        {
            MessengerInstance.Register<AnnouncementMessage>(this, (msg) =>
            {
            });
        }

        private async Task Run()
        {
            
            //  Task<bool> VerifyVendorWebsiteCredentialsAsync();

            try
            {
                Reset();
                cancellationTokenSource = new CancellationTokenSource();
                TestingStatus = "Running";
                IsTesting = true;
                bool skipRemaining = false;

                // TODO: make run in parallel

                foreach (var test in Tests)
                {
                    if (skipRemaining)
                    {
                        test.Status = TestStatus.Skipped;
                    }
                    else
                    {
                        test.Status = TestStatus.Running;

                        if (test.Vendor.IsTestable)
                        {

                            var result = await test.Vendor.VerifyVendorWebsiteCredentialsAsync();

                            if (cancellationTokenSource.IsCancellationRequested)
                            {
                                test.Status = TestStatus.Cancelled;
                            }
                            else if (result == true)
                            {
                                test.Status = TestStatus.Successful;
                                PassedTestsCount++;
                            }
                            else
                            {
                                test.Status = TestStatus.Failed;
                                FailedTestsCount++;
                            }
                        }
                        else
                        {
                            // do not interfere with any currently scanning vendors - just skip and move on
                            test.Status = TestStatus.Skipped;
                        }

                        if (cancellationTokenSource.IsCancellationRequested)
                            skipRemaining = true;
                    }
                }

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    TestingStatus = "Cancelled";
                }
                else
                    TestingStatus = "Finished";

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            cancellationTokenSource = null;
            IsTesting = false;
            InvalidateButtons();

            return;
        }

        private void Cancel()
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            InvalidateButtons();
        }

        private void InvalidateButtons()
        {
            RunCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            ClearCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
        }

        private void PopulateTests()
        {
            // ordered list of tests, all starting out with Unknown status.

            if (Tests != null)
            {
                foreach (var t in Tests)
                    t.Status = TestStatus.Unknown;
                return;
            }

            var tests = (from v in Store.Vendors
                         where v.IsFullyImplemented
                         orderby v.Name
                        select new TestRecord(v)).ToList();
            
            Tests = new ObservableCollection<TestRecord>(tests);
        }

        private void Reset()
        {
            TestingStatus = null;
            PopulateTests();
            TotalTestsCount = Tests.Count;
            PassedTestsCount = 0;
            FailedTestsCount = 0;
            InvalidateButtons();
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Login tests are running at this moment.
        /// </summary>
        private bool _isTesting = false;
        public bool IsTesting
        {
            get
            {
                return _isTesting;
            }
            set
            {
                Set(() => IsTesting, ref _isTesting, value);
            }
        }

        private ObservableCollection<TestRecord> _tests = null;
        public ObservableCollection<TestRecord> Tests
        {
            get
            {
                return _tests;
            }

            set
            {
                if (_tests == value)
                {
                    return;
                }

                _tests = value;
                RaisePropertyChanged(() => Tests);
            }
        }

        /// <summary>
        /// Where are we...Running, Finished, Cancelled, blank
        /// </summary>
        private string _testingStatus = null;
        public string TestingStatus
        {
            get
            {
                return _testingStatus;
            }

            set
            {
                if (_testingStatus == value)
                {
                    return;
                }

                _testingStatus = value;
                RaisePropertyChanged(() => TestingStatus);
            }
        }

        private int _totalTestsCount = 0;
        public int TotalTestsCount
        {
            get
            {
                return _totalTestsCount;
            }

            set
            {
                if (_totalTestsCount == value)
                {
                    return;
                }

                _totalTestsCount = value;
                RaisePropertyChanged(() => TotalTestsCount);
            }
        }

        private int _passedTestsCount = 0;
        public int PassedTestsCount
        {
            get
            {
                return _passedTestsCount;
            }

            set
            {
                if (_passedTestsCount == value)
                {
                    return;
                }

                _passedTestsCount = value;
                RaisePropertyChanged(() => PassedTestsCount);
            }
        }

        private int _failedTestsCount = 0;
        public int FailedTestsCount
        {
            get
            {
                return _failedTestsCount;
            }

            set
            {
                if (_failedTestsCount == value)
                {
                    return;
                }

                _failedTestsCount = value;
                RaisePropertyChanged(() => FailedTestsCount);
            }
        }

        #endregion

        #region Commands

        private RelayCommand _runCommand;
        public RelayCommand RunCommand
        {
            get
            {
                return _runCommand
                    ?? (_runCommand = new RelayCommand(
                    async () =>
                    {
                        if (!RunCommand.CanExecute(null))
                        {
                            return;
                        }

                        await Run();
                    },
                    () => !IsTesting));
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        if (!CancelCommand.CanExecute(null))
                        {
                            return;
                        }

                        Cancel();
                    },
                    () => IsTesting));
            }
        }


        private RelayCommand _clearCommand;
        public RelayCommand ClearCommand
        {
            get
            {
                return _clearCommand
                    ?? (_clearCommand = new RelayCommand(
                    () =>
                    {
                        if (!ClearCommand.CanExecute(null))
                        {
                            return;
                        }

                        Reset();
                    },
                    () => !IsTesting));
            }
        }

        private RelayCommand _exportCommand;
        public RelayCommand ExportCommand
        {
            get
            {
                return _exportCommand
                    ?? (_exportCommand = new RelayCommand(
                    () =>
                    {
                        if (!ExportCommand.CanExecute(null))
                        {
                            return;
                        }

                        Task.Run(() =>
                        {
                            var exportRecords = (from t in Tests
                                                 orderby t.Vendor.Name
                                                 select new ExportRecord()
                                                 {
                                                     Vendor = t.Vendor.Name,
                                                     Website = t.Vendor.VendorWebsiteUrl,
                                                     Username = t.Vendor.VendorWebsiteUsername,
                                                     Password = t.Vendor.VendorWebsitePassword,
                                                     Status = t.Status.ToString(),
                                                 }).ToList();

                            var suggestedName = string.Format("{0} Vendor Website Logins.xlsx", Store.Name);
                            ExportManager.SaveExcelFile(exportRecords, suggestedName);

                        });
                    },
                    () => !IsTesting));
            }
        }

        #endregion


    }
}