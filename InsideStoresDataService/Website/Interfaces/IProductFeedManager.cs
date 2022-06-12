using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Manages all product feeds for a specific store.
    /// </summary>
    public interface IProductFeedManager
    {
        /// <summary>
        /// Regenerate all product feeds.
        /// </summary>
        /// <returns></returns>
        void GenerateAll();

        /// <summary>
        /// Only regenerated feeds which are missing or specifically in need of regeneration.
        /// </summary>
        void GenerateMissingFeeds();

        /// <summary>
        /// Generated the named product feed.
        /// </summary>
        /// <param name="feedKey"></param>
        /// <returns></returns>
        bool Generate(ProductFeedKeys feedKey);

        /// <summary>
        /// Returns the set of feeds registered with this manager.
        /// </summary>
        IEnumerable<IProductFeed> RegisteredFeeds { get; }
    }
}