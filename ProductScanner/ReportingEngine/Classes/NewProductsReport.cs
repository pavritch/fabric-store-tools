using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace ReportingEngine.Classes
{
    public class NewProductsReport : ReportBase
    {
        #region Locals

        private readonly string inputFilename;
        private readonly string outputFilename;

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

                    productRecords = LoadCsvFile(inputFilename);
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

        public NewProductsReport(string manufacturerName, string inputFilename, string outputFilename, CancellationToken cancelToken = default(CancellationToken))
            : base(cancelToken)
        {
            this.ManufacturerName = manufacturerName;
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
                var html = "NewProductsReport.html".GetEmbeddedTextFile();
                var stringBuilder = new StringBuilder(html);

                // create a type which will perfectly format to the json we want

                var aryProducts = (from p in Products
                                   orderby p.Key
                                   select new
                                   {
                                       SKU = p.Key,
                                       Properties = p.Value
                                   }).Take(maxProducts).ToArray();

                var batches = new List<string>();
                for (var i = 0; i < aryProducts.Length/10000 + 1; i++)
                {
                    var batch = JsonConvert.SerializeObject(aryProducts.Skip(10000*i).Take(10000), SerializerSettings);
                    // remove the first and last characters ([])
                    batch = new string(batch.Skip(1).Take(batch.Length - 2).ToArray());
                    batches.Add(batch);
                }

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
                html = html.Replace("{{report-date}}", DateTime.Now.ToString("f"));
                html = html.Replace("{{csv-filename}}", inputFilename);

                var firstPart = html.Substring(0, html.IndexOf("//{{json-products}}"));
                var finalPart = html.Substring(html.IndexOf("//{{json-properties}}") + "//{{json-properties}}".Length);

                // save to disk
                using (var stream = new StreamWriter(outputFilename))
                {
                    stream.Write(firstPart);
                    stream.Write("var jsonProducts = [");
                    foreach (var batch in batches)
                    {
                        if (batches.IndexOf(batch) != 0) stream.Write(",");
                        foreach (var c in batch)
                        {
                            stream.Write(c);
                        }
                    }
                    stream.WriteLine("];");

                    stream.Write("var jsonProperties = ");
                    foreach (var c in jsonProperties)
                    {
                        stream.Write(c);
                    }
                    stream.WriteLine(";");

                    stream.Write(finalPart);
                }
                return true;
            }
            catch (Exception Ex)
            {
                ErrorMessage = String.Format("Exception: {0}", Ex.Message);
                return false;
            }
        }

        private List<ProductRecord> LoadCsvFile(string inputFilename)
        {
            var list = new List<ProductRecord>();

            if (!File.Exists(inputFilename))
                return list;

            using (var fs = File.Open(inputFilename, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    var config = new CsvConfiguration()
                    {
                        HasHeaderRecord = false,
                    };

                    using (var csv = new CsvReader(sr, config))
                    {

                        while (csv.Read())
                        {
                            var fields = csv.CurrentRecord;

                            var record = new ProductRecord(fields[0], fields[1], fields[2]);
                            list.Add(record);
                        }
                    }
                }
            }

            return list;
        }
    }
}
