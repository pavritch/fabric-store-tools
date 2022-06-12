using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Capel.Details
{
    public class CapelProductBuilder : IProductBuilder<CapelVendor>
    {
        // 90% of them are listed as 0 quantity in the file, so we're ignoring that for now
        public VendorProduct Build(ScanData data)
        {
            var rugProduct = new RugProduct();

            var vendor = new CapelVendor();
            /*var vendorVariants = data.Variants.Select(x => new VendorVariant(
                x[ProductPropertyType.ManufacturerPartNumber],
                RugParser.ParseDimensions(x[ProductPropertyType.Size], GetShape(x[ProductPropertyType.Size], x[ProductPropertyType.Shape])).GetSkuSuffix(),
                x[ProductPropertyType.WholesalePrice].ToDecimalSafe(),
                x[ProductPropertyType.StockCount].ToDoubleSafe() > 0,
                vendor,
                rugProduct)
            {
                RetailPrice = x[ProductPropertyType.RetailPrice].ToDecimalSafe(),
                OurPrice = x[ProductPropertyType.MAP].ToDecimalSafe(),
                Description = x[ProductPropertyType.Size],
                Shape = GetShape(x[ProductPropertyType.Size], x[ProductPropertyType.Shape]),
                UPC = x[ProductPropertyType.UPC]
            }).ToList();

            rugProduct.AddVariants(vendorVariants);*/

            var variant = data.Variants.First();
            var color = FormatColor(variant[ProductPropertyType.Color]);
            var productName = variant[ProductPropertyType.ProductName].TitleCase().Replace("-", " ");


            rugProduct.PublicProperties[ProductPropertyType.Construction] = variant[ProductPropertyType.Construction];

            rugProduct.Correlator = productName;
            rugProduct.Name = new[] {color, productName, "by", vendor.DisplayName}.BuildName();
            rugProduct.ProductGroup = ProductGroup.Rug;
            rugProduct.ScannedImages = GetImages(data.Variants);
            rugProduct.SKU = string.Format("{0}-{1}", vendor.SkuPrefix, variant[ProductPropertyType.SKU]);
            rugProduct.UnitOfMeasure = UnitOfMeasure.Each;

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);
            return rugProduct;
        }

            //rugProduct.PublicProperties[ProductPropertyType.PackageHeight] = FormatDimension(variant[ProductPropertyType.PackageHeight]);
            //rugProduct.PublicProperties[ProductPropertyType.PackageLength] = FormatDimension(variant[ProductPropertyType.PackageLength]);
            //rugProduct.PublicProperties[ProductPropertyType.PackageWidth] = FormatDimension(variant[ProductPropertyType.PackageWidth]);
            //rugProduct.PublicProperties[ProductPropertyType.Weight] = Math.Round(variant[ProductPropertyType.Weight].ToDoubleSafe()) + " ounces";

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();
            var features = new RugProductFeatures();

            features.Color = FormatColor(firstVariant[ProductPropertyType.Color]);
            features.CountryOfOrigin = firstVariant[ProductPropertyType.CountryOfOrigin].ToFormattedCountry();

            //rugProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(variant[ProductPropertyType.Content]);
            //rugProduct.PublicProperties[ProductPropertyType.Content] = variant[ProductPropertyType.Construction];
            //rugProduct.PublicProperties[ProductPropertyType.ProductName] = productName;
            return features;
        }

        private ProductShapeType GetShape(string size, string shape)
        {
            // sometimes size contains shape
            if (size.Contains("Runner")) return ProductShapeType.Runner;
            return shape.GetShape();
        }

        private List<ScannedImage> GetImages(List<ScanData> variants)
        {
            var productImages = new List<ScannedImage>();

            // all of the other images in the spreadsheet are just lower quality versions of the one in Image4
            var closeupImages = variants.Select(x => x[ProductPropertyType.Image4]).Distinct();

            // set the shape based on the Shape in the spreadsheet
            var shape = GetShape(variants.First()[ProductPropertyType.Size], variants.First()[ProductPropertyType.Shape]);
            closeupImages.ForEach(x => productImages.Add(new ScannedImage(shape.ToImageVariantType(), x)));

            return productImages.Where(x => x.Url != "N/A").ToList();
        }

        private string FormatDimension(string height)
        {
            if (height == "0") return string.Empty;
            return height + " inches";
        }

        private string FormatColor(string color)
        {
            color = color.Trim();
            color = color.Replace("Lt.", "Light");
            color = color.Replace("Dk.", "Dark");
            color = color.Replace("Dk", "Dark");
            color = color.Replace("Brt.", "Bright");
            color = color.Replace("BLue", "Blue");

            if (color.ContainsIgnoreCase("No Color")) return string.Empty;
            return color;
        }

        private string FormatContent(string content)
        {
            return content.ToFormattedFabricContent();
        }
    }
}