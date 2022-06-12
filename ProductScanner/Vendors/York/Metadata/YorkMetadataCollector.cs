using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelLibrary.SpreadSheet;
using Fizzler.Systems.HtmlAgilityPack;
using OfficeOpenXml;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace York.Metadata
{
    public class YorkMetadataCollector : IMetadataCollector<YorkVendor>
    {
        private readonly IPageFetcher<YorkVendor> _pageFetcher;
        private readonly IStorageProvider<YorkVendor> _storageProvider;
        private readonly IVendorScanSessionManager<YorkVendor> _sessionManager;
        private readonly YorkMasterFileLoader _masterFileLoader;
        private readonly YorkPricingFileLoader _priceFileLoader;
        private readonly YorkInventoryFileLoader _inventoryFileLoader;
        private readonly YorkInventoryFileDownloader _inventoryFileDownloader;

        public YorkMetadataCollector(IPageFetcher<YorkVendor> pageFetcher, 
            IStorageProvider<YorkVendor> storageProvider,
            IVendorScanSessionManager<YorkVendor> sessionManager,
            YorkMasterFileLoader masterFileLoader,
            YorkPricingFileLoader priceFileLoader,
            YorkInventoryFileLoader inventoryFileLoader,
            YorkInventoryFileDownloader inventoryFileDownloader)
        {
            _pageFetcher = pageFetcher;
            _storageProvider = storageProvider;
            _sessionManager = sessionManager;
            _masterFileLoader = masterFileLoader;
            _priceFileLoader = priceFileLoader;
            _inventoryFileLoader = inventoryFileLoader;
            _inventoryFileDownloader = inventoryFileDownloader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var availableThemes = await DiscoverAvailableThemes();
            var productsInThemes = await DiscoverProductsInThemes(availableThemes);

            // pulls in a few extra properties include Color, Coordinates, Country, Design, Style, UPC
            var fileProducts = ReadFromSpreadsheets();
            SetMetadataProperties(products, fileProducts, productsInThemes);

            _inventoryFileDownloader.Download();

            var pricingData = _masterFileLoader.LoadProducts();
            var pricingDataBackup = _priceFileLoader.LoadInventoryData();
            var inventoryData = _inventoryFileLoader.LoadInventoryData();
            foreach (var product in products)
            {
                var match = pricingData.FirstOrDefault(x => product[ScanField.ManufacturerPartNumber] == x[ScanField.ManufacturerPartNumber]);
                if (match != null)
                {
                    product[ScanField.MAP] = match[ScanField.MAP];
                    product[ScanField.RetailPrice] = match[ScanField.RetailPrice];
                }

                var matchTwo = pricingDataBackup.FirstOrDefault(x => product[ScanField.ManufacturerPartNumber] == x[ScanField.ManufacturerPartNumber]);
                if (matchTwo != null)
                {
                    if (product[ScanField.MAP] == "0" || product[ScanField.MAP] == string.Empty) product[ScanField.MAP] = matchTwo[ScanField.MAP];
                    if (product[ScanField.RetailPrice] == "0" || product[ScanField.MAP] == string.Empty) product[ScanField.RetailPrice] = matchTwo[ScanField.RetailPrice];
                }

                var inventoryMatch = inventoryData.FirstOrDefault(x => product[ScanField.ManufacturerPartNumber] == x[ScanField.ManufacturerPartNumber]);
                if (inventoryMatch != null)
                {
                    product[ScanField.StockCount] = inventoryMatch[ScanField.StockCount];
                }
            }

            return products;
        }

        private void SetMetadataProperties(List<ScanData> products, List<ScanData> excelProducts, Dictionary<string, HashSet<string>> productsInThemes)
        {
            foreach (var product in products)
            {
                var mpn = product[ScanField.ManufacturerPartNumber];
                var matches = excelProducts.Where(x => x[ScanField.ManufacturerPartNumber] == mpn).ToList();
                if (matches.Count == 0)
                    continue;
                var match = matches.OrderByDescending(x => x.Count).First();

                product[ScanField.UPC] = match[ScanField.UPC];
                product[ScanField.Country] = match[ScanField.Country].Replace("Made in the ", "").Replace("Made in ", "");
                product[ScanField.MinimumQuantity] = match[ScanField.MinimumQuantity];
                product[ScanField.Description] = match[ScanField.Description];

                // set width only when wallpaper
                if ((product.ContainsKey(ScanField.Category)) && product[ScanField.Category] == "Wallpaper")
                    product[ScanField.Width] = match[ScanField.PackageLength];

                // colors
                var colors = match[ScanField.Color].Split('/', ',').ToCommaDelimitedList().TitleCase();
                product[ScanField.Color] = colors;
                product[ScanField.Packaging] = match[ScanField.Packaging];

                product[ScanField.Bullet1] = match[ScanField.Bullet1];
                product[ScanField.Bullet2] = match[ScanField.Bullet2];
                product[ScanField.Bullet3] = match[ScanField.ProductName];
                product[ScanField.Bullet4] = match[ScanField.Dimensions];

                if (product[ScanField.Dimensions] == string.Empty) product[ScanField.Dimensions] = match[ScanField.Dimensions];

                // coordinating patterns
                var coord = new[]
                {
                    match[ScanField.Coordinates],
                    match[ScanField.Coordinates2],
                    match[ScanField.Coordinates3],
                    match[ScanField.Coordinates4],
                    match[ScanField.Coordinates5]
                };
                product[ScanField.Coordinates] = coord.ToCommaDelimitedList();
                product[ScanField.Style] = productsInThemes.Where(item => item.Value.Contains(mpn)).Select(item => item.Key).ToCommaDelimitedList();
            }
        }

        private async Task<Dictionary<string, HashSet<string>>> DiscoverProductsInThemes(IEnumerable<string> availableThemes)
        {
            var products = new Dictionary<string, HashSet<string>>();
            foreach (var themeName in availableThemes)
            {
                var productsInTheme = await DiscoverProductsInTheme(themeName);

                if (!products.ContainsKey(themeName))
                    products.Add(themeName, productsInTheme);
            }
            return products;
        }

        private async Task<List<string>> DiscoverAvailableThemes()
        {
            // root page for searching themes
            const string rootSearchUrl = "http://www.yorkwall.com/static/product_search";

            var page = await _pageFetcher.FetchAsync(rootSearchUrl, CacheFolder.Search, "search-menu");

            var container = page.QuerySelector("li[id='productThemeField']");

            if (container == null)
                throw new Exception("Unable to locate list of themes on root search page.");

            var themeNames1 = container.QuerySelectorAll("a[class='option productThemeField']").Select(e => e.InnerText.Trim()).ToList();
            var themeNames2 = container.QuerySelectorAll("a[class='special_effects_dimensional_suboption option productThemeField']").Select(e => e.InnerText.Trim()).ToList();
            return themeNames1.Union(themeNames2).ToList();
        }

        // Return the set of MPN for a given theme.
        private async Task<HashSet<string>> DiscoverProductsInTheme(string themeName)
        {
            var products = new HashSet<string>();
            var productTypes = new[] 
            {
                "Wallpaper",
                "Contract Wallcoverings",
                "Borders",
                "Murals",
                "Wall Appliques",
                "Fabrics",
                "Embellishments",
                "ALL" // might be redundant, but error on safety.
            };

            // one of the reasons we're being extra careful on the theme lookup is that it seemed the searches were capping out
            // at 18 pages no matter what... so maybe needed a more granular search than just ALL.

            await _sessionManager.ForEachNotifyAsync("Scanning metadata", productTypes, async productType =>
            {
                var cacheFoldername = string.Format("Themes\\{0}\\{1}", productType.MakeSafeSEName(), themeName.MakeSafeSEName());

                int pageNo = 0;
                string pageUrl;

                var rootSearchUrl = "http://www.yorkwall.com/static/product_search";
                var page = await _pageFetcher.FetchAsync(rootSearchUrl, CacheFolder.Search, "search-menu");

                // don't know how many products are in a theme - just spin through pages until come to the end

                while (true)
                {
                    pageNo++;

                    // POST to fixed url

                    pageUrl = "http://www.yorkwall.com/CGI-BIN/lansaweb?webapp=WPSE+webrtn=RESULTS+ml=LANSA:XHTML+partition=YWP+language=ENG+sid=;";

                    var nvCol = new NameValueCollection();

                    nvCol.Add("CURRPAGE", pageNo.ToString());
                    //nvCol.Add("_SESSIONKEY", sessionKey);
                    nvCol.Add("DSPTHEME", themeName); // Historic Reproduction, Contemporary, Novelty
                    nvCol.Add("PRODTYPE", productType); // or one of the N defined types, ex Wallpaper, Borders

                    var namedElements = new string[]
                    {
                        "STDRENTRY",
                        "STDSESSID",
                        "STDWEBUSR",
                        "STDWEBC01",
                        "STDTABFLR",
                        "STDROWNUM",
                        "STDUSERST",
                        "STDUSRTYP",
                        "LW3VARFLD",
                        "STDNXTFUN",
                        "STDPRVFUN",
                        "LW3SITTOT",
                        "LW3SITCNT",
                        "LW3EASTAT",
                        "LW3CUSIND",
                        "STDCUSIND",
                        "LW3PROCID",
                        "LW3VNDNME",
                        "STDLISTID",
                        "STD_ADLIN",
                        "PRIMCOLR",
                        "ACNTCOLR",
                        "PATTERN",
                        "KEYWORD",
                        "WW3SUBSIT",
                        "PRESS",
                        "NEWNBRPAG",
                        "CLKNBRPAG",
                        "NBRPAG",
                        "_SERVICENAME",
                        "_WEBAPP",
                        "_WEBROUTINE",
                        "_PARTITION",
                        "_LANGUAGE",
                        "_LW3TRCID"
                    };

                    foreach (var item in page.OwnerDocument.GetFormPostValuesByName(namedElements))
                        nvCol.Add(item.Key, item.Value);

                    var pageNode = await _pageFetcher.FetchAsync(pageUrl, CacheFolder.Search,
                        Path.Combine(cacheFoldername, "Page" + pageNo), nvCol);

                    // when no results--
                    // <li>Nothing found for your search.  Please try again.</li>

                    if (pageNo == 1 && pageNode.InnerHtml.ContainsIgnoreCase("Nothing found for your search."))
                        break;

                    var table = pageNode.QuerySelector("table[class='prdList']");

                    if (table == null)
                        break;

                    var productCells = table.QuerySelectorAll("td[width='230']").ToList();

                    if (!productCells.Any())
                        break;

                    // each cell is one product

                    foreach (var cell in productCells)
                    {

                        try
                        {
                            var firstCellTable = cell.SelectSingleNode("table");

                            var mpn = firstCellTable.SelectSingleNode("tbody/tr/td[2]").InnerText.Trim();
                            products.Add(mpn);

                        }
                        catch (Exception Ex)
                        {
                            var msg = string.Format("Error reading theme page: {0} page {1}\n{2}", themeName.MakeSafeSEName(), pageNo, Ex.Message);
                            //LogErrorEvent(msg);
                        }
                    }

                    // see if any more pages

                    //<li class="paginationNext"><a href="JavaScript:SubmitBRAND3Nxt()">Next</a></li>

                    if (pageNode.QuerySelectorAll("li[class='paginationNext']").Count() == 0)
                        break;

                    // else loop and get next page
                }
            });

            return products;
        }

        private List<ScanData> ReadFromSpreadsheets()
        {
            var path = _storageProvider.GetProductsFileStaticPath(ProductFileType.Xlsx);
            var outputFileInfo = new FileInfo(path);
            var outputFilepath = outputFileInfo.FullName;

            var folder = Path.Combine(Path.GetDirectoryName(outputFilepath), "Spreadsheets");
            var files = Directory.GetFiles(folder, "*.xls?", SearchOption.TopDirectoryOnly);

            var products = new List<ScanData>();
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file).ToLower();
                if (extension == ".xls") products.AddRange(ReadXLSFile(file));
                if (extension == ".xlsx") products.AddRange(ReadXLSXFile(file));
            }
            return products;
        }

        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Brand Name", ScanField.Brand },
            { "Vendor Item #", ScanField.ManufacturerPartNumber },
            { "Vendor", ScanField.ManufacturerPartNumber },
            { "Substrate", ScanField.Substrate },
            { "Product Name", ScanField.ProductName },
            { "Color", ScanField.Color },
            { "UPC", ScanField.UPC },
            { "UPC/EAN", ScanField.UPC },
            { "UPC #", ScanField.UPC },
            { "EAN", ScanField.UPC },
            { "MSRP", ScanField.RetailPrice },
            { "SR MSRP", ScanField.RetailPrice },
            // MAP only comes from the MapMaster.xlsx file
            { "MAP", ScanField.Ignore },
            { "SR MAP", ScanField.Ignore },
            { "Minimum Order Qty", ScanField.MinimumQuantity },
            { "Drop Ship Minimum Order Qty", ScanField.MinimumQuantity },
            { "Item Package Length", ScanField.PackageLength },
            { "Package Length", ScanField.PackageLength },
            { "Item Package Height", ScanField.PackageHeight },
            { "Package Height", ScanField.PackageHeight },
            { "Item Package Width", ScanField.PackageWidth },
            { "Package Width", ScanField.PackageWidth },
            { "Item Package Weight", ScanField.ShippingWeight },
            { "Package Weight", ScanField.ShippingWeight },
            { "Shipping Package Length", ScanField.Ignore },
            { "Shipping Package Height", ScanField.Ignore },
            { "Shipping Package Width", ScanField.Ignore },
            { "Shipping Package Weight", ScanField.ShippingWeight },
            { "Pattern Repeat", ScanField.Repeat },
            { "Pattern Repeat Inches", ScanField.Repeat },
            { "(Inches) Pattern Repeat", ScanField.Repeat },
            { "Drop Match", ScanField.Match },
            { "Match", ScanField.Match },
            { "Advertising Copy", ScanField.Description },
            { "Item Packaging", ScanField.Packaging },
            { "Product Dimension", ScanField.Dimensions },
            { "Key Features", ScanField.Bullet2 },
            { "Key Features II", ScanField.Bullet7 },
            { "Key Feature A", ScanField.Bullet2 },
            { "Key Feature B", ScanField.Bullet7 },
            { "Country of Origin", ScanField.Country },
            { "Country", ScanField.Country },
            { "Key Search Words", ScanField.Bullet1 },
            { "Print Type", ScanField.Ignore },
            { "Border Height", ScanField.BorderHeight },
            { "Theme", ScanField.Theme },
            { "License", ScanField.Ignore },
            { "Coordinating Patterns", ScanField.Coordinates },
            { "Coordinating Patterns 1", ScanField.Coordinates },
            { "2", ScanField.Coordinates2 },
            { "3", ScanField.Coordinates3 },
            { "4", ScanField.Coordinates4 },
            { "5", ScanField.Coordinates5 },
        };

        private List<ScanField> GetFields(Worksheet sheet)
        {
            var fieldsLower = _fields.ToDictionary(x => x.Key.ToLower(), x => x.Value);

            var fields = new List<ScanField>();
            var colNum = 0;
            while (true)
            {
                var headerText = string.Join(" ", sheet.Cells[0, colNum].StringValue.Trim(),
                    sheet.Cells[1, colNum].StringValue.Trim(),
                    sheet.Cells[2, colNum].StringValue.Trim());
                if (string.IsNullOrWhiteSpace(headerText)) break;

                fields.Add(fieldsLower[headerText.Trim().ToLower()]);
                colNum++;
            }
            return fields;
        }

        private List<ScanField> GetFields(ExcelWorksheet sheet)
        {
            var fieldsLower = _fields.ToDictionary(x => x.Key.ToLower(), x => x.Value);

            var fields = new List<ScanField>();
            var colNum = 1;
            while (true)
            {
                var headerText = string.Join(" ", (sheet.GetValue<string>(1, colNum) ?? string.Empty).Trim(),
                    (sheet.GetValue<string>(2, colNum) ?? string.Empty).Trim(),
                    (sheet.GetValue<string>(3, colNum) ?? string.Empty).Trim());
                if (string.IsNullOrWhiteSpace(headerText)) break;

                fields.Add(fieldsLower[headerText.Trim().ToLower()]);
                colNum++;
            }
            return fields;
        }

        private List<ScanData> ReadXLSFile(string filepath)
        {
            var products = new List<ScanData>();
            var workbook = Workbook.Load(filepath);
            var sheet = workbook.Worksheets[0];
            var fields = GetFields(sheet);

            for (var rowIndex = 4; rowIndex <= sheet.Cells.Rows.Count; rowIndex++)
            {
                var scanData = new ScanData();
                for (var colNum = 0; colNum < fields.Count; colNum++)
                {
                    var field = fields[colNum];
                    var value = sheet.Cells[rowIndex, colNum].StringValue;
                    scanData[field] = value;
                }
                products.Add(scanData);
            }
            return products;
        }

        private List<ScanData> ReadXLSXFile(string filepath)
        {
            var products = new List<ScanData>();

            using (var xls = new ExcelPackage(new FileInfo(filepath)))
            {
                var sheet = xls.Workbook.Worksheets[1];
                var fields = GetFields(sheet);
                for (var rowIndex = 5; rowIndex <= sheet.Dimension.End.Row; rowIndex++)
                {
                    var scanData = new ScanData();
                    for (var colNum = 1; colNum <= fields.Count; colNum++)
                    {
                        var field = fields[colNum - 1];
                        var value = sheet.GetValue<string>(rowIndex, colNum);
                        scanData[field] = value;
                    }
                    products.Add(scanData);
                }
            }
            return products;
        }
    }
}