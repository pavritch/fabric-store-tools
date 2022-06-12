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
    /// Interaction logic for StoreLoginsSummary.xaml
    /// </summary>
    public partial class StoreLoginsSummary : UserControl
    {
        public StoreLoginsSummary()
        {
            InitializeComponent();
        }


        private void RadGridView_SelectionChanging(object sender, Telerik.Windows.Controls.SelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Common support for any link button which needs to launch a browser. Tag must have Url.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var url = (string)btn.Tag;
            Messenger.Default.Send(new RequestLaunchBrowser(url));
            e.Handled = true;
        }
    }
}
