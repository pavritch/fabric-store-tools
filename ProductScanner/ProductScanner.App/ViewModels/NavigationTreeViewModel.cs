using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

namespace ProductScanner.App.ViewModels
{
    public class NavigationTreeViewModel : ViewModelBase
    {
        IAppModel appModel;
        private bool suppressAnnouncements = false;

        public NavigationTreeViewModel(IAppModel appModel)
        {
            this.appModel = appModel;
            Stores = appModel.Stores;

            if (!IsInDesignMode)
            {
                HookMessages();
            }
        }

        private void HookMessages()
        {
            MessengerInstance.Register<ChangeTreeViewSelection>(this, (msg) =>
            {
                if (msg.Store == null && msg.Vendor == null)
                {
                    Selected = null;
                    return;
                }
                else if (msg.Store != null)
                {
                    if (!msg.Store.Equals(Selected))
                    {
                        suppressAnnouncements = true;
                        Selected = msg.Store;
                        suppressAnnouncements = false;
                    }
                }
                else if (msg.Vendor != null)
                {
                    suppressAnnouncements = true;
                    Selected = msg.Vendor;
                    suppressAnnouncements = false;
                }
                
            });

            MessengerInstance.Register<ShowContentPage>(this, (msg) =>
                {
                    // not sure yet if actually needed
                });
        }

        private void AnnounceSelectionChange(object selectedItem)
        {
            if (selectedItem is IStoreModel)
            {
                var store = selectedItem as IStoreModel;
                var msg = new RequestNavigationToContentPageType(store);
                MessengerInstance.Send(msg);
            }
            else if (selectedItem is IVendorModel)
            {
                var vendor = selectedItem as IVendorModel;
                var msg = new RequestNavigationToContentPageType(vendor);
                MessengerInstance.Send(msg);
            }
        }

        private ObservableCollection<IStoreModel> _stores = null;

        /// <summary>
        ///  Collection of stores. Each store has a collection of vendors.
        /// </summary>
        public ObservableCollection<IStoreModel> Stores
        {
            get
            {
                return _stores;
            }
            set
            {
                Set(() => Stores, ref _stores, value);
            }
        }


        private object _selected = null;

        /// <summary>
        /// Currently selected node in the tree.
        /// </summary>
        /// <remarks>
        /// Can be either IStoreModel or IVendorModel - or null.
        /// </remarks>
        public object Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                var mustAnnounce = Set(() => Selected, ref _selected, value);

                if (mustAnnounce && value != null && !suppressAnnouncements && !IsInDesignMode)
                    AnnounceSelectionChange(value);
            }
        }
    }
}