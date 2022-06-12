using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Kravet.Discovery
{
    public class KravetBaseFileLoader<T> : ProductFileLoader<T> where T : Vendor, new()
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item", ScanField.ManufacturerPartNumber),
            new FileProperty("Pattern", ScanField.Pattern),
            new FileProperty("Color", ScanField.ColorName),
            new FileProperty("Brand", ScanField.Brand),
            new FileProperty("Vert. Repeat", ScanField.VerticalRepeat),
            new FileProperty("Horz. Repeat", ScanField.HorizontalRepeat),
            new FileProperty("Repeat UOM", ScanField.Ignore),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("Width UOM", ScanField.Ignore),
            new FileProperty("Country of Origin", ScanField.Country),
            new FileProperty("WHLS Price", ScanField.Cost),
            new FileProperty("Unit of measure", ScanField.UnitOfMeasure),
            new FileProperty("Content", ScanField.Content),
            new FileProperty("Finish", ScanField.Finish),
            new FileProperty("Clean Code", ScanField.Cleaning),
            new FileProperty("Durability", ScanField.Durability),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("Use", ScanField.ProductUse),
            new FileProperty("Type1", ScanField.Type1),
            new FileProperty("Type2", ScanField.Type2),
            new FileProperty("Style1", ScanField.Style1),
            new FileProperty("Style2", ScanField.Style2),
            new FileProperty("Image exists", ScanField.Ignore),
            new FileProperty("Inventory Available", ScanField.StockCount),
            new FileProperty("Image File Name - HI RES", ScanField.ImageUrl),
            new FileProperty("Image File Name - LO RES", ScanField.AlternateImageUrl),
            new FileProperty("Color 1", ScanField.Color1),
            new FileProperty("Color 2", ScanField.Color2),
            new FileProperty("Color 3", ScanField.Color3),
            new FileProperty("Weight", ScanField.Weight),
            new FileProperty("Weight UOM", ScanField.Ignore),
            new FileProperty("Display Status", ScanField.Status),
            new FileProperty("New Wholesale Price", ScanField.Ignore),
            new FileProperty("New Price effective date", ScanField.Ignore),
            new FileProperty("Prop 65", ScanField.Ignore),
            new FileProperty(" UFAC", ScanField.Flammability),
            new FileProperty(" Direction", ScanField.Direction),
            new FileProperty("Wallcover Length(YD)", ScanField.Length),
            new FileProperty("Minimum order qty", ScanField.MinimumQuantity),
            new FileProperty("Order increment qty", ScanField.OrderIncrement),
            new FileProperty("Horizontal half drop repeat", ScanField.HorizontalHalfDrop),
            new FileProperty(" CA TB117", ScanField.CA_TB117),
            new FileProperty("Ship From", ScanField.Ignore),
            new FileProperty("Memo Sample Available", ScanField.Ignore),
            new FileProperty("PROP 65 CHEMICAL", ScanField.Ignore),
            new FileProperty("Prop 65 ASSOCIATED EFFECT", ScanField.Ignore),
            new FileProperty("Qty On Hand", ScanField.Ignore),
            new FileProperty("Lead Time in Days", ScanField.Ignore),
            new FileProperty("Barcode", ScanField.Ignore),
        };

        private readonly List<string> _associatedBrands; 
        public KravetBaseFileLoader(IStorageProvider<T> storageProvider, List<string> associatedBrands)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xls)
        {
            _associatedBrands = associatedBrands;
        }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var fileProducts = await base.LoadProductsAsync();
            var productsForBrand = fileProducts.Where(x => _associatedBrands.Contains(x[ScanField.Brand])).ToList();
            foreach (var product in productsForBrand)
            {
                product[ScanField.ManufacturerPartNumber] = product[ScanField.ManufacturerPartNumber].Replace("`","").ToUpper();
                product[ScanField.UnitOfMeasure] = product[ScanField.UnitOfMeasure].TitleCase().Replace("Panel","Each");
                product[ScanField.Pattern] = product[ScanField.Pattern].TitleCase();
                product[ScanField.MinimumQuantity] = MinimumQuantity(product[ScanField.MinimumQuantity]).ToString();
                product[ScanField.OrderIncrement] = OrderIncrement(product[ScanField.OrderIncrement]).ToString();
            }
            return productsForBrand.Where(x => !ShouldExclude(x)).ToList();
        }

        private int MinimumQuantity(string minimumQuantity)
        {
            var minQty = minimumQuantity.ToIntegerSafe();
            return minQty == 0 ? 1 : minQty;
        }

        private int OrderIncrement(string orderIncrement)
        {
            var ordInc = orderIncrement.ToIntegerSafe();
            return ordInc == 0 ? 1 : ordInc;
        }

        protected virtual bool ShouldExclude(ScanData data)
        {
            // a few products (3 at last check) have 2 rows - one with the correct MPN, and one with this extra character
            // this will exclude the ones with the extra character
            var mpn = data[ScanField.ManufacturerPartNumber];
            if (mpn.Contains("�")) 
                return true;

            var catb117 = data[ScanField.CA_TB117];
            return catb117.ContainsIgnoreCase("FAIL");
        }
    }
}