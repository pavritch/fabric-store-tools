using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExcelLibrary.SpreadSheet;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using OfficeOpenXml;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace ProductScanner.Core
{
    public static class ExtensionMethods
    {
        public static string RemoveParens(this string s)
        {
            return s.Replace("(", "").Replace(")", "");
        }

        public static string RemoveQuotes(this string s)
        {
            return s.Replace("\"", "").Replace("'", "");
        }

        public static string MoveSetInfo(this string s)
        {
            var trimmer = new Regex(@"\s\s+");
            var replacements = new List<string> {"(Set of 2)", "(Set of 3)", "Set of 2", "Set of 3", "Set of 4", 
                "Set of 5", "Set of 6", "Set of 7", "Set of 12"};

            s = s.Replace("Set Of", "Set of");

            foreach (var replace in replacements)
            {
                if (s.Contains(replace))
                {
                    s = s.Replace(replace, "").Trim() + " (" + replace.RemoveParens() + ")";
                    break;
                }
            }
            return trimmer.Replace(s.Trim(), " ");
        }

        public static int CategoryAttr<T>(this T source)
        {
            var fi = source.GetType().GetField(source.ToString());

            var attributes = (CategoryAttribute[])fi.GetCustomAttributes(
                typeof(CategoryAttribute), false);

            return attributes.Length > 0 ? attributes[0].Id : 0;
        }

        public static bool CategoryIncluded<T>(this T source)
        {
            var fi = source.GetType().GetField(source.ToString());

            var attributes = (CategoryAttribute[])fi.GetCustomAttributes(
                typeof(CategoryAttribute), false);

            return attributes.Length > 0 && attributes[0].Included;
        }

        public static decimal ToDecimalSafe(this string value)
        {
            decimal result;
            if (Decimal.TryParse(value, out result)) return result;
            return 0M;
        }

        public static double ToDoubleSafe(this string value)
        {
            double result;
            if (Double.TryParse(value, out result)) return result;
            return 0;
        }

        public static int ToIntegerSafe(this string value)
        {
            int result;
            if (Int32.TryParse(value, out result)) return result;
            return 0;
        }

        public static string MeasurementFromFraction(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var fraction = input.CaptureWithinMatchedPattern(@"\s?(?<capture>(\d+/\d+))(" + "\"" + @"|\s*)");
            decimal fractionalValue = 0M;
            decimal integerValue;

            if (fraction != null)
            {
                var ary = fraction.Trim().Split('/');
                fractionalValue = Math.Round(decimal.Parse(ary[0]) / decimal.Parse(ary[1]), 4); ;
                integerValue = input.Replace(fraction, string.Empty).TakeOnlyFirstDecimalToken();
            }
            else
            {
                integerValue = input.TakeOnlyFirstDecimalToken();
            }

            var value = integerValue + fractionalValue;
            return value.ToString();
        }

        public static string CombineProperty(params string[] values)
        {
            var tokens = new List<string>();

            Action<string> add = (s) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                    return;

                var titleCaseToken = s.TitleCase();
                if (!tokens.Contains(titleCaseToken))
                    tokens.Add(titleCaseToken);
            };

            values.ForEach(add);

            if (!tokens.Any())
                return null;

            return tokens.ToCommaDelimitedList();
        }

        public static string RemoveLabel(this string value)
        {
            if (value == null)
                return null;

            var colon = value.IndexOf(":");

            return value.Substring(colon + 1).Trim();
        }

        public static string BuildName(this string[] nameParts)
        {
            var partsList = new List<string>();
            Action<string> add = (s) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                    return;
                partsList.Add(s.Trim());
            };

            nameParts.ToList().Distinct().ForEach(add);

            return string.Join(" ", partsList).Replace(",", "");
        }

        /// <summary>
        /// Verify an html page has some minimal set of found elements - to see if generally what we're expecting.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="selectors"></param>
        /// <returns></returns>
        public static bool HasAllRequiredElements(this string html, IEnumerable<string> selectors)
        {
            var page = html.ParseHtmlPage();
            var docNode = page.DocumentNode;

            foreach (var selector in selectors)
            {
                var node = docNode.QuerySelector(selector);
                if (node == null)
                    return false;
            }

            return true;
        }

        public static HtmlDocument ParseHtmlPage(this string htmlPage)
        {
            var htmlDoc = new HtmlDocument();
            //htmlDoc.OptionFixNestedTags = true;
            htmlDoc.OptionUseIdAttribute = true;
            htmlDoc.LoadHtml(htmlPage);

            return htmlDoc;
        }

        public async static Task<byte[]> DownloadBytesFromWebAsync(this Uri url, NetworkCredential credential)
        {
            try
            {
                var client = new WebClientExtended();
                client.UseDefaultCredentials = true;
                client.Credentials = credential;
                return await client.DownloadDataTaskAsync(url);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Capture viewstate and other named elements on the page. Input element with Name.
        /// </summary>
        /// <remarks>
        /// The results are intended to be used to populate into the POST collection sent to the server.
        /// </remarks>
        /// <param name="htmlPage"></param>
        /// <param name="namedElements"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFormPostValuesByName(this string htmlPage, IEnumerable<string> namedElements)
        {
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.OptionUseIdAttribute = true;
                htmlDoc.LoadHtml(htmlPage);

                return GetFormPostValuesByName(htmlDoc, namedElements);

            }
            catch (Exception Ex)
            {
                Debug.WriteLine("Exception: " + Ex.ToString());
                throw;
            }

        }

        public static Dictionary<string, string> GetFormPostValuesByName(this HtmlDocument htmlDoc, IEnumerable<string> namedElements)
        {
            var PostValues = new Dictionary<string, string>();

            try
            {
                foreach (var itemKey in namedElements)
                {
                    // look for form element name
                    var foundElement = htmlDoc.DocumentNode.Descendants("input").Where(e => e.GetAttributeValue("name", string.Empty).Equals(itemKey, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (foundElement == null)
                        continue;

                    var value = foundElement.GetAttributeValue("value", string.Empty);
                    if (string.IsNullOrEmpty(value))
                        continue;

                    PostValues.Add(itemKey, value);
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine("Exception: " + Ex.ToString());
                throw;
            }

            return PostValues;
        }

        /// <summary>
        /// Decompresses a byte array using gzip compression.
        /// </summary>
        /// <param name="bytes">Byte array to decompress.</param>
        /// <returns>A decompressed byte array.</returns>
        public static byte[] Decompress(this byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream();
            const int bufferSize = 65536;

            using (GZipStream gzipStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
            {
                byte[] buffer = new byte[bufferSize];

                int bytesRead = 0;

                do
                {
                    bytesRead = gzipStream.Read(buffer, 0, bufferSize);
                }
                while (bytesRead == bufferSize);

                memoryStream.Write(buffer, 0, bytesRead);
            }

            return memoryStream.ToArray();


            //MemoryStream ms = new MemoryStream();
            //int msgLength = BitConverter.ToInt32(bytes, 0);
            //ms.Write(bytes, 4, bytes.Length - 4);

            //byte[] buffer = new byte[msgLength];

            //ms.Position = 0;
            //GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            //zip.Read(buffer, 0, buffer.Length);

            //return buffer;
        }

        private static readonly string[] forcedLowerCaseTokens = new string[] 
        {
            "inch", "inches", "inch)", "inches)", "and", "by", "to"
        };

        public static string TitleCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            var sTrim = s.Trim();

            var tokens = sTrim.Split(' ');

            if (tokens.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(s.Length);
            int tokenIndex = 0;
            foreach (string token in tokens)
            {
                var trimToken = token.Trim();
                if (string.IsNullOrWhiteSpace(trimToken))
                    continue;

                // if token has digits, then use as is
                if (Regex.IsMatch(token, @"\d+"))
                {
                    // but check for slash
                    var ary = token.Split('/');
                    if (ary.Length == 1)
                    {
                        sb.Append(token);
                        sb.Append(" ");
                        tokenIndex++;
                        continue;
                    }
                    
                    // title case each part separately.

                    for (int j = 0; j < ary.Length; j++)
                        ary[j] = ary[j].TitleCase();
                    sb.Append(string.Join("/", ary));

                }
                else if (tokenIndex > 0 && forcedLowerCaseTokens.Contains(token.ToLower()))
                {
                    // keep as lower case
                    sb.Append(token.ToLower());
                }
                else
                {
                    sb.Append(trimToken[0].ToString().ToUpper());

                    if (trimToken.Length > 1)
                    {
                        var sb2 = new StringBuilder(100);
                        bool skipLower = false;
                        foreach (var c in trimToken.Substring(1))
                        {
                            if (skipLower)
                            {
                                sb2.Append(c.ToString());
                                skipLower = false;
                            }
                            else
                                sb2.Append(c.ToString().ToLower());

                            if (!Char.IsLetter(c) && c != '\'')
                                skipLower = true;
                        }


                        sb.Append(sb2.ToString());
                    }
                }

                sb.Append(" ");
                tokenIndex++;
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Given a string, make some of the tokens upper case. 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string MakeTheseTokensUpperCase(this string input, string[] tokens)
        {
            if (tokens == null || tokens.Count() == 0)
                return input;

            var ary = input.Split(new char[] { ' ' });

            var sb = new StringBuilder(100);

            int count = 0;
            foreach (var item in ary)
            {
                if (count > 0)
                    sb.Append(" ");

                var s = item;
                if (tokens.Contains(item))
                    s = s.ToUpper();

                sb.Append(s);

                count++;

            }
            return sb.ToString();
        }

        public static string ToCommaDelimitedList(this IEnumerable<string> values)
        {
            if (values == null)
                return string.Empty;

            var sb = new StringBuilder(500);
            int count = 0;
            foreach (var s in values)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                if (count > 0)
                    sb.Append(", ");

                sb.Append(s);
                count++;
            }

            return sb.ToString();
        }

        private static bool IsSafeSEFilenameChar(char c)
        {
            if (c >= 'a' && c <= 'z')
                return true;

            if (c >= 'A' && c <= 'Z')
                return true;

            if (c >= '0' && c <= '9')
                return true;

            if ("-".Contains(c))
                return true;

            return false;
        }


        public static string SkuTweaks(this string input)
        {
            //AS-FACINGLEFT:CAT2-MIAC
            return input.ToUpper()
                .Replace(" ", "")
                .Replace(".", "")
                .Replace("`", "")
                .Replace("+", "")
                .Replace("&", "")
                .Replace("\\", "-")
                .Replace("/", "-")
                .Replace("#", "")
                .Replace(";", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("_", "")
                .Replace(":", "")
                .Replace("\"", "")
                .Replace("'", "")
                .Replace("È", "E")
                .Replace("É", "E")
                .Replace("Ê", "E")
                .Replace("Î", "I")
                .Replace("®", "")
                .Replace("Ñ", "N");
        }

        public static string MakeSafeSEName(this string s)
        {
            var sb = new StringBuilder(200);
            var str = s.ToLower()
                .Replace("   ", " ")
                .Replace("  ", " ")
                .Replace(" ", "-")
                .Replace("/", "-")
                .Replace("_", "-")
                .Replace(".", "-")
                .Replace("è", "e")
                .Replace("é", "e")
                .Replace("&", "and")
                .Replace("--", "-").Replace("--", "-").Replace("--", "-"); // three times

            foreach (var c in str)
                if (IsSafeSEFilenameChar(c))
                    sb.Append(c);
            return sb.ToString();
        }

        private static readonly string[] validProductGroups = new string[] { "Fabric", "Trim", "Wallcovering", "Hardware" };

        public static bool IsValidProductGroup(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return validProductGroups.Contains(value);
        }


        private static readonly string[] validUnitsOfMeasure = new string[] { "Each", "Pair", "BAG10", "Yard", "Roll", "Square Foot", "Square Meter", "Meter", "Panel" };
        public static bool IsValidUnitOfMeasure(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return validUnitsOfMeasure.Contains(value);
        }

        public static bool IsAllUpperCase(this string value)
        {
            foreach(var c in value)
            {
                if (Char.IsLower(c))
                    return false;
            }

            return true;
        }

        // so something like Spun Rayon 100 become Spun-Rayon 100
        public static string AddContentDelimiters(this string input)
        {
            // find all spaces that are surrounded by letters, and replace them with a dash
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(input.First());
            for (int i = 1; i < input.Length - 1; i++)
            {
                if (input[i] == ' ' && Char.IsLetter(input[i - 1]) && Char.IsLetter(input[i + 1]))
                    stringBuilder.Append("-");
                else stringBuilder.Append(input[i]);
            }
            stringBuilder.Append(input.Last());
            return stringBuilder.ToString();
        }

        public static string TruncateStartingWith(this string input, string match)
        {
            if (input == null)
                return null;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var index = input.IndexOf(match, 0, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
                return input;

            return input.Substring(0, index).TrimEnd();
        }

        public static string ToCentimetersMeasurement(this string input)
        {
            var inchesMeasure = input.ToInchesMeasurement();
            if (inchesMeasure == null) return null;
            return inchesMeasure.Replace(" inches", " centimeters");
        }

        public static string ToInchesMeasurement(this string input)
        {
            // input must be known by caller to start with a number which
            // is the inches representation, and whatever else is on the line is ignored

            // do not return unnecessary decimals

            //0
            //54
            //3.7
            //6.2
            //0.30000000000000004
            //15.5
            //18.5
            //2
            //21.5
            //8.5
            //5
            //0.60000000000000009
            //9.2
            //13.7

            if (string.IsNullOrWhiteSpace(input) || string.Equals(input, "n/a", StringComparison.OrdinalIgnoreCase))
                return null;

            var inches = input.TakeOnlyFirstDecimalToken();

            if (inches == 0M)
                return null;

            return string.Format("{0} {1}", inches.ToString("#.####"), (inches == 1M ? "inch" : "inches"));
        }

        public static string ToFormattedColors(this string input)
        {
            //Brown, Taupe / Tan
            //Aqua / Teal, Brown
            //Black, Grey / Linen
            //Grey / Linen, Off White
            //Taupe / Tan, Grey / Linen
            //Gold, Taupe / Tan
            //Off White, Beige
            //Aqua / Teal, Lt Green
            //Green, Lt Green
            //Brown, Orange / Spice
            //Bronze, Brown
            //Red, Orange / Spice
            //Brown, Green
            //Aqua / Teal, Green
            //Gold, Yellow

            if (string.IsNullOrWhiteSpace(input))
                return null;

            var s = input.Replace(" / ", ", ").Replace(" ,", ",").AddSpacesAfterCommas()
                .Replace("Lt ", "Light ")
                .Replace("LT ", "Light ")
                .Replace("DK ", "Dark ")
                .Replace("Drk ", "Dark ")
                .Replace("Wht ", "White ")
                .Replace("Sft ", "Soft ")
                .Replace("Dk ", "Dark ");

            return s.TitleCase();
        }

        public static string FormatContentTypes(this Dictionary<string, int> content)
        {
            if (!content.Any()) return string.Empty;
            // remove any over 100%
            var orderedTypes = content.OrderByDescending(x => x.Value).Where(x => x.Value <= 100).ToDictionary(x => x.Key, x => x.Value);

            var total = orderedTypes.Sum(x => x.Value);
            if (total > 100)
            {
                // remove ones from the end until we have less than 100%
                for (int i = 1; i < orderedTypes.Count; i++)
                {
                    var firstN = orderedTypes.Take(orderedTypes.Count - i).ToDictionary(x => x.Key, x => x.Value);
                    if (firstN.Sum(x => x.Value) <= 100)
                    {
                        orderedTypes = firstN;
                        break;
                    }
                }
            }

            total = orderedTypes.Sum(x => x.Value);
            var missingAmount = 100 - total;
            var otherValue = missingAmount;
            if (orderedTypes.ContainsKey("Other"))
            {
                otherValue += orderedTypes["Other"];
                orderedTypes.Remove("Other");
            }

            if (!orderedTypes.Any()) return String.Empty;
            var typeString = orderedTypes.Select(x => string.Format("{0}% {1}", x.Value, x.Key)).Aggregate((a, b) => a + ", " + b);
            if (otherValue != 0)
            {
                typeString += string.Format(", {0}% Other", otherValue);
            }
            return typeString;
        }

        public static string ToFormattedFabricContent(this string input)
        {
            //100% Polyester
            //100% Cotton
            //100% Linen
            //100% Silk
            //55% Linen 45% Rayon
            //100% Polyester Inherent Fr
            //55% Linen, 45% Rayon
            //60% Cotton 40% Polyester
            //52% Cotton 48% Polyester
            //64% Rayon 36% Acetate
            //100% Polyvinylchloride Face 75% Polyester25% Cotton Back
            //100% Polyester,Embroidery: 100% Rayon
            //100% Bella Dura@
            //Pile 65% Polyester 35% Cotton Back 82% Polyester18% Cotton
            //100% Bella-Dura
            //Pile:100% Rayon Back:51% Cotton 49% Rayon
            //55% Cotton, 45% Polyester
            //60% Cotton, 40% Polyester
            //51% Cotton, 49% Polyester
            //100% Polyester,Embroidery: 100% Rayon
            //100% Silk 100% Rayon Embroidery
            //100% Linen,Embroidery: 100% Viscose
            //Pile:100% Rayon Back:51% Cotton 49% Rayon
            //Pile: 50% Cotton 50% PolyesterBack: 65% Polyester 35%cotton
            //Face: 100% CelluloseBack: 100% Foam
            //Face: 100% Silk,Embroidery: 100% ViscoseBacking: 100% Cotton  Polyester Batting 100%

            Func<string, string> cleanup = (cs) =>
            {
                //Polyester25%
                //40%Polyester

                bool lastWasPct = false;

                var sbClean = new StringBuilder(100);

                for (int k = 0; k < cs.Count(); k++)
                {
                    if (lastWasPct)
                    {
                        sbClean.Append(' ');
                        lastWasPct = false;
                    }

                    if (cs[k] == '%')
                    {
                        lastWasPct = true;
                    }
                    else if (char.IsDigit(cs[k]))
                    {
                        if (k > 0 && char.IsLetter(cs[k - 1]))
                            sbClean.Append(' ');
                    }
                    sbClean.Append(cs[k]);
                }
                return sbClean.ToString();
            };

            if (string.IsNullOrWhiteSpace(input))
                return null;

            var s = cleanup(input.Replace("ViscoseBacking", "Viscose Backing")
                .Replace("Polyester Batting", "Polyester Backing")
                .Replace("Dura@", "Dura")
                .Replace("PolyesterEmbroidery", "Polyester Embroidery")
                .Replace("ViscoseEmbroidery", "Viscose Embroidery")
                .Replace("LinenEmbroidery", "Linen Embroidery")
                .Replace("WoolEmbroidery", "Wool Embroidery")
                .Replace("CottonEmbroidery", "Cotton Embroidery")
                .Replace("NylonEmbroidery", "Nylon Embroidery")
                .Replace("RayonEmbroidery", "Rayon Embroidery"));


            var parts = s.Split(new char[] {' ', ';', ',', ':'}, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Count(); i++)
                parts[i] = parts[i].Trim();

            var sbContent = new StringBuilder(100);

            int processedCount = 0;
            bool lastWasPercentage = false;
            bool lastWasColon = false;

            for (var j = 0; j < parts.Count(); j++)
            {
                if (string.IsNullOrWhiteSpace(parts[j]))
                    continue;

                if (lastWasPercentage)
                {
                    sbContent.Append(" ");
                    sbContent.Append(parts[j]);
                    processedCount++;
                    lastWasPercentage = false;
                    continue;
                }

                if (Regex.IsMatch(parts[j], @"^\d{1,3}%"))
                {
                    if (processedCount > 0 && !lastWasColon)
                        sbContent.Append(", ");

                    sbContent.Append(parts[j]);
                    lastWasPercentage = true;
                    lastWasColon = false;
                    continue;
                }

                if (lastWasColon)
                {
                    sbContent.Append(parts[j]);
                    processedCount++;
                    lastWasColon = false;
                    continue;
                }

                if (parts[j] == "Pile" || parts[j] == "Face" || parts[j] == "Embroidery" || parts[j] == "Backing")
                {
                    if (processedCount > 0)
                        sbContent.Append(", ");

                    sbContent.Append(parts[j]);
                    if (j < (parts.Count() - 1))
                    {
                        sbContent.Append(": ");
                        lastWasColon = true;
                    }
                    processedCount++;
                    continue;
                }

            }

            return sbContent.ToString();
        }

        static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// Convert a C# date to the long integer format used by javascript.
        /// </summary>
        /// <remarks>
        /// Return the number of milliseconds since 1970/01/01
        /// </remarks>
        /// <param name="date"></param>
        /// <param name="Time_Zone"></param>
        /// <returns></returns>
        public static long ConvertDatetimeToUnixTimeStamp(this DateTime date, int Time_Zone = 0)
        {
            TimeSpan The_Date = (date - EPOCH);
            return (long)(Math.Floor(The_Date.TotalSeconds) - (Time_Zone * 3600));
        }

        public static int NthIndexOf(this string target, string value, int n)
        {
            Match m = Regex.Match(target, "((" + value + ").*?){" + n + "}");

            if (m.Success)
                return m.Groups[2].Captures[n - 1].Index;
            else
                return -1;
        }

        public static int GetNumColumns(this ExcelWorksheet sheet)
        {
            var i = 1;
            while (true)
            {
                var header = sheet.GetValue<string>(1, i + 1);
                var headerNext = sheet.GetValue<string>(1, i + 2);
                if (string.IsNullOrWhiteSpace(header) && string.IsNullOrWhiteSpace(headerNext)) return i - 1;
                i++;
            }
        }

        public static int GetNumRows(this ExcelWorksheet sheet)
        {
            var i = 1;
            while (true)
            {
                var header = sheet.GetValue<string>(i++, 1);
                if (string.IsNullOrWhiteSpace(header)) return i - 2;
            }
        }

        public static int GetNumColumns(this Worksheet sheet)
        {
            var i = 0;
            while (true)
            {
                var header = sheet.Cells[0, i++].StringValue;
                if (string.IsNullOrWhiteSpace(header)) return i - 1;
            }
        }

        public static int GetNumRows(this Worksheet sheet)
        {
            var i = 0;
            while (true)
            {
                var header = sheet.Cells[i++, 0].StringValue;
                if (string.IsNullOrWhiteSpace(header)) return i - 1;
            }
        }

        public static bool Contains(this List<string> list, string value, bool ignoreCase = false)
        {
            return ignoreCase ?
                list.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase)) :
                list.Contains(value);
        }

        public static bool IsImage(this HttpWebResponse response)
        {
            return response.StatusCode == HttpStatusCode.OK && response.ContentType.ToLower().Contains("image");
        }

        public static string GetFieldValue(this HtmlNode page, string selector)
        {
            var field = page.QuerySelector(selector);
            return field != null ? field.InnerText.Trim().Replace("&nbsp;", "").Replace("&quot;", "") : null;
        }

        public static string GetFieldHtml(this HtmlNode page, string selector)
        {
            var field = page.QuerySelector(selector);
            return field != null ? field.InnerHtml.Trim() : null;
        }

        /// <summary>
        /// This is a default pricing curve. To be used when appropriate for specific vendors.
        /// </summary>
        /// <remarks>
        /// Intended to fit a curve which boosts margins at the lower end of the scale.
        /// </remarks>
        /// <param name="cost"></param>
        /// <returns></returns>
        public static decimal ComputeOurPrice(this decimal cost, bool isColeAndSon = false)
        {
            if (cost >= 18M || isColeAndSon)
                return Math.Round(cost * 1.4M, 2);

            // else use a formula which gives a curve close to this:
            //3,12
            //5,12
            //10,20
            //15,30
            //20,38
            //25,45
            //30,51
            //35,56
            //45,67
            // starting at 50, always 1.4

            // no matter what, math to use a min cost of 7 for computations.
            var adjustedCost = Math.Max((double)cost, 7);

            // y = 5.735818447•10-5 x4 - 5.639380435•10-3 x3 + 1.712695045•10-1 x2 - 2.614520087•10-1 x + 10.68992025
            // formula using this website: http://www.xuru.org/rt/PR.asp#CopyPaste

            var price = (0.00005735818447 * Math.Pow(adjustedCost, 4)) - (0.005639380435 * Math.Pow(adjustedCost, 3)) + (0.1712695045 * Math.Pow(adjustedCost, 2)) - (0.2614520087 * adjustedCost) + 10.68992025;

            return Math.Round((decimal)price, 2);
        }

        public static int GetNextIncrement(int minValue, int orderIncrement)
        {
            return RoundUp(minValue, orderIncrement);
        }

        public static int RoundUp(int num, int multiple)
        {
            if (multiple == 0)
                return 0;
            int add = multiple / Math.Abs(multiple);
            return ((num + multiple - add) / multiple) * multiple;
        }
    }
}