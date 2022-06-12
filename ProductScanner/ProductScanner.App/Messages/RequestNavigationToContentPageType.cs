using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    /// <summary>
    /// Request navigation a a type of page using the supplied instance data.
    /// </summary>
    /// <remarks>
    /// Navigation requests are received by the navigation service and subsequently tracked and reflected
    /// to the rest of the system as confirmed navigation moves.
    /// </remarks>
    class RequestNavigationToContentPageType : IMessage
    {
        /// <summary>
        /// The kind of page to be requested. Required.
        /// </summary>
        /// <remarks>
        /// Many of the page types require additional parameters. The Home type requires none.
        /// </remarks>
        public ContentPageTypes PageType{ get; set; }

        /// <summary>
        /// Optional reference to a store when the target vm is for a store.
        /// </summary>
        /// <remarks>
        /// Leave null when not needed. The navigation service will only reference this field when needed for the page type.
        /// </remarks>
        public IStoreModel Store { get; set; }

        /// <summary>
        /// Optional reference to a vendor when the target vm is for a vendor.
        /// </summary>
        /// <remarks>
        /// Leave null when not needed. The navigation service will only reference this field when needed for the page type.
        /// </remarks>
        public IVendorModel Vendor { get; set; }

        /// <summary>
        /// Optional reference to some specific item when needed, such as a BatchID.
        /// </summary>
        public int ItemIdentifier { get; set; }

        public RequestNavigationToContentPageType(ContentPageTypes pageType)
        {
            this.PageType = pageType;
        }

        /// <summary>
        /// Request navigation to a store type page using the provided store as the source.
        /// </summary>
        /// <remarks>
        /// The page type must be one that supports direct navigation.
        /// </remarks>
        /// <param name="store"></param>
        /// <param name="pageType"></param>
        public RequestNavigationToContentPageType(IStoreModel store, ContentPageTypes pageType= ContentPageTypes.StoreDashboard) : this(pageType)
        {
            this.Store = store;
        }

        /// <summary>
        /// Request navigation to a vendor type page using the provided vendor as the source.
        /// </summary>
        /// <remarks>
        /// The page type must be one that supports direct navigation.
        /// </remarks>
        /// <param name="vendor"></param>
        /// <param name="pageType"></param>
        public RequestNavigationToContentPageType(IVendorModel vendor, ContentPageTypes pageType = ContentPageTypes.VendorDashboard)
            : this(pageType)
        {
            this.Vendor = vendor;
        }
    }
}
