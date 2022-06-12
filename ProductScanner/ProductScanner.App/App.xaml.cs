using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.App.ViewModels;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.SimpleInjector;
using ProductScanner.Data;
using ProductScanner.StoreData;
using SimpleInjector;
using SimpleInjector.Extensions;
using Utilities.Extensions;

namespace ProductScanner.App
{
    /// <summary>
    /// Global application class.
    /// </summary>
    /// <remarks>
    /// This code is never executed by Blend/VS design time. See VM locator for 
    /// the local IoC container used at design time.
    /// </remarks>
    public partial class App : Application
    {
        #region Locals
        private static Container _container;
        private static NavigatationService _navigationService;
        private static IAppModel _appModel;
        private static int _preventAppCloseCounter = 0;
        private System.Threading.Timer _oneSecondTimer;

        #endregion

        #region Static Service Locator Helpers
        [System.Diagnostics.DebuggerStepThrough]
        public static TService GetInstance<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static object GetInstance(Type type)
        {
            return _container.GetInstance(type);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static IEnumerable<TService> GetAllInstances<TService>() where TService : class
        {
            return _container.GetAllInstances<TService>();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static IEnumerable<object> GetAllInstances(Type type)
        {
            return _container.GetAllInstances(type);
        }

        #endregion

        static App()
        {
            // init the dispatch support so it knows the UI thread.
            DispatcherHelper.Initialize();
        }

        public App()
        {
           this.InitializeComponent();
           HookMessages();

            // wire up an app-wide one-second interval timer
           _oneSecondTimer = new System.Threading.Timer((st) =>
           {
               Messenger.Default.Send(new AnnouncementMessage(Announcement.OneSecondIntervalTimer));
           }, null, 0, 1000);
        }

        /// <summary>
        /// Typed access to the instance of the global App class.
        /// </summary>
        public new static App Current
        {
            get
            {
                return Application.Current as App;
            }
        }

        /// <summary>
        /// Returns the singleton instance of the navigation service.
        /// </summary>
        /// <remarks>
        /// The nav service does most of its work by hooking/sending messages, so once
        /// instantiated, pretty much runs on its own.
        /// </remarks>
        public static NavigatationService NavService
        {
            get
            {
                return _navigationService;
            }
        }

        /// <summary>
        /// Returns the singleton instance of the application model with collections of stores, vendors, etc.
        /// </summary>
        public static IAppModel AppModel
        {
            get
            {
                return _appModel;
            }
        }

        public static void IncrementPreventApplicationClose()
        {
            _preventAppCloseCounter++;

            // when moves above zero, announce that closing is blocked.
            if (_preventAppCloseCounter == 1)
                Messenger.Default.Send(new AnnouncementMessage(Announcement.ApplicationCloseBlocked));
        }

        public static void DecrementPreventApplicationClose()
        {
            _preventAppCloseCounter--;
            Debug.Assert(_preventAppCloseCounter >= 0);

            // if count goes to zero, announce we're clear to close

            if (_preventAppCloseCounter == 0)
                Messenger.Default.Send(new AnnouncementMessage(Announcement.ApplicationCloseUnBlocked));
        }

        public static bool PreventApplicationClose
        {
            get
            {
                // checked by MainWindow.xaml.cs to put up messagebox.
                return _preventAppCloseCounter != 0;
            }    
        }

        #region Initialization

        private  void HookMessages()
        {
            // manage when app will be restricted from being closed at some inopportune time

            Messenger.Default.Register<AnnouncementMessage>(this, (msg) =>
            {
                switch(msg.Kind)
                {
                    case Announcement.RequestIncrementApplicationCloseBlocked:
                        IncrementPreventApplicationClose();
                        break;

                    case Announcement.RequestDecrementApplicationCloseBlocked:
                        DecrementPreventApplicationClose();
                        break;

                    default:
                        break;
                }
            });

            // force app to terminate after displaying message

            Messenger.Default.Register<ForceAppTerminationMessage>(this, (msg) =>
            {
                var dlg = new Telerik.Windows.Controls.DialogParameters()
                {
                    Header = "Program Will Terminate",
                    Content = msg.Message,
                    IconContent = "Error16.png".ToImageControl(true),
                    Closed = (s, e1) =>
                    {
                        Application.Current.Shutdown();
                    }
                };

                Telerik.Windows.Controls.RadWindow.Alert(dlg);
            });


            // launch default browser to given url

            Messenger.Default.Register<RequestLaunchBrowser>(this, (msg) =>
            {
                if (!msg.Url.TryOpenUrl())
                    ReportErrorAlert("Unable to open browser.");
            });


            // open a file or folder using default system settings

            Messenger.Default.Register<RequestOpenFileOrFolder>(this, (msg) =>
            {
                if (string.IsNullOrWhiteSpace(msg.Path))
                    return;

                try
                {
                    System.Diagnostics.Process.Start(msg.Path);
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.Message);
                    ReportErrorAlert(string.Format("Unable to open file: {0}", msg.Path));
                }
            });

            // export/save files to disk

            Messenger.Default.Register<RequestExportTextFile>(this, (msg) =>
            {
                ExportManager.SaveTextFile(msg.TextLines, msg.SuggestedFilename);
            });
        }

        /// <summary>
        /// Show alert msgbox with simple text error message.
        /// </summary>
        /// <param name="msg"></param>
        public void ReportErrorAlert(string msg)
        {
            var dlg = new Telerik.Windows.Controls.DialogParameters()
            {
                Header = "Error",
                Content = msg,
                IconContent = "Error16.png".ToImageControl(true),
            };

            Telerik.Windows.Controls.RadWindow.Alert(dlg);
        }

		/// <summary>
        /// Called by OnStartup to initialize the container.
        /// </summary>
        private static void Bootstrap()
		{
		    var container = DefaultContainerConfiguration.BuildContainer();
            container.RegisterSingle<IMessenger>(() => Messenger.Default);
            // these are used by a
            container.RegisterSingle<IStoreDatabaseConnector, StoreDatabaseConnector>();
            container.RegisterSingle<IScannerDatabaseConnector, ScannerDatabaseConnector>();
            container.RegisterSingle<IFileManager, FileManager>();

            // these registrations are set here so the Core doesn't need to reference data dlls
            container.RegisterOpenGeneric(typeof(IStoreDatabase<>), typeof(BaseStoreDatabase<>));
            container.Register<IPlatformDatabase, PlatformDatabase>();
#if DEBUG
            // to help dev, we can work with either of these app model versions at runtime.
            // Blend/VS design time uses the DesignAppModel, but not from here - the VM locator
            // will create a local container and put the design model into it.

            // the reason to use DesignAppModel here is simply to be able to run the app with fake data
            // outside of blend.

            //container.RegisterSingle<IAppModel, DesignAppModel>();  // enable only one of the other!
            container.RegisterSingle<IAppModel, AppModel>();
#else
            container.RegisterSingle<IAppModel, AppModel>();
#endif


            container.Verify();

            // the container becomes locked at this point, no more registrations allowed

            _container = container;
        }

        static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            /* Load the assembly specified in 'args' here and return it, 
               if the assembly is already loaded you can return it here */
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.FullName.StartsWith(args.Name + ","));
            return assembly;
        }

    	#endregion 

        #region Event Handlers

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain current = AppDomain.CurrentDomain;
            current.AssemblyResolve += HandleAssemblyResolve;
            current.UnhandledException += UnhandledException;

            base.OnStartup(e);
            Bootstrap();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

            _navigationService = _container.GetInstance<NavigatationService>();

            _appModel = _container.GetInstance<IAppModel>();

        }

        /// <summary>
        /// User session logoff or machine shutdown event.
        /// </summary>
        /// <remarks>
        /// If system suddenly shuts down, suspend everything immediately.
        /// </remarks>
        /// <param name="e"></param>
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            // logoff or shutdown - don't care which one.

            try
            {
                AppModel.SuspendAll();
            }
            catch { }
            base.OnSessionEnding(e);
        }

        /// <summary>
        /// Application is terminating. Main windows already closed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            // make sure nothing still running on exit - should already
            // be handled since we don't allow close from main window
            // when things are running.

            base.OnExit(e);
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            var msg = string.Format("{0}\n\nApplication is going to close!", ex.Message);
            MessageBox.Show(msg, "Product Scanner - Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            ex.WriteEventLog();
        }

        /// <summary>
        /// On unhandled exception, show messagebox and exit gracefully.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if !DEBUG
            var msg = string.Format("{0}\n\nApplication is going to close!", e.Exception.Message);
            MessageBox.Show(msg, "Product Scanner - Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            this.Shutdown();
#endif
        }

        #endregion
    }
}
