using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.FileLoading
{
    public class ExcelFileLoader
    {
        public List<ScanData> Load(string filePath, List<FileProperty> properties, ScanField _keyProperty,
            int headerRow, int startRow)
        {
            var products = new List<ScanData>();
            ExcelPackage xls = null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                xls = new ExcelPackage(fileInfo);

                var worksheets = xls.Workbook.Worksheets.Take(1);
                foreach (var worksheet in worksheets)
                {
                    var columnDefinition = new List<ExcelColumnInfo>();
                    // Track the column number for detected properties
                    var keyProperty = ScanField.Ignore;
                    int keyColumn = -1;
                    var propertyTracker = new Dictionary<ScanField, bool>();

                    var colIndex = 0;
                    foreach (var property in properties)
                    {
                        colIndex++;
                        var columnProperty = property.Property;
                        var header = property.Header;
                        if ((columnProperty == ScanField.Ignore) || (String.IsNullOrEmpty(header))) continue;

                        var colName = worksheet.GetValue<string>(headerRow, colIndex) ?? "";
                        var formattedName = colName.ToLower().Trim().Replace("\n", " ");
                        var formattedHeader = header.ToLower().Trim().Replace("\n", " ");
                        if (!string.Equals(formattedName, formattedHeader))
                            throw new Exception("Column name in spreadsheet is not as expected. Looking for \"" + header +
                                                "\", but found \"" + colName + "\"");

                        // Set the keyColumn value only if the matching property exists in the column collection
                        if (_keyProperty == columnProperty)
                        {
                            keyProperty = _keyProperty;
                            keyColumn = colIndex;
                        }

                        if (propertyTracker.ContainsKey(columnProperty))
                            throw new Exception("A column with property \"" + columnProperty +
                                                "\" has already been defined.");

                        propertyTracker.Add(columnProperty, true);
                        columnDefinition.Add(new ExcelColumnInfo {Property = columnProperty, ColumnNumber = colIndex});
                    }

                    if (keyProperty == ScanField.Ignore)
                        throw new Exception("A column matching the \"keyProperty\" of \"" + _keyProperty +
                                            "\" is missing in the price file.");

                    int rowNumber = startRow; // Tracks the spreadsheet row being processed

                    // Read each row of the worksheet and add it to a product collection
                    while (true)
                    {
                        //vendorModule.CancelToken.ThrowIfCancellationRequested();

                        // If there is no key value, then we are done
                        if (string.IsNullOrWhiteSpace(worksheet.GetValue<string>(rowNumber, keyColumn)))
                            break;

                        var rowData = new Dictionary<ScanField, string>();

                        // Read each property value stored in columns
                        foreach (var columnInfo in columnDefinition)
                        {
                            var property = columnInfo.Property;
                            var colNumber = columnInfo.ColumnNumber;
                            try
                            {
                                rowData.Add(property, worksheet.GetValue<string>(rowNumber, colNumber).TrimToNull());
                            }
                            catch
                            {
                                rowData.Add(property, null);
                            }
                        }

                        var product = new ScanData();
                        var keys = rowData.Keys.ToList();
                        foreach (var property in keys)
                        {
                            product.Add(property, rowData[property] ?? string.Empty);
                        }

                        products.Add(product);
                        rowNumber++;
                    }
                }

                xls.Dispose();
                xls = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (xls != null)
                    xls.Dispose();
            }
            return products;
        }

        public List<List<string>> Load(string filePath)
        {
            var products = new List<List<string>>();
            ExcelPackage xls = null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                xls = new ExcelPackage(fileInfo);

                var worksheets = xls.Workbook.Worksheets.Take(1);
                foreach (var sheet in worksheets)
                {
                    var numRows = sheet.GetNumRows();
                    var columns = sheet.GetNumColumns();
                    for (int rowNum = 1; rowNum <= numRows; rowNum++)
                    {
                        var product = new List<string>();
                        for (int colNum = 1; colNum <= columns; colNum++)
                        {
                            var value = sheet.GetValue<string>(rowNum, colNum);
                            product.Add(value);
                        }
                        products.Add(product);
                    }
                }
                xls.Dispose();
                xls = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (xls != null)
                    xls.Dispose();
            }
            return products;
        }
    }
}