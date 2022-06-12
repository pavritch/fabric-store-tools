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
using GalaSoft.MvvmLight.Messaging;

namespace ProductScanner.App.Views
{
    /// <summary>
    /// Interaction logic for HomeDashboard.xaml
    /// </summary>
    public partial class HomeDashboard : UserControl
    {
        public HomeDashboard()
        {
            InitializeComponent();
            HookMessages();
        }

        private void RadGridView_SelectionChanging(object sender, Telerik.Windows.Controls.SelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }

        private void StoreLinkButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var store = button.Tag as IStoreModel;
            if (store == null)
                return;

            Messenger.Default.Send(new RequestNavigationToContentPageType(store));
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
