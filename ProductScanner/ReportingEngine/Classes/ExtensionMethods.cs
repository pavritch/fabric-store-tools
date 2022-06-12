using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Utilities.Extensions;

namespace ReportingEngine.Classes
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Convert csv file to xlsx.
        /// </summary>
        /// <remarks>
        /// Destination will be same as source with excel extension.
        /// Destination deleted if already exists.
        /// </remarks>
        /// <param name="inputCsvFilepath"></param>
        /// <returns>True if successful.</returns>
        public static bool CsvToXlsx(this string inputCsvFilepath)
        {
            var outputXlsxFilepath = Path.ChangeExtension(inputCsvFilepath, ".xlsx");

            return FileExtensions.ConvertCSVToXLSX(inputCsvFilepath, outputXlsxFilepath);
        }


        /// <summary>
        /// Fetch a text file which is located in the Resources folder of this assembly.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private static string GetEmbeddedTextResource(string res)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.Resources.{1}",
                Assembly.GetExecutingAssembly().GetName().Name, res))))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Return the referenced embedded file as a string.
        /// </summary>
        /// <remarks>
        /// Assumes the file is in the Resources folder for this DLL.
        /// </remarks>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetEmbeddedTextFile(this string filename)
        {
            return GetEmbeddedTextResource(filename);
        }

        /// <summary>
        /// Persist the referenced string as a text file.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filename"></param>
        public static void SaveTextAsFile(this string text, string filename)
        {
            try
            {
                File.WriteAllText(filename, text);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        public static HtmlAgilityPack.HtmlDocument ParseHtmlPage(this string htmlPage)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            //htmlDoc.OptionFixNestedTags = true;
            htmlDoc.OptionUseIdAttribute = true;
            htmlDoc.LoadHtml(htmlPage);

            return htmlDoc;
        }
    }
}
