using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ShopifySharp;

namespace ShopifyDeleteProducts
{
  class Program
  {
    static void Main(string[] args)
    {

      System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      bool isCanada = args.Any((v) => string.Equals(v, "/canada", StringComparison.OrdinalIgnoreCase));
      Console.WriteLine(string.Format("Store: {0}", isCanada ? "CA" : "US"));

      var app = new App(isCanada);

      app.ProductManager.Load();
      app.ProductManager.Save();

      //TestShopifyProduct(app.ShopifyStoreUrl, app.ShopifyAppPassword);

      var dn = new Downloader(app);
      dn.Run();

      Console.WriteLine("Press any key.");
      Console.ReadKey();
    }



    static private async void TestShopifyShop()
    {
      const string PASSWORD = "6785325435435310e02c1c8721";
      const string STOREURL = "http://is-theme-dev.myshopify.com";

      var service = new ShopifyShopService(STOREURL, PASSWORD);

      var shop = await service.GetAsync();

      Debug.WriteLine(string.Format("Shop: {0}", shop.MyShopifyDomain));
    }

    static private async void TestShopifyProduct(string url, string password)
    {

      const string ALL_FIELDS = "id,title,body_html,created_at,updated_at,published_at,vendor,product_type,handle,template_suffix,published_scope,tags,variants,options,images,metafields";

      // 8387410377

      var startTime = DateTime.Now;

      var serviceProduct = new ShopifyProductService(url, password);

      var product = await serviceProduct.GetAsync(8387410377, ALL_FIELDS);
      Debug.WriteLine("Duration: {0}", DateTime.Now - startTime);
      Debug.WriteLine(string.Format("Product: {0}", product.Handle));

      var serviceMetaFields = new ShopifyMetaFieldService(url, password);
      var fields = await serviceMetaFields.ListAsync(product.Id, "products");

      Debug.WriteLine(string.Format("Product Metafields count: {0}", fields.Count()));

      var fields2 = await serviceMetaFields.ListAsync(product.Variants.First().Id, "variants");

      Debug.WriteLine(string.Format("Variant Metafields count: {0}", fields2.Count()));


      var serviceSmartCollection = new ShopifySmartCollectionService(url, password);

      var collection = await serviceSmartCollection.GetAsync(346897673);
      var fieldsCollection = await serviceMetaFields.ListAsync(collection.Id, "smart_collections");
      Debug.WriteLine(string.Format("Collection Metafields count: {0}", fieldsCollection.Count()));


      var countAllProducts = await serviceProduct.CountAsync();
      Debug.WriteLine(string.Format("All product count: {0:N0}", countAllProducts));


      var countFabric = await serviceProduct.CountAsync(new ShopifySharp.Filters.ShopifyProductFilter { ProductType = "Fabric" });
      Debug.WriteLine(string.Format("Fabric product count: {0:N0}", countFabric));


      // meta for shop does not exist
      //var serviceShop = new ShopifyShopService(STOREURL, PASSWORD);
      //var shop = await serviceShop.GetAsync();
      //var fieldsShop = await serviceMetaFields.ListAsync(shop.Id, "shops");
      //Debug.WriteLine(string.Format("Shop Metafields count: {0}", fieldsShop.Count()));

    }

  }
}
