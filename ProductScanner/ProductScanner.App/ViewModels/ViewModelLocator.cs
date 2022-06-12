using System.Collections.Generic;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using SimpleInjector;

namespace ProductScanner.App.ViewModels
{

    /// <summary>
    /// Creates view models for the models which can be created through this aproach.
    /// </summary>
    /// <remarks>
    /// Some other view models are created directly by the navigation system.
    /// </remarks>
    public class ViewModelLocator
    {
        // used only in design mode when the container is created herein
        private static Container _container;
        private static Dictionary<string, ViewModelBase> _dicViewInstances;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            // these view models should have only a single instance
            // for the life of the app

            //NavigationTreeViewModel
            //SplashScreenlViewModel
            //MainWindowViewModel
            //MainViewModel
            //BreadcrumbsPanelViewModel
            //ContentHostViewModel
            //ActivityPanelViewModel
            //BackNavigationPanelViewModel

        }

        static ViewModelLocator()
        {
            _dicViewInstances = new Dictionary<string, ViewModelBase>();

            if (IsDesignMode)
            {
                _container = new Container();

                // a few registrations needed to support BLEND.
                _container.RegisterSingle<IAppModel, DesignAppModel>();
                _container.RegisterSingle<IScannerDatabaseConnector, ScannerDatabaseConnector>();
                _container.RegisterSingle<IStoreDatabaseConnector, StoreDatabaseConnector>();
                _container.RegisterSingle<IFileManager, FileManager>();
            }
        }

        private static bool IsDesignMode
        {
            get
            {
                // made into a local property in case for dev we need to fake it out.
                return ViewModelBase.IsInDesignModeStatic;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public TService GetInstance<TService>() where TService : class
        {
            // in design mode, we use a locally created container, but otherwise we're
            // asking the app to resolve view models.

            if (IsDesignMode)
                return _container.GetInstance<TService>();

            return App.GetInstance<TService>();
        }


        [System.Diagnostics.DebuggerStepThrough]
        public TService GetSingleInstance<TService>() where TService : class
        {
            // only one instance of views when this entry point used, so keep a
            // dictionary

            var key = typeof(TService).Name;

            ViewModelBase vm;
            if (_dicViewInstances.TryGetValue(key, out vm))
            {
                //Debug.WriteLine(string.Format("Locator returning CACHED version of: {0}", key));
                return vm as TService;
            }
            // need to get for the first time and cache
            vm = GetInstance<TService>() as ViewModelBase;
            _dicViewInstances[key] = vm;

            //Debug.WriteLine(string.Format("Locator returning NEW version of: {0}", key));
            
            return vm as TService;
        }


        public NavigationTreeViewModel NavigationTreeViewModel
        {
            get
            {

                return GetSingleInstance<NavigationTreeViewModel>();
            }
        }

        public SplashScreenlViewModel SplashScreenViewModel
        {
            get
            {
                return GetSingleInstance<SplashScreenlViewModel>();
            }
        }


        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                return GetSingleInstance<MainWindowViewModel>();
            }
        }


        public MainViewModel MainViewModel
        {
            get
            {
                return GetSingleInstance<MainViewModel>();
            }
        }

        public BreadcrumbsPanelViewModel BreadcrumbsPanelViewModel
        {
            get
            {
                return GetSingleInstance<BreadcrumbsPanelViewModel>();
            }
        }

        public ContentHostViewModel ContentHostViewModel
        {
            get
            {
                return GetSingleInstance<ContentHostViewModel>();
            }
        }


        public ActivityPanelViewModel ActivityPanelViewModel
        {
            get
            {
                return GetSingleInstance<ActivityPanelViewModel>();
            }
        }


        public BackNavigationPanelViewModel BackNavigationPanelViewModel
        {
            get
            {
                return GetSingleInstance<BackNavigationPanelViewModel>();
            }
        }

        public CommitSummaryPageViewModel CommitSummaryPageViewModel
        {
            get
            {
                return GetInstance<CommitSummaryPageViewModel>();
            }
        }

        public CommitBatchPageViewModel CommitBatchPageViewModel
        {
            get
            {
                return GetInstance<CommitBatchPageViewModel>();
            }
        }


        // one of the tabs on the vendor scanner page
        public VendorScanFilesTabViewModel VendorScanFilesTabViewModel
        {
            get
            {
                return GetInstance<VendorScanFilesTabViewModel>();
            }
        }

        // one of the tabs on the vendor scanner page
        public VendorScanTuningTabViewModel VendorScanTuningTabViewModel
        {
            get
            {
                return GetInstance<VendorScanTuningTabViewModel>();
            }
        }

       
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}