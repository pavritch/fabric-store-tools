//------------------------------------------------------------------------------
// 
// Class: StockCheckNotificationService 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Website.Emails;

namespace Website
{
    /// <summary>
    /// Background thread to monitor the quality and availability of remote generator.
    /// </summary>
    public class StockCheckNotificationService : SimpleWorkerServiceBase
    {
#if DEBUG
        private const int intervalSeconds = 60 * 60 * 24; // 24hrs
        private const int initialDelaySeconds = 120; 
#else
        private const int intervalSeconds = 60 * 60 * 24; // 24hrs
        private const int initialDelaySeconds = 60 * 60 * 4; // 4hrs
#endif

        private bool isBusy = false;

        public StockCheckNotificationService()
            : base(intervalSeconds, "StockCheckNotificationService", initialDelaySeconds)
        {
        }

        /// <summary>
        /// Performs one unit (one pass) of work.
        /// </summary>
        protected override void RunSingleTask()
        {
            if (MvcApplication.Current.WebStores == null || isBusy)
                return;

            isBusy = true;
            foreach (var store in MvcApplication.Current.WebStores.Values.Where(e => e.StockCheckManager.IsEnabled).ToList())
            {
                if (IsStopRequested())
                    break;

                store.StockCheckManager.ReviewAndSendNotificationEmails(() => IsStopRequested(), 180);
            }
            isBusy = false;
        }

#if false
        // intended to ensure that emails can be sent on this thread without true HttpContext.
        private void TestEmail()
        {
            var products = new List<InStockNotificationEmail.ProductInfo>()
            {
                new InStockNotificationEmail.ProductInfo()
                {
                    SKU = "CH-34354-20",
                    Name = "34354-20 Belgian Linen Smoke by Clarence House",
                    OurPrice = 163.80M,
                    PageUrl = "http://www.insidefabric.com/p-1055254-34354-20-belgian-linen-smoke-by-clarence-house.aspx",
                    ImageUrl= "http://www.insidefabric.com/images/product/icon/34354-20-belgian-linen-smoke-by-clarence-house.jpg",
                },
            };

            var email = new InStockNotificationEmail(products, "pavritch@pcdynamics.com");

            var emailService = new Postal.EmailService();

            var message = emailService.CreateMailMessage(email);

            var from = message.From;
            var to = message.To; // array, use .Address
            var body = message.Body;
            var subject = message.Subject;
            var sender = message.Sender; // null
        }
#endif
    }
}