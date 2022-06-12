using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline;
using Utilities;

namespace ProductScanner.App.ViewModels
{

    public class VendorScanViewModel : VendorContentPageViewModel
    {

        #region ScanOptionViewModel Class

        /// <summary>
        /// Model used for a single scanner option (options tab).
        /// </summary>
        public class ScanOptionViewModel : ViewModelBase
        {

            private ScanOptions _option = default(ScanOptions);
            public ScanOptions Option
            {
                get
                {
                    return _option;
                }
                set
                {
                    Set(() => Option, ref _option, value);
                }
            }

            private bool _isChecked = false;
            public bool IsChecked
            {
                get
                {
                    return _isChecked;
                }
                set
                {
                    Set(() => IsChecked, ref _isChecked, value);
                }
            }

            private bool _isEnabled = false;
            public bool IsEnabled
            {
                get
                {
                    return _isEnabled;
                }
                set
                {
                    Set(() => IsEnabled, ref _isEnabled, value);
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
                    Set(() => Description, ref _description, value);
                }
            }

            public ScanOptionViewModel(ScanOptions opt, bool isChecked=false)
            {
                Option = opt;
                IsChecked = isChecked;
                IsEnabled = true;
                Description = opt.DescriptionAttr();
            }

        }

        #endregion        

        DateTime? lastChangeMaxDialValue = null;
        double? lastMaxDialValue = null;

#if DEBUG
        public VendorScanViewModel()
            : this(new DesignVendorModel() { Name = "Kravet", VendorId = 5 })
        {
            if (IsInDesignMode)
            {
                
                TimeLastCheckpoint = DateTime.Now.ToString();
                ScanRuntime = TimeSpan.FromHours(4.2).ToString();
                TimeScanStarted = DateTime.Now.ToString();
                TimeLastCheckpoint = DateTime.Now.ToString();
                IsIndeterminateProgress = false;
                ProgressMeterValue = 30;
                ScanErrorCount = 10.ToString();
                RequestsPerMinuteMeterMaxValue = 200;
                RequestsPerMinuteMeterValue = 60;
            }
        }
#endif

        public VendorScanViewModel(IVendorModel vendor)
            : base(vendor)
        {
            RequiresToBeCached = true;
            PageType = ContentPageTypes.VendorScan;
            PageSubTitle = "Scan Products";
            BreadcrumbTemplate = "{Home}/{Store}/{Vendor}/Scan";
            IsNavigationJumpTarget = true;

            PopulateOptions();
            //if (Vendor is VendorModel)
            //{
            //    var vendorModel = Vendor as VendorModel;
            //    vendorModel.PopulateFakeScanningData();
            //}

            RequestsPerMinuteMeterMaxValue = CaclRequestsPerMinuteMaxValue(0);

            if (!IsInDesignMode)
            {
                HookMessages();
            }

            UpdateScanningStartTime();
            UpdateLastCheckpoint();
            UpdateDuration();
            UpdateErrorCount();
            UpdateProgressPercentComplete();
        }

        #region Local Methods

        private void InvalidateButtons()
        {
            StartCommand.RaiseCanExecuteChanged();
            SuspendCommand.RaiseCanExecuteChanged();
            ResumeCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            ExportLogCommand.RaiseCanExecuteChanged();
            ClearLogCommand.RaiseCanExecuteChanged();
        }

        private void UpdateScanningStartTime()
        {
            if (Vendor.ScanningStartTime.HasValue)
            {
                TimeScanStarted = Vendor.ScanningStartTime.Value.ToString();
            }
            else
            {
                TimeScanStarted = "n/a";
            }
        }

        private void UpdateLastCheckpoint()
        {
            if (Vendor.LastCheckpointDate.HasValue)
            {
                TimeLastCheckpoint = Vendor.LastCheckpointDate.Value.ToString();
            }
            else if (Vendor.IsScanning || Vendor.ScanningStartTime.HasValue)
            {
                TimeLastCheckpoint = "None";
            }
            else
            {
                TimeLastCheckpoint = "n/a";
            }
        }

        private void UpdateDuration()
        {
            if (Vendor.ScanningDuration.HasValue)
            {
                ScanRuntime = Vendor.ScanningDuration.Value.ToString(@"d\.hh\:mm\:ss");
            }
            else
            {
                ScanRuntime = "n/a";
            }
        }

        private void UpdateErrorCount()
        {
            if (Vendor.ScanningErrorCount.HasValue)
                ScanErrorCount = Vendor.ScanningErrorCount.GetValueOrDefault().ToString("N0");
            else
                ScanErrorCount = "n/a";
        }

        private void UpdateProgressPercentComplete()
        {
            // if neg one, then change meter to indeterminate
            if (!Vendor.ScanningProgressPercentComplete.HasValue)
            {
                IsIndeterminateProgress = false;
                ProgressMeterValue = 0;
            }
            if (Vendor.ScanningProgressPercentComplete == -1)
            {
                IsIndeterminateProgress = true;
                ProgressMeterValue = 0;
            }
            else
            {
                IsIndeterminateProgress = false;
                ProgressMeterValue = Vendor.ScanningProgressPercentComplete.GetValueOrDefault();
            }
        }

        protected override void VendorPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            // deal with vendor model properties we need to proxy through local properties

            bool requiresInvalidate = true;

            switch(e.PropertyName)
            {
                case "ScanningOperationStatus":
                    switch(Vendor.ScanningOperationStatus)
                    {
                        case ScanningStatus.Running:
                        case ScanningStatus.RunningWithErrors:
                        case ScanningStatus.Suspended:
                            SetOptionsEnabledState(false);
                            break;

                        default:
                            SetOptionsEnabledState(true);
                            break;
                    }
                    break;

                case "ScanningProgressPercentComplete":
                    UpdateProgressPercentComplete();
                    break;

                case "ScanningStartTime":
                    UpdateScanningStartTime();
                    break;

                case "ScanningDuration":
                    UpdateDuration();
                    break;

                case "ScanningRequestsPerMinute":
                    RequestsPerMinuteMeterValue = Vendor.ScanningRequestsPerMinute; 
                    break;

                case "ScanningErrorCount":
                    UpdateErrorCount();
                    break;

                case "ScanningOptions":
                    // could have rehydrated from checkpoint, but also could be set when
                    // starting out a new scan operation.
                    break;

                case "LastCheckpointDate":
                    UpdateLastCheckpoint();
                    break;

                case "IsCallingScanningOperation":
                    // in the process of invoking one of Start/Cancel/Resume/Suspend
                    // this is the middle state where we don't have a status back yet
                    // so no buttons should be enabled until fully transitioned or not
                    // to the next state.
                    break;

                case "IsScanning":
                    break;

                case "IsSuspended":
                    break;

                case "IsPerformingTests":
                    break;

                case "IsCheckingCredentials":
                    break;

                case "Status":
                    break;

                default:
                    requiresInvalidate = false;
                    break;
            }

            if (requiresInvalidate)
                InvalidateButtons();
        }

        private void HookMessages()
        {
            MessengerInstance.Register<VendorChangedNotification>(this, (msg) =>
            {
                if (!msg.Vendor.Equals(Vendor))
                    return;

                InvalidateButtons();
            });

            MessengerInstance.Register<ScanningOperationNotification>(this, (msg) =>
            {
                if (!msg.Vendor.Equals(Vendor))
                    return;

                switch(msg.ScanningEvent)
                {
                    case ScanningOperationEvent.CachedFilesChanged:
                        break;

                    case ScanningOperationEvent.Started:
                        PopulateOptions();
                        break;
                }
                InvalidateButtons();
            });

        }

        /// <summary>
        /// Given a current requests per minute, determine what should be the max value on the dial
        /// </summary>
        /// <remarks>
        /// Notice how it maintains state to debounce dial indicator.
        /// </remarks>
        /// <param name="rpm"></param>
        /// <returns></returns>
        private double CaclRequestsPerMinuteMaxValue(double rpm)
        {
            if (lastChangeMaxDialValue.HasValue)
            {
                // don't change too often, unless at the limit
                if (DateTime.Now - lastChangeMaxDialValue < TimeSpan.FromMilliseconds(15000) && rpm < lastMaxDialValue.Value * .95)
                    return lastMaxDialValue.Value;
            }

            double calcValue;

            if (RequestsPerMinuteMeterMaxValue <= 60 && rpm < 15)
                calcValue = 30.0;
            else if (RequestsPerMinuteMeterMaxValue <= 120 && rpm < 45)
                calcValue = 60.0;
            else if (RequestsPerMinuteMeterMaxValue <= 300 && rpm < 175)
                calcValue = 200.0;
            else if (rpm < 400)
                calcValue = 500.00;
            else if (rpm < 800)
                calcValue = 1000.00;
            else if (rpm < 2000)
                calcValue = 3000.00;
            else 
                calcValue = 10000.0;

            lastMaxDialValue = calcValue;
            lastChangeMaxDialValue = DateTime.Now;

            return calcValue;
        }



        private void PopulateOptions()
        {
            // create view models for complete set of all possible options, but only
            // set as enabled the ones identified as current by the model.

            // model options are locked when IsScanning|IsSuspended.

            var list = new List<ScanOptionViewModel>();

            foreach (var opt in LibEnum.GetValues<ScanOptions>().Where(x => x != ScanOptions.None))
                list.Add(new ScanOptionViewModel(opt, Vendor.ScanningOptions.Contains(opt)));

            Options = new ObservableCollection<ScanOptionViewModel>(list);
            SetOptionsEnabledState(!(Vendor.IsScanning || Vendor.IsSuspended));
        }

        private void SetOptionsEnabledState(bool isEnabled)
        {
            if (Options == null)
                return;

            foreach (var item in Options)
                item.IsEnabled = isEnabled;
        }

        #endregion


        #region Public Properties

        private ObservableCollection<ScanOptionViewModel> _options = null;
        public ObservableCollection<ScanOptionViewModel> Options
        {
            get
            {
                return _options;
            }
            set
            {
                Set(() => Options, ref _options, value);
            }
        }


        /// <summary>
        /// The maximum value for the radial meter.
        /// </summary>
        /// <remarks>
        /// We want to set specifically so does not bounce around too much.
        /// </remarks>
        private double _requestsPerMinuteMeterMaxValue = 0.0;
        public double RequestsPerMinuteMeterMaxValue
        {
            get
            {
                return _requestsPerMinuteMeterMaxValue;
            }
            set
            {
                Set(() => RequestsPerMinuteMeterMaxValue, ref _requestsPerMinuteMeterMaxValue, value);
            }
        }

        /// <summary>
        /// The current requests per minute for the scanning operation.
        /// </summary>
        private double _requestsPerMinuteMeterValue = 0.0;
        public double RequestsPerMinuteMeterValue
        {
            get
            {
                return _requestsPerMinuteMeterValue;
            }
            set
            {
                if (Set(() => RequestsPerMinuteMeterValue, ref _requestsPerMinuteMeterValue, value))
                {
                    RequestsPerMinuteMeterMaxValue = CaclRequestsPerMinuteMaxValue(value);
                }
            }
        }

        /// <summary>
        /// When true, the progress meter is presented in indeterminate mode.
        /// </summary>
        private bool _isIndeterminateProgress = false;
        public bool IsIndeterminateProgress
        {
            get
            {
                return _isIndeterminateProgress;
            }
            set
            {
                Set(() => IsIndeterminateProgress, ref _isIndeterminateProgress, value);
            }
        }

        private double _progressMeterValue = 0.0;
        public double ProgressMeterValue
        {
            get
            {
                return _progressMeterValue;
            }
            set
            {
                Set(() => ProgressMeterValue, ref _progressMeterValue, value);
            }
        }


        private string _timeScanStarted = null;
        public string TimeScanStarted
        {
            get
            {
                return _timeScanStarted;
            }
            set
            {
                Set(() => TimeScanStarted, ref _timeScanStarted, value);
            }
        }

        private string _timeLastCheckpoint = null;
        public string TimeLastCheckpoint
        {
            get
            {
                return _timeLastCheckpoint;
            }
            set
            {
                Set(() => TimeLastCheckpoint, ref _timeLastCheckpoint, value);
            }
        }

        private string _scanRuntime = null;
        public string ScanRuntime
        {
            get
            {
                return _scanRuntime;
            }
            set
            {
                Set(() => ScanRuntime, ref _scanRuntime, value);
            }
        }

        private string _scanErrorCount = null;
        public string ScanErrorCount
        {
            get
            {
                return _scanErrorCount;
            }
            set
            {
                Set(() => ScanErrorCount, ref _scanErrorCount, value);
            }
        }


        #endregion

        #region Commands

        private RelayCommand _startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand
                    ?? (_startCommand = new RelayCommand(
                    async () =>
                    {
                        if (!StartCommand.CanExecute(null))
                        {
                            return;
                        }

                        if ( Vendor.HasPendingCommitBatches)
                        {
                            var prompt = string.Format("Discard {0:N0} pending batches?", Vendor.PendingCommitBatchCount);
                            var activity = new ConfirmationPromptActivity(prompt);

                            var okayToDiscard = await activity.Show(activity) == ActivityResult.Success;

                            if (!okayToDiscard)
                                return;

                            // if user accepts, then just drop through... the model code will do the discard automatically
                        }

                        var options = Options.Where(e => e.IsChecked).Select(x => x.Option).Cast<int>().Sum();
                        var result = await Vendor.StartScanning((ScanOptions)options);

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to start new scanning operation.");
                    },
                    () => Vendor.IsScanningStartable));
            }
        }

        private RelayCommand _suspendCommand;
        public RelayCommand SuspendCommand
        {
            get
            {
                return _suspendCommand
                    ?? (_suspendCommand = new RelayCommand(
                    async () =>
                    {
                        if (!SuspendCommand.CanExecute(null))
                        {
                            return;
                        }

                        var result = await Vendor.SuspendScanning();

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to suspend current scanning operation.");
                        
                    },
                    () => Vendor.IsScanningSuspendable));
            }
        }

        private RelayCommand _resumeCommand;
        public RelayCommand ResumeCommand
        {
            get
            {
                return _resumeCommand
                    ?? (_resumeCommand = new RelayCommand(
                    async () =>
                    {
                        if (!ResumeCommand.CanExecute(null))
                        {
                            return;
                        }

                        var result = await Vendor.ResumeScanning();

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to resume current scanning operation.");


                    },
                    () => Vendor.IsScanningResumable));
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    async () =>
                    {
                        if (!CancelCommand.CanExecute(null))
                        {
                            return;
                        }

                        var result = await Vendor.CancelScanning();

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to cancel current scanning operation.");

                    },
                    () => Vendor.IsScanningCancellable));
            }
        }

        private RelayCommand _exportLogCommand;
        public RelayCommand ExportLogCommand
        {
            get
            {
                return _exportLogCommand
                    ?? (_exportLogCommand = new RelayCommand(
                    () =>
                    {
                        if (!ExportLogCommand.CanExecute(null))
                        {
                            return;
                        }

                        if (Vendor.ScanningLogEvents.Count == 0)
                        {
                            App.Current.ReportErrorAlert("Log is empty.");
                            return;
                        }

                        var logText = Vendor.ScanningLogEvents.Select(e => e.Text).ToList();
                        var suggestedName = string.Format("{0} Scan Log.txt", Vendor.Name);
                        MessengerInstance.Send(new RequestExportTextFile(logText, suggestedName));
                    },
                    () => true));
            }
        }

        private RelayCommand _clearLogCommand;

        /// <summary>
        /// Gets the ClearLogCommand.
        /// </summary>
        /// <remarks>
        /// As implemented, clears more than just log. Also resets/clears to full idle state.
        /// </remarks>
        public RelayCommand ClearLogCommand
        {
            get
            {
                return _clearLogCommand
                    ?? (_clearLogCommand = new RelayCommand(
                    () =>
                    {
                        if (!ClearLogCommand.CanExecute(null))
                        {
                            return;
                        }

                        Debug.Assert(!Vendor.IsScanning);
                        Debug.Assert(!Vendor.IsSuspended);

                        Vendor.ClearScanningState();
                    },
                    () => Vendor.IsScanningLogClearable));
            }
        }
        #endregion

    }
}