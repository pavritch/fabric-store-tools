//------------------------------------------------------------------------------
// 
// Interface: ITicklerCampaignsManager 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using Website.Entities;

namespace Website
{
    public interface ITicklerCampaignsManager
    {
        /// <summary>
        /// True when enabled in web.config.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Called periodically TicklerCampaignsService() to wake up manager and see if any work to do.
        /// </summary>
        /// <param name="isStopRequested"></param>
        void WakeUp(Func<bool> isStopRequested);

        /// <summary>
        /// Immediately terminate the specified campaign.
        /// </summary>
        /// <remarks>
        /// Set trigger to null, completedOn = now, status=Cancelled.
        /// </remarks>
        /// <param name="campaignID"></param>
        void CancelCampaign(int campaignID);

        /// <summary>
        /// Indicated abnormal termination (failed).
        /// </summary>
        /// <param name="campaignID"></param>
        void AbortCampaign(int campaignID);

        /// <summary>
        /// Unsubscribe from a specific campaign. Optionally qualified with customerID.
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="customerID"></param>
        /// <returns></returns>
        bool UnsubscribeCampaign(int campaignID, int? customerID = null);

        /// <summary>
        /// Get the email address associated with the customerID.
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        string GetCustomerEmailAddress(int customerID);

        /// <summary>
        /// Get the CustomerID associated with the email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        int? GetCustomerIDFromEmail(string email);

        /// <summary>
        /// Lowest level create campaign.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <param name="triggerOn"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool CreateCampaign(int customerID, TicklerCampaignKind kind, DateTime triggerOn, object data, bool isStaged=false);

        /// <summary>
        /// Mark a campaign as having been completed.
        /// </summary>
        /// <param name="campaignID"></param>
        void CompleteCampaign(int campaignID);

        TicklerCampaignEmailDates GetLastEmailDates(int customerID, TicklerCampaignKind? kind = null, int? campaignID = null);

        /// <summary>
        /// Create a new campaign for abandoned carts.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        Task<bool> CreateAbandonedCartCampaign(int customerID, string template = null);

        T Deserialize<T>(string json);
        string Serialize(object data);

        /// <summary>
        /// See if customer has any other running campaigns of this kind.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        bool IsAnyOtherRunningCampaigns(int customerID, TicklerCampaignKind kind);

        /// <summary>
        /// See if customer has unsubscribed from any other campaigns of this kind.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        bool IsUnsubscribed(int customerID, TicklerCampaignKind kind);

        TimeSpan? ElaspedTimeSinceSameKindOfCampaign(int customerID, TicklerCampaignKind kind);

        /// <summary>
        /// When was the last time this customer was mailed on a campaign of this kind.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        TicklerCampaign GetMostRecentMailedCampaign(int customerID, TicklerCampaignKind kind);

        /// <summary>
        /// Find out when the most recent running campaign of this kind was.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        TicklerCampaign GetMostRecentRunningCampaign(int customerID, TicklerCampaignKind kind);

        /// <summary>
        /// Get the count of campaign records for this status.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        int GetCampaignCount(TicklerCampaignStatus status);

        /// <summary>
        /// Create a campaign record for the customer for long time no see. Qualified on last email and must have stuff in cart.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="minDaysSinceLastEmail"></param>
        /// <returns></returns>
        Task<bool> CreateLongTimeNoSeeCampaign(int customerID, int minDaysSinceLastEmail, DateTime triggerOn);

        /// <summary>
        /// Create a campaign record for a customer who has only purchased swatches.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="minDaysSinceLastEmail"></param>
        /// <param name="triggerOn"></param>
        /// <returns></returns>
        Task<bool> CreateSwatchOnlyBuyerCampaign(int customerID, int minDaysSinceLastEmail, DateTime triggerOn);

        /// <summary>
        /// Called from website to indicate a new order has been received.
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        Task<bool> NewOrderNotification(int customerID, int orderNumber);
    }
}