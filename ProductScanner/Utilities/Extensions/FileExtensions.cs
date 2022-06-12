using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelLibrary.SpreadSheet;
using OfficeOpenXml;

namespace Utilities.Extensions
{
    public static class FileExtensions
    {
        public static byte[] ToBytes(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static void CreateXLSXFile(IEnumerable<IEnumerable<string>> rows, string outputFilename)
        {
            if (File.Exists(outputFilename))
                File.Delete(outputFilename);

            var parent = Directory.GetParent(outputFilename).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

            using (var xls = new ExcelPackage(new FileInfo(outputFilename)))
            {
                var ws = xls.Workbook.Worksheets.Add("Sheet 1");
                var rowNumber = 1;
                foreach (var row in rows)
                {
                    var colNumber = 1;
                    foreach (var colData in row)
                        ws.SetValue(rowNumber, colNumber++, colData);

                    rowNumber++;
                }
                xls.Save();
            }
        }

        public static bool ConvertCSVToXLSX(string inputFilename, string outputFilename)
        {
            try
            {
                if (!File.Exists(inputFilename))
                    return false;

                using (var fs = File.Open(inputFilename, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        var config = new CsvConfiguration
                        {
                            HasHeaderRecord = false,
                        };

                        using (var csv = new CsvReader(sr, config))
                        {
                            if (File.Exists(outputFilename))
                                File.Delete(outputFilename);

                            var outFileInfo = new FileInfo(outputFilename);
                            using (var xls = new ExcelPackage(outFileInfo))
                            {
                                ExcelWorksheet ws = xls.Workbook.Worksheets.Add("Sheet 1");

                                int row = 1;
                                while (csv.Read())
                                {
                                    var fields = csv.CurrentRecord;

                                    for (int i = 0; i < fields.Length; i++)
                                    {
                                        ws.SetValue(row, i + 1, fields[i]);
                                    }
                                    row++;
                                }
                                xls.Save();
                            }
                        }
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                // for cancel, just return and let caller deal with it gracefully
                if (ex is AggregateException && ex.InnerException is TaskCanceledException)
                    return false;

                Debug.WriteLine(ex is AggregateException ? ex.InnerException.Message : ex.Message);
                throw;
            }
        }

        public static void ConvertXLSToXLSX(byte[] data, string filePath)
        {
            using (var ms = new MemoryStream(data))
            {
                // hydrate the xls object from the memory stream - for reading
                var workbook = Workbook.Load(ms);
                var sheet = workbook.Worksheets[0];

                // create a new xlsx object for writing

                if (File.Exists(filePath))
                    File.Delete(filePath);

                var parent = Directory.GetParent(filePath).FullName;
                if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

                using (var xls = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet ws = xls.Workbook.Worksheets.Add("Sheet 1");

                    // spin through all the xls cells
                    for (int rowIndex = sheet.Cells.FirstRowIndex; rowIndex <= sheet.Cells.LastRowIndex; rowIndex++)
                    {
                        Row row = sheet.Cells.GetRow(rowIndex);
                        for (int colIndex = row.FirstColIndex; colIndex <= row.LastColIndex; colIndex++)
                        {
                            Cell cell = row.GetCell(colIndex);

                            ws.SetValue(rowIndex + 1, colIndex + 1, cell.StringValue);
                        }
                    }
                    xls.Save();
                }
            }
        }

        public static void ConvertCSVToXLS(string data, string outputFilepath)
        {
            ConvertCSVToXLS(new MemoryStream(Encoding.UTF8.GetBytes(data ?? "")), outputFilepath);
        }

        public static void ConvertCSVToXLS(byte[] data, string outputFilepath)
        {
            ConvertCSVToXLS(new MemoryStream(data), outputFilepath);
        }

        public static void ConvertCSVToXLS(Stream stream, string outputFilepath)
        {
            using (var reader = new StreamReader(stream))
            {
                var config = new CsvConfiguration()
                {
                    HasHeaderRecord = false,
                };

                using (var csv = new CsvReader(reader, config))
                {
                    if (File.Exists(outputFilepath))
                        File.Delete(outputFilepath);

                    var parent = Directory.GetParent(outputFilepath).FullName;
                    if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

                    using (var xls = new ExcelPackage(new FileInfo(outputFilepath)))
                    {
                        var ws = xls.Workbook.Worksheets.Add("Sheet 1");
                        var row = 1;
                        while (csv.Read())
                        {
                            var fields = csv.CurrentRecord;

                            for (int i = 0; i < fields.Length; i++)
                            {
                                ws.SetValue(row, i + 1, fields[i]);
                            }
                            row++;
                        }
                        xls.Save();
                    }
                }
            }
        }

        public static long DirSize(DirectoryInfo d)
        {
            var fis = d.GetFiles();
            var size = fis.Sum(fi => fi.Length);

            // Add subdirectory sizes
            var dis = d.GetDirectories();
            size += dis.Sum(di => DirSize(di));
            return (size);
        }
    }
}