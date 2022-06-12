using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Single product feed.
    /// </summary>
    public interface IProductFeed
    {
        /// <summary>
        /// Indicates which kind of feed this is.
        /// </summary>
        ProductFeedKeys Key {get;}

        /// <summary>
        /// Generate this product feed.
        /// </summary>
        /// <returns></returns>
        bool Generate();

        /// <summary>
        /// The disk location where this feed is stored.
        /// </summary>
        /// <remarks>
        /// Full absolute path.
        /// </remarks>
        string FeedFilePath { get; }

        /// <summary>
        /// Determine if the feed has a need to regenerate itself.
        /// </summary>
        /// <remarks>
        /// Typical reason would be that it's file is missing.
        /// </remarks>
        /// <returns></returns>
        bool IsNeedToGenerate();

        /// <summary>
        /// Indicates if this feed is presently being generated.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// The store associated with this feed.
        /// </summary>
        IWebStore Store { get; }

        /// <summary>
        /// Cancel any feed generation which may be in progress at earliest moment.
        /// </summary>
        void CancelOperation();

        /// <summary>
        /// Add a product to the feed while being populated.
        /// </summary>
        /// <remarks>
        /// This is more of a utility method to be called only be
        /// some of the plumbing during population. Not intended
        /// to be called arbitrarily at any time.
        /// </remarks>
        /// <param name="feedProduct"></param>
        void AddProduct(FeedProduct feedProduct);

        /// <summary>
        /// Used for debugging to cap off the max number of products.
        /// </summary>
        bool IsMaxedOut { get; }

        /// <summary>
        /// The kind of file txt|csv which is generated.
        /// </summary>
        /// <remarks>
        /// Most are tab-del txt files, but shopify is comma-del csv
        /// </remarks>
        ProductFeedFormats DefaultFileFormat {get;}
    }
}