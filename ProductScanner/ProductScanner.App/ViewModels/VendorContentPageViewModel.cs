using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using Utilities.Extensions;

namespace ProductScanner.App.ViewModels
{
    /// <summary>
    /// All content pages for vendors should inherit this class.
    /// </summary>
    public class VendorContentPageViewModel : ContentPageViewModel
    {
        public VendorContentPageViewModel(IVendorModel vendor)
        {
            Vendor = vendor;
            PageTitle = Vendor.Name;
            BreadcrumbTemplate = "{Home}/{Store}";
        }

        protected void RequestNavigation(ContentPageTypes pageType)
        {
            if (IsInDesignMode)
                return;

            MessengerInstance.Send(new RequestNavigationToContentPageType(this.Vendor, pageType));
        }

        private IVendorModel _vendor = null;
        public IVendorModel Vendor
        {
            get
            {
                return _vendor;
            }
            set
            {
                var old = _vendor;

                if (Set(() => Vendor, ref _vendor, value))
                {
                    if (old != null && old.GetType().Implements<INotifyPropertyChanged>())
                        (old as INotifyPropertyChanged).PropertyChanged -= Vendor_PropertyChanged;

                    if (value != null && value.GetType().Implements<INotifyPropertyChanged>())
                        (value as INotifyPropertyChanged).PropertyChanged += Vendor_PropertyChanged;
                }
            }
        }


        private void Vendor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VendorPropertyChanged(e);
        }

        protected virtual void VendorPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
        
        }
    }
}