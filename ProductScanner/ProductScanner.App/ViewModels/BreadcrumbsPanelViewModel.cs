using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;

namespace ProductScanner.App.ViewModels
{
    public class BreadcrumbsPanelViewModel : ViewModelBase
    {
        public BreadcrumbsPanelViewModel(IAppModel appModel)
        {
            if (!IsInDesignMode)
            {
                HookMessages();
                Breadcrumbs = new ObservableCollection<BreadcrumbLinkItemViewModel>(new List<BreadcrumbLinkItemViewModel>());
            }
            else
            {
                PageTitle = "My Page Title";
                PageSubTitle = "My Page SubTitle";

                var fakeList = new List<BreadcrumbLinkItemViewModel>()
                {
                    new BreadcrumbLinkItemViewModel("Home", false, ContentPageTypes.Home, null),
                    new BreadcrumbLinkItemViewModel("InsideFabric", true, ContentPageTypes.StoreDashboard, null),
                    new BreadcrumbLinkItemViewModel("Kravet", true, ContentPageTypes.VendorDashboard, null),
                };

                Breadcrumbs = new ObservableCollection<BreadcrumbLinkItemViewModel>(fakeList);
            }
        }

        private void HookMessages()
        {
            MessengerInstance.Register<ShowContentPage>(this, (msg) =>
            {
                PageTitle = msg.Page.PageTitle;
                PageSubTitle = msg.Page.PageSubTitle;
                var crumbs = ParseBreadcrumbTemplate(msg.Page);
                Breadcrumbs = new ObservableCollection<BreadcrumbLinkItemViewModel>(crumbs);
            });

            MessengerInstance.Register<ShowContentView>(this, (msg) =>
            {
                PageTitle = "Product Scanner";
                PageSubTitle = string.Empty;
                Breadcrumbs = new ObservableCollection<BreadcrumbLinkItemViewModel>(new List<BreadcrumbLinkItemViewModel>()
                    {
                        new BreadcrumbLinkItemViewModel("Home", false, ContentPageTypes.Home, null),
                    });
            });
        }

        /// <summary>
        /// Given a template, return a resolved list of breadcrumb items.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        private List<BreadcrumbLinkItemViewModel> ParseBreadcrumbTemplate(IContentPage page)
        {
            Debug.Assert(page != null);
            var template = page.BreadcrumbTemplate;

            var list = new List<BreadcrumbLinkItemViewModel>();

            if (!string.IsNullOrWhiteSpace(template))
            {
                var tokens = template.Split('/');
                bool isFirst = true;
                foreach(var token in tokens)
                {
                    StoreContentPageViewModel storePage;
                    VendorContentPageViewModel vendorPage;

                    switch(token)
                    {
                        case "{Home}":
                            list.Add(new BreadcrumbLinkItemViewModel("Home", !isFirst, ContentPageTypes.Home, null));
                            break;

                        case "{Store}":
                            if (page is StoreContentPageViewModel)
                            {
                                storePage = page as StoreContentPageViewModel;
                                Debug.Assert(storePage != null);
                                list.Add(new BreadcrumbLinkItemViewModel(storePage.Store.Name, !isFirst, ContentPageTypes.StoreDashboard, storePage.Store));
                            }
                            else if (page is VendorContentPageViewModel)
                            {
                                vendorPage = page as VendorContentPageViewModel;
                                Debug.Assert(vendorPage != null);
                                list.Add(new BreadcrumbLinkItemViewModel(vendorPage.Vendor.ParentStore.Name, !isFirst, ContentPageTypes.StoreDashboard, vendorPage.Vendor.ParentStore));
                            }
                            break;

                        case "{StoreNotLink}":
                            if (page is StoreContentPageViewModel)
                            {
                                storePage = page as StoreContentPageViewModel;
                                Debug.Assert(storePage != null);
                                list.Add(new BreadcrumbLinkItemViewModel(storePage.Store.Name, !isFirst));
                            }
                            else if (page is VendorContentPageViewModel)
                            {
                                vendorPage = page as VendorContentPageViewModel;
                                Debug.Assert(vendorPage != null);
                                list.Add(new BreadcrumbLinkItemViewModel(vendorPage.Vendor.ParentStore.Name, !isFirst));
                            }
                            break;


                        case "{Vendor}":
                            if (page is VendorContentPageViewModel)
                            {
                                vendorPage = page as VendorContentPageViewModel;
                                Debug.Assert(vendorPage != null);
                                list.Add(new BreadcrumbLinkItemViewModel(vendorPage.Vendor.Name, !isFirst, ContentPageTypes.VendorDashboard, vendorPage.Vendor));
                            }
                            break;

                        case "{VendorNotLink}":
                            if (page is VendorContentPageViewModel)
                            {
                                vendorPage = page as VendorContentPageViewModel;
                                Debug.Assert(vendorPage != null);
                                list.Add(new BreadcrumbLinkItemViewModel(vendorPage.Vendor.Name, !isFirst));
                            }
                            break;

                        case "{Self}":
                            // this one is not a link
                            list.Add(new BreadcrumbLinkItemViewModel(page.PageSubTitle, !isFirst));
                            break;

                        default:
                            list.Add(new BreadcrumbLinkItemViewModel(token, !isFirst));
                            break;
                    }
                    isFirst = false;
                }
            }

            return list;
        }

        private string _pageTitle = null;

        public string PageTitle
        {
            get
            {
                return _pageTitle;
            }
            set
            {
                Set(() => PageTitle, ref _pageTitle, value);
            }
        }


        private string _pageSubTitle = null;

        public string PageSubTitle
        {
            get
            {
                return _pageSubTitle;
            }
            set
            {
                Set(() => PageSubTitle, ref _pageSubTitle, value);
            }
        }


        private ObservableCollection<BreadcrumbLinkItemViewModel> _breadcrumbs = null;

        public ObservableCollection<BreadcrumbLinkItemViewModel> Breadcrumbs
        {
            get
            {
                return _breadcrumbs;
            }
            set
            {
                Set(() => Breadcrumbs, ref _breadcrumbs, value);
            }
        }       
    }
}