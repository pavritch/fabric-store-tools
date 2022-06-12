using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using GalaSoft.MvvmLight.Messaging;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App.Views
{
    /// <summary>
    /// Interaction logic for StoreDashboard.xaml
    /// </summary>
    public partial class StoreDashboard : UserControl
    {
        public StoreDashboard()
        {
            InitializeComponent();
            HookMessages();
        }

        private void RadGridView_SelectionChanging(object sender, Telerik.Windows.Controls.SelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }
 
        private void VendorLinkButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var vendor = button.Tag as IVendorModel;
            if (vendor == null || !vendor.IsFullyImplemented)
                return;

            Messenger.Default.Send(new RequestNavigationToContentPageType(vendor));
        }

        private void VendorScanIcon_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var vendor = button.Tag as IVendorModel;
            if (vendor == null || !vendor.IsFullyImplemented)
                return;

            if (vendor.IsPerformingTests || vendor.IsCheckingCredentials || vendor.Status == VendorStatus.Disabled)
            {
                App.Current.ReportErrorAlert(string.Format("Access denied. The {0} scanning page is currently blocked.", vendor.Name));
                return;
            }

            Messenger.Default.Send(new RequestNavigationToContentPageType(vendor, ContentPageTypes.VendorScan));
        }


        private void HookMessages()
        {
            Messenger.Default.Register<VendorChangedNotification>(this, (msg) =>
            {
                Dispatcher.Invoke(() => RadGrid1.CalculateAggregates());
            });

            Messenger.Default.Register<ScanningOperationNotification>(this, (msg) =>
            {
                Dispatcher.Invoke(() => RadGrid1.CalculateAggregates());
            });
        }

    }
}
