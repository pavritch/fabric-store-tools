using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App
{
    public class StoreModel : ObservableObject, IStoreModel
    {
        public StoreModel(Store store, bool isImplemented)
        {
            Key = store.StoreType;
            Name = store.Name;
            ShortName = store.ShortName;
            WebsiteUrl = store.Url;

            // if false from the start, then no attempt will be made to go further and the store
            // will appear in the UI as nothing more than a visual placeholder (as disabled).
            IsFullyImplemented = isImplemented;

            // don't take IsFullyImplemented directly from StoreDescriptor becaue the value passed on 
            // ctor has potentially be adjusted to refect other factors - such as if has SQL support.

            // when not fully implemented, we skip all the initialization stuff
            IsInitialized = !IsFullyImplemented;

            Vendors = new ObservableCollection<IVendorModel>();

            if (!ViewModelBase.IsInDesignModeStatic)
            {
                HookMessages();
            }
        }

        private void Refresh()
        {
            if (!IsFullyImplemented)
            {
                ResetPropertyDefaultValues();
                return;
            }

            VendorCount = Vendors.Count;
            ProductCount = Vendors.Sum(e => e.ProductCount);
            ProductVariantCount = Vendors.Sum(e => e.ProductVariantCount);
            ClearanceProductCount = Vendors.Sum(e => e.ClearanceProductCount);
            ScanningVendorsCount = Vendors.Count(e => e.ScannerState ==  ScannerState.Scanning);
            SuspendedVendorsCount = Vendors.Count(e => e.ScannerState == ScannerState.Suspended);
            CommitsVendorsCount = Vendors.Count(e => e.ScannerState == ScannerState.Committable);
            DisabledVendorsCount = Vendors.Count(e => e.Status == VendorStatus.Disabled); 
        }

        private void ResetPropertyDefaultValues()
        {
            VendorCount = 0;
            ProductCount = 0;
            ProductVariantCount = 0;
            ClearanceProductCount = 0;
            ScanningVendorsCount = 0;
            SuspendedVendorsCount = 0;
            CommitsVendorsCount = 0;
            DisabledVendorsCount = 0;
        }

        private void HookMessages()
        {
            Messenger.Default.Register<VendorChangedNotification>(this, (msg) => Refresh());
            Messenger.Default.Register<ScanningOperationNotification>(this, (msg) => Refresh());
        }

        #region Public Properties

        /// <summary>
        /// Unique key to identify this store.
        /// </summary>
        /// <remarks>
        /// Required. Format as:  InsideFabric
        /// </remarks>
        private StoreType _storeKey = StoreType.InsideFabric;

        public StoreType Key
        {
            get
            {
                return _storeKey;
            }
            private set
            {
                Set(() => Key, ref _storeKey, value);
            }
        }


        /// <summary>
        /// Display name for this store.
        /// </summary>
        /// <remarks>
        /// Required.
        /// </remarks>
        private string _storeName = null;

        public string Name
        {
            get
            {
                return _storeName;
            }
            private set
            {
                Set(() => Name, ref _storeName, value);
            }
        }


        /// <summary>
        /// Short two-letter display name for this store.
        /// </summary>
        /// <remarks>
        /// Required.
        /// </remarks>
        private string _shortName = null;

        public string ShortName
        {
            get
            {
                return _shortName;
            }
            private set
            {
                Set(() => ShortName, ref _shortName, value);
            }
        }

        /// <summary>
        /// Url to home page. All lower case - http://www.insidefabric.com
        /// </summary>
        private string _websiteUrl = null;
        public string WebsiteUrl
        {
            get
            {
                return _websiteUrl;
            }
            private set
            {
                Set(() => WebsiteUrl, ref _websiteUrl, value);
            }
        }

        /// <summary>
        /// Indicates if this store is implemented to the point where the store model is operational. Required.
        /// </summary>
        /// <remarks>
        /// By operational, means won't blow up. Does not mean all vendors work, etc. If not operational,
        /// the UI will not do much of anything other than show the name of the store (but as disabled).
        /// </remarks>
        private bool _isFullyImplemented = false;

        public bool IsFullyImplemented
        {
            get
            {
                return _isFullyImplemented;
            }
            private set
            {
                Set(() => IsFullyImplemented, ref _isFullyImplemented, value);
            }
        }


        private int _vendorCount = 0;

        public int VendorCount
        {
            get
            {
                return _vendorCount;
            }
            private set
            {
                Set(() => VendorCount, ref _vendorCount, value);
            }
        }

        private ObservableCollection<IVendorModel> _vendors = null;

        public ObservableCollection<IVendorModel> Vendors
        {
            get
            {
                return _vendors;
            }
            private set
            {
                Set(() => Vendors, ref _vendors, value);
                VendorCount = (value == null) ? 0 : value.Count();
            }
        }


        /// <summary>
        /// Total number of products in store SQL.
        /// </summary>
        private int _productCount = 0;
        public int ProductCount
        {
            get
            {
                return _productCount;
            }
            private set
            {
                Set(() => ProductCount, ref _productCount, value);
            }
        }

        /// <summary>
        /// Total number of products in store SQL.
        /// </summary>
        private int _productVariantCount = 0;
        public int ProductVariantCount
        {
            get
            {
                return _productVariantCount;
            }
            private set
            {
                Set(() => ProductVariantCount, ref _productVariantCount, value);
            }
        }

        private int _clearanceProductCount = 0;
        public int ClearanceProductCount
        {
            get
            {
                return _clearanceProductCount;
            }
            private set
            {
                Set(() => ClearanceProductCount, ref _clearanceProductCount, value);
            }
        }

        /// <summary>
        /// Number of vendors presently scanning.
        /// </summary>
        private int _scanningVendorsCount = 0;
        public int ScanningVendorsCount
        {
            get
            {
                return _scanningVendorsCount;
            }
            private set
            {
                Set(() => ScanningVendorsCount, ref _scanningVendorsCount, value);
            }
        }


        /// <summary>
        /// Number of vendors presently suspended and can be resumed.
        /// </summary>
        private int _suspendedVendorsCount = 0;
        public int SuspendedVendorsCount
        {
            get
            {
                return _suspendedVendorsCount;
            }
            private set
            {
                Set(() => SuspendedVendorsCount, ref _suspendedVendorsCount, value);
            }
        }


        /// <summary>
        /// Number of vendors who have commits ready.
        /// </summary>
        private int _commitsVendorsCount = 0;

        public int CommitsVendorsCount
        {
            get
            {
                return _commitsVendorsCount;
            }
            private set
            {
                Set(() => CommitsVendorsCount, ref _commitsVendorsCount, value);
            }
        }


        /// <summary>
        /// Number of vendors who are disabled.
        /// </summary>
        private int _disabledVendorsCount = 0;
        public int DisabledVendorsCount
        {
            get
            {
                return _disabledVendorsCount;
            }
            private set
            {
                Set(() => DisabledVendorsCount, ref _disabledVendorsCount, value);
            }
        }

        /// <summary>
        /// For safety - don't allow much if anything unless we've been initialized.
        /// </summary>
        private bool _isInitialized = false;

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            private set
            {
                Set(() => IsInitialized, ref _isInitialized, value);
            }
        }



        /// <summary>
        /// True when any vendor in entire store is scanning.
        /// </summary>
        /// <returns></returns>
        public bool IsAnyScanning
        {
            get
            {
                return ScanningVendorsCount > 0;
            }
        }


        /// <summary>
        /// True when any vendor in entire store is scanning.
        /// </summary>
        /// <returns></returns>
        public bool IsAnySuspended
        {
            get
            {
                return SuspendedVendorsCount > 0;
            }
        }

        /// <summary>
        /// Number of vendors presently scanning or suspended.
        /// </summary>
        public int IsScanningOrSuspendedCount
        {
            get
            {
                if (Vendors == null)
                    return 0;

                return GetAllVendors().Where(e => e.IsScanning || e.IsSuspended).Count();
            }
        }

        #endregion



        /// <summary>
        /// Start running scanning operation.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAll(ScanOptions options)
        {
            var activeVendors = GetAllVendors().Where(e => e.IsScanningStartable && e.Status == Core.DataInterfaces.VendorStatus.AutoPilot).ToList();

            if (!activeVendors.Any())
                return true;

            var tasks = new List<Task<ScanningActionResult>>();
            foreach (var v in activeVendors)
                tasks.Add(v.StartScanning(options)); 

            await Task.WhenAll(tasks);

            //Debug.WriteLine(string.Format("Started Scanning: {0} of {1}", tasks.Where(e => e.Result == ScanningActionResult.Success).Count(), activeVendors.Count()));

            var isSuccess = tasks.All(e => e.Result == ScanningActionResult.Success);

            //if (!isSuccess)
            //    errMessage = string.Format("{0} of {1} operations failed to start.", tasks.Where(e => e.Result == ScanningActionResult.Success).Count(), activeVendors.Count());

            return isSuccess;          
        }

        /// <summary>
        /// Resume every running scanning operation.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ResumeAll()
        {
            var activeVendors = GetAllVendors().Where(e => e.IsSuspended).ToList();

            if (!activeVendors.Any())
                return true;

            var tasks = new List<Task<ScanningActionResult>>();
            foreach (var v in activeVendors)
                tasks.Add(v.ResumeScanning());

            await Task.WhenAll(tasks);

            var isSuccess = tasks.All(e => e.Result == ScanningActionResult.Success);

            return isSuccess;
        }

        private List<IVendorModel> GetAllVendors()
        {
            if (Vendors == null)
                return new List<IVendorModel>();

            return Vendors.ToList();
        }

        /// <summary>
        /// Cancel every running scanning operation.
        /// </summary>
        /// <remarks>
        /// Returns true if All associated operations cancellled successfully.
        /// </remarks>
        /// <returns></returns>
        public async Task<bool> CancelAll()
        {
            var activeVendors = GetAllVendors().Where(e => e.IsScanning || e.IsSuspended).ToList();

            if (!activeVendors.Any())
                return true;

            var tasks = new List<Task<ScanningActionResult>>();
            foreach (var v in activeVendors)
                tasks.Add(v.CancelScanning());

            await Task.WhenAll(tasks);

            var isSuccess = tasks.All(e => e.Result == ScanningActionResult.Success);

            return isSuccess;
        }

        /// <summary>
        /// Suspend every running scanning operation.
        /// </summary>
        /// <remarks>
        /// Returns true if All associated operations suspended successfully.
        /// </remarks>
        /// <returns></returns>
        public async Task<bool> SuspendAll()
        {
            var activeVendors = GetAllVendors().Where(e => e.IsScanning).ToList();

            if (!activeVendors.Any())
                return true;

            var tasks = new List<Task<ScanningActionResult>>();
            foreach (var v in activeVendors)
                tasks.Add(v.SuspendScanning());

            await Task.WhenAll(tasks);

            var isSuccess = tasks.All(e => e.Result == ScanningActionResult.Success);

            return isSuccess;
        }



        #region Initialization Logic
        /// <summary>
        /// Initializes this store instance.
        /// </summary>
        /// <remarks>
        /// Called from MainWindowViewModel when the splash screen is showing so we don't
        /// attempt to show any normal UX until we've populated our stores and vendors.
        /// </remarks>
        /// <returns>False to terminate application due to fatal error situation.</returns>
        public async Task<bool> InitializeAsync()
        {
            if (IsInitialized)
                return true;

            // failing to initialize is a drastic thing that will result in the app terminating itself,
            // so only return false here if truly unable to continue to run.

            try
            {
                var listVendors = new List<IVendorModel>();
                var vendorInitializationTasks = new List<Task>();

                foreach (var vendorInfo in GetDiscoveredVendors())
                {
                    // may need some extra logic - but the notion is that if we say it's not cooked, then it surely
                    // isn't, but we also need to know we've got SQL for it.
                    var isInSQL = await IsVendorInSQL(vendorInfo.Id);

                    var vendor = new VendorModel(this, vendorInfo, isInSQL);
                    listVendors.Add(vendor);

                    // the stores now also need to be initialized, which in turn will recursively
                    // initialize their associated vendors

                    // run all in parallel and wait for all to finish below
                    vendorInitializationTasks.Add(vendor.InitializeAsync());
                }

                await Task.WhenAll(vendorInitializationTasks);

                // failing initialization is 100% fatal to the app.

                // if there is a non-fatal issue, the recommended action is to set IsFullyImplemented to false
                // so the rest of the app will display normally but consider this store not fuly cooked rather
                // than to return as not initialized.

                IsInitialized = listVendors.All(e => e.IsInitialized);

                if (IsInitialized)
                {
                    Vendors = new ObservableCollection<IVendorModel>(listVendors);
                    Refresh();
                }
            }
            catch (Exception Ex)
            {
                // bomb the app - will terminate with a message
                Debug.WriteLine(Ex.Message);
                IsInitialized = false;
            }

            return IsInitialized;
        }

        /// <summary>
        /// Determine if this vendor has a record in SQL (both scanner and store).
        /// </summary>
        /// <remarks>
        /// The vendor must exist in the ASPDNSF Manufacturer table and scanner VendorData table.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <returns></returns>
        private async Task<bool> IsVendorInSQL(int manufacturerID)
        {
            try
            {
                var dbStore = App.GetInstance<IStoreDatabaseConnector>();
                return await dbStore.IsVendorInDatabaseAsync(Key, manufacturerID);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Fetch commit batches in descending order.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        public Task<List<CommitBatchSummary>> GetCommitBatchesAsync(int? skip = null, int? take = null)
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            return dbScanner.GetCommitBatchSummariesAsync(Key, skip, take);
        }

        #endregion

        private List<Vendor> GetDiscoveredVendors()
        {
            return Vendor.GetAll().Where(x => x.Store == Key).OrderBy(x => x.DisplayName).ToList();
        }
    }
}
