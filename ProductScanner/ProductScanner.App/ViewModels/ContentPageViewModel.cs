using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;

namespace ProductScanner.App.ViewModels
{
    public class ContentPageViewModel : ViewModelBase, IContentPage
    {
        private Guid _token;

        public ContentPageViewModel()
        {
            _token = Guid.NewGuid();
        }

        /// <summary>
        /// Indicates what kind of page this is.
        /// </summary>
        public ContentPageTypes PageType { get; set; }

        /// <summary>
        /// Page title to be shown at top of screen.
        /// </summary>
        public virtual string PageTitle { get; set; }

        /// <summary>
        /// Page subtitle to be shown at upper right of screen.
        /// </summary>
        public virtual string PageSubTitle { get; set; }

        /// <summary>
        /// Called when the page has just been made active.
        /// </summary>
        /// <remarks>
        /// Intended for when a cached view model is once again shown on screen.
        /// </remarks>
        public virtual void Activated()
        {

        }

        /// <summary>
        /// Called when a page is about to be hidden from view.
        /// </summary>
        /// <remarks>
        /// Provides a last chance opportunity to save state or clean up.
        /// </remarks>
        public virtual void DeActivated()
        {
            if (!RequiresToBeCached)
                Cleanup();
        }

        public override void Cleanup()
        {
            // unhook messenger
            base.Cleanup();
        }

        /// <summary>
        /// When true, indicates that the VM for this page should be cached and used as a singleton.
        /// </summary>
        /// <remarks>
        /// Some pages are intended to stick around for the life, while others are transient.
        /// </remarks>
        public bool RequiresToBeCached { get; protected set; }

        /// <summary>
        /// Token used to identify a single instance of a kind of page.
        /// </summary>
        public Guid PageToken { get { return _token; } }

        /// <summary>
        /// Indicates if the page can be directly jumped to without regard to ancestry.
        /// </summary>
        /// <remarks>
        /// Such pages to include store dashboards, vendor dashboards, etc.
        /// Not included would be something like a commit batch or commit batch log.
        /// </remarks>
        public bool IsNavigationJumpTarget { get; protected set; }

        /// <summary>
        /// Template which describes how breadcrumbs should be presented when this page is showing.
        /// </summary>
        /// <remarks>
        /// Breadcrumbs are not a true stack. Rather, they're more of a perspective on context. More of
        /// where you are rather than where you've been - even though sometimes sorty of the same.
        /// </remarks>
        public string BreadcrumbTemplate { get; protected set; }

    }
}