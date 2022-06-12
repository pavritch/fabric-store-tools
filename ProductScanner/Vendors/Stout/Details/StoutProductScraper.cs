using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using Newtonsoft.Json;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Stout.Details
{
    /*
    {  
   "sku":"AARO-1",
   "desc":"AARON 1 ECRU",
   "avail":"9.000",
   "phase":"1",
   "lead":"42",
   "uom":"YARD",
   "vert":"7.000",
   "horz":"6.500",
   "itype":"1",
   "usable":"54.00",
   "col":"1",
   "exl":"2",
   "price":"29.990",
   "map":"44.99",
   "colors":[  
      "AARO-1"
   ],
   "clean":[  
      "S-SOLVENT BASED CLEANER",
      "DRY CLEAN ONLY"
   ],
   "fnsh":[  
      "KISS-COAT BACKING"
   ],
   "minydg":"1",
   "tsty":"Transitional Abstract",
   "tcon":"Upholstery Jacquard",
   "tcolor":"",
   "book":[  
      "RAINBOW LIBRARY ALMOND/OATMEAL"
   ],
   "booknum":[  
      "1466"
   ],
   "width":"54.00",
   "country":"Usa",
   "content":[  
      "65% Polyester",
      "35% Cotton"
   ],
   "test":[  
      "FLAME RETARDANT-N.F.P.A. 260A CLASS 1",
      "CATB 117-2013",
      "WYZENBEEK 20,000 DOUBLE RUB WEAR TEST (HEAVY DUTY)"
   ],
   "rr":"N",
   "prop65":"",
   "suitable":"",
   "API":"c6564b6b-d6ee-414b-b7a6-213e3d87d918"
}
    */

    public class StoutReturn
    {
        public int total { get; set; }
        public int cnt { get; set; }
        public List<StoutProduct> result { get; set; }
    }

    public class StoutProduct
    {
        public string sku { get; set; }
        public string desc { get; set; }
        public double avail { get; set; }
        public string uom { get; set; }
        public double vert { get; set; }
        public double horz { get; set; }
        public double price { get; set; }
        public double map { get; set; }
        public string phase { get; set; }
        public int lead { get; set; }
        public List<string> clean { get; set; }
        public List<string> fnsh { get; set; }
        public string minydg { get; set; }
        public string tsty { get; set; }
        public string tcon { get; set; }
        public string tcolor { get; set; }
        public List<string> book { get; set; }
        public List<string> booknum { get; set; }
        public double width { get; set; }
        public string country { get; set; }
        public List<string> content { get; set; }
        public List<string> test { get; set; }
        public string rr { get; set; }
        public string prop65 { get; set; }
        public string suitable { get; set; }
    }

    public class StoutProductScraper : ProductScraper<StoutVendor>
    {
        private const string DetailUrl = "https://www.estout.com/api/search?api=c6564b6b-d6ee-414b-b7a6-213e3d87d918&detail=2&id={0}";

        public StoutProductScraper(IPageFetcher<StoutVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct discoveredProduct)
        {
            var url = string.Format(DetailUrl, discoveredProduct.MPN);
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, discoveredProduct.MPN);

            var objects = JsonConvert.DeserializeObject<StoutReturn>(page.InnerText);
            var productData = objects.result.First();

            var scanData = new ScanData();
            scanData[ScanField.ManufacturerPartNumber] = discoveredProduct.MPN;
            scanData.DetailUrl = new Uri(url);
            scanData[ScanField.StockCount] = productData.avail.ToString();
            scanData[ScanField.Cleaning] = string.Join(", ", productData.clean);
            scanData[ScanField.Railroaded] = productData.rr;
            scanData[ScanField.LeadTime] = productData.lead.ToString();
            scanData[ScanField.Status] = productData.phase;
            scanData[ScanField.Country] = productData.country;
            scanData[ScanField.Finish] = string.Join(", ", productData.fnsh);
            scanData[ScanField.Description] = string.Join(", ", productData.desc);
            scanData.Cost = Convert.ToDecimal(productData.price);
            scanData[ScanField.MAP] = productData.map.ToString();
            scanData[ScanField.UnitOfMeasure] = productData.uom;
            scanData[ScanField.VerticalRepeat] = productData.vert.ToString();
            scanData[ScanField.HorizontalRepeat] = productData.horz.ToString();
            scanData[ScanField.MinimumOrder] = productData.minydg.ToString();
            scanData[ScanField.Prop65] = productData.prop65;

            // see if this works
            var imageUrl = "ftp://insidestores:sbFtp64296@ftp.estout.com/" + productData.sku + ".jpg";
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

            /*
        public string tsty { get; set; }
        public string tcon { get; set; }
        public string tcolor { get; set; }
        public List<string> book { get; set; }
        public List<int> booknum { get; set; }
        public double width { get; set; }
        public List<string> content { get; set; }
        public List<string> test { get; set; }
        public string suitable { get; set; }
            */

            return new List<ScanData> { scanData };
        }
    }
}