using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ProductScanner.App.ViewData;

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Interaction logic for BatchNewSwatchesRadGrid.xaml
    /// </summary>
    public partial class BatchNewSwatchesRadGrid : UserControl
    {
        private ObservableCollection<IViewData> data;

        public BatchNewSwatchesRadGrid(ObservableCollection<IViewData> data)
        {
            this.data = data;
            DataContext = this;
            InitializeComponent();
            HookMessages();
            SetFrozenColumns(false);
            Unloaded += UserControl_Unloaded;
        }

        void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UnHookMessages();
        }

        public ObservableCollection<IViewData> ViewDataItemsSource
        {
            get
            {
                return data;
            }
        }

        protected void RadGridView_SelectionChanging(object sender, Telerik.Windows.Controls.SelectionChangingEventArgs e)
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
            var viewData = btn.Tag as IViewData;
            if (viewData != null)
            {
                var details = viewData.GetDetails();
                var dlg = new ProductScanner.App.Controls.CommitRecordDlg(details);
                dlg.ShowDialog();
            }

            e.Handled = true;

        }

        private void HookMessages()
        {
            Messenger.Default.Register<AnnouncementMessage>(this, (msg) =>
            {
                switch (msg.Kind)
                {
                    case Announcement.RequestFreezeGridColumns:
                        SetFrozenColumns(true);
                        break;

                    case Announcement.RequestUnFreezeGridColumns:
                        SetFrozenColumns(false);
                        break;

                    default:
                        break;
                }
            });

        }

        private void SetFrozenColumns(bool isEnabled)
        {
            if (isEnabled)
            {
                radGridView.FrozenColumnCount = 1;
                radGridView.FrozenColumnsSplitterVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                radGridView.FrozenColumnCount = 0;
                radGridView.FrozenColumnsSplitterVisibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void UnHookMessages()
        {
            Messenger.Default.Unregister(this);
        }
    }


    public class DesignBatchNewSwatchesRadGrid
    {
        public ObservableCollection<IViewData> ViewDataItemsSource
        {
            get
            {
                var list = new List<NewVariantsViewData>()
                {
                    new NewVariantsViewData
                    {
                        StoreUrl = "http://www.insidefabric.com/p-1065467-bc1582578-wine-bottles-by-blue-mountain.aspx",
                        UnitOfMeasure = "Yard",
                        ProductID = 879812,
                        SKU = "BM-BC1582578",
                        Name = "BC1582578 Wine Bottles by Blue Mountain",
                        ProductGroup = "Wallcovering",
                        Cost = 100.21M,
                        RetailPrice = 230.98M,
                        OurPrice = 189.76M,
                    },

                    new NewVariantsViewData
                    {
                        StoreUrl = "http://www.insidefabric.com/p-1065467-bc1582578-wine-bottles-by-blue-mountain.aspx",
                        UnitOfMeasure = "Yard",
                        ProductID = 879812,
                        SKU = "KR-33134-35",
                        Name = "BC1582578 Wine Bottles by Blue Mountain",
                        ProductGroup = "Fabric",
                        Cost = 100.21M,
                        RetailPrice = 230.98M,
                        OurPrice = 189.76M,
                    },

                    new NewVariantsViewData
                    {
                        StoreUrl = "http://www.insidefabric.com/p-1065467-bc1582578-wine-bottles-by-blue-mountain.aspx",
                        UnitOfMeasure = "Roll",
                        ProductID = 879812,
                        SKU = "BH-211427",
                        Name = "211427 Garden Glow Amber by Beacon Hill",
                        ProductGroup = "Fabric",
                        Cost = 100.21M,
                        RetailPrice = 230.98M,
                        OurPrice = 189.76M,
                    },
                };

                return new ObservableCollection<IViewData>(list);
            }
        }

    }
}
