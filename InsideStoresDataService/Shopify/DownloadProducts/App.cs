using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopifyCommon;

namespace ShopifyDeleteProducts
{
    public class App
    {
        public ProductInformationManager ProductManager { get; private set; }
        public ShopifyApiSentry ApiSentry { get; private set; }

        public string ShopifyStoreDomain { get; private set; }
        public string ShopifyStoreUrl { get; private set; }
        public string ShopifyAppPassword { get; private set; }
        public int ShopifyApiProductBatchSize { get; private set; }
        public bool IsCanada { get; private set; }
        public double MarkupCanada { get; private set; }

        public App(bool isCanada=false)
        {
            IsCanada = isCanada;

            // some of the config values are augmented with -CA for the Canadian store.
            Func<string, string> makeStoreKey = (key) =>
            {
                //<add key="ShopifyStoreDomain-CA" value="insidestores-ca.myshopify.com" />
                //<add key="ShopifyAppPassword-CA" value="10ee581615e1d692cca10b85cf5df4fb" />
                //<add key="ProductFilePath-CA" value="c:\temp\ShopifyProducts-CA.json"/>

                if (isCanada)
                    return key + "-CA";
                return key;
            };

            if (isCanada)
            {
                double markup = 0.0;
                Double.TryParse(ConfigurationManager.AppSettings[makeStoreKey("PriceMarkup")], out markup);
                if (markup < 1.0)
                    throw new Exception("Invalid markup for Canada in app.config file.");
                MarkupCanada = markup;
            }


            // API sentry
            var bucketSize = int.Parse(ConfigurationManager.AppSettings["ShopifyApiBucketMaxLevel"]);
            var leakRate = int.Parse(ConfigurationManager.AppSettings["ShopifyApiBucketLeakRate"]);
            ApiSentry = new ShopifyApiSentry(bucketSize, leakRate);

            // product manager
            var filepath = ConfigurationManager.AppSettings[makeStoreKey("ProductFilePath")];
            ProductManager = new ProductInformationManager(filepath, isCanada);

            ShopifyStoreDomain = ConfigurationManager.AppSettings[makeStoreKey("ShopifyStoreDomain")];
            ShopifyStoreUrl = string.Format("https://{0}", ShopifyStoreDomain);
            ShopifyAppPassword = ConfigurationManager.AppSettings[makeStoreKey("ShopifyAppPassword")];
            ShopifyApiProductBatchSize= int.Parse(ConfigurationManager.AppSettings["ShopifyApiProductBatchSize"]);



        }

    }
}
