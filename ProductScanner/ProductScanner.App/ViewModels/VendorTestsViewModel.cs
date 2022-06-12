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
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace ProductScanner.App.ViewModels
{
    public class VendorTestsViewModel : VendorContentPageViewModel
    {
        #region TestRecord Class

        public class TestRecord : ObservableObject
        {
            #region Properties
            private string _key = null;
            public string Key
            {
                get
                {
                    return _key;
                }

                set
                {
                    if (_key == value)
                    {
                        return;
                    }

                    _key = value;
                    RaisePropertyChanged(() => Key);
                }
            }

            private string _description = null;
            public string Description
            {
                get
                {
                    return _description;
                }

                set
                {
                    if (_description == value)
                    {
                        return;
                    }

                    _description = value;
                    RaisePropertyChanged(() => Description);
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

            public IVendorTest VendorTest { get; set; }

            public TestRecord()
            {

            }

            public TestRecord(TestDescriptor test)
            {
                Status = TestStatus.Unknown;
                Key = test.Key;
                Description = test.Description;
                VendorTest = test.VendorTest;
            }
        }
        
        #endregion

        private CancellationTokenSource cancellationTokenSource;

#if DEBUG
        public VendorTestsViewModel()
            : this(new DesignVendorModel() { Name = "Kravet", VendorId = 5 })
        {

        }
#endif


        public VendorTestsViewModel(IVendorModel vendor)
            : base(vendor)
        {
            PageType = ContentPageTypes.VendorTests;
            PageSubTitle = "Scanner Tests";
            BreadcrumbTemplate = "{Home}/{Store}/{Vendor}/Tests";
            IsNavigationJumpTarget = true;

            IsGridView = true;
            IsLogView = false;

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

            var logList = new List<string>()
            {
                "This is my first log entry.",
                "And now we have another log entry.",
                "What we be a log without more log entries.",
                "We're on a roll now - extra log entries.",
                "Don't worry, more to come.",
                "See, here was the more to come."
            };

            LogEvents = new ObservableCollection<string>(logList);
            
            TotalTestsCount = 8;
            PassedTestsCount = 5;
            FailedTestsCount = 3;

            var testList = new List<TestRecord>()
            {
                new TestRecord()
                {
                    Key = "KeyOne",
                    Description = "This is the very first test.",
                    Status = TestStatus.Unknown,
                },
                new TestRecord()
                {
                    Key = "KeyTwo",
                    Description = "Another very good test. We like this one.",
                    Status = TestStatus.Running,
                },
                new TestRecord()
                {
                    Key = "KeyThree",
                    Description = "The third is a charmer. More testing please.",
                    Status = TestStatus.Successful,
                },
                new TestRecord()
                {
                    Key = "KeyFour",
                    Description = "Four more and out the door.",
                    Status = TestStatus.Failed,
                },
                new TestRecord()
                {
                    Key = "KeyFive",
                    Description = "Just one more test before we go.",
                    Status = TestStatus.Cancelled,
                },
                new TestRecord()
                {
                    Key = "KeySix",
                    Description = "That was not the last, but this one is.",
                    Status = TestStatus.Skipped,
                },
            };

            Tests = new ObservableCollection<TestRecord>(testList);

        }

        private void LogEvent(string msg)
        {
            LogEvents.Add(msg);
        }

        private void HookMessages()
        {
            MessengerInstance.Register<AnnouncementMessage>(this, (msg) =>
            {
            });
        }

        private async Task Run()
        {
            try
            {
                Reset();
                cancellationTokenSource = new CancellationTokenSource();
                TestingStatus = "Running";
                await Vendor.BeginTestingAsync();
                bool skipRemaining = false;
                DateTime startTime = DateTime.Now;
                LogEvent(string.Format("Testing {0}", Vendor.Name));
                LogEvent(string.Format("Start: {0}", startTime));
                LogEvent(string.Format("Vendor Username: {0}", Vendor.VendorWebsiteUsername));
                LogEvent(string.Format("Vendor Password: {0}", Vendor.VendorWebsitePassword));

                foreach (var test in Tests)
                {
                    LogEvent("");

                    if (skipRemaining)
                    {
                        test.Status = TestStatus.Skipped;
                        LogEvent(string.Format("Skipped Test: {0} - {1}", test.Key, test.Description));
                    }
                    else
                    {
                        LogEvent(string.Format("Begin Test: {0} - {1}", test.Key, test.Description));

                        test.Status = TestStatus.Running;
                        var result = await Vendor.RunTestAsync(test.VendorTest);
                        switch (result)
                        {
                            case TestResultCode.Successful:
                                test.Status = TestStatus.Successful;
                                PassedTestsCount++;
                                break;

                            case TestResultCode.Failed:
                                test.Status = TestStatus.Failed;
                                FailedTestsCount++;
                                break;
                        }

                        LogEvent(string.Format("Completed Test: {0} - {1}", test.Key, test.Status));

                        if (cancellationTokenSource.IsCancellationRequested)
                            skipRemaining = true;
                    }
                }


                DateTime endTime = DateTime.Now;

                LogEvent("");
                LogEvent(string.Format("End: {0}", endTime));
                LogEvent(string.Format("Duration: {0}", endTime - startTime));
                LogEvent("");

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    LogEvent("******* Cancelled by Operator *******");
                    LogEvent("");
                    TestingStatus = "Cancelled";
                }
                else
                    TestingStatus = "Finished";

                LogEvent("Final Results:");
                LogEvent(string.Format("     Total: {0}", TotalTestsCount));
                LogEvent(string.Format("     Passed: {0}", PassedTestsCount));
                LogEvent(string.Format("     Failed: {0}", FailedTestsCount));
                LogEvent(string.Format("     Skipped: {0}", TotalTestsCount - (PassedTestsCount + FailedTestsCount)));
                LogEvent("");

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                LogEvent(string.Format("Exception: {0}", Ex.Message));
            }

            if (Vendor.IsPerformingTests)
                Vendor.EndTesting();

            cancellationTokenSource = null;
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
            ShowGridCommand.RaiseCanExecuteChanged();
            ShowLogCommand.RaiseCanExecuteChanged();
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

            var tests = (from t in Vendor.GetTests()
                         select new TestRecord(t)).ToList();

            Tests = new ObservableCollection<TestRecord>(tests);
        }

        private void Reset()
        {
            TestingStatus = null;
            PopulateTests();
            LogEvents = new ObservableCollection<string>();
            TotalTestsCount = Tests.Count;
            PassedTestsCount = 0;
            FailedTestsCount = 0;

            InvalidateButtons();
        }

        #endregion


        #region Public Properties

        // Bind directly to Vendor object
        // IsScanning
        // IsPerformingTests

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

        private ObservableCollection<string> _logEvents = null;
        public ObservableCollection<string> LogEvents
        {
            get
            {
                return _logEvents;
            }

            set
            {
                if (_logEvents == value)
                {
                    return;
                }

                _logEvents = value;
                RaisePropertyChanged(() => LogEvents);
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

        private bool _isGridView = true;
        public bool IsGridView
        {
            get
            {
                return _isGridView;
            }

            set
            {
                if (_isGridView == value)
                {
                    return;
                }

                _isGridView = value;
                RaisePropertyChanged(() => IsGridView);
            }
        }

        private bool _isLogVIew = false;
        public bool IsLogView
        {
            get
            {
                return _isLogVIew;
            }

            set
            {
                if (_isLogVIew == value)
                {
                    return;
                }

                _isLogVIew = value;
                RaisePropertyChanged(() => IsLogView);
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
                    () => !Vendor.IsPerformingTests && Vendor.IsTestable));
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
                    () => Vendor.IsPerformingTests));
            }
        }

        private RelayCommand _showGridCommand;
        public RelayCommand ShowGridCommand
        {
            get
            {
                return _showGridCommand
                    ?? (_showGridCommand = new RelayCommand(
                    () =>
                    {
                        if (!ShowGridCommand.CanExecute(null))
                        {
                            return;
                        }

                        IsLogView = false;
                        IsGridView = true;
                    },
                    () => true));
            }
        }

        private RelayCommand _showLogCommand;
        public RelayCommand ShowLogCommand
        {
            get
            {
                return _showLogCommand
                    ?? (_showLogCommand = new RelayCommand(
                    () =>
                    {
                        if (!ShowLogCommand.CanExecute(null))
                        {
                            return;
                        }

                        IsGridView = false;
                        IsLogView = true;
                    },
                    () => true));
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
                    () => !Vendor.IsPerformingTests));
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

                        var log = LogEvents.ToList();
                        var suggestedName = string.Format("{0} Test Log.txt", Vendor.Name);
                        MessengerInstance.Send(new RequestExportTextFile(log, suggestedName));
                        
                    },
                    () => !Vendor.IsPerformingTests));
            }
        }

        #endregion


    }
}