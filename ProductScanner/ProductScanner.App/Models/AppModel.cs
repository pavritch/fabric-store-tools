using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.Core;
using ProductScanner.Core.Scanning;

namespace ProductScanner.App
{
    /// <summary>
    /// Root level model for stores and vendors. Very little logic - mostly a container for 
    /// collection of stores.
    /// </summary>
    /// <remarks>
    /// As new stores are to be supported, just add them to the list of known stores below.
    /// </remarks>
    public class AppModel :  ObservableObject, IAppModel
    {
        private readonly IStoreDatabaseConnector _storeDatabaseConnector;

        public AppModel(IStoreDatabaseConnector storeDatabaseConnector)
        {
            _storeDatabaseConnector = storeDatabaseConnector;
            Stores = new ObservableCollection<IStoreModel>();
            IsInitialized = false;
        }

        #region Public Properties
        private ObservableCollection<IStoreModel> _stores = null;
        public ObservableCollection<IStoreModel> Stores
        {
            get
            {
                return _stores;
            }
            private set
            {
                Set(() => Stores, ref _stores, value);
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


        private List<IVendorModel> GetAllVendors()
        {
            if (Stores == null)
                return new List<IVendorModel>();

            return Stores.SelectMany(e => e.Vendors).ToList();
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
            var activeVendors = GetAllVendors().Where(e => e.IsScanningCancellable).ToList();

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
            var activeVendors = GetAllVendors().Where(e => e.IsScanningSuspendable).ToList();

            if (activeVendors.Count() == 0)
                return true;

            var tasks = new List<Task<ScanningActionResult>>();
            foreach (var v in activeVendors)
                tasks.Add(v.SuspendScanning());

            await Task.WhenAll(tasks);

            var isSuccess = tasks.All(e => e.Result == ScanningActionResult.Success);

            return isSuccess;
        }


        /// <summary>
        /// True when any vendor in entire app is scanning.
        /// </summary>
        /// <returns></returns>
        public bool IsAnyScanning
        {
            get
            {
                return IsScanningCount > 0;
            }
        }

        /// <summary>
        /// Number of vendors presently scanning.
        /// </summary>
        public int IsScanningCount
        {
            get
            {
                if (Stores == null)
                    return 0;

                return GetAllVendors().Count(e => e.IsScanning);
            }
        }


        /// <summary>
        /// Number of vendors presently scanning or suspended.
        /// </summary>
        public int IsScanningOrSuspendedCount
        {
            get
            {
                if (Stores == null)
                    return 0;

                return GetAllVendors().Count(e => e.IsScanning || e.IsSuspended);
            }
        }

        /// <summary>
        /// How many vendors have warnings showing.
        /// </summary>
        public int VendorsWithWarningsCount
        {
            get
            {
                if (Stores == null)
                    return 0;

                return GetAllVendors().Where(e => e.HasWarning).Count();
            }
        }

        #endregion

        #region Initialization Logic

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// Called from MainWindowViewModel when the splash screen is showing so we don't
        /// attempt to show any normal UX until we've populated our stores and vendors.
        /// </remarks>
        /// <returns>False to terminate application due to fatal error situation.</returns>
        public async Task<bool> InitializeAsync()
        {
            // ensure executed only once - just in case, but only expect to be called once at app start.

            if (IsInitialized)
                return true;

            // failing to initialize is a drastic thing that will result in the app terminating itself,
            // so only return false here if truly unable to continue to run.

            try
            {
                var listStores = new List<IStoreModel>();
                var storeInitializationTasks = new List<Task>();

                var knownStores = StoreExtensions.GetAll();
                foreach (var storeInfo in knownStores)
                {
                    // may need some extra logic - but the notion is that if we say it's not cooked, then it surely
                    // isn't, but we also need to know we've got SQL for it.

                    var isImplemented = await IsStoreSQLDatabaseAvailable(storeInfo.StoreType);

                    var store = new StoreModel(storeInfo, isImplemented);
                    listStores.Add(store);

                    // the stores now also need to be initialized, which in turn will recursively
                    // initialize their associated vendors

                    // run all in parallel and wait for all to finish below
                    storeInitializationTasks.Add(store.InitializeAsync());
                }

                await Task.WhenAll(storeInitializationTasks);

                // failing initialization is 100% fatal to the app.

                // if there is a non-fatal issue, the recommended action is to set IsFullyImplemented to false
                // so the rest of the app will display normally but consider this store not fuly cooked rather
                // than to return as not initialized.

                IsInitialized = listStores.All(e => e.IsInitialized == true);

                if (IsInitialized)
                    Stores = new ObservableCollection<IStoreModel>(listStores);
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
        /// Determine if a SQL database connection is available for this store.
        /// </summary>
        /// <remarks>
        /// Can be as simple as seeing if can read just a single row from a stock table, just
        /// to see if succeeds or throws exception.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <returns></returns>
        private Task<bool> IsStoreSQLDatabaseAvailable(StoreType storeKey)
        {
            return _storeDatabaseConnector.IsDatabaseAvailableAsync(storeKey);
        } 
        #endregion
    }
}
