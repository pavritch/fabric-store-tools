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

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Interaction logic for BatchPriceUpdatesRadGrid.xaml
    /// </summary>
    public partial class BatchPriceUpdatesRadGrid : UserControl
    {
        private ObservableCollection<IViewData> data;

        public BatchPriceUpdatesRadGrid(ObservableCollection<IViewData> data)
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

    public class DesignBatchPriceUpdatesRadGrid
    {
        public ObservableCollection<IViewData> ViewDataItemsSource
        {
            get
            {
                var list = new List<PriceUpdatesViewData>()
                {
                    new PriceUpdatesViewData
                    {
                        VariantID = 2005467,
                        ProductID = 1065467,
                        SKU = "BM-BC1582578",
                        Name = "BC1582578 Wine Bottles by Blue Mountain",
                        ProductGroup = "Wallcovering",
                        StoreUrl = "http://www.insidefabric.com/p-1065467-bc1582578-wine-bottles-by-blue-mountain.aspx",
                        UnitOfMeasure = "Roll",

                        NewCost = 100.21M,
                        NewRetailPrice = 230.98M,
                        NewOurPrice = 189.76M,
                        OldCost = 45.03M,
                        OldRetailPrice = 78.13M,
                        OldOurPrice = 134.66M,
                        IsClearance = null,

                    },

                    new PriceUpdatesViewData
                    {
                        VariantID = 2006014,
                        ProductID = 1066014,
                        SKU = "KR-33134-35",
                        Name = "BC1582578 Wine Bottles by Blue Mountain",
                        ProductGroup = "Fabric",
                        StoreUrl = "http://www.insidefabric.com/p-1066014-33134-35-by-kravet-smart.aspx",
                        UnitOfMeasure = "Yard",

                        NewCost = 100.21M,
                        NewRetailPrice = 230.98M,
                        NewOurPrice = 189.76M,
                        OldCost = 45.03M,
                        OldRetailPrice = 78.13M,
                        OldOurPrice = 134.66M,
                        IsClearance = null,
                    },

                    new PriceUpdatesViewData
                    {
                        VariantID = 2006014,
                        ProductID = 1066014,
                        SKU = "KR-33134-35-SWATCH",
                        Name = "BC1582578 Wine Bottles by Blue Mountain",
                        ProductGroup = "Fabric",
                        StoreUrl = "http://www.insidefabric.com/p-1066014-33134-35-by-kravet-smart.aspx",
                        UnitOfMeasure = "Swatch",

                        NewCost = 100.21M,
                        NewRetailPrice = 230.98M,
                        NewOurPrice = 189.76M,
                        OldCost = 45.03M,
                        OldRetailPrice = 78.13M,
                        OldOurPrice = 134.66M,
                        IsClearance = false,
                    },


                    new PriceUpdatesViewData
                    {
                        VariantID = 2002094,
                        ProductID = 1102094,
                        SKU = "BH-211427",
                        Name = "211427 Garden Glow Amber by Beacon Hill",
                        ProductGroup = "Fabric",
                        StoreUrl = "http://www.insidefabric.com/p-1102094-211427-garden-glow-amber-by-beacon-hill.aspx",
                        UnitOfMeasure = "Yard",

                        NewCost = 100.21M,
                        NewRetailPrice = 230.98M,
                        NewOurPrice = 189.76M,
                        OldCost = 45.03M,
                        OldRetailPrice = 78.13M,
                        OldOurPrice = 134.66M,
                        IsClearance = true,

                    },
                };

                return new ObservableCollection<IViewData>(list);
            }
        }
    }
}
