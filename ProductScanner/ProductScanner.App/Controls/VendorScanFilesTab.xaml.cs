using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ProductScanner.App.ViewModels;
using Utilities.Extensions;

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Interaction logic for VendorScanFilesTab.xaml
    /// </summary>
    public partial class VendorScanFilesTab : UserControl
    {
        public VendorScanFilesTab()
        {
            InitializeComponent();
        }

        private VendorScanFilesTabViewModel VM
        {
            get
            {
                return this.DataContext as VendorScanFilesTabViewModel;
            }
        }

        #region Vendor Property
        /// <summary>
        /// The <see cref="Vendor" /> dependency property's name.
        /// </summary>
        public const string VendorPropertyName = "Vendor";

        /// <summary>
        /// Gets or sets the value of the <see cref="Vendor" />
        /// property. This is a dependency property.
        /// </summary>
        public IVendorModel Vendor
        {
            get
            {
                return (IVendorModel)GetValue(VendorProperty);
            }
            set
            {
                SetValue(VendorProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Vendor" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty VendorProperty = DependencyProperty.Register(
            VendorPropertyName,
            typeof(IVendorModel),
            typeof(VendorScanFilesTab),
        new UIPropertyMetadata(null, new PropertyChangedCallback(VendorChanged)));

        protected static void VendorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vendor = e.NewValue as IVendorModel;
            var ctrl = (VendorScanFilesTab)d;
            ctrl.VM.SetVendor(vendor);
        }
        #endregion
    }
}


namespace ProductScanner.App.ViewModels
{
    public class VendorScanFilesTabViewModel : ViewModelBase
    {
        private FileStorageMetrics staticFilesMetrics = null;
        private FileStorageMetrics cachedFilesMetrics = null;

        public VendorScanFilesTabViewModel()
        {
            if (IsInDesignMode)
            {
                SetVendor(new DesignVendorModel() { Name = "Kravet", VendorId = 5 });
            }
            else
            {
                HookMessages();
            }
        }

        public void SetVendor(IVendorModel vendor)
        {
            this.Vendor = vendor;
            Refresh();
        }

        #region Local Methods

        private async void Refresh()
        {
            var tasks = new List<Task>()
            {
                PopulateCachedFiles(false),
                PopulateStaticFiles(false),
            };

            await Task.WhenAll(tasks);

            InvalidateCommands();
        }

        private void InvalidateCommands()
        {
            DeleteCachedFilesCommand.RaiseCanExecuteChanged();
            RefreshCachedFilesCommand.RaiseCanExecuteChanged();
            RefreshStaticFilesCommand.RaiseCanExecuteChanged();
        }

        private List<dynamic> MakeFileMetrics(FileStorageMetrics metrics)
        {
            var dic = new Dictionary<string, string>()
            {
                {"Total Files", metrics.TotalFiles.ToString("N0")},
                {"Total Size", metrics.TotalSize.ToFileSize()},
                {"Oldest", metrics.Oldest.HasValue ? metrics.Oldest.Value.ToString() : "--" },
                {"Newest", metrics.Newest.HasValue ? metrics.Newest.Value.ToString() : "--" },
            };

            return dic.ToDynamicNameValueList();
        }

        private List<dynamic> MakeCalculatingMetrics()
        {
            var dic = new Dictionary<string, string>()
            {
                {"Total Files", "calculating..."},
                {"Total Size", ""},
                {"Oldest", "" },
                {"Newest", "" },
            };

            return dic.ToDynamicNameValueList();
        }

        private List<dynamic> MakeMissingMetrics()
        {
            var dic = new Dictionary<string, string>()
            {
                {"Total Files", "(folder does not exist)"},
                {"Total Size", ""},
                {"Oldest", "" },
                {"Newest", "" },
            };

            return dic.ToDynamicNameValueList();
        }


        private async Task PopulateStaticFiles(bool recalculate)
        {
            if (Vendor == null)
                return;


            IsPopulatingStaticFiles = true;
            InvalidateCommands();

            // temporarily put something up in place of metrics
            if (recalculate || !Vendor.HasStaticFileStorageMetrics)
                StaticFilesMetrics = new ObservableCollection<dynamic>(MakeCalculatingMetrics());

            if (!IsInDesignMode && Vendor.Vendor.UsesStaticFiles)
                staticFilesMetrics = await Vendor.GetStaticFileStorageMetricsAsync(recalculate);

            if (staticFilesMetrics != null)
            {
                var list = MakeFileMetrics(staticFilesMetrics);
                StaticFilesMetrics = new ObservableCollection<dynamic>(list);
            }
            else // if no folder, will come back null, so show that
                StaticFilesMetrics = new ObservableCollection<dynamic>(MakeMissingMetrics());

            IsPopulatingStaticFiles = false;
            InvalidateCommands();
        }

        private void HookMessages()
        {
            MessengerInstance.Register<ScanningOperationNotification>(this, async (msg) =>
            {
                if (Vendor == null || !Vendor.Equals(msg.Vendor))
                    return;

                switch (msg.ScanningEvent)
                {
                    case ScanningOperationEvent.CachedFilesChanged:
                        await PopulateCachedFiles(false);
                        break;
                }
            });
        }

        private async Task PopulateCachedFiles(bool recalculate)
        {
            if (Vendor == null)
                return;

            IsPopulatingCachedFiles = true;
            InvalidateCommands();

            // temporarily put something up in place of metrics
            if (recalculate || !Vendor.HasCachedFileStorageMetrics)
                CachedFilesMetrics = new ObservableCollection<dynamic>(MakeCalculatingMetrics());

            cachedFilesMetrics = await Vendor.GetCachedFileStorageMetricsAsync(recalculate);
            if (cachedFilesMetrics != null)
            {
                var list = MakeFileMetrics(cachedFilesMetrics);
                CachedFilesMetrics = new ObservableCollection<dynamic>(list);
            }
            else // if no folder, will come back null, so show that
                CachedFilesMetrics = new ObservableCollection<dynamic>(MakeMissingMetrics());

            IsPopulatingCachedFiles = false;
            InvalidateCommands();
        }

        /// <summary>
        /// Clean up cached files.
        /// </summary>
        public static async void DeleteCachedFilesActivity(IVendorModel vendor)
        {

            var inputsControl = new ProductScanner.App.Controls.DeleteCachedFilesInputs();
            var inputs = inputsControl.DataContext as DeleteCachedFilesInputsViewModel;

            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = "Delete Cached Files?",
                IsIndeterminateProgress = true,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = false,
                CustomElement = inputsControl,
                IsAcceptanceDisabled = false,

                OnAccept = async (a) =>
                {
                    inputs.IsDisabled = true;

                    try
                    {
                        // this first condition is wrong, since it does not take into account how many files escaped being
                        // deleted due to being out of bounds of the filter criteria.
                        //if (cachedFilesMetrics != null)
                        //    a.StatusMessage = string.Format("Deleting {0:N0} files...", cachedFilesMetrics.TotalFiles);
                        //else
                        a.StatusMessage = "Deleting files...";

                        var result = await vendor.DeleteCachedFilesAsync(inputs.DayCount, a.CancelToken, null);

                        // if was cancelled by user, then we don't need to set cancel here

                        if (!a.CancelToken.IsCancellationRequested)
                        {
                            if (result == ActivityResult.Success)
                                a.SetCompleted(ActivityResult.Success, "Finished deleting cached files.");
                            else
                                a.SetCompleted(ActivityResult.Failed, "Error deleting files. Operation terminated.");
                        }
                    }
                    catch (Exception Ex)
                    {
                        a.SetCompleted(ActivityResult.Failed, Ex.Message);
                    }
                    finally
                    {
                        // finalize the task, return from original await
                        a.FinishUp();
                    }
                },

                OnCancel = (a) =>
                {
                    // typically won't need to do anything here
                    // FinishUp() called automatically upon return from this method.
                },
            };


            await activity.Show(activity);
        }


        #endregion


        #region Public Properties

        private IVendorModel _vendor = null;
        public IVendorModel Vendor
        {
            get
            {
                return _vendor;
            }
            set
            {
                var old = _vendor;

                if (Set(() => Vendor, ref _vendor, value))
                {
                    if (old != null && old.GetType().Implements<INotifyPropertyChanged>())
                        (old as INotifyPropertyChanged).PropertyChanged -= Vendor_PropertyChanged;

                    if (value != null && value.GetType().Implements<INotifyPropertyChanged>())
                        (value as INotifyPropertyChanged).PropertyChanged += Vendor_PropertyChanged;
                }

                if (!IsInDesignMode)
                {
                    UsesStaticFiles = _vendor.Vendor.UsesStaticFiles;
                }
            }
        }


        void Vendor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsScanning")
            {

            }

            InvalidateCommands();
        }


        private ObservableCollection<dynamic> _staticFilesMetrics = null;
        public ObservableCollection<dynamic> StaticFilesMetrics
        {
            get
            {
                return _staticFilesMetrics;
            }
            set
            {
                Set(() => StaticFilesMetrics, ref _staticFilesMetrics, value);
            }
        }


        private ObservableCollection<dynamic> _cachedFilesMetrics = null;
        public ObservableCollection<dynamic> CachedFilesMetrics
        {
            get
            {
                return _cachedFilesMetrics;
            }
            set
            {
                Set(() => CachedFilesMetrics, ref _cachedFilesMetrics, value);
            }
        }

        private bool _isPopulatingStaticFiles = false;
        public bool IsPopulatingStaticFiles
        {
            get
            {
                return _isPopulatingStaticFiles;
            }
            set
            {
                Set(() => IsPopulatingStaticFiles, ref _isPopulatingStaticFiles, value);
            }
        }

        private bool _isPopulatingCachedFiles = false;
        public bool IsPopulatingCachedFiles
        {
            get
            {
                return _isPopulatingCachedFiles;
            }
            set
            {
                Set(() => IsPopulatingCachedFiles, ref _isPopulatingCachedFiles, value);
            }
        }

        private bool _usesStaticFiles = false;
        public bool UsesStaticFiles
        {
            get
            {
                return _usesStaticFiles;
            }
            set
            {
                Set(() => UsesStaticFiles, ref _usesStaticFiles, value);
            }
        }
        #endregion

        #region Commands

        private RelayCommand _deleteCachedFilesCommand;
        public RelayCommand DeleteCachedFilesCommand
        {
            get
            {
                return _deleteCachedFilesCommand
                    ?? (_deleteCachedFilesCommand = new RelayCommand(
                    () =>
                    {
                        if (!DeleteCachedFilesCommand.CanExecute(null))
                        {
                            return;
                        }

                        DeleteCachedFilesActivity(Vendor);
                    },
                    () => Vendor != null && Vendor.IsFileCacheClearable && !IsPopulatingCachedFiles));
            }
        }

        /// <summary>
        /// Open a disk folder.
        /// </summary>
        private RelayCommand<string> _openFolderCommand;
        public RelayCommand<string> OpenFolderCommand
        {
            get
            {
                return _openFolderCommand
                    ?? (_openFolderCommand = new RelayCommand<string>(
                    folderPath =>
                    {

                        if (string.IsNullOrEmpty(folderPath))
                        {
                            App.Current.ReportErrorAlert("Folder does not exist.");
                            return;
                        }

                        // folderPath is passed in from the source binding
                        MessengerInstance.Send(new RequestOpenFileOrFolder(folderPath));
                    }));
            }
        }


        private RelayCommand _refreshCachedFilesCommand;
        public RelayCommand RefreshCachedFilesCommand
        {
            get
            {
                return _refreshCachedFilesCommand
                    ?? (_refreshCachedFilesCommand = new RelayCommand(
                    async () =>
                    {
                        if (!RefreshCachedFilesCommand.CanExecute(null))
                            return;

                        await PopulateCachedFiles(true);

                    },
                    () => Vendor != null && !IsPopulatingCachedFiles && !string.IsNullOrEmpty(Vendor.CachedFilesFolder)));
            }
        }


        private RelayCommand _refreshStaticFilesCommand;
        public RelayCommand RefreshStaticFilesCommand
        {
            get
            {
                return _refreshStaticFilesCommand
                    ?? (_refreshStaticFilesCommand = new RelayCommand(
                    async () =>
                    {
                        if (!RefreshStaticFilesCommand.CanExecute(null))
                        {
                            return;
                        }

                        await PopulateStaticFiles(true);
                    },
                    () => Vendor != null && UsesStaticFiles && !IsPopulatingStaticFiles && !string.IsNullOrEmpty(Vendor.StaticFilesFolder)));
            }
        }



        #endregion


    }
}

