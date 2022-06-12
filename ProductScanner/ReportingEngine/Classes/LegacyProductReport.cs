using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace ReportingEngine.Classes
{
    public class LegacyProductReport : ReportBase
    {
        #region Locals

        private readonly string inputFilename;  // LegacyProductProperties.xlsx
        private readonly string outputFilename; // LegacyProductProperties.html
        private readonly int manufacturerID; // our SQL id for the manufacturer

        private List<ProductRecord> productRecords;
        private Dictionary<string, List<ProductRecord>> products;
        private Dictionary<string, Dictionary<string, int>> properties;

        #endregion

        #region Private Properties

        /// <summary>
        /// Name of vendor to show in report.
        /// </summary>
        private string ManufacturerName { get; set; }


        private List<ProductRecord> PropertyRecords
        {
            get
            {
                if (productRecords == null)
                {
                    if (string.IsNullOrWhiteSpace(inputFilename) || !File.Exists(inputFilename))
                        throw new Exception("Report input filename missing.");

                    productRecords = LoadCsvFile(inputFilename, manufacturerID);
                }
                return productRecords;
            }
        }

        /// <summary>
        /// Dic[SKU, list of product records]
        /// </summary>
        /// <remarks>
        /// Associate every product record into dictionary with SKU as key.
        /// </remarks>
        private Dictionary<string, List<ProductRecord>> Products
        {
            get
            {
                if (products == null)
                {
                    products = new Dictionary<string, List<ProductRecord>>();

                    foreach (var rec in PropertyRecords)
                    {
                        List<ProductRecord> items;
                        if (!products.TryGetValue(rec.SKU, out items))
                        {
                            items = new List<ProductRecord>();
                            products.Add(rec.SKU, items);
                        }
                        items.Add(rec);

                        ThrowOnCancel();
                    }
                }

                return products;
            }
        }


        /// <summary>
        /// Collection indexed by unique property name, with associated values and counts for each value.
        /// </summary>
        private Dictionary<string, Dictionary<string, int>> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, Dictionary<string, int>>();

                    foreach (var rec in PropertyRecords)
                    {
                        Dictionary<string, int> values;
                        if (!properties.TryGetValue(rec.Name, out values))
                        {
                            values = new Dictionary<string, int>();
                            properties.Add(rec.Name, values);
                        }

                        // now see if the value exists, and if so, bump the count, else
                        // add a new item to the dictionary with count = 1

                        int count;
                        if (values.TryGetValue(rec.Value, out count))
                            values[rec.Value] = count + 1;
                        else
                            values.Add(rec.Value, 1);

                        ThrowOnCancel();
                    }
                }

                return properties;
            }
        }


        #endregion

        public string ErrorMessage { get; private set; }

        public LegacyProductReport(string manufacturerName, int manufacturerID, string inputFilename, string outputFilename, CancellationToken cancelToken = default(CancellationToken))
            : base(cancelToken)
        {
            this.ManufacturerName = manufacturerName;
            this.manufacturerID = manufacturerID;
            this.inputFilename = inputFilename;
            this.outputFilename = outputFilename;
        }

        public Task<bool> GenerateReportAsync(int maxProducts = 1000000)
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Factory.StartNew(() =>
            {
                tcs.SetResult(GenerateReport(maxProducts));
            });
            return tcs.Task;

        }

        public bool GenerateReport(int maxProducts = 1000000)
        {
            try
            {
                ErrorMessage = string.Empty;
                var html = "LegacyProductReport.html".GetEmbeddedTextFile();

                // create a type which will perfectly format to the json we want

                var aryProducts = (from p in Products
                                   orderby p.Key
                                   select new
                                   {
                                       SKU = p.Key,
                                       Properties = p.Value
                                   }).Take(manufacturerID == 0 ? 0 : maxProducts).ToArray();

                var jsonProducts = JsonConvert.SerializeObject(aryProducts, SerializerSettings);

                ThrowOnCancel();

                var aryProperties = (from p in Properties
                                     orderby p.Key
                                     let values = (from k in p.Value
                                                   orderby p.Key
                                                   select new
                                                   {
                                                       Value = k.Key,
                                                       Count = k.Value.ToString("N0"),
                                                   }).ToArray()
                                     select new
                                     {
                                         Name = p.Key,
                                         Count = values.Count().ToString("N0"),
                                         Values = values,
                                     }).Take(maxProducts).ToArray();

                var jsonProperties = JsonConvert.SerializeObject(aryProperties, SerializerSettings);

                // perform replacements

                html = html.Replace("{{manufacturer-name}}", ManufacturerName);
                html = html.Replace("{{report-date}}", DateTime.Now.ToString("f"));   // Monday, June 15, 2009 1:45 PM 
                html = html.Replace("{{csv-filename}}", inputFilename);
                html = html.Replace("//{{json-products}}", string.Format("var jsonProducts = {0};", jsonProducts));
                html = html.Replace("//{{json-properties}}", string.Format("var jsonProperties = {0};", jsonProperties));

                ThrowOnCancel();

                // save to disk
                html.SaveTextAsFile(outputFilename);

                return true;
            }
            catch (Exception Ex)
            {
                ErrorMessage = String.Format("Exception: {0}", Ex.Message);
                return false;
            }
        }


        private List<ProductRecord> LoadCsvFile(string inputFilename, int manufacturerID)
        {
            // the fields in the XLSX file are as follows:
            // Name
            // Value
            // ProductID
            // SKU
            // ManufacturerID
            
            var list = new List<ProductRecord>();

            if (!File.Exists(inputFilename))
                return list;

            using (var xls = new ExcelPackage(new FileInfo(inputFilename)))
            {
                ExcelWorksheet ws = xls.Workbook.Worksheets[1];

                for (int row = 1; row < int.MaxValue; row++)
                {
                    var name =  ws.GetValue<string>(row, 1);
                    var value = ws.GetValue<string>(row, 2);
                    //var productID = ws.GetValue<string>(row, 3);
                    var sku = ws.GetValue<string>(row, 4);
                    var manID = ws.GetValue<int>(row, 5);

                    // end if reached end of used rows
                    if (string.IsNullOrWhiteSpace(name))
                        break;

                    // skip if not for this manufacturer
                    if (manID != manufacturerID && manufacturerID != 0)
                        continue;

                    var record = new ProductRecord(sku, name, value);
                    list.Add(record);
                }
            }

            return list;
        }
    }
}
