using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReportingEngine;
using ReportingEngine.Classes;

namespace Report
{
    class Program
    {
        readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Console app as a simple test harness for the reporting engine.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var program = new Program();
            
            // discovery 
            //program.RunPropertyDiscoveryReport().Wait();

            // new products 
            //program.RunNewProductReport().Wait();

            // keyword analysis
            program.RunKeywordAnalysisReport().Wait();

            // legacy property values for a vendor
            //program.RunLegacyProductReport().Wait();

            // legacy combined report
            //program.RunCombinedLegacyProductReport().Wait();

            // legacy property values for a vendor
            //program.RunAllLegacyProductReports().Wait();


#if DEBUG
            Console.WriteLine("Press any key.");
            Console.ReadKey();
#endif
        }

        public async Task RunPropertyDiscoveryReport()
        {
            Console.WriteLine("Generating property discovery report.");

            var csvDiscoveryFile = @"D:\Dropbox\InsideStores\InsideFabricStockData\FSchumacher\Development\FSchumacher_PropertyAudit.csv";

            var report = new PropertyDiscoveryReport("F Schumacher", csvDiscoveryFile, Path.ChangeExtension(csvDiscoveryFile, ".html"), cts.Token);

            if (await report.GenerateReportAsync())
            {
                Console.WriteLine("Success.");
            }
            else
            {
                Console.WriteLine(report.ErrorMessage);
                Console.WriteLine("Failed.");
            }

        }

        public async Task RunNewProductReport()
        {
            Console.WriteLine("Generating new product report.");

            var csvNewProductsFile = @"D:\Dropbox\InsideStores\InsideFabricStockData\FSchumacher\Development\FSchumacher_NewProductAudit.csv";

            var report = new NewProductsReport("F Schumacher", csvNewProductsFile, Path.ChangeExtension(csvNewProductsFile, ".html"), cts.Token);

            if (await report.GenerateReportAsync())
            {
                Console.WriteLine("Success.");
            }
            else
            {
                Console.WriteLine(report.ErrorMessage);
                Console.WriteLine("Failed.");
            }
        }


        public async Task RunLegacyProductReport()
        {
            int manufacturerID = 30;

            Console.WriteLine("Generating legacy product report for ManufacturerID: " + manufacturerID.ToString());

            // uses an XLSX version of the SQL ProductLabels file to generate a report depicting the legacy properties for the specified vendor.

            string xlsxLegacyProductsFile = @"D:\Dropbox\InsideStores\InsideFabricStockData\Legacy\LegacyProductProperties.xlsx";
            string path = Path.GetDirectoryName(xlsxLegacyProductsFile);
            string outfile = Path.Combine(path,string.Format("{0}-{1}.html", Path.GetFileNameWithoutExtension(xlsxLegacyProductsFile), manufacturerID));

            var report = new LegacyProductReport("F Schumacher", manufacturerID, xlsxLegacyProductsFile, outfile, cts.Token);

            if (await report.GenerateReportAsync())
            {
                Console.WriteLine("Success.");
            }
            else
            {
                Console.WriteLine(report.ErrorMessage);
                Console.WriteLine("Failed.");
            }
        }


        public async Task RunCombinedLegacyProductReport()
        {

            Console.WriteLine("Generating legacy product report for all manufacturers.");

            // uses an XLSX version of the SQL ProductLabels file to generate a report depicting the legacy properties for the specified vendor.

            string xlsxLegacyProductsFile = @"D:\Dropbox\InsideStores\InsideFabricStockData\Legacy\LegacyProductProperties.xlsx";
            string path = Path.GetDirectoryName(xlsxLegacyProductsFile);
            
            var report = new LegacyProductReport("All Manufacturers", 0, xlsxLegacyProductsFile, Path.ChangeExtension(xlsxLegacyProductsFile, ".html"), cts.Token);

            if (await report.GenerateReportAsync())
            {
                Console.WriteLine("Success.");
            }
            else
            {
                Console.WriteLine(report.ErrorMessage);
                Console.WriteLine("Failed.");
            }
        }



        public async Task RunAllLegacyProductReports()
        {
            var vendors = new Dictionary<string, int>()
            {
                {"Beacon Hill", 9},
                {"Clarence House", 63},
                {"Duralee", 11},
                {"Fabricut", 67},
                {"F Schumacher", 30},
                {"Greenhouse", 32},
                {"Highland Court", 9},
                {"Kasmir", 58},
                {"Kravet", 5},
                {"Lee Jofa", 8},
                {"Maxwell", 56},
                {"Pindler", 51},
                {"Ralph Lauren", 52},
                {"RM Coco", 57},
                {"Robert Allen", 6},
                {"Scalamandre", 59},
                {"Stout", 55},
                {"Stroheim", 69},
                {"Suburban Home", 71},
                {"Trend", 70},
                {"Vervain", 68},
                {"York", 74},
            };

            foreach (var vendor in vendors)
            {
                try
                {
                    int manufacturerID = vendor.Value;
                    string vendorName = vendor.Key;

                    Console.Write(string.Format("Generating legacy report for {0}....", vendorName));

                    // uses an XLSX version of the SQL ProductLabels file to generate a report depicting the legacy properties for the specified vendor.

                    string xlsxLegacyProductsFile = @"D:\Dropbox\InsideStores\InsideFabricStockData\Legacy\LegacyProductProperties.xlsx";
                    string path = Path.GetDirectoryName(xlsxLegacyProductsFile);
                    string outfile = Path.Combine(path, string.Format("{0}-{1}-{2}.html", Path.GetFileNameWithoutExtension(xlsxLegacyProductsFile), vendorName.Replace(" ", string.Empty), manufacturerID));

                    var report = new LegacyProductReport(vendorName, manufacturerID, xlsxLegacyProductsFile, outfile, cts.Token);

                    if (await report.GenerateReportAsync())
                    {
                        Console.WriteLine("Success.");
                    }
                    else
                    {
                        Console.WriteLine("Failed.");
                        Console.WriteLine("      " + report.ErrorMessage);
                    }

                }
                catch (Exception Ex)
                {
                    Console.WriteLine(string.Format("Failed.\n  Report terminated with exception: {0}", Ex.Message));
                }
            }

        }




        public async Task RunKeywordAnalysisReport()
        {
            Console.WriteLine("Generating keyword analysis report.");

            var progress = new Progress<int>((pct) =>
            {
                Console.WriteLine(string.Format("Progress: {0}%", pct));
            });

            var rootFolder = @"D:\Dropbox\InsideStores\InsideFabricStockData\York\Cache\Products";
            var htmlReportFile = @"D:\Dropbox\InsideStores\InsideFabricStockData\York\Development\York_KeywordAnalysis.html";

            var report = new KeywordAnalysisReport("York", rootFolder, htmlReportFile, cts.Token);

            if (await report.GenerateReportAsync(progress))
            {
                Console.WriteLine("Success.");
            }
            else
            {
                Console.WriteLine(report.ErrorMessage);
                Console.WriteLine("Failed.");
            }
        }




    }
}
