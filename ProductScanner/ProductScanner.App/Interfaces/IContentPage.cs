using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    /// <summary>
    /// Every page which is displayed within ContentHost should implement this interface.
    /// </summary>
    public interface IContentPage
    {
        /// <summary>
        /// Indicates what kind of page this is.
        /// </summary>
        ContentPageTypes PageType {get;}

        /// <summary>
        /// Page title to be shown at top of screen.
        /// </summary>
        string PageTitle { get; }

        /// <summary>
        /// Page subtitle to be shown at upper right of screen.
        /// </summary>
        string PageSubTitle { get; }

        /// <summary>
        /// Called when the page has just been made active.
        /// </summary>
        /// <remarks>
        /// Intended for when a cached view model is once again shown on screen.
        /// </remarks>
        void Activated();

        /// <summary>
        /// Called when a page is about to be hidden from view.
        /// </summary>
        /// <remarks>
        /// Provides a last chance opportunity to save state or clean up.
        /// </remarks>
        void DeActivated();

        /// <summary>
        /// When true, indicates that the VM for this page should be cached and used as a singleton.
        /// </summary>
        /// <remarks>
        /// Some pages are intended to stick around for the life, while others are transient.
        /// </remarks>
        bool RequiresToBeCached { get; }

        Guid PageToken { get; }

        /// <summary>
        /// Indicates if the page can be directly jumped to without regard to ancestry.
        /// </summary>
        /// <remarks>
        /// Such pages to include store dashboards, vendor dashboards, etc.
        /// Not included would be something like a commit batch or commit batch log.
        /// </remarks>
        bool IsNavigationJumpTarget { get; }

        /// <summary>
        /// Template which describes how breadcrumbs should be presented when this page is showing.
        /// </summary>
        /// <remarks>
        /// Breadcrumbs are not a true stack. Rather, they're more of a perspective on context. More of
        /// where you are rather than where you've been - even though sometimes sorty of the same.
        /// </remarks>
        string BreadcrumbTemplate { get; }
    }
}
