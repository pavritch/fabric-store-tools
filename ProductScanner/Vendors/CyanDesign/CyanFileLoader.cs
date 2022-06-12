using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace CyanDesign
{
    public class CyanFileLoader
    {
        private readonly IStorageProvider<CyanDesignVendor> _storageProvider;

        public CyanFileLoader(IStorageProvider<CyanDesignVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("item", ScanField.ManufacturerPartNumber),
            new FileProperty("description", ScanField.Description),
            // seems kind of like collection?
            new FileProperty("series", ScanField.Collection),
            // always 'SRW'
            new FileProperty("facility", ScanField.Ignore),
            // Active or Obsolete
            new FileProperty("status", ScanField.Status),
            new FileProperty("avail_for_sale", ScanField.StockCount),
            new FileProperty("list_price", ScanField.RetailPrice),
            new FileProperty("net_price", ScanField.Cost),
            new FileProperty("disc_price", ScanField.DiscountedPrice),
            // basically nothing here
            new FileProperty("product_style", ScanField.Ignore),
            new FileProperty("product_type", ScanField.Category),
            new FileProperty("catalog_description", ScanField.ProductName),
            new FileProperty("catalog_material", ScanField.Material),
            // some codes that don't mean anything to me
            new FileProperty("finish_code", ScanField.Ignore),
            new FileProperty("finish", ScanField.Finish),
            new FileProperty("item_height", ScanField.Height),
            new FileProperty("item_width", ScanField.Width),
            new FileProperty("item_length", ScanField.Length),
            new FileProperty("item_depth", ScanField.Depth),
            new FileProperty("item_diameter", ScanField.Diameter),
            new FileProperty("item_extension", ScanField.ItemExtension),
            new FileProperty("package_height", ScanField.PackageHeight),
            new FileProperty("package_width", ScanField.PackageWidth),
            new FileProperty("package_length", ScanField.PackageLength),
            new FileProperty("weight", ScanField.ShippingWeight),
            new FileProperty("freight_only", ScanField.FreightOnly),
            new FileProperty("not_ul_approved", ScanField.Ignore),
            new FileProperty("energy_star", ScanField.Ignore),
            // basically nothing in the next 20 props
            new FileProperty("energy_saving", ScanField.Ignore),
            new FileProperty("es_lamp_included", ScanField.Ignore),
            new FileProperty("dark_sky", ScanField.Ignore),
            new FileProperty("ul_type", ScanField.Ignore),
            new FileProperty("amps", ScanField.Ignore),
            new FileProperty("fan_watts", ScanField.Ignore),
            new FileProperty("rpm", ScanField.Ignore),
            new FileProperty("motor_size", ScanField.Ignore),
            new FileProperty("motor_poles", ScanField.Ignore),
            new FileProperty("motor_warranty", ScanField.Ignore),
            new FileProperty("motor_lead_wire", ScanField.Ignore),
            new FileProperty("motor_pull_switch_type", ScanField.Ignore),
            new FileProperty("motor_reverse_switch_type", ScanField.Ignore),
            new FileProperty("number_of_blades", ScanField.Ignore),
            new FileProperty("sweep", ScanField.Ignore),
            new FileProperty("blade_side_a_color", ScanField.Ignore),
            new FileProperty("blade_side_b_color", ScanField.Ignore),
            new FileProperty("arm_pitch", ScanField.Ignore),
            new FileProperty("downrod_length1", ScanField.Ignore),
            new FileProperty("downrod_length2", ScanField.Ignore),
            new FileProperty("overall_fan_height", ScanField.Ignore),
            new FileProperty("ceiling_to_lower_edge_of_blade", ScanField.Ignore),
            new FileProperty("canopy_height", ScanField.Ignore),
            new FileProperty("fan_housing_width", ScanField.Ignore),

            new FileProperty("bulb_type", ScanField.Bulbs),
            new FileProperty("number_of_lights", ScanField.NumberOfLights),
            new FileProperty("light_watts", ScanField.WattagePerLight),
            new FileProperty("socket", ScanField.Socket),
            new FileProperty("diffuser_type", ScanField.Ignore),
            new FileProperty("diffuser_material", ScanField.Ignore),
            new FileProperty("diffuser_panels", ScanField.Ignore),
            new FileProperty("shade_pattern", ScanField.Ignore),
            new FileProperty("shade_color", ScanField.ShadeColor),
            new FileProperty("diffuser_shape", ScanField.Ignore),
            new FileProperty("multi_piece_count", ScanField.Ignore),
            new FileProperty("multi_piece_height", ScanField.Ignore),
            new FileProperty("multi_piece_width", ScanField.Ignore),
            new FileProperty("multi_piece_length", ScanField.Ignore),
            new FileProperty("multi_piece_depth", ScanField.Ignore),
            new FileProperty("multi_piece_diameter", ScanField.Ignore),
            new FileProperty("multi_piece_extension", ScanField.Ignore),
            new FileProperty("vendor_origin", ScanField.Country),
            new FileProperty("master_pack", ScanField.Ignore),
            new FileProperty("inner_pack", ScanField.Ignore),
            new FileProperty("cartons_per_item", ScanField.Ignore),
            new FileProperty("carb_required", ScanField.Ignore),
            new FileProperty("lacey_required", ScanField.Ignore),
            new FileProperty("fws_required", ScanField.Ignore),
            new FileProperty("fda_required", ScanField.Ignore),
            new FileProperty("carb_compliant", ScanField.Ignore),
            new FileProperty("map_price", ScanField.MAP),
        };

        public List<ScanData> LoadStockData(string filename)
        {
            var stockFilePath = Path.Combine(_storageProvider.GetStaticFolder(), filename);
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(stockFilePath, Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}