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
    public partial class BatchSimpleProductRadGrid 
    {
        private ObservableCollection<IViewData> data;

        public BatchSimpleProductRadGrid(ObservableCollection<IViewData> data)
        {
            this.data = data;
            DataContext = this;
            InitializeComponent();
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
    }

    public class DesignBatchSimpleProductRadGrid
    {
        public ObservableCollection<IViewData> ViewDataItemsSource
        {
            get
            {
                var list = new List<SimpleProductViewData>()
                {
                    new SimpleProductViewData
                    {
                        ProductID = 1065467,
                        SKU = "BM-BC1582578",
                        Name = "BC1582578 Wine Bottles by Blue Mountain",
                        ProductGroup = "Wallcovering",
                        StoreUrl = "http://www.insidefabric.com/p-1065467-bc1582578-wine-bottles-by-blue-mountain.aspx",
                    },

                    new SimpleProductViewData
                    {
                        ProductID = 1066014,
                        SKU = "KR-33134-35",
                        Name = "BC1582578 Wine Bottles by Blue Mountain",
                        ProductGroup = "Fabric",
                        StoreUrl = "http://www.insidefabric.com/p-1066014-33134-35-by-kravet-smart.aspx",
                    },

                    new SimpleProductViewData
                    {
                        ProductID = 1102094,
                        SKU = "BH-211427",
                        Name = "211427 Garden Glow Amber by Beacon Hill",
                        ProductGroup = "Fabric",
                        StoreUrl = "http://www.insidefabric.com/p-1102094-211427-garden-glow-amber-by-beacon-hill.aspx",
                    },
                };

                return new ObservableCollection<IViewData>(list);
            }
        }
    }
}
