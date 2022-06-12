using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ProductScanner.App.ViewModels
{
    public class BreadcrumbLinkItemViewModel : ViewModelBase
    {
        private ContentPageTypes pageType;
        private object navigationParameter;

        public BreadcrumbLinkItemViewModel(string linkText, bool prependSeparator, ContentPageTypes pageType, object navigationParameter=null)
        {
            this.pageType = pageType;
            this.navigationParameter = navigationParameter;
            LinkText = linkText;
            PrependSeparator = prependSeparator;
            IsLink = true;
        }

        public BreadcrumbLinkItemViewModel(string linkText, bool prependSeparator)
        {
            LinkText = linkText;
            PrependSeparator = prependSeparator;
            IsLink = false;
        }

        private string _linkText = null;

        public string LinkText
        {
            get
            {
                return _linkText;
            }
            set
            {
                Set(() => LinkText, ref _linkText, value);
            }
        }



        private bool _isLink = false;

        public bool IsLink
        {
            get
            {
                return _isLink;
            }
            set
            {
                Set(() => IsLink, ref _isLink, value);
            }
        }

        private bool _prependSeparator = false;

        public bool PrependSeparator
        {
            get
            {
                return _prependSeparator;
            }
            set
            {
                Set(() => PrependSeparator, ref _prependSeparator, value);
            }
        }


        private RelayCommand _requestNavigation;

        public RelayCommand RequestNavigation
        {
            get
            {
                return _requestNavigation
                    ?? (_requestNavigation = new RelayCommand(ExecuteRequestNavigation));
            }
        }

        private void ExecuteRequestNavigation()
        {
            if (IsInDesignMode)
                return;

            // the kind of message we send depends on the target page type

            switch(pageType)
            {
                case ContentPageTypes.Home:
                    MessengerInstance.Send(new RequestNavigationToContentPageType(ContentPageTypes.Home));
                    break;

                case ContentPageTypes.StoreDashboard:
                    Debug.Assert(navigationParameter is IStoreModel);
                    MessengerInstance.Send(new RequestNavigationToContentPageType(navigationParameter as IStoreModel));
                    break;

                case ContentPageTypes.VendorDashboard:
                    Debug.Assert(navigationParameter is IVendorModel);
                    MessengerInstance.Send(new RequestNavigationToContentPageType(navigationParameter as IVendorModel));
                    break;
            }
        }
    }
}