using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using CsvHelper;
using MoreLinq;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using ReportingEngine.Classes;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Reports
{
    public class AuditFileCreator<T> : IAuditFileCreator<T> where T : Vendor, new()
    {
        private readonly IStorageProvider<T> _storageProvider;
        private readonly IVendorScanSessionManager<T> _sessionManager;

        public AuditFileCreator(IStorageProvider<T> storageProvider, IVendorScanSessionManager<T> sessionManager)
        {
            _storageProvider = storageProvider;
            _sessionManager = sessionManager;
        }

        public void BuildPropertyAuditFile(List<ScanData> scanDatas)
        {
            if (!_sessionManager.HasFlag(ScanOptions.GenerateReports)) return;

            var path = _storageProvider.GetCSVReportPath("VendorProperties.csv");
            var parent = Directory.GetParent(path).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            var vendor = new T();

            using (var fileStream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(fileStream))
            {
                foreach (var scanData in scanDatas)
                {
                    foreach (var propertyType in scanData.Keys)
                    {
                        if (scanData[propertyType] == null) continue;

                        writer.WriteLine("\"" + scanData[ScanField.ManufacturerPartNumber]
                            + "\",\"" + propertyType + "\",\"" + scanData[propertyType].Replace("\"", "\"\"") + "\"");
                    }

                    var variantProps = scanData.Variants.SelectMany(x => x).ToList();
                    foreach (var property in variantProps)
                    {
                        writer.WriteLine("\"" + scanData[ScanField.ManufacturerPartNumber]
                            + "\",\"" + property.Key + "-variant" + "\",\"" + property.Value.Replace("\"", "\"\"") + "\"");
                    }

                    writer.WriteLine("\"" + scanData[ScanField.ManufacturerPartNumber]
                        + "\",\"" + "Cost" + "-variant" + "\",\"" + scanData.Cost + "\"");
                }
                writer.Flush();
            }

            var report = new PropertyDiscoveryReport(vendor.DisplayName, path, Path.ChangeExtension(path, ".html"));
            report.GenerateReport();
        }

        public void BuildNewProductsAuditFile(List<VendorVariant> variants)
        {
            BuildProductAuditFile(variants, "NewProducts.csv");
        }

        public void BuildProductAuditFile(List<VendorVariant> variants)
        {
            BuildProductAuditFile(variants, "AllProducts.csv");
        }

        public void BuildFilteredProductAuditFile(List<VendorVariant> variants)
        {
            BuildProductAuditFile(variants, "FilteredProducts.csv");
        }

        public void BuildCSVRawAnalysisFile(List<ScanData> scanDatas)
        {
            if (!_sessionManager.HasFlag(ScanOptions.GenerateReports)) return;

            BuildCSVRawAnalysisFileProducts(scanDatas);
            BuildCSVRawAnalysisFileVariants(scanDatas);
        }

        public void BuildValidationResultsFile(List<ProductValidationResult> invalidResults)
        {
            var path = _storageProvider.GetCSVReportPath("ProductValidationResult.csv");
            var parent = Directory.GetParent(path).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            using (var writer = new StreamWriter(path))
            {
                var csv = new CsvWriter(writer);
                csv.WriteField("Name");
                csv.WriteField("Validation");
                csv.NextRecord();

                foreach (var result in invalidResults)
                {
                    csv.WriteField(result.Product.Name);
                    var reasonsList = string.Join(", ", result.ExcludedReasons);
                    csv.WriteField(reasonsList);
                    csv.NextRecord();
                }
            }
        }

        public void BuildValidationResultsFile(List<VariantValidationResult> invalidResults)
        {
            var path = _storageProvider.GetCSVReportPath("VariantValidationResult.csv");
            var parent = Directory.GetParent(path).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            using (var writer = new StreamWriter(path))
            {
                var csv = new CsvWriter(writer);
                csv.WriteField("MPN");
                csv.WriteField("Validation");
                csv.NextRecord();

                foreach (var result in invalidResults)
                {
                    csv.WriteField(result.Variant.ManufacturerPartNumber);
                    var reasonsList = string.Join(", ", result.ExcludedReasons);
                    csv.WriteField(reasonsList);
                    csv.NextRecord();
                }
            }
        }

        private void BuildCSVRawAnalysisFileProducts(List<ScanData> scanDatas)
        {
            var path = _storageProvider.GetCSVReportPath("RawDataAnalysisProducts.csv");
            var parent = Directory.GetParent(path).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            // output everything where a value exists
            var reportProps = Enum.GetValues(typeof (ScanField)).Cast<ScanField>().ToList();
            reportProps = reportProps.Where(x => scanDatas.Any(data => !string.IsNullOrEmpty(data[x]))).ToList();

            using (var writer = new StreamWriter(path))
            {
                var csv = new CsvWriter(writer);
                foreach (var prop in reportProps)
                {
                    csv.WriteField(prop);
                }
                csv.WriteField("Cost");
                csv.WriteField("NumVariants");
                csv.NextRecord();

                foreach (var record in scanDatas)
                {
                    foreach (var prop in reportProps)
                    {
                        csv.WriteField(record[prop]);
                    }
                    csv.WriteField(record.Cost);
                    csv.WriteField(record.Variants.Count);
                    csv.NextRecord();
                }
            }
        }

        private void BuildCSVRawAnalysisFileVariants(List<ScanData> scanDatas)
        {
            var path = _storageProvider.GetCSVReportPath("RawDataAnalysisVariant.csv");
            var parent = Directory.GetParent(path).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            // output everything where a value exists
            var reportProps = Enum.GetValues(typeof (ScanField)).Cast<ScanField>().ToList();
            reportProps = reportProps.Where(x => scanDatas.SelectMany(d => d.Variants).Any(data => !string.IsNullOrEmpty(data[x]))).ToList();

            using (var writer = new StreamWriter(path))
            {
                var csv = new CsvWriter(writer);
                foreach (var prop in reportProps)
                {
                    csv.WriteField(prop);
                }
                csv.WriteField("Images");
                csv.NextRecord();

                foreach (var record in scanDatas.SelectMany(x => x.Variants))
                {
                    foreach (var prop in reportProps)
                    {
                        csv.WriteField(record[prop]);
                    }

                    var images = record.GetScannedImages();
                    csv.WriteField(string.Join(", ", images.Select(x => x.Url)));
                    csv.NextRecord();
                }
            }
        }

        public void BuildCSVFinalAnalysisFile(List<VendorVariant> variants)
        {
            if (!_sessionManager.HasFlag(ScanOptions.GenerateReports)) return;

            var path = _storageProvider.GetCSVReportPath("FinalAnalysis.csv");
            var parent = Directory.GetParent(path).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            var records = new List<Dictionary<string, string>>();
            foreach (var variant in variants)
            {
                var product = variant.VendorProduct;
                var record = new Dictionary<string, string>();
                if (product is HomewareProduct)
                {
                    var homewareProduct = product as HomewareProduct;
                    var homewareFeatures = homewareProduct.ProductFeatures;
                    record["Brand"] = homewareFeatures.Brand;
                    record["CareInstructions"] = homewareFeatures.CareInstructions;
                    record["Color"] = homewareFeatures.Color;
                    record["Depth"] = homewareFeatures.Depth.ToString();
                    record["Height"] = homewareFeatures.Height.ToString();
                    record["LeadTime"] = homewareFeatures.LeadTime;
                    record["PleaseNote"] = homewareFeatures.PleaseNote;
                    record["ShippingWeight"] = homewareFeatures.ShippingWeight.GetValueOrDefault().ToString();
                    record["Width"] = homewareFeatures.Width.ToString();

                    var specs = homewareFeatures.Features;
                    if (specs != null)
                        foreach (var spec in specs)
                            record[spec.Key] = spec.Value;

                    record["HomewareCategory"] = homewareProduct.HomewareCategory.ToString();
                    record["Correlator"] = homewareProduct.Correlator;
                    record["Description"] = homewareProduct.ManufacturerDescription;
                    record["InStock"] = homewareProduct.StockData.InStock.ToString();
                    record["MPN"] = homewareProduct.MPN;
                    record["Name"] = homewareProduct.Name;
                    record["ProductGroup"] = homewareProduct.ProductGroup.ToString();

                    record["Images"] = string.Join(", ", homewareProduct.ScannedImages.Select(x => x.Url));

                    record = record.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                }
                else if (product is RugProduct)
                {
                    //var rugFeatures = (product as RugProduct).RugProductFeatures;
                    //record.Backing = rugFeatures.Backing;
                    //record.Care = rugFeatures.CareInstructions;
                    //record.Collection = rugFeatures.Collection;
                    //record.Color = rugFeatures.Color;
                    //record.Colors = rugFeatures.Colors == null ? "" : string.Join(", ", rugFeatures.Colors);
                    //record.ColorGroup = rugFeatures.ColorGroup;
                    //record.CountryOfOrigin = rugFeatures.CountryOfOrigin;
                    //record.Designer = rugFeatures.Designer;
                    //record.Shape = (variant as RugVendorVariant).RugProductVariantFeatures.Shape;
                    //if (rugFeatures.Tags != null) record.Tags = string.Join(", ", rugFeatures.Tags);
                    //record.PatternName = rugFeatures.PatternName;
                    //record.PatternNumber = rugFeatures.PatternNumber;
                    //record.Warranty = rugFeatures.Warranty;
                    //record.Weave = rugFeatures.Weave;
                    //record.Name = product.Name;

                    //if (rugFeatures.Description != null) record.Description = string.Join(", ", rugFeatures.Description);

                    //if (rugFeatures.Material != null && rugFeatures.Material.Any())
                    //{
                    //    var content = rugFeatures.Material.Select(x => x.Key).Aggregate((a, b) => a + ", " + b);
                    //    record.Material = content;
                    //}
                }
                else
                {
                    //record.AverageBolt = product.GetPublicProperty(ProductPropertyType.AverageBolt);
                    //record.Book = product.GetPublicProperty(ProductPropertyType.Book);
                    //record.Category = product.GetPublicProperty(ProductPropertyType.Category);
                    //record.Care = product.GetPublicProperty(ProductPropertyType.Cleaning);
                    //record.Collection = product.GetPublicProperty(ProductPropertyType.Collection);
                    //record.Color = product.GetPublicProperty(ProductPropertyType.Color);
                    //record.ColorName = product.GetPublicProperty(ProductPropertyType.ColorName);
                    //record.ColorGroup = product.GetPublicProperty(ProductPropertyType.ColorGroup);
                    //record.Cost = variant.Cost;
                    //record.DetailUrl = product.GetPublicProperty(ProductPropertyType.ProductDetailUrl);
                    //record.Design = product.GetPublicProperty(ProductPropertyType.Design);
                    //record.Durability = product.GetPublicProperty(ProductPropertyType.Durability);
                    //record.FireCode = product.GetPublicProperty(ProductPropertyType.FireCode);
                    //record.HorizontalRepeat = product.GetPublicProperty(ProductPropertyType.HorizontalRepeat);
                    //record.InStock = variant.StockData.InStock;
                    //record.Length = product.GetPublicProperty(ProductPropertyType.Length);
                    //record.Material = product.GetPublicProperty(ProductPropertyType.Material);
                    //record.MPN = variant.ManufacturerPartNumber;
                    //record.PatternName = product.GetPublicProperty(ProductPropertyType.PatternName);
                    //record.PatternNumber = product.GetPublicProperty(ProductPropertyType.PatternNumber);
                    //record.Railroaded = product.GetPublicProperty(ProductPropertyType.Railroaded);
                    //record.Unit = product.UnitOfMeasure;
                    //record.VerticalRepeat = product.GetPublicProperty(ProductPropertyType.VerticalRepeat);
                    //record.Width = product.GetPublicProperty(ProductPropertyType.Width);
                }
                records.Add(record);
            }

            var columns = records.SelectMany(x => x.Keys).Distinct().ToList();
            using (var writer = new StreamWriter(path))
            {
                var csv = new CsvWriter(writer);
                foreach (var column in columns)
                {
                    csv.WriteField(column);
                }
                csv.NextRecord();

                foreach (var row in records)
                {
                    foreach (var column in columns)
                    {
                        csv.WriteField(!row.ContainsKey(column) ? "" : row[column]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private void BuildProductAuditFile(List<VendorVariant> variants, string filename)
        {
            if (!_sessionManager.HasFlag(ScanOptions.GenerateReports)) return;

            var vendor = new T();

            var path = _storageProvider.GetCSVReportPath(filename);
            var parent = Directory.GetParent(path).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            using (var fileStream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(fileStream))
            {
                foreach (var variant in variants)
                {
                    var sbCsvRecord = new StringBuilder("");
                    Action<string, string> add = (n, v) =>
                    {
                        if (string.IsNullOrWhiteSpace(v)) return;
                        sbCsvRecord.AppendLine("\"" + variant.ManufacturerPartNumber + "\",\"" + n + "\",\"" + (string.IsNullOrEmpty(v) ? string.Empty : v).Replace("\"", "\"\"") + "\"");
                    };

                    var product = variant.VendorProduct;
                    add("SKUSuffix", variant.SKUSuffix);
                    add("SameAsSKU", product.SameAsSKU);
                    add("Name", product.Name);
                    add("IsClearance", product.IsClearance.ToString());
                    add("ProductGroup", product.ProductGroup.ToString());
                    add("ProductClass", product.ProductClass.ToString());
                    add("PatternCorrelator", product.Correlator);
                    add("SEName", product.Name.MakeSafeSEName());
                    add("SETitle", product.SETitle);
                    add("UnitOfMeasure", product.UnitOfMeasure.ToString());
                    add("ManufacturerPartNumber", variant.ManufacturerPartNumber);
                    add("Cost", variant.Cost.ToString());
                    add("RetailPrice", variant.RetailPrice.ToString());
                    add("OurPrice", variant.OurPrice.ToString());
                    add("InStock", variant.StockData.InStock.ToString());
                    add("MinimumQuantity", variant.MinimumQuantity.ToString());
                    add("OrderIncrement", variant.OrderIncrementQuantity.ToString());
                    add("Description", variant.GetDescription());
                    add("ManufacturerDescription", product.ManufacturerDescription.Left(200));
                    add("OrderRequirementsNotice", product.OrderRequirementsNotice);
                    add("IsDiscontinued", product.IsDiscontinued.ToString());
                    add("UPC", variant.UPC);
                    add("Area", variant.Area);
                    add("AlternateItemNumber", variant.AlternateItemNumber);

                    if (product is RugProduct)
                    {
                        var rugFeatures = (product as RugProduct).RugProductFeatures;
                        add("Backing", rugFeatures.Backing);
                        add("CareInstructions", rugFeatures.CareInstructions);
                        add("Collection", rugFeatures.Collection);
                        add("Color", rugFeatures.Color);
                        add("ColorGroup", rugFeatures.ColorGroup);
                        add("CountryOfOrigin", rugFeatures.CountryOfOrigin);
                        add("Designer", rugFeatures.Designer);
                        add("PatternName", rugFeatures.PatternName);
                        add("PatternNumber", rugFeatures.PatternNumber);
                        add("Warranty", rugFeatures.Warranty);
                        add("Weave", rugFeatures.Weave);
                        if (rugFeatures.Description != null) add("FeatureDescription", string.Join(", ", rugFeatures.Description));

                        if (rugFeatures.Material != null && rugFeatures.Material.Any())
                        {
                            var content = rugFeatures.Material.Select(x => (x.Value.HasValue ? x.Value.ToString() + " " : "") + x.Key).Aggregate((a, b) => a + "," + b);
                            add("Material", content);
                        }
                    }

                    if (product is HomewareProduct)
                    {
                        var category = (product as HomewareProduct).HomewareCategory;
                        var features = (product as HomewareProduct).ProductFeatures;
                        add("HomewareCategory", category.ToString());
                        add("Width", features.Width.ToString());
                        add("Length", features.Depth.ToString());
                        add("Height", features.Height.ToString());
                    }

                    if (product is FabricProduct)
                    {
                        var fabricProduct = product as FabricProduct;
                        add("SKU", fabricProduct.SKU);
                    }

                    //foreach (var image in product.GetProductImages())
                    //{
                        //add(image.ImageVariant + "Image", image.SourceUrl);
                    //}

                    foreach (var prop in product.PublicProperties)
                    {
                        add(prop.Key.DescriptionAttr(), prop.Value);
                    }

                    foreach (var prop in product.PrivateProperties)
                    {
                        add(prop.Key + " (private)", prop.Value);
                    }
                    writer.Write(sbCsvRecord.ToString());
                }
                writer.Flush();
            }

            var report = new NewProductsReport(vendor.DisplayName, path, Path.ChangeExtension(path, ".html"));
            report.GenerateReport();
        }
    }
}