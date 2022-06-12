using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProductScanner.App.ViewModels;
using Telerik.Windows.Controls;


namespace ProductScanner.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Used for centering dialogs within the topmost window of the app.
        /// </summary>
        public static ContentControl Current {get; private set;}

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.Current = this as ContentControl;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
                return;

            if (App.PreventApplicationClose)
            {
                e.Cancel = true;
                ShowDlg("Close not allowed while running activity.");
            }
            else if (App.AppModel.IsAnyScanning)
            {
                e.Cancel = true;
                var msg = string.Format("Close not allowed while scanning. {0:N0} active vendors.", App.AppModel.IsScanningCount);
                ShowDlg(msg);
            }
        }

        private void ShowDlg(string message)
        {
            var dlg = new Telerik.Windows.Controls.DialogParameters()
            {
                Header = "Product Scanner",
                Content = message,
                IconContent = "Error16.png".ToImageControl(true),
            };

            RadWindow.Alert(dlg);

        }
    }
}
