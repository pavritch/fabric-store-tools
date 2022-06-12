using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Arteriors
{
    public class ArteriorsProductScraper : ProductScraper<ArteriorsVendor>
    {
        public ArteriorsProductScraper(IPageFetcher<ArteriorsVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.DetailUrl.GetDocumentName();
            var detailPage = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details, mpn);

            if (detailPage.InnerText.ContainsIgnoreCase("BUT THE PAGE YOU ARE LOOKING FOR CANNOT BE FOUND")) return new List<ScanData>();

            var scanData = new ScanData(product.ScanData);
            scanData.DetailUrl = product.DetailUrl;
            scanData[ScanField.ProductName] = detailPage.GetFieldValue(".product-name h1");
            scanData[ScanField.ManufacturerPartNumber] = detailPage.GetFieldValue("#spansku");

            scanData[ScanField.Height] = detailPage.GetFieldValue("span.product-diamen:contains('H:')");
            scanData[ScanField.Width] = detailPage.GetFieldValue("span.product-diamen:contains('W:')");
            scanData[ScanField.Depth] = detailPage.GetFieldValue("span.product-diamen:contains('D:')");
            scanData[ScanField.Diameter] = detailPage.GetFieldValue("span.product-diamen:contains('Dia:')");
            scanData[ScanField.Description] = detailPage.GetFieldValue("#description").Replace(@"’", "'");
            scanData[ScanField.Color] = detailPage.GetFieldValue("#appearance_color");
            scanData[ScanField.Use] = detailPage.GetFieldValue("#appearance_environ");
            scanData[ScanField.Finish] = detailPage.GetFieldValue("#appearance_finish");
            scanData[ScanField.Material] = detailPage.GetFieldValue("#appearance_material");
            scanData[ScanField.StockCount] = detailPage.GetFieldValue("#availability .data");

            var price = detailPage.GetFieldValue(".normal-price .price");
            if (price == null) return new List<ScanData>();

            var properties = detailPage.QuerySelectorAll(".data .table-row").ToList();
            foreach (var property in properties)
            {
                var cells = property.QuerySelectorAll(".table-cell").ToList();
                var label = cells.First().InnerText.Trim(':');
                var value = cells.Last().InnerText;

                if (_properties.ContainsKey(label))
                {
                    var prop = _properties[label];
                    scanData[prop] = value;
                }
            }

            scanData.Cost = price.Replace("$", "").ToDecimalSafe();

            var image = detailPage.QuerySelector("#product-image-img");
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Attributes["src"].Value));
            return new List<ScanData> {scanData};
        }

        private readonly Dictionary<string, ScanField> _properties = new Dictionary<string, ScanField>
        {
            { "Actual Mirror Size", ScanField.Ignore },
            { "ADA Approved", ScanField.Ignore },
            { "Approved for Australia", ScanField.Ignore },
            { "*Approved for Australia", ScanField.Ignore },
            { "*Approved for Belarus", ScanField.Ignore },
            { "Approved for Canada", ScanField.Ignore },
            { "Approved for European Union", ScanField.Ignore },
            { "*Approved for European Union", ScanField.Ignore },
            { "Approved for Kazakhstan", ScanField.Ignore },
            { "*Approved for Kazakhstan", ScanField.Ignore },
            { "*Approved for Kuwait", ScanField.Ignore },
            { "Approved for Mexico", ScanField.Ignore },
            { "Approved for New Zealand", ScanField.Ignore },
            { "*Approved for New Zealand", ScanField.Ignore },
            { "Approved for Russian Federatio", ScanField.Ignore },
            { "*Approved for Russian Federati", ScanField.Ignore },
            { "*Approved for Saudi Arabia", ScanField.Ignore },
            { "Approved for Ukraine", ScanField.Ignore },
            { "Arm D", ScanField.Ignore },
            { "Arm H", ScanField.Ignore },
            { "Arm W", ScanField.Ignore },
            { "Arm Finish", ScanField.Finish },
            { "Arm Material", ScanField.Material },
            { "Backplate Dia", ScanField.Ignore },
            { "Backplate D", ScanField.Ignore },
            { "Backplate H", ScanField.Ignore },
            { "Backplate W", ScanField.Ignore },
            { "Backplate Finish", ScanField.Ignore },
            { "Backplate Finish 1", ScanField.Ignore },
            { "Backplate Material", ScanField.Ignore },
            { "Backplate Material 1", ScanField.Ignore },
            { "Base Dimension D", ScanField.Ignore },
            { "Base Dimension Dia", ScanField.Ignore },
            { "Base Dimension H", ScanField.Ignore },
            { "Base Dimension W", ScanField.Ignore },
            { "Body Dimension D", ScanField.Ignore },
            { "Body Dimension Diameter", ScanField.Ignore },
            { "Body Dimension Footprint", ScanField.Ignore },
            { "Body Dimension H", ScanField.Ignore },
            { "Body Dimension W", ScanField.Ignore },
            { "Bottom Shelf Clearance", ScanField.Ignore },
            { "Bulb", ScanField.Bulb },
            { "*CB Certified", ScanField.Ignore },
            { "CFL Bulb Type (Title 20)", ScanField.Ignore },
            { "Cabinet Inside Dimension D", ScanField.Ignore },
            { "Cabinet Inside Dimension H", ScanField.Ignore },
            { "Cabinet Inside Dimension W", ScanField.Ignore },
            { "Canopy Dimension Dia", ScanField.Ignore },
            { "Canopy Dimension D", ScanField.Ignore },
            { "Canopy Dimension H", ScanField.Ignore },
            { "Canopy Dimension W", ScanField.Ignore },
            { "Canopy Finish", ScanField.Finish },
            { "Canopy Material", ScanField.Material },
            { "Center of Mount (from btm)", ScanField.Ignore },
            { "Chandelier Dim Dia", ScanField.Ignore },
            { "Chandelier Dim D", ScanField.Ignore },
            { "Chandelier Dim H", ScanField.Ignore },
            { "Chandelier Dim W", ScanField.Ignore },
            { "Clearance", ScanField.Ignore },
            { "Color", ScanField.Color },
            { "Color 1", ScanField.Color },
            { "Color 2", ScanField.Color },
            { "Color 3", ScanField.Color },
            { "Color 4", ScanField.Color },
            { "Contract Note", ScanField.Ignore },
            { "Cord Color", ScanField.CordColor },
            { "Cord Length (Exit)", ScanField.CordLength },
            { "Cord Type", ScanField.CordType },
            { "Damp Rated - Covered", ScanField.DampRatedCovered },
            { "Diffuser Location", ScanField.Ignore },
            { "Diffuser Material", ScanField.Ignore },
            { "Diffuser Size", ScanField.Ignore },
            { "Diffuser Thickness", ScanField.Ignore },
            { "Door Dimensions", ScanField.Ignore },
            { "Drawer Interior D", ScanField.Ignore },
            { "Drawer Interior H", ScanField.Ignore },
            { "Drawer Interior W", ScanField.Ignore },
            { "Finial Description", ScanField.Ignore },
            { "Finial Dimensions", ScanField.Ignore },
            { "Finish", ScanField.Finish },
            { "Finish will vary", ScanField.Finish },
            { "Finish 1", ScanField.Finish },
            { "Finish 2", ScanField.Finish },
            { "Finish 3", ScanField.Finish },
            { "Finish 4", ScanField.Finish },
            { "Globe 1 Diameter", ScanField.Ignore },
            { "Globe 1 Height", ScanField.Ignore },
            { "Globe 1 Qty", ScanField.Ignore },
            { "Globe 2 Diameter", ScanField.Ignore },
            { "Globe 2 Height", ScanField.Ignore },
            { "Globe 2 Qty", ScanField.Ignore },
            { "Globe 3 Diameter", ScanField.Ignore },
            { "Globe 3 Height", ScanField.Ignore },
            { "Globe 3 Qty", ScanField.Ignore },
            { "Hardware Finish", ScanField.HardwareFinish },
            { "Harp Size", ScanField.HarpSize },
            { "Installation Environment", ScanField.Ignore },
            { "Installation Environment 1", ScanField.Ignore },
            { "Installation Environment 2", ScanField.Ignore },
            { "Interior Color", ScanField.Finish },
            { "Interior Finish", ScanField.Finish },
            { "Interior Material", ScanField.Material },
            { "Lamp Base Color 1", ScanField.Color },
            { "Lamp Base Color 2", ScanField.Color },
            { "Lamp Base Finish 1", ScanField.Finish },
            { "Lamp Base Material 1", ScanField.Material },
            { "Lamp Base Material 2", ScanField.Material },
            { "Lamp Base Material Type 1", ScanField.Material },
            { "Lamp Body Color 1", ScanField.Color },
            { "Lamp Body Color 2", ScanField.Color },
            { "Lamp Body Finish 1", ScanField.Finish },
            { "Lamp Body Finish 2", ScanField.Finish },
            { "Lamp Body Finish 3", ScanField.Finish },
            { "Lamp Body Material 1", ScanField.Material },
            { "Lamp Body Material 2", ScanField.Material },
            { "Lamp Body Material 3", ScanField.Material },
            { "Lamp Bottom", ScanField.LampBottom },
            { "Link Info", ScanField.Ignore },
            { "Material 1", ScanField.Material },
            { "Material 2", ScanField.Material },
            { "Material 3", ScanField.Material },
            { "Material 4", ScanField.Material },
            { "Material Type", ScanField.Material },
            { "Material Type 1", ScanField.Material },
            { "Material Type 2", ScanField.Material },
            { "Material Type 3", ScanField.Material },
            { "Material Type 4", ScanField.Material },
            { "Max Watt", ScanField.Material },
            { "Maximum Chain/Pipe Extension", ScanField.Material },
            { "Middle Shelf Clearance", ScanField.Ignore },
            { "Mirror Type", ScanField.Ignore },
            { "Mount Size Dia", ScanField.Ignore },
            { "Mount Size D", ScanField.Ignore },
            { "Mount Size H", ScanField.Ignore },
            { "Mount Size W", ScanField.Ignore },
            { "Neck Description", ScanField.Ignore },
            { "Neck Dimension D", ScanField.Ignore },
            { "Neck Dimension H", ScanField.Ignore },
            { "Neck Dimension Dia", ScanField.Ignore },
            { "Overall Adjustable Depth", ScanField.Ignore },
            { "Overall Adjustable Height", ScanField.Ignore },
            { "Overall Adjustable Width", ScanField.Ignore },
            { "Overall Dimension D", ScanField.Ignore },
            { "Overall Dimension H", ScanField.Ignore },
            { "Overall Dimension W", ScanField.Ignore },
            { "Overall Diameter", ScanField.Ignore },
            { "Overall Foot Print Dimensions", ScanField.Ignore },
            { "Overall Shape", ScanField.Shape },
            { "Pendant Dim Dia", ScanField.Ignore },
            { "Pendant Dim D", ScanField.Ignore },
            { "Pendant Dim H", ScanField.Ignore },
            { "Pendant Dim W", ScanField.Ignore },
            { "Pipe/Chain Finish", ScanField.Finish },
            { "Pipe/Chain Length", ScanField.Ignore },
            { "Plug", ScanField.Plug },
            { "Primary Material", ScanField.Material },
            { "Pull Dimensions", ScanField.Ignore },
            { "*REWIRING REQUIRED", ScanField.Ignore },
            { "Seat Back H", ScanField.Ignore },
            { "Seat Cushion (thickness)", ScanField.Ignore },
            { "Seat D", ScanField.Ignore },
            { "Seat H (flr to top of seat)", ScanField.Ignore },
            { "Seat Interior W", ScanField.Ignore },
            { "Set Dimension D", ScanField.Ignore },
            { "Set Dimension D1", ScanField.Ignore },
            { "Set Dimension D2", ScanField.Ignore },
            { "Set Dimension D3", ScanField.Ignore },
            { "Set Dimension D4", ScanField.Ignore },
            { "Set Dimension H", ScanField.Ignore },
            { "Set Dimension H1", ScanField.Ignore },
            { "Set Dimension H2", ScanField.Ignore },
            { "Set Dimension H3", ScanField.Ignore },
            { "Set Dimension H4", ScanField.Ignore },
            { "Set Dimension W", ScanField.Ignore },
            { "Set Dimension W1", ScanField.Ignore },
            { "Set Dimension W2", ScanField.Ignore },
            { "Set Dimension W3", ScanField.Ignore },
            { "Set Dimension W4", ScanField.Ignore },
            { "Shade Attribute", ScanField.Ignore },
            { "Shade Bottom Diameter", ScanField.ShadeBottomDiameter },
            { "Shade Bottom Width x Depth", ScanField.Ignore },
            { "Shade Diffuser (Y/N)", ScanField.Ignore },
            { "Shade Drop", ScanField.Ignore },
            { "Shade Hardware Finish", ScanField.Ignore },
            { "Shade Lining Color", ScanField.Ignore },
            { "Shade Lining Material", ScanField.Ignore },
            { "Shade Outer Color", ScanField.Ignore },
            { "Shade Outer Material", ScanField.Ignore },
            { "Shade Shape", ScanField.Ignore },
            { "Shade Side", ScanField.ShadeSide },
            { "Shade Style", ScanField.Ignore },
            { "Shade Top Diameter", ScanField.ShadeTopDiameter },
            { "Shade Top Width x Depth", ScanField.Ignore },
            { "Shade Trim", ScanField.Ignore },
            { "Shade Type", ScanField.Ignore },
            { "Shelf D", ScanField.Ignore },
            { "Shelf H (thickness)", ScanField.Ignore },
            { "Shelf W", ScanField.Ignore },
            { "Socket Color", ScanField.SocketColor },
            { "Socket Quantity", ScanField.SocketQuantity },
            { "Socket Type", ScanField.SocketType },
            { "Socket Wattage", ScanField.SocketWattage },
            { "Switch Color", ScanField.SwitchColor },
            { "Switch Location", ScanField.SwitchLocation },
            { "Switch Type", ScanField.SwitchType },
            { "Table Top Clearance", ScanField.Ignore },
            { "Table Top Dimensions", ScanField.Ignore },
            { "Table Top Thickness", ScanField.Ignore },
            { "Top Coat/Sealant", ScanField.Ignore },
            { "Top Shelf Clearance", ScanField.Ignore },
            { "UL/cUL Approved", ScanField.Ignore },
            { "Voltage", ScanField.Voltage },
            { "Wall Sconce Adjustable", ScanField.Ignore },
            { "Wall Sconce Mount Finish", ScanField.Finish },
            { "Wall Sconce Mount Material", ScanField.Material },
            { "Wireway position", ScanField.WirewayPosition },
            { "Yardage for Reupholstery", ScanField.Ignore },
        };
    }
}