using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using Gen4.Util.Misc;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace Website
{
    /// <summary>
    /// Interface for dealing with Share a Sale affiliate program.
    /// </summary>
    public interface IShareASaleManager
    {

        /// <summary>
        /// Downloads the most recent version of the specified merchant's data feed.
        /// </summary>
        /// Saved in ShareASale folder root as MERCHANT-ID.txt.
        /// <param name="merchantID"></param>
        /// <returns></returns>
        Task<bool> DownloadDataFeed(int merchantID);

        /// <summary>
        /// Gets a random set of products.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        List<AffiliateProduct> GetRandomProductList(int count, int? seed = null);
    }
}