using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;
using Website.Entities;
using Gen4;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters;

namespace Website
{

    public enum TicklerCampaignStatus
    {
        /// <summary>
        /// Campaign is being staged/prepared. Code will switch to running state when ready.
        /// </summary>
        [Description("Staged")]
        Staged,

        /// <summary>
        /// Campaign is currently running.
        /// </summary>
        [Description("Running")]
        Running,

        /// <summary>
        /// Campaign is currently suspended.
        /// </summary>
        [Description("Suspended")]
        Suspended,

        /// <summary>
        /// Cancelled (typically by user).
        /// </summary>
        /// <remarks>
        /// Unsubscribe causes cancellation if was still running at the time. Otherwise,
        /// unsubscribing does not alter a non-running terminal status.
        /// </remarks>
        [Description("Cancelled")]
        Cancelled,

        /// <summary>
        /// Aborted (typically due to exception).
        /// </summary>
        [Description("Failed")]
        Failed,

        /// <summary>
        /// Campaign ran its course and completed without errors.
        /// </summary>
        [Description("Completed")]
        Completed,
    }

    public enum TicklerCampaignKind
    {
        [Description("Generic")]
        Generic,

        [Description("AbandonedCart")]
        AbandonedCart,

        [Description("OnlySwatches")]
        OnlySwatches,

    }

    public class TicklerCampaignEmailDates
    {
        public DateTime? LastEmailEver {get; set;}
        public DateTime? LastEmailForKind {get; set;}
        public DateTime? LastEmailForCampaign {get; set;}
    }

    public class TicklerCampaignsManager : ITicklerCampaignsManager
    {
        protected IWebStore Store { get; private set; }


        public TicklerCampaignsManager(IWebStore store)
        {
            this.Store = store;
        }

        public bool IsEnabled
        {
            get
            {
                return Store.IsTicklerCampaignsEnabled;
            }
        }


        public virtual void ProcessSingleRecord(TicklerCampaign record)
        {
            try
            {
                // can only get here if hit a trigger, which also means should be running

                // we do not worry about any race condition if unsubscribed while record already triggered
                // and waiting in queue for processing. Unsubscribe is eventually consistent.

                ITicklerCampaignHandler handler = null;

                switch(record.Kind)
                {
                    case "AbandonedCart":
                        handler = new AbandonedCartTicklerCampaignHandler(Store, this);
                        break;

                    case "OnlySwatches":
                        handler = new OnlySwatchesTicklerCampaignHandler(Store, this);
                        break;

                    default:
                        // if we don't recognize the kind, then kill it now
                        AbortCampaign(record.CampaignID);
                        break;
                }

                if (handler != null)
                {
                    // it is the responsibility of the handler to update record disposition
                    // using callbacks to manager.
                    handler.WakeUp(record);
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                LogException(Ex);
            }
        }

        /// <summary>
        /// Are there any other presently running campaigns of this same kind.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public bool IsAnyOtherRunningCampaigns(int customerID, TicklerCampaignKind kind)
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var isOthers = dc.TicklerCampaigns.Where(e => e.CustomerID == customerID && e.Kind == kind.DescriptionAttr() && e.Status == TicklerCampaignStatus.Running.DescriptionAttr()).Count() > 0;

                return isOthers;
            }
        }

        /// <summary>
        /// Has customer ever unsubscribed from a campaign of this kind.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public bool IsUnsubscribed(int customerID, TicklerCampaignKind kind)
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var isUnsubscribed = dc.TicklerCampaigns.Where(e => e.CustomerID == customerID && e.Kind == kind.DescriptionAttr() && e.UnsubscribedOn.HasValue).Count() > 0;

                return isUnsubscribed;
            }
        }

        public TicklerCampaignEmailDates GetLastEmailDates(int customerID, TicklerCampaignKind? kind=null, int? campaignID=null)
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var records = dc.TicklerCampaigns.Where(e => e.CustomerID == customerID).Select(e => new { e.CampaignID, e.Kind, e.EmailedOn}).ToList();

                var dates = new TicklerCampaignEmailDates();

                if (records.Any(e => e.EmailedOn.HasValue))
                {
                    // ever
                    dates.LastEmailEver = records.Where(e => e.EmailedOn.HasValue).Select(e => e.EmailedOn).Max();

                    // by kind
                    if (kind.HasValue && records.Any(e => e.Kind == kind.Value.DescriptionAttr()))
                        dates.LastEmailForKind = records.Where(e => e.EmailedOn.HasValue && e.Kind == kind.Value.DescriptionAttr()).Select(e => e.EmailedOn).Max();

                    // by campaign
                    if (campaignID.HasValue && records.Any(e => e.CampaignID == campaignID.Value))
                        dates.LastEmailForCampaign = records.Where(e => e.CampaignID == campaignID.Value).Select(e => e.EmailedOn).FirstOrDefault();
                }

                return dates;
            }
        }

        public TicklerCampaign GetMostRecentMailedCampaign(int customerID, TicklerCampaignKind kind)
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var record = dc.TicklerCampaigns.Where(e => e.CustomerID == customerID && e.Kind == kind.DescriptionAttr() && e.Status == TicklerCampaignStatus.Completed.DescriptionAttr() && e.EmailedOn != null).OrderByDescending(e => e.EmailedOn).FirstOrDefault();
                return record;
            }
        }

        public TicklerCampaign GetMostRecentRunningCampaign(int customerID, TicklerCampaignKind kind)
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var record = dc.TicklerCampaigns.Where(e => e.CustomerID == customerID && e.Kind == kind.DescriptionAttr() && e.Status == TicklerCampaignStatus.Running.DescriptionAttr() && e.EmailedOn != null).OrderByDescending(e => e.TriggerOn).FirstOrDefault();
                return record;
            }
        }

        public int GetCampaignCount(TicklerCampaignStatus status)
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var count = dc.TicklerCampaigns.Where(e => e.Status == status.DescriptionAttr()).Count();
                return count;
            }
        }


        public TimeSpan? ElaspedTimeSinceSameKindOfCampaign(int customerID, TicklerCampaignKind kind)
        {
            var previous = GetLastEmailDates(customerID, kind);

            if (!previous.LastEmailForKind.HasValue)
                return null;

            return DateTime.Now - previous.LastEmailForKind;
        }

        /// <summary>
        /// Create a new campaign record and associate with the customer. 
        /// </summary>
        /// <remarks>
        /// In theory, customerID can be 0 for some action not associated with a specific customer. Logic at this level 
        /// treats this as an opaque identifier.
        /// </remarks>
        /// <param name="customerID"></param>
        /// <param name="kind"></param>
        /// <param name="triggerOn"></param>
        /// <param name="data"></param>
        /// <returns>CampaignID for the new record.</returns>
        public virtual bool CreateCampaign(int customerID, TicklerCampaignKind kind, DateTime triggerOn, object data, bool isStaged=false)
        {
            using (var dc = new AspStoreDataContext(Store.ConnectionString))
            {
                var record = new TicklerCampaign()
                {
                    CustomerID = customerID,
                    Kind = kind.DescriptionAttr(),
                    TriggerOn = triggerOn,
                    JsonData =   data != null ? Serialize(data) : null,
                    CreatedOn = DateTime.Now,
                    TerminatedOn = null,
                    Status = isStaged ? TicklerCampaignStatus.Staged.DescriptionAttr() : TicklerCampaignStatus.Running.DescriptionAttr()
                    // unsubscribedOn, emailedOn left null
                };

                dc.TicklerCampaigns.InsertOnSubmit(record);
                dc.SubmitChanges(); 
   
                return true;
            }
        }

        public string GetCustomerEmailAddress(int customerID)
        {

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var email = dc.Customers.Where(e => e.CustomerID == customerID).Select(e => e.Email).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(email))
                    return email;

                return null;
            }

        }

        public int? GetCustomerIDFromEmail(string email)
        {
            // DEBUG ONLY!!!!
            // The same email could be in SQL multiple times under different accounts.
            // Cannot rely on results.

            int customerID;

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                customerID = dc.Customers.Where(e => e.Email == email).Select(e => e.CustomerID).FirstOrDefault();
                if (customerID != 0)
                    return customerID;
            }

            return null;
        }

        /// <summary>
        /// Primary entry point called by store website to fire up a campaign.
        /// </summary>
        /// <remarks>
        /// We must have a customerID and record in SQL to associate a campaign.
        /// </remarks>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public virtual Task<bool> CreateAbandonedCartCampaign(int customerID, string template = null)
        {
            var obj = new AbandonedCartTicklerCampaignHandler(Store, this);
            var result = obj.CreateCampaign(customerID);
            return Task.FromResult(result);
        }


        public virtual Task<bool> CreateLongTimeNoSeeCampaign(int customerID, int minDaysSinceLastEmail, DateTime triggerOn)
        {
            var obj = new AbandonedCartTicklerCampaignHandler(Store, this);
            var result = obj.CreateCampaign(customerID, "LongTimeNoSeeEmail", triggerOn, minDaysSinceLastEmail);
            return Task.FromResult(result);
        }

        public virtual Task<bool> CreateSwatchOnlyBuyerCampaign(int customerID, int minDaysSinceLastEmail, DateTime triggerOn)
        {
            var obj = new OnlySwatchesTicklerCampaignHandler(Store, this);
            var result = obj.CreateCampaign(customerID, null, triggerOn, minDaysSinceLastEmail);
            return Task.FromResult(result);
        }

        public virtual Task<bool> NewOrderNotification(int customerID, int orderNumber)
        {
            var obj = new OnlySwatchesTicklerCampaignHandler(Store, this);
            var result = obj.NewOrderNotification(customerID, orderNumber);
            return Task.FromResult(result);
        }


        /// <summary>
        /// Customer requested unsubscribe. If still running, immediately cancel as well.
        /// </summary>
        /// <remarks>
        /// Doesn't matter what kind of campaign it is.
        /// </remarks>
        /// <param name="campaignID"></param>
        public bool UnsubscribeCampaign(int campaignID, int? customerID = null)
        {
            try
            {
                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    var record = dc.TicklerCampaigns.Where(e => e.CampaignID == campaignID).FirstOrDefault();

                    // if already unsubcribed, don't allow pior date to be replaced

                    if (record == null)
                        return false;

                    if (customerID.HasValue && record.CustomerID != customerID.Value)
                        return false;

                    // make idemtpotent on unsubscribe so can call multiple times,
                    // will only take action the first time, but always show success

                    if (record.UnsubscribedOn.HasValue)
                        return true;

                    if (!record.TerminatedOn.HasValue)
                    {
                        // is running, so need to cancel as well
                        record.TriggerOn = null;
                        record.TerminatedOn = DateTime.Now;
                        record.Status = TicklerCampaignStatus.Cancelled.DescriptionAttr();
                    }

                    record.UnsubscribedOn = DateTime.Now; 
                    dc.SubmitChanges();

                    return true;
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                LogException(Ex);
                return false;
            }            
        }

        /// <summary>
        /// Lowest level cancellation. Note this does not adjust unsubscribe.
        /// </summary>
        /// <remarks>
        /// Generally would use unsubscribe() over cancel(). This method exs
        /// in case we have a need to cancel without unsubscribe.
        /// </remarks>
        /// <param name="campaignID"></param>
        public virtual void CancelCampaign(int campaignID)
        {
            FinalizeRecord(campaignID, TicklerCampaignStatus.Cancelled);
        }

        /// <summary>
        /// A completed campaign is once which ran through to completion (not cancelled or failed).
        /// </summary>
        /// <remarks>
        /// Was not cancelled or failed while executing. However, customer may later
        /// unsubscribe, which does not change the termination status.
        /// </remarks>
        /// <param name="campaignID"></param>
        public virtual void CompleteCampaign(int campaignID)
        {
            FinalizeRecord(campaignID, TicklerCampaignStatus.Completed);
        }

        /// <summary>
        /// An aborted campaign means something fatal happened - like an exception thrown.
        /// </summary>
        /// <remarks>
        /// Generally reserved for system level problems where we want to stop things to 
        /// ensure don't keep sending emails over and over again to some customer.
        /// </remarks>
        /// <param name="campaignID"></param>
        public virtual void AbortCampaign(int campaignID)
        {
            FinalizeRecord(campaignID, TicklerCampaignStatus.Failed);
        }


        public virtual void FinalizeRecord(int campaignID, TicklerCampaignStatus status)
        {
            try
            {
                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    var record = dc.TicklerCampaigns.Where(e => e.CampaignID == campaignID).FirstOrDefault();

                    // ignore if not currently running

                    if (record == null || record.Status != TicklerCampaignStatus.Running.DescriptionAttr())
                        return;

                    // note that TerminatedOn is for any end of campaign.... it's when transition from Running to 
                    // one of the other terminal status values

                    var now = DateTime.Now;

                    record.TriggerOn = null;
                    record.TerminatedOn = now;
                    record.Status = status.DescriptionAttr();

                    if (status == TicklerCampaignStatus.Completed && !record.EmailedOn.HasValue)
                        record.EmailedOn = now;

                    dc.SubmitChanges();
                }
            
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                LogException(Ex);
            }
        }


        /// <summary>
        /// Called every few minutes by TicklerCampaignsService.
        /// </summary>
        /// <remarks>
        /// Wake up every few minutes and see if there is anything to do.
        /// Call ProcessSingleRecord() for each record that has triggered.
        /// </remarks>
        /// <param name="isStopRequested"></param>
        public virtual void WakeUp(Func<bool> isStopRequested)
        {
            if (Store.IsTicklerCampaignQueueProcessingPaused || !Store.IsTicklerCampaignsTimerEnabled)
                return;

            try
            {
                Func<bool> isCancelled = () =>
                {
                    if (isStopRequested != null)
                        return isStopRequested();

                    return false;
                };

                List<TicklerCampaign> readyRecords;

                using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
                {
                    // note that trigger could be non-null for staged and suspended campaigns

                    // get list of campaigns ready to do work
                    readyRecords = dc.TicklerCampaigns.Where(e => e.TriggerOn != null && DateTime.Now >= e.TriggerOn.Value && e.Status == TicklerCampaignStatus.Running.DescriptionAttr()).ToList();
                }

                foreach (var record in readyRecords)
                {
                    // this cancelled is for the wakeup call, not any specific campaign record -- will be true when shutting down
                    if (isCancelled())
                        break;


                    try
                    {
                        ProcessSingleRecord(record);
                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine(Ex.Message);

                        // any record that causes exception causes this campaign to die
                        AbortCampaign(record.CampaignID);
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                LogException(Ex);
            }
        }

        /// <summary>
        /// Provided as support to handlers so will use identical settings.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }

        public string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented, SerializerSettings);
        }

        /// <summary>
        /// Common settings used for serialization/deserialization.
        /// </summary>
        protected JsonSerializerSettings SerializerSettings
        {
            get
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>() { new IsoDateTimeConverter() },
                    TypeNameHandling = TypeNameHandling.None,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                };

                return jsonSettings;
            }
        }

        protected void LogException(Exception Ex)
        {
            var ev = new WebsiteRequestErrorEvent("Unhandled Exception in TicklerCampaignManager", this, WebsiteEventCode.UnhandledException, Ex);
            ev.Raise();
        }

    }
}