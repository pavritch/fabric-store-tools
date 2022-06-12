using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace DistinctiveDesigns
{
    public class DistinctiveDesignsFileLoader : ProductFileLoader<DistinctiveDesignsVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("SKU", ScanField.ManufacturerPartNumber),
            new FileProperty("Item_Name", ScanField.ProductName),
            new FileProperty("Item_Description", ScanField.Description),
            new FileProperty("Item_Status", ScanField.Status),
            new FileProperty("Item_Yes_No_e-ereseller", ScanField.Note),
            new FileProperty("Item_Category", ScanField.Category),
            new FileProperty("Item_Style_Category", ScanField.Style),
            new FileProperty("Item_Order_Minimum_QTY", ScanField.MinimumQuantity),
            new FileProperty("Item_Wholesale_Price", ScanField.Cost),
            new FileProperty("Item_Wholesale_Pack_Price", ScanField.Ignore),
            new FileProperty("Item_MSRP", ScanField.RetailPrice),
            new FileProperty("Item_MAPP", ScanField.MAP),
            new FileProperty("Item_Dimensions", ScanField.Dimensions),
            new FileProperty("Item_Boxed_Dimensions", ScanField.ShippingInfo),
            new FileProperty("Item_Status_Date", ScanField.Ignore),
            new FileProperty("Item_Intro_Date", ScanField.Ignore),
            new FileProperty("Item_Revision_Date", ScanField.Ignore),
            new FileProperty("Item_Price_Change_Date", ScanField.Ignore),
            new FileProperty("Item_Future_Availability_Date", ScanField.Ignore),
            new FileProperty("Item_Wholesale_Box", ScanField.Ignore),

            new FileProperty("Item_Ships_VIA", ScanField.ShippingMethod),
            new FileProperty("Item_Boxed_Weight_Per_Pack", ScanField.Ignore),
            new FileProperty("Item_Unboxed_Weight", ScanField.Weight),
            new FileProperty("Item_Search_Engine_Keywords", ScanField.Ignore),
            new FileProperty("Item_Dominant_Flower_1", ScanField.Ignore),
            new FileProperty("Item_Dominant_Flower_2", ScanField.Ignore),
            new FileProperty("Item_Dominant_Foliage_1", ScanField.Ignore),
            new FileProperty("Item_Dominant_Foliage_2", ScanField.Ignore),
            new FileProperty("Item_Dominant_Color_Family", ScanField.ColorGroup),
            new FileProperty("Item_Dominant_Color", ScanField.Color),
            new FileProperty("Item_Container_Material", ScanField.Material),
            new FileProperty("Item_Type", ScanField.Type),
            new FileProperty("Item_Photo_Name", ScanField.Image1),
            new FileProperty("Additional Description 1", ScanField.Ignore),
            new FileProperty("Additional Description 2", ScanField.Ignore),
            new FileProperty("Additional Description 3", ScanField.Ignore),
            new FileProperty("Additional Description 4", ScanField.Ignore),
            new FileProperty("#Days Lead Time", ScanField.Ignore),
        };

        public DistinctiveDesignsFileLoader(IStorageProvider<DistinctiveDesignsVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xlsx, 1, 2) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                product[ScanField.ItemNumber] = product[ScanField.ManufacturerPartNumber];
                product[ScanField.ManufacturerPartNumber] = "SKU # " + product[ScanField.ManufacturerPartNumber];
            }
            return products;
        }
    }
}