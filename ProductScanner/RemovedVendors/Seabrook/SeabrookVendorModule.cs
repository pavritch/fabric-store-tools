using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FabricUpdater.Core;
using FabricUpdater.Core.Interfaces;
using FabricUpdater.Core.Scanning;
using FabricUpdater.Core.Scanning.ProductProperties;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Utilities;

namespace FabricUpdater.Vendors.Seabrook
{
    public class SeabrookVendorModule : VendorModuleBase
    {
        private readonly string _bordersMainUrl = "http://www.seabrookwallpaper.com/products/default.aspx?t=1";
        private readonly string _wallpaperMainUrl = "http://www.seabrookwallpaper.com/products/default.aspx?t=2";
        private readonly string _muralsMainUrl = "http://www.seabrookwallpaper.com/products/default.aspx?t=3";
        private readonly HashSet<string> _discoveredProductIds = new HashSet<string>();
        private HtmlNode _lastPage;
        private string _loginUrl;
        private readonly List<List<string>> _patternGroups = new List<List<string>>(); 

        private readonly Dictionary<string, ProductPropertyType> _labels = new Dictionary<string, ProductPropertyType>
        {
            { "Pattern ID:", ProductPropertyType.ItemNumber },
            { "Collection Name:", ProductPropertyType.Collection },
            { "Brand:", ProductPropertyType.Brand },
            { "Type:", ProductPropertyType.ProductType },
            { "Prepasted:", ProductPropertyType.Prepasted },
            { "Sq. Ft. per Roll:", ProductPropertyType.AdditionalInfo },
            { "Wallpaper Width:", ProductPropertyType.Width },
            { "Special Finishes:", ProductPropertyType.Finishes },
            { "Vertical Repeat:", ProductPropertyType.VerticalRepeat },
            { "Horizontal Repeat:", ProductPropertyType.HorizontalRepeat },
            { "Pattern Match:", ProductPropertyType.Match },
            { "Length of Roll:", ProductPropertyType.Length },
            { "Length of Border Spool:", ProductPropertyType.Length },
            { "Primary Color 1:", ProductPropertyType.Color },
            { "Primary Color 2:", ProductPropertyType.Color1 },
            { "Secondary Color:", ProductPropertyType.Color2 },
            { "Background Color:", ProductPropertyType.BackgroundColor },
            { "Material/Composition:", ProductPropertyType.Material },
            { "Artist:", ProductPropertyType.Designer },
            { "Style/Category:", ProductPropertyType.TempContent1 },
            { "Subject:", ProductPropertyType.Style },
            { "NOTE:", ProductPropertyType.Note },
            { "Border Height:", ProductPropertyType.BorderHeight },
            { "Backing:", ProductPropertyType.Backing },
            { "Size:", ProductPropertyType.Dimensions }
        }; 

        public SeabrookVendorModule(IRepository service, IHtmlCodeHelper codeHelper, IWebsiteDataContext dataContext)
            : base(service, codeHelper, dataContext, "seabrook", "Seabrook", "SE")
        {
        }

        protected override bool Authenticate()
        {
            var page = LoadDocument("Stock", "default" + Guid.NewGuid(), VendorCredential.LoginUrl);

            var viewstate = page.QuerySelector("#__VIEWSTATE").Attributes["value"].Value;
            var eventvalidation = page.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value;

            var values = new NameValueCollection();
            values.Add("CEWFR_EVENTTARGET", "CEWFR_RNDR1_eaILGunORDun1");
            values.Add("__EVENTTARGET", "");
            values.Add("__EVENTARGUMENT", "");
            values.Add("__VIEWSTATE", viewstate);
            values.Add("__EVENTVALIDATION", eventvalidation);
            values.Add("CEWFR_RNDR1$eaILGunID", VendorCredential.Username);
            values.Add("CEWFR_RNDR1$eaILGunPSW", VendorCredential.Password);
            values.Add("CEWFR_RNDR1$eaILGunSTSun1", "Go to Order Entry Screen");
            values.Add("CEWFR_RNDR1$eaILGunPSWunN1", "");
            values.Add("CEWFR_RNDR1$eaILGunPSWunN2", "");

            _loginUrl = WebClient.ResponseUri.AbsoluteUri;
            _lastPage = LoadDocument("Stock", "defaultAuth" + Guid.NewGuid(), WebClient.ResponseUri.AbsoluteUri, values);
            return true;
        }

        protected override void VendorProductDiscoveryByWebsite()
        {
            var wallpaperPage = LoadDocument(string.Empty, "wallpapers", _wallpaperMainUrl);
            var styleIds = FindStyleIds(wallpaperPage);
            FindAllProducts(_wallpaperMainUrl, "wallpapers", styleIds);

            var borderPage = LoadDocument(string.Empty, "borders", _bordersMainUrl);
            styleIds = FindStyleIds(borderPage);
            FindAllProducts(_bordersMainUrl, "borders", styleIds);

            var muralPage = LoadDocument(string.Empty, "murals", _muralsMainUrl);
            styleIds = FindStyleIds(muralPage);
            FindAllProducts(_muralsMainUrl, "murals", styleIds);

            NotifyStatusMessage("Loading Product Details...");
            var products = LoadProductDetails();

            NotifyStatusMessage("Checking Stock...");
            for (int i = 0; i < products.Count; i++)
            {
                NotifyProgressAndRemaining(i, products.Count);
                CheckPriceAndStock(products[i], _loginUrl);
            }

            SetPatternCorrelators(_patternGroups, products.Cast<IVendorProduct>().ToList());
            VendorProducts = products.ToDictionary(k => k.ManufacturerPartNumber, v => v as IVendorProduct);
        }

        private void CheckPriceAndStock(SeabrookProduct product, string url)
        {
            var viewstate = _lastPage.QuerySelector("#__VIEWSTATE").Attributes["value"].Value;
            var eventvalidation = _lastPage.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value;
            var productId = product.Key;

            var values = new NameValueCollection();
            values.Add("CEWFR_EVENTTARGET", "CEWFR_RNDR1_eaORBunSTATUSun1");
            values.Add("__EVENTTARGET", "");
            values.Add("__EVENTARGUMENT", "");
            values.Add("__VIEWSTATE", viewstate);
            values.Add("__EVENTVALIDATION", eventvalidation);
            values.Add("CEWFR_RNDR1$eaORDunQTYunATun00", "2");
            values.Add("CEWFR_RNDR1$eaPATunACCESSunATun00", productId);
            values.Add("CEWFR_RNDR1$eaORBunSTATUSun1", "Check Price and Inventory Availability");

            var page = LoadDocument("Stock", "check-" + productId, url, values);

            if (page.OuterHtml.Contains("Password"))
            {
                // we're back on the main screen - reauthenticate?
                // haven't seen this again so not sure yet
                Authenticate();
            }

            var priceInCents = page.QuerySelector("#CEWFR_RNDR1_eaORDunDunPRICunATun00").InnerText;
            var stock = page.QuerySelector("#CEWFR_RNDR1_eaORDunAVAILunATun00").InnerText;
            var unit = page.QuerySelector("#CEWFR_RNDR1_eaSTKunUNITunATun00").InnerText;
            var notes = page.QuerySelector("#CEWFR_RNDR1_eaCMTunCMTunATun00").InnerText;

            var unitOfMeasure = "Roll";
            if (unit.ContainsIgnoreCase("Each"))
            {
                unitOfMeasure = "Each";
            }
            else if (unit.ContainsIgnoreCase("Yard"))
            {
                unitOfMeasure = "Yard";
            }
            product.VendorProperties[ProductPropertyType.UnitOfMeasure] = unitOfMeasure;
            product.VendorProperties[ProductPropertyType.Note] = notes;

            product.VendorProperties[ProductPropertyType.WholesalePrice] = (Convert.ToDouble(priceInCents) / 100.0).ToString();
            product.VendorProperties[ProductPropertyType.StockCount] = stock;

            _lastPage = page;
        }

        private List<SeabrookProduct> LoadProductDetails()
        {
            var products = new List<SeabrookProduct>();
            var ct = 0;
            foreach (var productId in _discoveredProductIds)
            {
                var productUrl = String.Format("http://www.seabrookwallpaper.com/products/default.aspx?t=2&patternID={0}", productId);
                var page = LoadDocument("Details", productId, productUrl);

                var propertiesTable = page.QuerySelector("#Table2 table table");
                if (propertiesTable == null)
                {
                    // occasionally a listing page is downloaded instead of the product?
                    // I'm not sure yet what causes this to happen
                    continue;
                }

                var propertyRows = propertiesTable.QuerySelectorAll("tr");
                var newProduct = new SeabrookProduct(ProductConfigValues);

                foreach (var row in propertyRows)
                {
                    var tdNodes = row.QuerySelectorAll("td").ToList();
                    var labelNode = tdNodes.First();
                    var valueNode = tdNodes.Last();

                    var label = labelNode.InnerText;
                    var value = valueNode.InnerText;

                    if (label.Contains("square footage")) continue;

                    var propertyType = _labels.SingleOrDefault(x => x.Key == label).Value;
                    // this is the default PPT, so if this is returned we didn't find a match
                    if (propertyType != ProductPropertyType.ManufacturerPartNumber)
                    {
                        if (propertyType == ProductPropertyType.Style || 
                            propertyType == ProductPropertyType.TempContent1 ||
                            propertyType == ProductPropertyType.Designer)
                        {
                            value = valueNode.QuerySelectorAll("a").Select(x => x.InnerText).Aggregate((a, b) => a + ", " + b);
                        }
                        newProduct.VendorProperties[propertyType] = value;
                    }
                }

                var imageNode = page.QuerySelector("#Table2 table div img");
                var imageSrc = imageNode.Attributes["src"].Value;
                var itemNumber = imageSrc.ToLower().CaptureWithinMatchedPattern("/patterns/(?<capture>(.*))_lg.jpg").ToUpper();

                var webItemNumber = "SBK" + productId;
                newProduct.VendorProperties[ProductPropertyType.WebItemNumber] = webItemNumber;
                newProduct.VendorProperties[ProductPropertyType.ItemNumber] = itemNumber;
                newProduct.VendorProperties[ProductPropertyType.ManufacturerPartNumber] = itemNumber;
                newProduct.VendorProperties[ProductPropertyType.ProductGroup] = "Wallpaper";
                newProduct.VendorProperties[ProductPropertyType.OrderIncrement] = "2";
                
                if (newProduct.VendorProperties.ContainsKey(ProductPropertyType.Note) && 
                    newProduct.VendorProperties[ProductPropertyType.Note].ContainsIgnoreCase("by the yard"))
                    newProduct.VendorProperties[ProductPropertyType.UnitOfMeasure] = "Yard";
                
                CaptureCorrelatedPatterns(itemNumber, page);

                var imagePageUrl = string.Format("http://www.seabrookwallpaper.com/products/photo.aspx?patternID={0}", productId);
                page = LoadDocument("Images", productId, imagePageUrl);

                newProduct.VendorProperties[ProductPropertyType.ImageUrl] = 
                    "http://www.seabrookwallpaper.com" + page.QuerySelector("#BigPhoto").Attributes["src"].Value;
                newProduct.SetAndValidateKey();

                // don't add any dupes
                if (!products.Any(x => x.GetVendorProperty(ProductPropertyType.ManufacturerPartNumber) == itemNumber))
                {
                    products.Add(newProduct);
                }

                NotifyProgressAndRemaining(ct++, _discoveredProductIds.Count);
            }
            return products;
        }

        private void CaptureCorrelatedPatterns(string itemNumber, HtmlNode page)
        {
            var correlatedPatterns = page.QuerySelectorAll("#PatternDisplay_PatternDetail_ctl01_ColorWays img");
            var patternIds = correlatedPatterns.Select(x => x.Attributes["src"].Value.CaptureWithinMatchedPattern("/patterns/(?<capture>(.*))_cw.jpg")).ToList();
            patternIds.Add(itemNumber);

            // need to sort so SequenceEqual works
            patternIds = patternIds.OrderBy(x => x).ToList();

            // see if we already have this group
            if (!GroupExists(patternIds))
            {
                _patternGroups.Add(patternIds);
            }
        }

        private bool GroupExists(List<string> ids)
        {
            return _patternGroups.Any(ids.SequenceEqual);
        }

        private void FindAllProducts(string baseUrl, string folder, List<string> styleIds)
        {
            var styleCt = 1;
            foreach (var styleId in styleIds)
            {
                NotifyStatusMessage(string.Format("Scanning by Style...{0} of {1}", styleCt++, styleIds.Count));
                var styleUrl = baseUrl + "&styleID=" + styleId;
                var page = LoadDocument(folder, "style-" + styleId, styleUrl);

                var subjectIds = FindSubjectIds(page);
                var subjectCt = 1;
                foreach (var subjectId in subjectIds)
                {
                    NotifyProgressAndRemaining(subjectCt++, subjectIds.Count);
                    var subjectUrl = styleUrl + "&subjectID=" + subjectId;
                    var pageName = "style-" + styleId + "-subject-" + subjectId;
                    page = LoadDocument(folder, pageName + "-1", subjectUrl);
                    var patternIds = FindPatternIds(page);

                    var pages = page.QuerySelector("#PatternDisplay_PageCountLabel");
                    var currentPage = page;
                    if (pages != null)
                    {
                        var numPages = Convert.ToInt32(pages.InnerText);
                        for (int pageId = 2; pageId <= numPages; pageId++)
                        {
                            currentPage = GetNextPage(folder, currentPage, pageName + "-" + pageId, pageId, subjectUrl);
                            patternIds.AddRange(FindPatternIds(currentPage));
                        }
                    }
                    patternIds.ForEach(x => _discoveredProductIds.Add(x));
                }
            }
        }

        private HtmlNode GetNextPage(string folder, HtmlNode currentPage, string pageName, int pageId, string subjectUrl)
        {
            var viewstate = currentPage.QuerySelector("#__VIEWSTATE").Attributes["value"].Value;
            var eventValidation = currentPage.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value;

            var values = new NameValueCollection();
            values.Add("__EVENTTARGET", "PatternDisplay$JumpPage");
            values.Add("__EVENTARGUMENT", "");
            values.Add("__LASTFOCUS", "");
            values.Add("__VIEWSTATE", viewstate);
            values.Add("__EVENTVALIDATION", eventValidation);
            values.Add("MainTopNav$Types", "");
            values.Add("MainTopNav$Subject", "0");
            values.Add("PatternDisplay$JumpPage", pageId.ToString());

            return LoadDocumentWithCheck(folder, pageName, subjectUrl, values);
        }

        private HtmlNode LoadDocumentWithCheck(string folder, string filename, string url, NameValueCollection values = null)
        {
            CancelToken.ThrowIfCancellationRequested();
            var page = LoadDocument(folder, filename, url, values);
            if (page.InnerText.Contains("Your Denial of Service attempt has been denied!!!"))
            {
                // we triggered the DOS checks on the server - wait several minutes and try again
                throw new Exception("Denial of Service checks triggered on server. Wait several minutes and try again.");
            }
            return page;
        }

        private List<string> FindPatternIds(HtmlNode page)
        {
            var hrefs = page.QuerySelectorAll("a[href*='patternID']").Select(x => x.Attributes["href"].Value);
            return hrefs.Select(x => x.CaptureWithinMatchedPattern("patternID=(?<capture>(.*))&search")).ToList();
        }

        private List<string> FindStyleIds(HtmlNode page)
        {
            var hrefs = page.QuerySelectorAll(".subject").Select(x => x.Attributes["href"].Value);
            return hrefs.Select(x => x.CaptureWithinMatchedPattern("styleID=(?<capture>(.*))")).ToList();
        }

        private List<string> FindSubjectIds(HtmlNode page)
        {
            var hrefs = page.QuerySelectorAll(".subject").Select(x => x.Attributes["href"].Value);
            return hrefs.Select(x => x.CaptureWithinMatchedPattern("subjectID=(?<capture>(.*))")).ToList();
        }

        protected override void Initialize()
        {
            _discoveredProductIds.Clear();
            VendorProducts.Clear();
            _lastPage = null;
        }

        protected override void CleanUp()
        {
            _discoveredProductIds.Clear();
            VendorProducts.Clear();
            _lastPage = null;
        }
    }
}
