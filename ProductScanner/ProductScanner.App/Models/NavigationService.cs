using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ProductScanner.App.ViewModels;

namespace ProductScanner.App
{
    /// <summary>
    /// Primary navigation coordinator.
    /// </summary>
    /// <remarks>
    /// Listens for messages from various UX components requesting navigation and causes
    /// that navigation to take place. Sends messages to announce navigation changes.
    /// </remarks>
    public class NavigatationService : ObservableObject
    {
        private IMessenger MessengerInstance {get; set;}
        private IContentPage CurrentPage {get; set;}
        private Dictionary<string, IContentPage> CachedViewModels { get; set; }

        private Stack<IContentPage> NavStack { get; set; }

        public NavigatationService(IMessenger messenger)
        {
            NavStack = new Stack<IContentPage>();
            CurrentPage = null;
            MessengerInstance = messenger;
            CachedViewModels = new Dictionary<string, IContentPage>();

            if (!ViewModelBase.IsInDesignModeStatic)
            {
                HookMessages();
            }
        }


        private IContentPage GetCachedVendorViewModel(IVendorModel vendor, ContentPageTypes pageType, Func<IContentPage> factory)
        {
            var key = string.Format("Vendor:{0}:{1}", vendor.Vendor.Id, pageType);
            IContentPage page;
            if (CachedViewModels.TryGetValue(key, out page))
                return page;

            page = factory();
            CachedViewModels[key] = page;
            return page;
        }


        private void HookMessages()
        {

            MessengerInstance.Register<AnnouncementMessage>(this, (msg) =>
            {
                switch(msg.Kind)
                {
                    case Announcement.ApplicationReady:
                        Messenger.Default.Send(new RequestNavigationToContentPageType(ContentPageTypes.Home));
                        break;

                    case Announcement.RequestBackNavigation:
                        // back nav button was clicked
                        PerformBackNavigation();
                        break;

                    case Announcement.RequestFlushBackNavigation:
                        FlushNavStack();
                        break;

                    default:
                        break;
                }
            });

            MessengerInstance.Register<RequestNavigationToContentPage>(this, (msg) =>
                {
                    NavigateToContentPage(msg.Page, ContentPageTransition.Fade);
                });

            MessengerInstance.Register<RequestNavigationToContentPageType>(this, (msg) =>
                {
                    // TODO: some of these view models are intended to be created only once and cached
                    // for the life of the app - presently being created new each time. See flag on all IContentPage.

                    // TODO: should call Cleanup() on our VMs when released. All of our VMs implement ICleanup throubh ViewModelBase.
                    // This may be mostly already handled when a page is deactivated and does not need to be cached (by ContentPageViewModel base class).

                    IContentPage page = null;
                    switch(msg.PageType)
                    {
                        case ContentPageTypes.Home:
                            if (CurrentPage != null && CurrentPage.PageType == ContentPageTypes.Home)
                                return;

                            page = new HomeDashboardViewModel(App.GetInstance<IAppModel>());
                            break;

                        // store related pages

                        case ContentPageTypes.StoreDashboard:
                            Debug.Assert(msg.Store != null);
                            page = new StoreDashboardViewModel(msg.Store);
                            break;

                        case ContentPageTypes.StoreLoginsSummary:
                            Debug.Assert(msg.Store != null);
                            page = new StoreLoginsSummaryViewModel(msg.Store);
                            break;

                        case ContentPageTypes.StoreScanSummary:
                            Debug.Assert(msg.Store != null);
                            page = new StoreScanSummaryViewModel(msg.Store);
                            break;

                        case ContentPageTypes.StoreCommitSummary:
                            Debug.Assert(msg.Store != null);
                            page = new StoreCommitSummaryViewModel(msg.Store);
                            break;

                        case ContentPageTypes.StoreCommitBatch:
                            Debug.Assert(msg.Store != null);
                            page = new StoreCommitBatchViewModel(msg.Store, msg.ItemIdentifier);
                            break;

                        // vendor related pages

                        case ContentPageTypes.VendorDashboard:
                            Debug.Assert(msg.Vendor != null);
                            page =  GetCachedVendorViewModel(msg.Vendor, ContentPageTypes.VendorDashboard, () => new VendorDashboardViewModel(msg.Vendor));
                            break;

                        case ContentPageTypes.VendorCommits:
                            Debug.Assert(msg.Vendor != null);
                            page = new VendorCommitsViewModel(msg.Vendor);
                            break;

                        case ContentPageTypes.VendorScan:
                            Debug.Assert(msg.Vendor != null);
                            page = GetCachedVendorViewModel(msg.Vendor, ContentPageTypes.VendorScan, () => new VendorScanViewModel(msg.Vendor));
                            break;

                        case ContentPageTypes.VendorStockCheck:
                            Debug.Assert(msg.Vendor != null);
                            page = new VendorStockCheckViewModel(msg.Vendor);
                            break;

                        case ContentPageTypes.VendorTests:
                            Debug.Assert(msg.Vendor != null);
                            page = new VendorTestsViewModel(msg.Vendor);
                            break;

                        case ContentPageTypes.VendorProperties:
                            Debug.Assert(msg.Vendor != null);
                            page = new VendorPropertiesViewModel(msg.Vendor);
                            break;

                        case ContentPageTypes.VendorCommitBatch:
                            Debug.Assert(msg.Vendor != null);
                            page = new VendorCommitBatchViewModel(msg.Vendor, msg.ItemIdentifier);
                            break;


                        default:
                            break;
                    }

                    if (page != null)
                        NavigateToContentPage(page, ContentPageTransition.Fade);

                });

            // any already instantiated xaml element - not sure if used
            MessengerInstance.Register<RequestNavigationToView>(this, (msg) =>
                {
                    MessengerInstance.Send(new BeginNavigation(CurrentPage, null));
                    MessengerInstance.Send(new ShowContentView(msg.Element, ContentPageTransition.Fade));
                    MessengerInstance.Send(new NavigationCompleted(CurrentPage, null));
                    CurrentPage = null;
                    MessengerInstance.Send(new ChangeTreeViewSelection());
                });

        }

        private void PerformBackNavigation()
        {
            if (NavStack.Count == 0)
                return;

            var vm = NavStack.Pop();
            NavigateToContentPage(vm, ContentPageTransition.Fade);
        }

        private void FlushNavStack()
        {
            NavStack.Clear();
            MessengerInstance.Send(new AnnouncementMessage(Announcement.DisableBackNavigation));
        }

        private void PushNavStack(IContentPage page)
        {
            // TODO: This stack manipulation stuff isn't quite right yet - in terms of 
            // how it cleans things up based on the new page

            // we never have two levels of the same kind of page, so if the nw page type matches the 
            // page type at the top of stack, then first pop off the old one.

            if (NavStack.Count > 0 && NavStack.Peek().PageType == page.PageType)
                NavStack.Pop();

            NavStack.Push(page);
            MessengerInstance.Send(new AnnouncementMessage(Announcement.EnableBackNavigation));
        }


        private void NavigateToContentPage(IContentPage newPage, ContentPageTransition transition)
        {
            Debug.Assert(newPage != null);

            //Debug.WriteLine(string.Format("Navigating to: {0}", newPage.PageType));
            MessengerInstance.Send(new BeginNavigation(CurrentPage, newPage));

            // no matter what, if on home page, there is no further back navigation
            if (newPage.PageType == ContentPageTypes.Home)
                FlushNavStack();
            else if (CurrentPage != null)
                PushNavStack(CurrentPage);

            MessengerInstance.Send(new ShowContentPage(newPage, transition));

            // remove selection from tree since is home
            if (newPage.PageType == ContentPageTypes.Home)
            {
                MessengerInstance.Send(new ChangeTreeViewSelection());
            }
            else if (newPage is StoreContentPageViewModel)
            {
                var store = (newPage as StoreContentPageViewModel).Store;
                MessengerInstance.Send(new ChangeTreeViewSelection(store));
            }
            else if (newPage is VendorContentPageViewModel)
            {
                var vendor = (newPage as VendorContentPageViewModel).Vendor;
                MessengerInstance.Send(new ChangeTreeViewSelection(vendor));
            }

            MessengerInstance.Send(new NavigationCompleted(CurrentPage, newPage));
            CurrentPage = newPage;
        }
    }
}
