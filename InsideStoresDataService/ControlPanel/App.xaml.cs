using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ServiceModel.DomainServices.Client.ApplicationServices;
using Intersoft.Client.Framework;
using Telerik.Windows.Controls;

namespace ControlPanel
{
    public partial class App : Application
    {
        // define the ApplicationID used to reference this application from UXShell or other containers
        public static string ApplicationID = "InsideStoresDataService";

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
            InitializeShell(); // intersoft startup
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            // Add the objects to Resources so they can be bound to the controls in XAML files
            this.Resources.Add("WebContext", WebContext.Current);
            this.Resources.Add("Shell", UXShell.Current);
            this.Resources.Add("AppSvc", AppSvc.Current);

            // the shell is just an empty frame with a navigation container, code logic
            // then figures everything out from there as the shell fires up.

            //this.RootVisual = new ControlPanel.Views.Shell();
            this.RootVisual = new ControlPanel.Views.Shell();
        }

        private void InitializeShell()
        {
            // Create a WebContext and add it to the ApplicationLifetimeObjects
            // collection.  This will then be available as WebContext.Current.
            // presently, none of the app features depend on authenticated users.
            WebContext webContext = new WebContext();
            //webContext.Authentication = new FormsAuthentication();
            this.ApplicationLifetimeObjects.Add(webContext);

            // Create the Intersoft Shell and add it to the ApplicationLifetimeObjects collection.
            // The Shell manages the applications life cycle, such as downloading external 
            // XAP only when required, save them to isolated storage, and load them from 
            // cache in the next launch.

            UXShell shell = new UXShell();
            shell.RootApplication = UXShell.CreateApplicationFromType(typeof(App), ApplicationID, ApplicationID);

            this.ApplicationLifetimeObjects.Add(shell);

            this.ApplicationLifetimeObjects.Add(AppSvc.Current);
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
