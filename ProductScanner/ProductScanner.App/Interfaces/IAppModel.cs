using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    /// <summary>
    /// Instantiated at app start, but not ready to use until after Initialize() is called.
    /// </summary>
    /// <remarks>
    /// Async initialization called from MainWindowViewModel while splash screen is displayed.
    /// Guaranteed that no visuals will be displayed which depend on this model until after
    /// it has been initialized.
    /// </remarks>
    public interface IAppModel
    {
        /// <summary>
        /// List of supported stores.
        /// </summary>
        /// <remarks>
        /// Home content page directly binds to this collection.
        /// </remarks>
        ObservableCollection<IStoreModel> Stores { get; }

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
        /// Cancel every running scanning operation.
        /// </summary>
        /// <returns></returns>
        Task<bool> CancelAll();

        /// <summary>
        /// Suspend every running scanning operation.
        /// </summary>
        /// <returns></returns>
        Task<bool> SuspendAll();

        /// <summary>
        /// True when any vendor in entire app is scanning.
        /// </summary>
        /// <returns></returns>
        bool IsAnyScanning { get; }

        /// <summary>
        /// Number of vendors presently scanning.
        /// </summary>
        int IsScanningCount { get; }

        /// <summary>
        /// Number of vendors presently scanning or suspended.
        /// </summary>
        int IsScanningOrSuspendedCount { get; }

        /// <summary>
        /// How many vendors have warnings showing.
        /// </summary>
        int VendorsWithWarningsCount { get; }

    }
}
