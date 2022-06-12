using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App
{
    public class DesignStoreModel : ObservableObject, IStoreModel
    {
       
        /// <summary>
        /// Unique identifier for this vendor. InsideFabric, InsideRugs, etc.
        /// </summary>
        public StoreType Key { get; set; }

        /// <summary>
        /// Display name to show in UX.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Two-letter short name: IF, IA, IR, etc.
        /// </summary>
        public string ShortName { get; set; }

        public string WebsiteUrl { get; set; }

        private ObservableCollection<IVendorModel> _vendors = null;

        /// <summary>
        /// Collection of all supported vendors.
        /// </summary>
        /// <remarks>
        /// List of vendors baked into the code, irrespective of if SQL has an associated row
        /// for the vendors. The code will later determine which vendors have full SQL support
        /// and only allow actions on those fully supported vendors.
        /// </remarks>
        public ObservableCollection<IVendorModel> Vendors
        {
            get
            {
                return _vendors;
            }
            set
            {
                Set(() => Vendors, ref _vendors, value);
            }
        }

        public DesignStoreModel()
        {
            IsFullyImplemented = true;
            ProductCount = 201987; 
            ProductVariantCount = 700123; 
            ScanningVendorsCount = 4;
            SuspendedVendorsCount = 2; 
            CommitsVendorsCount = 6; 
            DisabledVendorsCount =1;

            var listVendors = new List<IVendorModel>()
            {
                new DesignVendorModel()
                {
                    VendorId = 1,
                    Name = "Kravet",
                    ScannerState = ScannerState.Idle,
                    IsFullyImplemented = true,
                    Status = VendorStatus.Manual,
                    HasWarning = true,
                    WarningText = "Hey there - this is a warning to you.",
                    ParentStore = this,
                },

                new DesignVendorModel()
                {
                    VendorId = 2,
                    Name = "Pindler",
                    ScannerState = ScannerState.Scanning,
                    IsFullyImplemented = true,
                    Status = VendorStatus.Manual,
                    HasWarning = false,
                    WarningText = null,
                    ParentStore = this,
                },

                new DesignVendorModel()
                {
                    VendorId = 3,
                    Name = "Maxwell",
                    ScannerState = ScannerState.Suspended,
                    IsFullyImplemented = true,
                    Status = VendorStatus.Disabled,
                    HasWarning = false,
                    WarningText = null,
                    ParentStore = this,
                },

                new DesignVendorModel()
                {
                    VendorId = 4,
                    Name = "Robert Allen",
                    ScannerState = ScannerState.Committable,
                    IsFullyImplemented = true,
                    Status = VendorStatus.AutoPilot,
                    HasWarning = true,
                    WarningText = "This is some kind of warning text here.",
                    ParentStore = this,
                },

                new DesignVendorModel()
                {
                    VendorId = 5,
                    Name = "Brewster",
                    ScannerState = ScannerState.Committable,
                    IsFullyImplemented = true,
                    Status = VendorStatus.AutoPilot,
                    HasWarning = false,
                    WarningText = null,
                    ParentStore = this,
                },

                new DesignVendorModel()
                {
                    VendorId = 6,
                    Name = "Fabricut",
                    ScannerState = ScannerState.Committable,
                    IsFullyImplemented = true,
                    Status = VendorStatus.AutoPilot,
                    HasWarning = false,
                    WarningText = null,
                    ParentStore = this,
                },

                new DesignVendorModel()
                {
                    VendorId = 7,
                    Name = "Scalamndre",
                    ScannerState = ScannerState.Idle,
                    IsFullyImplemented = true,
                    Status = VendorStatus.AutoPilot,
                    HasWarning = false,
                    WarningText = null,
                    ParentStore = this,
                },

                new DesignVendorModel()
                {
                    VendorId = 8,
                    Name = "Trend",
                    ScannerState = ScannerState.Disabled,
                    IsFullyImplemented = true,
                    Status = VendorStatus.AutoPilot,
                    HasWarning = false,
                    WarningText = null,
                    ParentStore = this,
                },


                new DesignVendorModel()
                {
                    IsFullyImplemented = false,
                    VendorId = 100,
                    Name = "York Wallpaper",
                    ParentStore = this,
                }
            };


            Vendors = new ObservableCollection<IVendorModel>(listVendors);
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
            return Task.FromResult(MakeFakeRecentCommits());
        }



        private List<CommitBatchSummary> MakeFakeRecentCommits()
        {
            var list = new List<CommitBatchSummary>()
            {
                new CommitBatchSummary
                {
                    Id = 100,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-1),
                    BatchType = CommitBatchType.Discontinued,
                    QtySubmitted = 1020,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 101,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-2),
                    BatchType = CommitBatchType.NewProducts,
                    QtySubmitted = 10234,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 102,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-3),
                    BatchType = CommitBatchType.InStock,
                    QtySubmitted = 233,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 103,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-4),
                    BatchType = CommitBatchType.NewProducts,
                    QtySubmitted = 11456,
                    SessionStatus= CommitBatchStatus.Discarded,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },

                new CommitBatchSummary
                {
                    Id = 104,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-5),
                    BatchType = CommitBatchType.NewVariants,
                    QtySubmitted = 909,
                    SessionStatus= CommitBatchStatus.Committed,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },

            };

            return list;
        }


        /// <summary>
        /// Number of vendors.
        /// </summary>
        public int VendorCount
        {
            get
            {
                if (Vendors == null)
                    return 0;

                return Vendors.Count();
            }
        }

        public bool IsFullyImplemented { get; set; }


        /// <summary>
        /// Total number of products in store SQL.
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Total number of product variants in store SQL.
        /// </summary>
        public int ProductVariantCount { get; set;}

        /// <summary>
        /// Number of vendors presently scanning.
        /// </summary>
        public int ScanningVendorsCount { get; set; }

        /// <summary>
        /// Number of vendors presently suspended and can be resumed.
        /// </summary>
        public int SuspendedVendorsCount { get; set; }

        /// <summary>
        /// Number of vendors who have commits ready.
        /// </summary>
        public int CommitsVendorsCount { get; set; }

        /// <summary>
        /// Number of vendors who are disabled.
        /// </summary>
        public int DisabledVendorsCount { get; set; }

        private int _clearanceProductCount = 0;
        public int ClearanceProductCount
        {
            get
            {
                return _clearanceProductCount;
            }
            set
            {
                Set(() => ClearanceProductCount, ref _clearanceProductCount, value);
            }
        }


        /// <summary>
        /// Start running scanning operation.
        /// </summary>
        /// <returns></returns>
        public Task<bool> StartAll(ScanOptions options)
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
        /// Resume every running scanning operation.
        /// </summary>
        /// <returns></returns>
        public Task<bool> ResumeAll()
        {
            throw new NotImplementedException();
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
        /// True when any vendor in entire store is scanning.
        /// </summary>
        /// <returns></returns>
        public bool IsAnyScanning { get; set; }

        /// <summary>
        /// True when any vendor in entire store is scanning.
        /// </summary>
        /// <returns></returns>
        public bool IsAnySuspended { get; set; }

        /// <summary>
        /// Number of vendors presently scanning or suspended.
        /// </summary>
        public int IsScanningOrSuspendedCount { get; set; }


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


    }
}
