using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using ProductScanner.App;

namespace ProductScanner.App.ViewModels
{
    /// <summary>
    /// All content pages for top level stores should inherit this class.
    /// </summary>
    public class StoreContentPageViewModel : ContentPageViewModel
    {

        public StoreContentPageViewModel(IStoreModel store)
        {
            Store = store;
            PageTitle = Store.Name;
            BreadcrumbTemplate = "{Home}";
        }

        protected void RequestNavigation(ContentPageTypes pageType)
        {
            if (IsInDesignMode)
                return;

            MessengerInstance.Send(new RequestNavigationToContentPageType(this.Store, pageType));
        }

        private IStoreModel _store = null;

        public IStoreModel Store
        {
            get
            {
                return _store;
            }
            set
            {
                Set(() => Store, ref _store, value);
            }
        }
    }
}