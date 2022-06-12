using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.Core;

namespace ProductScanner.App
{
    public class DesignAppModel : ObservableObject, IAppModel
    {
        public DesignAppModel()
        {
            PopulateStores();
        }

        private void PopulateStores()
        {
            var listStores = new List<IStoreModel>()
            {
                MakeStore(StoreType.InsideFabric, "IF"),
                MakeStore(StoreType.InsideRugs, "IR"),
                MakeStore(StoreType.InsideAvenue, "IH"),
            };

            Stores = new ObservableCollection<IStoreModel>(listStores);
        }

        private IStoreModel MakeStore(StoreType name, string shortName, bool isFullyImplemented = true)
        {
            return new DesignStoreModel()
            {
                Key = name,
                // TODO: Description
                Name = name.ToString(),
                ShortName = shortName,
                IsFullyImplemented = isFullyImplemented,
                WebsiteUrl = "http://www.insidefabric.com"
            };
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


        private ObservableCollection<IStoreModel> _stores = null;

        /// <summary>
        /// Collection of stores.
        /// </summary>
        public ObservableCollection<IStoreModel> Stores
        {
            get
            {
                return _stores;
            }
            set
            {
                Set(() => Stores, ref _stores, value);
            }
        }

        /// <summary>
        /// Populate the model with stores and vendors.
        /// </summary>
        /// <remarks>
        /// Performed external to constructor since could be a long-running action (a second or two).
        /// </remarks>
        /// <returns></returns>
        public Task<bool> InitializeAsync()
        {
            IsInitialized = true;
            // nothing needed for design time.
            return Task.FromResult<bool>(IsInitialized);
        }


        /// <summary>
        /// Cancel every running scanning operation.
        /// </summary>
        /// <returns></returns>
        public Task<bool> CancelAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Suspend every running scanning operation.
        /// </summary>
        /// <returns></returns>
        public Task<bool> SuspendAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// True when any vendor in entire app is scanning.
        /// </summary>
        /// <returns></returns>
        public bool IsAnyScanning { get; set; }

        /// <summary>
        /// Number of vendors presently scanning.
        /// </summary>
        public int IsScanningCount { get; set; }

        /// <summary>
        /// Number of vendors presently scanning or suspended.
        /// </summary>
        public int IsScanningOrSuspendedCount { get; set; }

        /// <summary>
        /// How many vendors have warnings showing.
        /// </summary>
        public int VendorsWithWarningsCount { get; set; }

    }
}
