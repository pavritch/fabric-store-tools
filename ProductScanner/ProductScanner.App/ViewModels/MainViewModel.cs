using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.App.Views;

namespace ProductScanner.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// This is the main view seen while using the application.
        /// </summary>
        /// <remarks>
        /// This view replaces the SplashScreen view in the MainWindow after the AppModel
        /// has been initialized. We're therefore guaranteed to have a fully-populated model to work with.
        /// </remarks>
        public MainViewModel()
        {

        }

    }
}