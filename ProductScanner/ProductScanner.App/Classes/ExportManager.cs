using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Dynamic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Utilities.Extensions;
using System.Globalization;
using System.Diagnostics;

namespace ProductScanner.App
{
    #region Utility Classes for Excel Export

    public enum ExcelColumnType
    {
        Default,  // we don't touch, let it do its thing
        Auto,     // apply the auto rules defined in GetExcelColumnNumberFormatString()
        Text,
        Date,     // mm/dd/yyyy  
        DateTime, // mm/dd/yyyy 12:31 PM
        Money,    // #,##0.00
        Number,    // #,##0
        Decimal,    // #,##0.00
        Url   
    }


    public class ExcelColumnAttribute : Attribute
    {
        public ExcelColumnType ColumnType { get; set; }
        public ExcelHorizontalAlignment? Alignment  { get; set; }

        public ExcelColumnAttribute()
        {

        }

        public ExcelColumnAttribute(ExcelColumnType ColumnType = ExcelColumnType.Default)
        {
            this.ColumnType = ColumnType;
        }

        public ExcelColumnAttribute(ExcelColumnType ColumnType, ExcelHorizontalAlignment alignment)
        {
            this.ColumnType = ColumnType;
            this.Alignment = alignment;
        }


    }

    // Sample of how a class would look for creating data to be output as an excel file.
    //
    // public class MyData
    // {
    //    [ExcelColumn(ExcelColumnType.Text)]
    //    public string OneString { get; set; }

    //    [ExcelColumn(ExcelColumnType.Money)]
    //    public decimal DollarAmount { get; set; }

    //    [ExcelColumn(ExcelColumnType.Number)]
    //    public int BigCount { get; set; }
    // }

    #endregion

    /// <summary>
    /// Various utils to write work with files.
    /// </summary>
    public static class ExportManager
    {

        public static void SaveTextFile(List<string> textLines, string suggestedFilename=null)
        {
            // Example:
            // var lines = new List<string>()
            //        {
            //            "one",
            //            "two",
            //            "three",
            //        };
            // MessengerInstance.Send(new RequestExportTextFile("SampleLog.txt", lines));

            var sb = new StringBuilder(10000);
            foreach (var line in textLines)
                sb.AppendLine(line);

            SaveTextFile(sb.ToString(), suggestedFilename);
        }

        public static void SaveTextFile(string textFileContent, string SuggestedFilename = null)
        {
            // Create OpenFileDialog 

            var dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 

            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            dlg.FileName = SuggestedFilename;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                using (StreamWriter outfile = new StreamWriter(filename))
                {
                    outfile.Write(textFileContent);
                }
            }
        }


        /// <summary>
        /// Save a collection of objects to disk as an excel file.
        /// </summary>
        /// <remarks>
        /// Each public property becomes a column.
        /// </remarks>
        /// <param name="dataSet"></param>
        /// <param name="suggestedFilename"></param>
        public static void SaveExcelFile<T>(IEnumerable<T> dataSet, string suggestedFilename=null)
        {
            if (dataSet == null)
                return;

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet 1");

                ws.Cells["A1"].LoadFromCollection(dataSet, true);

                //Format the header
                using (var rng = ws.Cells[1,1,1,ws.Dimension.Columns])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;    //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                var namedStyle = ws.Workbook.Styles.CreateNamedStyle("HyperLink");
                //namedStyle.Style.Font.UnderLine = true;
                namedStyle.Style.Font.Color.SetColor(Color.FromArgb(255, 19, 120, 185));

                if (dataSet.Count() > 0)
                {
                    // TODO: this format stuff has not been tested out much...may need some tweaking once code is actually being used

                    int columnOffset = 0;
                    foreach (var prop in typeof(T).MemberProperties())
                    {
                        var colAttr = prop.GetCustomAttribute<ExcelColumnAttribute>();
                        if (colAttr != null)
                        {
                            switch(colAttr.ColumnType)
                            {
                                case ExcelColumnType.Date:
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = "mm/dd/yyyy";
                                        col.Style.HorizontalAlignment = colAttr.Alignment ?? ExcelHorizontalAlignment.Right;
                                    }
                                    break;

                                case ExcelColumnType.DateTime:
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = "mm/dd/yyyy hh:mm";
                                        col.Style.HorizontalAlignment = colAttr.Alignment ?? ExcelHorizontalAlignment.Right;
                                    }
                                    break;

                                case ExcelColumnType.Money:
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = "#,##0.00";
                                        col.Style.HorizontalAlignment = colAttr.Alignment ?? ExcelHorizontalAlignment.Right;
                                    }
                                    break;

                                case ExcelColumnType.Number:
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = "#,##0";
                                        col.Style.HorizontalAlignment = colAttr.Alignment ?? ExcelHorizontalAlignment.Right;
                                    }
                                    break;

                                case ExcelColumnType.Decimal:
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = "#,##0.00";
                                        col.Style.HorizontalAlignment = colAttr.Alignment ?? ExcelHorizontalAlignment.Right;
                                    }
                                    break;

                                case ExcelColumnType.Text: 
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = "@";
                                        col.Style.HorizontalAlignment = colAttr.Alignment ?? ExcelHorizontalAlignment.Left;
                                    }
                                    break;

                                case ExcelColumnType.Url:
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = "@";
                                        col.Style.HorizontalAlignment = colAttr.Alignment ?? ExcelHorizontalAlignment.Left;
                                        col.StyleName = "HyperLink";

                                        for (int rowIndex = col.Start.Row; rowIndex <= col.End.Row; rowIndex++)
                                        {
                                            for (int columnIndex = col.Start.Column; columnIndex <= col.End.Column; columnIndex++)
                                            {
                                                var link = ws.Cells[rowIndex, columnIndex].Text;
                                                if (!string.IsNullOrWhiteSpace(link))
                                                    ws.Cells[rowIndex, columnIndex].Hyperlink = new Uri(link, UriKind.Absolute);
                                            }
                                        }
                                    }
                                    break;


                                case ExcelColumnType.Auto:
                                  using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        col.Style.Numberformat.Format = GetExcelColumnAutoFormatString(prop.ReflectedType);
                                        if (colAttr.Alignment.HasValue)
                                            col.Style.HorizontalAlignment = colAttr.Alignment.Value;
                                    }
                                    break;

                                case ExcelColumnType.Default:
                                    using (var col = ws.Cells.Offset(1, columnOffset, dataSet.Count(), 1))
                                    {
                                        // note that does not set the format (uses whatever native mode is), but does allow
                                        // the alignment to be specified

                                        if (colAttr.Alignment.HasValue)
                                            col.Style.HorizontalAlignment = colAttr.Alignment.Value;
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                            
                        columnOffset++;
                    }
                }

                ws.Cells.AutoFitColumns();

                ws.View.FreezePanes(2, 1);

                var byteData = pck.GetAsByteArray();

                var dlg = new Microsoft.Win32.SaveFileDialog();

                // Set filter for file extension and default file extension 

                dlg.DefaultExt = ".xlsx";
                dlg.Filter = "Excel documents (.xlsx)|*.xlsx";
                dlg.FileName = suggestedFilename;

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;

                    if (File.Exists(filename))
                        File.Delete(filename);

                    using (var fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
                    {
                        fs.Write(byteData, 0, byteData.Length);
                    }
                }
            }
        }

        private static string GetExcelColumnAutoFormatString(Type type)
        {
            string format = "General";
            if (type == typeof(int))
                format = "0";
            else if (type == typeof(uint))
                format = "0";
            else if (type == typeof(long))
                format = "0";
            else if (type == typeof(ulong))
                format = "0";
            else if (type == typeof(short))
                format = "0";
            else if (type == typeof(ushort))
                format = "0";
            else if (type == typeof(double))
                format = "0.00";
            else if (type == typeof(float))
                format = "0.00";
            else if (type == typeof(decimal))
                format = NumberFormatInfo.CurrentInfo.CurrencySymbol + " #,##0.00";
            else if (type == typeof(DateTime))
                format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern + " " + DateTimeFormatInfo.CurrentInfo.LongTimePattern;
            else if (type == typeof(string))
                format = "@";
            else if (type == typeof(bool))
                format = "\"" + bool.TrueString + "\";\"" + bool.TrueString + "\";\"" + bool.FalseString + "\"";

            return format;

        }
    }
}