using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Postal;
using Website.Emails;

namespace Website.Controllers
{
    public class PreviewEmailsController : Controller
    {
        // to preview, browse to /Preview/InStockNotificationEmail
        // if multipart, then /Preview/InStockNotificationEmail?format=html

        // the name of the CSHTML files must match the type name [SampleTextOnlyEmail, InStockNotificationEmail] since
        // templates are located based on rules.

        // this format paramater is built into the Postal rendering engine.

        // ?format=html
        // ?format=text

        // 
        public ActionResult SampleTextOnlyEmail()
        {
            // these substitutions take place at sendgrid and will not show the substituted
            // values on the browser preview
            var subs = new Dictionary<string, string>()
            {
                {"-cat-", "This was put in for my cat."}
            };

            var email = new SampleTextOnlyEmail() { To = "pavritch@pcdynamics.com", IncludeUnsubscribeSubstitution = true, Substitutions = subs };

            MvcApplication.Current.WebStores[StoreKeys.InsideFabric].SendEmail(email);

            return new EmailViewResult(email);

        }

        public ActionResult SampleHtmlOnlyEmail()
        {
            // these substitutions take place at sendgrid and will not show the substituted
            // values on the browser preview
            var subs = new Dictionary<string, string>()
            {
                {"-cat-", "This was put in for my cat."}
            };
            

            var email = new SampleHtmlOnlyEmail() { To = "pavritch@pcdynamics.com", IncludeUnsubscribeSubstitution=true, Substitutions = subs };

            MvcApplication.Current.WebStores[StoreKeys.InsideFabric].SendEmail(email);

            return new EmailViewResult(email);
        }


        public ActionResult InStockNotificationEmail()
        {
            var products = new List<InStockNotificationEmail.ProductInfo>()
            {
                new InStockNotificationEmail.ProductInfo()
                {
                    SKU = "CH-34354-20",
                    Name = "34354-20 Belgian Linen Smoke by Clarence House",
                    OurPrice = 163.80M,
                    PageUrl = "http://www.insidefabric.com/p-1055254-34354-20-belgian-linen-smoke-by-clarence-house.aspx",
                    ImageUrl= "http://www.insidefabric.com/images/product/icon/34354-20-belgian-linen-smoke-nc14-by-clarence-house.jpg",
                },

                new InStockNotificationEmail.ProductInfo()
                {
                    SKU = "FS-173277",
                    Name = "173277 Chiang Mai Dragon Jade by F Schumacher",
                    OurPrice = 156.80M,
                    PageUrl = "http://www.insidefabric.com/p-998600-173277-chiang-mai-dragon-jade-by-f-schumacher.aspx",
                    ImageUrl= "http://www.insidefabric.com/images/product/icon/173277-chiang-mai-dragon-jade-by-fschumacher.jpg",
                },

            };



            // NOTE - sending to petera hits phone, ipad and outlook for Peter due to redirections in MailEnable.

            var email = new InStockNotificationEmail(products, "petera@pcdynamics.com");
            MvcApplication.Current.WebStores[StoreKeys.InsideFabric].SendEmail(email);


            // sample:
            //var emailService = new Postal.EmailService();
            //var message = emailService.CreateMailMessage(email);

            //var from = message.From;
            //var to = message.To; // array, use .Address
            //var body = message.Body;
            //var subject = message.Subject;
            //var sender = message.Sender; // null

            return new EmailViewResult(email);
        }



        public ActionResult AbandonedCartEmail()
        {
            // to preview:
            // /Preview/AbandonedCartEmail?format=html

            var store = MvcApplication.Current.WebStores[StoreKeys.InsideFabric];

            var products = new List<TicklerCampaignHandler.ProductInfo>()
            {
                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "CH-34354-20",
                    Name = "34354-20 Belgian Linen Smoke by Clarence House",
                    //OurPrice = 163.80M,
                    PageUrl = "https://www.insidefabric.com/p-1055254-34354-20-belgian-linen-smoke-by-clarence-house.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/34354-20-belgian-linen-smoke-nc14-by-clarence-house.jpg",
                },

                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "FS-173277",
                    Name = "173277 Chiang Mai Dragon Jade by F Schumacher",
                    //OurPrice = 156.80M,
                    PageUrl = "https://www.insidefabric.com/p-998600-173277-chiang-mai-dragon-jade-by-f-schumacher.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/173277-chiang-mai-dragon-jade-by-fschumacher.jpg",
                },

            };

            // NOTE - sending to peterav hits phone, ipad and outlook for Peter due to redirections in MailEnable.

            // cartProducts, favoriteProducts - for test, the same
            var email = new AbandonedCartEmail(products, products, "peterav@pcdynamics.com")
            {
                StoreName = store.FriendlyName,
                StoreLink = string.Format("https://www.{0}", store.Domain),
                StoreLogoUrl = string.Format("http://image01.insidefabric.com/images/{0}Logo.png", store.StoreKey),
                UnsubscribeUrl = string.Format("https://www.{0}/Unsubscribe.aspx?t={1}&k=ac", store.Domain, AbandonedCartTicklerCampaignHandler.MakeUnsubscribeToken(1, 2, TicklerCampaignKind.AbandonedCart)),
                FindSimilarUrlTemplate = string.Format("https://www.{0}/Search.aspx?SearchTerm=like:@@SKU@@&{1}", store.Domain, "fake-tracking"),
                MyFavoritesUrl = string.Format("https://www.{0}/MyFavorites.aspx?id={1}", store.Domain, AbandonedCartTicklerCampaignHandler.MakeCustomerIDToken(customerID: 1)), // fake
            };

            store.SendEmail(email);


            return new EmailViewResult(email);
        }


        public ActionResult LongTimeNoSeeEmail()
        {
            // to preview:
            // /Preview/LongTimeNoSeeEmail?format=html

            var store = MvcApplication.Current.WebStores[StoreKeys.InsideFabric];

            var products = new List<TicklerCampaignHandler.ProductInfo>()
            {
                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "CH-34354-20",
                    Name = "34354-20 Belgian Linen Smoke by Clarence House",
                    //OurPrice = 163.80M,
                    PageUrl = "https://www.insidefabric.com/p-1055254-34354-20-belgian-linen-smoke-by-clarence-house.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/34354-20-belgian-linen-smoke-nc14-by-clarence-house.jpg",
                },

                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "FS-173277",
                    Name = "173277 Chiang Mai Dragon Jade by F Schumacher",
                    //OurPrice = 156.80M,
                    PageUrl = "https://www.insidefabric.com/p-998600-173277-chiang-mai-dragon-jade-by-f-schumacher.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/173277-chiang-mai-dragon-jade-by-fschumacher.jpg",
                },

            };

            // NOTE - sending to peterav hits phone, ipad and outlook for Peter due to redirections in MailEnable.

            // cartProducts, favoriteProducts - for test, the same
            var email = new LongTimeNoSeeEmail(products, products, "peterav@pcdynamics.com")
            {
                StoreName = store.FriendlyName,
                StoreLink = string.Format("https://www.{0}", store.Domain),
                StoreLogoUrl = string.Format("http://image01.insidefabric.com/images/{0}Logo.png", store.StoreKey),
                UnsubscribeUrl = string.Format("https://www.{0}/Unsubscribe.aspx?t={1}&k=ac", store.Domain, AbandonedCartTicklerCampaignHandler.MakeUnsubscribeToken(1, 2, TicklerCampaignKind.AbandonedCart)),
                FindSimilarUrlTemplate = string.Format("https://www.{0}/Search.aspx?SearchTerm=like:@@SKU@@&{1}", store.Domain, "fake-tracking"),
                MyFavoritesUrl = string.Format("https://www.{0}/MyFavorites.aspx?id={1}", store.Domain, AbandonedCartTicklerCampaignHandler.MakeCustomerIDToken(customerID:1)), // fake

            };

            store.SendEmail(email);


            return new EmailViewResult(email);
        }



        public ActionResult OnlySwatchesEmail()
        {
            // to preview:
            // /Preview/OnlySwatchesEmail?format=html

            var store = MvcApplication.Current.WebStores[StoreKeys.InsideFabric];

            var products = new List<TicklerCampaignHandler.ProductInfo>()
            {
                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "CH-34354-20",
                    Name = "34354-20 Belgian Linen Smoke by Clarence House",
                    //OurPrice = 163.80M,
                    PageUrl = "https://www.insidefabric.com/p-1055254-34354-20-belgian-linen-smoke-by-clarence-house.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/34354-20-belgian-linen-smoke-nc14-by-clarence-house.jpg",
                },

                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "FS-173277",
                    Name = "173277 Chiang Mai Dragon Jade by F Schumacher",
                    //OurPrice = 156.80M,
                    PageUrl = "https://www.insidefabric.com/p-998600-173277-chiang-mai-dragon-jade-by-f-schumacher.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/173277-chiang-mai-dragon-jade-by-fschumacher.jpg",
                },

            };

            // NOTE - sending to peterav hits phone, ipad and outlook for Peter due to redirections in MailEnable.

            // cartProducts, favoriteProducts - for test, the same
            var email = new OnlySwatchesEmail(products, "peterav@pcdynamics.com")
            {
                IncludeCoupon = true,
                StoreName = store.FriendlyName,
                StoreLink = string.Format("https://www.{0}", store.Domain),
                StoreLogoUrl = string.Format("http://image01.insidefabric.com/images/{0}Logo.png", store.StoreKey),
                UnsubscribeUrl = string.Format("https://www.{0}/Unsubscribe.aspx?t={1}&k=os", store.Domain, AbandonedCartTicklerCampaignHandler.MakeUnsubscribeToken(1, 2, TicklerCampaignKind.OnlySwatches)), // fake
                MyPurchasesUrl = string.Format("https://www.{0}/MyPurchases.aspx?id={1}", store.Domain, AbandonedCartTicklerCampaignHandler.MakeCustomerIDToken(customerID:1)), // fake
                FindSimilarUrlTemplate = string.Format("https://www.{0}/Search.aspx?SearchTerm=like:@@SKU@@&{1}", store.Domain, "fake-tracking")
            };

            store.SendEmail(email);


            return new EmailViewResult(email);
        }


        public ActionResult RecentSwatchPurchaseEmail()
        {
            // to preview:
            // /Preview/RecentSwatchPurchaseEmail?format=html

            var store = MvcApplication.Current.WebStores[StoreKeys.InsideFabric];

            var products = new List<TicklerCampaignHandler.ProductInfo>()
            {
                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "CH-34354-20",
                    Name = "34354-20 Belgian Linen Smoke by Clarence House",
                    //OurPrice = 163.80M,
                    PageUrl = "https://www.insidefabric.com/p-1055254-34354-20-belgian-linen-smoke-by-clarence-house.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/34354-20-belgian-linen-smoke-nc14-by-clarence-house.jpg",
                },

                new TicklerCampaignHandler.ProductInfo()
                {
                    SKU = "FS-173277",
                    Name = "173277 Chiang Mai Dragon Jade by F Schumacher",
                    //OurPrice = 156.80M,
                    PageUrl = "https://www.insidefabric.com/p-998600-173277-chiang-mai-dragon-jade-by-f-schumacher.aspx",
                    ImageUrl= "https://www.insidefabric.com/images/product/icon/173277-chiang-mai-dragon-jade-by-fschumacher.jpg",
                },

            };

            // NOTE - sending to peterav hits phone, ipad and outlook for Peter due to redirections in MailEnable.

            Website.Entities.Order order;

            // come up with a random swatch order
            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                // variantName column is TEXT, so cannot do a compare in SQL directly
                var allItems = dc.Orders_ShoppingCarts.Where(e => e.CreatedOn > DateTime.Parse("12/1/2016")).Select(e => new { e.OrderNumber, e.OrderedProductVariantName }).ToList();
                allItems.RemoveAll(e => !string.Equals(e.OrderedProductVariantName, "Swatch", StringComparison.OrdinalIgnoreCase));
                // get a distinct list of orders having at least one swatch, pick one at random
                var orderNumbers = allItems.Select(e => e.OrderNumber).Distinct().ToList();
                orderNumbers.Shuffle();
                var orderNumber = orderNumbers.FirstOrDefault();
                order = dc.Orders.Where(e => e.OrderNumber == orderNumber).FirstOrDefault();
            }

            // cartProducts, favoriteProducts - for test, the same
            var email = new RecentSwatchPurchaseEmail(order, products, "peterav@pcdynamics.com")
            {
                IncludeCoupon = false,
                StoreName = store.FriendlyName,
                StoreLink = string.Format("https://www.{0}", store.Domain),
                StoreLogoUrl = string.Format("http://image01.insidefabric.com/images/{0}Logo.png", store.StoreKey),
                UnsubscribeUrl = string.Format("https://www.{0}/Unsubscribe.aspx?t={1}&k=os", store.Domain, AbandonedCartTicklerCampaignHandler.MakeUnsubscribeToken(1, 2, TicklerCampaignKind.OnlySwatches)), // fake
                MyPurchasesUrl = string.Format("https://www.{0}/MyPurchases.aspx?id={1}", store.Domain, AbandonedCartTicklerCampaignHandler.MakeCustomerIDToken(customerID: 1)), // fake
                FindSimilarUrlTemplate = string.Format("https://www.{0}/Search.aspx?SearchTerm=like:@@SKU@@&{1}", store.Domain, "fake-tracking")
            };

            store.SendEmail(email);


            return new EmailViewResult(email);
        }


    }
}
