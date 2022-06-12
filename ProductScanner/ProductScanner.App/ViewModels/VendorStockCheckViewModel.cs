using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;

namespace ProductScanner.App.ViewModels
{
    public class VendorStockCheckViewModel : VendorContentPageViewModel
    {

#if DEBUG
        public VendorStockCheckViewModel()
            : this(new DesignVendorModel() { Name = "Kravet", VendorId = 5 })
        {

        }
#endif
        public VendorStockCheckViewModel(IVendorModel vendor)
            : base(vendor)
        {
            PageType = ContentPageTypes.VendorStockCheck;
            PageSubTitle = "Stock Check";
            BreadcrumbTemplate = "{Home}/{Store}/{Vendor}/Stock Check";
            IsNavigationJumpTarget = true;

            MakeFakeResults();

        }

        private void MakeFakeResults()
        {
            var dic = new Dictionary<string, string>()
            {
                {"ProductID", "12345"},
                {"VariantID", "89990"},
                {"Vendor Capabilities", "Everything"},
                {"Stock Status", "InStock"},
                {"Date", DateTime.Now.ToString()},
                {"DateWhenMore", "N/A"},
                {"Quantity on Hand", "Unknown"},
            };

            StockCheckResults = new ObservableCollection<dynamic>(dic.ToDynamicNameValueList());
        }


        private ObservableCollection<dynamic> _stockCheckResults = null;

        /// <summary>
        /// Sets and gets the StockCheckResults property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<dynamic> StockCheckResults
        {
            get
            {
                return _stockCheckResults;
            }
            set
            {
                Set(() => StockCheckResults, ref _stockCheckResults, value);
            }
        }
    }
}