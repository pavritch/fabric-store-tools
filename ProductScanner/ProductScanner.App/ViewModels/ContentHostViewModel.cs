using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using GalaSoft.MvvmLight;

namespace ProductScanner.App.ViewModels
{
    /// <summary>
    /// The content host is the main container which transitions in the different kinds of content pages.
    /// </summary>
    /// <remarks>
    /// Listens for messages to display a given view model.
    /// </remarks>
    public class ContentHostViewModel : ViewModelBase
    {
        public ContentHostViewModel()
        {
            if (!IsInDesignMode)
            {
                HookMessages();
            }
        }

        private void HookMessages()
        {
            MessengerInstance.Register<ShowContentPage>(this, (msg) =>
            {
                DeactivateOldPage();

                Transition = msg.Transition;

                // initially implemented this using implicit styles, passing the vm and using template definitions
                // within the xaml file. However, this proved troublesome when the pages made use of the 
                // visual state manager - and after lots of grief, finally determined that implicit templates
                // have problems with the VSM, so punted and simple created the controls directly here.

                // using implicit styles
                // PageContent = msg.Page;

                // using control factory
                PageContent = ControlFactory(msg.Page);

                msg.Page.Activated();
            });

            MessengerInstance.Register<ShowContentView>(this, (msg) =>
            {
                DeactivateOldPage();
                Transition = msg.Transition;
                PageContent = msg.Element;
            });

        }
        
        private void DeactivateOldPage()
        {
            // due to transition from using implicit styles (passing in just the VM) to using
            // a control factory, we test for either kind of object to see if there 
            // is an IContentPage which needs to be deactivated.

            if (PageContent != null)
            {
                if (PageContent is IContentPage)
                {
                    var page = PageContent as IContentPage;
                    page.DeActivated();
                }
                else if (PageContent is Control)
                {
                    var dc = (PageContent as Control).DataContext;
                    if (dc is IContentPage)
                    {
                        var page = dc as IContentPage;
                        page.DeActivated();
                    }
                }
            }
        }

        private Control ControlFactory(IContentPage page)
        {
            Control ctrl = null;

            switch (page.PageType)
            {
                case ContentPageTypes.Home:
                    ctrl = new ProductScanner.App.Views.HomeDashboard();
                    break;

                // store related pages

                case ContentPageTypes.StoreDashboard:
                    ctrl = new ProductScanner.App.Views.StoreDashboard();
                    break;

                case ContentPageTypes.StoreLoginsSummary:
                    ctrl = new ProductScanner.App.Views.StoreLoginsSummary();
                    break;

                case ContentPageTypes.StoreScanSummary:
                    ctrl = new ProductScanner.App.Views.StoreScanSummary();
                    break;

                case ContentPageTypes.StoreCommitSummary:
                    ctrl = new ProductScanner.App.Views.StoreCommitSummary();
                    break;

                case ContentPageTypes.StoreCommitBatch:
                    ctrl = new ProductScanner.App.Views.StoreCommitBatch();
                    break;

                // vendor related pages

                case ContentPageTypes.VendorDashboard:
                    ctrl = new ProductScanner.App.Views.VendorDashboard();
                    break;

                case ContentPageTypes.VendorCommits:
                    ctrl = new ProductScanner.App.Views.VendorCommits();
                    break;

                case ContentPageTypes.VendorScan:
                    ctrl = new ProductScanner.App.Views.VendorScan();
                    break;

                case ContentPageTypes.VendorStockCheck:
                    ctrl = new ProductScanner.App.Views.VendorStockCheck();
                    break;

                case ContentPageTypes.VendorTests:
                    ctrl = new ProductScanner.App.Views.VendorTests();
                    break;

                case ContentPageTypes.VendorProperties:
                    ctrl = new ProductScanner.App.Views.VendorProperties();
                    break;

                case ContentPageTypes.VendorCommitBatch:
                    ctrl = new ProductScanner.App.Views.VendorCommitBatch();
                    break;

                default:
                    break;
            }

            if (ctrl != null)
                ctrl.DataContext = page;

            return ctrl;
        }
        
        private ContentPageTransition _transition = ContentPageTransition.None;

        public ContentPageTransition Transition
        {
            get
            {
                return _transition;
            }
            set
            {
                Set(() => Transition, ref _transition, value);
            }
        }        

        private object _pageContent = null;

        /// <summary>
        /// Holds the view model for the page to be displayed.
        /// </summary>
        /// <remarks>
        /// The Xaml will use implicit binding to connect to the correct UserControl.
        /// </remarks>
        public object PageContent
        {
            get
            {
                return _pageContent;
            }
            set
            {
                Set(() => PageContent, ref _pageContent, value);
            }
        }
    }
}