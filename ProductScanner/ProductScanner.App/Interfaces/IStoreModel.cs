using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App
{

    /// <summary>
    /// Virtualization of the in-memory model which represents a store.
    /// </summary>
    /// <remarks>
    /// One of these models is created for each store supported in the code at the start of the 
    /// program. This acts like a middle man between the various UX view models and the core runtime modules.
    /// </remarks>
    public interface IStoreModel
    {
        /// <summary>
        /// Indicates that initialization has been completed.
        /// </summary>
        /// <remarks>
        /// App will immediately terminate if initialization not completed successfully.
        /// </remarks>
        bool IsInitialized { get; }

        /// <summary>
        /// Populate the model with stores and vendors.
        /// </summary>
        /// <remarks>
        /// Performed external to constructor since could be a long-running action (a second or two).
        /// App will immediately terminate if initialization not completed successfully.
        /// </remarks>
        /// <returns></returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Unique identifier for this vendor. InsideFabric, InsideRugs, etc.
        /// </summary>
        StoreType Key { get; }

        /// <summary>
        /// Display name to show in UX.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Two-letter nickname for store: IF, IW, IR, IH, etc.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Url to home page. All lower case - http://www.insidefabric.com
        /// </summary>
        string WebsiteUrl { get; }

        /// <summary>
        /// Collection of all supported vendors.
        /// </summary>
        /// <remarks>
        /// List of vendors baked into the code, irrespective of if SQL has an associated row
        /// for the vendors. The code will later determine which vendors have full SQL support
        /// and only allow actions on those fully supported vendors.
        /// </remarks>
        ObservableCollection<IVendorModel> Vendors { get; }

        /// <summary>
        /// Number of vendors in Vendors collection.
        /// </summary>
        int VendorCount { get; }

        /// <summary>
        /// Indicates if this store is fully supported in code, store SQL and platform SQL.
        /// </summary>
        bool IsFullyImplemented { get; }

        /// <summary>
        /// Total number of products in store SQL.
        /// </summary>
        int ProductCount { get; }

        /// <summary>
        /// Total number of product variants in store SQL.
        /// </summary>
        /// <remarks>
        /// Used in Home dashboard page gridview.
        /// </remarks>
        int ProductVariantCount { get; }

        /// <summary>
        /// Number of clearance products.
        /// </summary>
        int ClearanceProductCount { get; }

        /// <summary>
        /// Number of vendors presently scanning.
        /// </summary>
        /// <remarks>
        /// Used in Home dashboard page gridview.
        /// </remarks>
        int ScanningVendorsCount { get; }

        /// <summary>
        /// Number of vendors presently suspended and can be resumed.
        /// </summary>
        /// <remarks>
        /// Used in Home dashboard page gridview.
        /// </remarks>
        int SuspendedVendorsCount { get; }

        /// <summary>
        /// Number of vendors who have commits ready.
        /// </summary>
        /// <remarks>
        /// Used in Home dashboard page gridview.
        /// </remarks>
        int CommitsVendorsCount { get; }

        /// <summary>
        /// Number of vendors who are disabled.
        /// </summary>
        /// <remarks>
        /// Used in Home dashboard page gridview.
        /// </remarks>
        int DisabledVendorsCount { get; }

        /// <summary>
        /// Fetch commit batches in descending order.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        Task<List<CommitBatchSummary>> GetCommitBatchesAsync(int? skip = null, int? take = null);


        /// <summary>
        /// Start running scanning operation.
        /// </summary>
        /// <returns></returns>
        Task<bool> StartAll(ScanOptions options);

        /// <summary>
        /// Suspend every running scanning operation.
        /// </summary>
        /// <returns></returns>
        Task<bool> SuspendAll();

        /// <summary>
        /// Resume every running scanning operation.
        /// </summary>
        /// <returns></returns>
        Task<bool> ResumeAll();

        /// <summary>
        /// Cancel every running scanning operation.
        /// </summary>
        /// <returns></returns>
        Task<bool> CancelAll();


        /// <summary>
        /// True when any vendor in entire store is scanning.
        /// </summary>
        /// <returns></returns>
        bool IsAnyScanning { get; }

        /// <summary>
        /// True when any vendor in entire store is scanning.
        /// </summary>
        /// <returns></returns>
        bool IsAnySuspended { get; }

        /// <summary>
        /// Number of vendors presently scanning or suspended.
        /// </summary>
        int IsScanningOrSuspendedCount { get; }

    }

}
