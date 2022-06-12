using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Website
{
    public interface IStockCheckManager
    {
        /// <summary>
        /// Is stock check enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Return a list of all store vendors with their associated stock check capabilities.
        /// </summary>
        /// <returns></returns>
        Task<List<StockCheckVendorInfo>> GetStockCheckVendorInformation();

        /// <summary>
        /// Check stock for a single variant.
        /// </summary>
        /// <param name="variantID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Task<StockCheckResult> CheckStockForSingleVariant(int variantID, int quantity, string ip=null);


        /// <summary>
        /// Check stock for a set of variants.
        /// </summary>
        /// <param name="variantID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Task<List<StockCheckResult>> CheckStockForVariants(IEnumerable<StockCheckQuery> variants, string ip=null);


        /// <summary>
        /// User has requested to be notified when this item is back in stock.
        /// </summary>
        /// <param name="variantID"></param>
        /// <param name="quantity"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> NotifyRequest(int variantID, int quantity, string email);

        /// <summary>
        /// Peform one round of checks to see if any notifications can be sent out.
        /// </summary>
        /// <remarks>
        /// Can be run manually or via timer automation.
        /// </remarks>
        void ReviewAndSendNotificationEmails(Func<bool> isStopRequested, int expirationDays = 180);

    }
}