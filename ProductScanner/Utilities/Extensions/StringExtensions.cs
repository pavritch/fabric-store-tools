using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceUnicode(this string s)
        {
            // remove multiple spaces
            s = Regex.Replace(s, @"\s+", " ");
            s = s.Replace("\\n ", "");
            s = s.Replace(@"\u003C", "<");
            s = s.Replace(@"\u003E", ">");
            s = s.Replace(@"\u0022", "\"");

            s = s.Replace("\\", "");
            return s;
        }

        public static string UnicodeToASCII(this string s)
        {
            Encoding ascii = Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;

            // Convert the string into a byte[].
            byte[] unicodeBytes = unicode.GetBytes(s);

            // Perform the conversion from one encoding to the other.
            byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

            // Convert the new byte[] into a char[] and then into a string.
            // This is a slightly different approach to converting to illustrate
            // the use of GetCharCount/GetChars.
            char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            return new string(asciiChars);
        }

        public static string InsertSpacesBetweenWords(this string source)
        {
            return string.Join(" ", source.SplitCamelCase());
        }

        public static string[] SplitCamelCase(this string source)
        {
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }

        public static T Clone<T>(this T a)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T) formatter.Deserialize(stream);
            }
        }

        public static string UnGZipMemoryToString(this byte[] input)
        {
            var aryBytes = UnGZipMemory(input);
            var utf = new UTF8Encoding();
            return utf.GetString(aryBytes);
        }

        /// <summary>
        /// Takes a binary input buffer and GZip encodes the input
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(this byte[] buffer)
        {
            using (var ms = new MemoryStream())
            {
                using (var GZip = new GZipStream(ms, CompressionMode.Compress))
                {
                    GZip.Write(buffer, 0, buffer.Length);
                }

                return ms.ToArray();
            }
        }

        public static byte[] GZipMemory(this string input)
        {
            return GZipMemory(Encoding.UTF8.GetBytes(input));
        }


        public static byte[] UnGZipMemory(this byte[] input)
        {
            using (var stream = new GZipStream(new MemoryStream(input), CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        public static string AsPercentage(this float val) { return string.Format("{0:n2}", val * 100) + "%"; }

        public static string UnEscape(this string input)
        {
            // http://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references

            if (String.IsNullOrWhiteSpace(input))
                return String.Empty;

            var returnString = input.Replace("&apos;", "'");

            returnString = returnString.Replace("&quot;", "\"");

            returnString = returnString.Replace("&gt;", ">");

            returnString = returnString.Replace("&lt;", "<");

            returnString = returnString.Replace("&amp;", "&");

            returnString = returnString.Replace("&ndash;", "-");

            returnString = returnString.Replace("&reg;", "");
            returnString = returnString.Replace("&trade;", "");

            returnString = returnString.Replace("&#39;", "'");
            returnString = returnString.Replace("&#039;", "\"");

            return returnString;
        }

        public static string ReplaceBR(this string sInput, string sReplacement = " ")
        {
            if (String.IsNullOrWhiteSpace(sInput))
                return String.Empty;

            var output = sInput.Replace("<BR>", sReplacement).Replace("<br>", sReplacement).Replace("<br />", sReplacement).Replace("<BR />", sReplacement);

            return output;
        }

        public static string ReplaceBrAndCrLfWithSpace(this string s)
        {
            return s.Replace("<BR>", " ").Replace("<br>", " ").Replace("<br />", " ").Replace("<BR />", " ").Replace("\0xd", "").Replace("\0xa", " ");
        }

        /// <summary>
        /// Convert string only if exact match.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string Change(this string s, string from, string to)
        {
            if (String.IsNullOrWhiteSpace(s))
                return s;

            if (s == @from)
                return to;

            return s;
        }

        public static string UrlEncode(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            return HttpUtility.UrlEncode(s);
        }

        public static string UrlDecode(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            return HttpUtility.UrlDecode(s);
        }

        public static string HtmlDecode(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            return HttpUtility.HtmlDecode(s).Replace("\r\n", " ");
        }

        public static string HtmlEncode(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            return HttpUtility.HtmlEncode(s);
        }

        public static byte[] ToByteArray(this string str)
        {
            if (String.IsNullOrEmpty(str))
                return new byte[0];

            var encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        public static bool ContainsAnyOf(this string s, params string[] strs)
        {
            return strs.Any(s.Contains);
        }

        public static bool ContainsAnyOfIgnoreCase(this string s, params string[] strs)
        {
            return strs.Any(s.ContainsIgnoreCase);
        }

        public static bool ContainsIgnoreCase(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return false;

            return (s1.IndexOf(s2, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public static double TakeOnlyNumericPart(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            var sb = new StringBuilder();

            foreach (var ch in s)
            {
                if (char.IsDigit(ch) || ch == '.')
                    sb.Append(ch);
                else
                    break;
            }
            return Convert.ToDouble(sb.ToString());
        }


        public static string FilterForNumericPart(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var ch in s)
            {
                if (char.IsDigit(ch) || ch == '.')
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Capture an explicit regex pattern.
        /// </summary>
        /// <remarks>
        /// You must include a single explicit capture within the pattern.
        /// </remarks>
        /// <param name="?"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string CaptureWithinMatchedPattern(this string input, string pattern)
        {
            // Example:
            // input: ../../product_images/thumbnails/resize.php?2151055.jpg&amp;size=126
            // Pattern : @"resize.php\?(?<capture>(\d{1,7})).jpg"
            if (input == null) return null;

            string capturedText = null;

            var options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

            var matches = Regex.Matches(input, pattern, options);

            if (matches.Count == 0 || matches[0].Groups.Count < 2)
                return null;

            capturedText = matches[0].Groups[1].ToString();

            return capturedText;
        }

        /// <summary>
        /// Capture the text that matched a regex pattern.
        /// </summary>
        /// <remarks>
        /// The entire matched pattern is returned. 
        /// </remarks>
        /// <param name="?"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string CaptureMatchedPattern(this string input, string pattern)
        {

            string capturedText = null;

            var options = RegexOptions.Compiled | RegexOptions.IgnoreCase;

            var matches = Regex.Matches(input, pattern, options);

            if (matches.Count == 0)
                return null;

            capturedText = matches[0].Groups[0].ToString();

            return capturedText;
        }


        public static string RemovePattern(this string input, string pattern)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var s = Regex.Replace(input, pattern, System.String.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return s;
        }

        public static string RemoveTextAfterLastChar(this string text, char c)
        {
            int lastIndexOfSeparator;

            if (!String.IsNullOrEmpty(text) &&
                ((lastIndexOfSeparator = text.LastIndexOf(c)) > -1))
            {

                return text.Remove(lastIndexOfSeparator);
            }
            return text;
        }

        public static string Remove(this string input, string toRemove)
        {
            if (toRemove == string.Empty) return input;
            return input.Replace(toRemove, "");
        }

        static public string ReplaceWholeWord(this string original, string wordToFind, string replacement, RegexOptions regexOptions = RegexOptions.None)
        {
            var pattern = String.Format(@"\b{0}\b", wordToFind);
            var ret = Regex.Replace(original, pattern, replacement, regexOptions);
            return ret;
        }

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
        {
            int startIndex = 0;
            while (true)
            {
                startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1)
                    break;

                originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

                startIndex += newValue.Length;
            }
            return originalString;
        }

        public static string RemoveInitialDash(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            if (s.StartsWith("-"))
                s = s.Remove(0, 1);

            return s;
        }

        public static string RemoveLastChar(this string s)
        {
            return s.Substring(0, s.Length - 1);
        }

        public static string RemoveSpecificTrailingChars(this string s, string trailingChars)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            bool done = false;

            while (!done)
            {
                // keep running the loop until no changes made
                bool madeChange = false;
                foreach (var c in trailingChars)
                {
                    if (s.EndsWith(c.ToString()))
                    {
                        s = s.Substring(0, s.Length - 1);
                        madeChange = true;
                        break;
                    }
                }

                if (!madeChange)
                    done = true;
            }

            return s;
        }

        public static string RomanNumeralCase(this string s)
        {
            const string RomanNumerals = @"^((?=[MDCLXVI])((M{0,3})((C[DM])|(D?" +
                @"C{0,3}))?((X[LC])|(L?XX{0,2})|L)?((I[VX])|(V?(II{0,2}))|V)?)),?$";

            if (string.IsNullOrWhiteSpace(s))
                return s;

            var sTrim = s.Trim();

            var tokens = sTrim.Split(' ');

            if (tokens.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(s.Length);
#if DEBUG
            bool bFound = false;
#endif
            foreach (string token in tokens)
            {
                var trimToken = token.Trim();
                if (string.IsNullOrWhiteSpace(trimToken))
                    continue;

                if (Regex.IsMatch(trimToken, RomanNumerals, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    sb.Append(trimToken.ToUpper());
#if DEBUG
                    bFound = true;
#endif
                }
                else
                    sb.Append(trimToken);

                sb.Append(" ");
            }

            var finalText = sb.ToString().Trim();

#if DEBUG
            if (bFound)
            {
                //Debug.WriteLine(string.Format("ROMAN: {0}", finalText));
            }
#endif
            return finalText;
        }

        public static string AddSpacesAfterCommas(this string s)
        {
            if (s == null)
                return null;

            // ensure a space after comma, unless the very next character is a digit

            var sb = new StringBuilder(200);

            int count = s.Length;
            for (int i = 0; i < count; i++)
            {
                var c = s[i];
                if (c == ',')
                {
                    if (i + 1 >= count)
                        break; // do not put a trailing comma with nothing to follow

                    var nextCh = s[i + 1]; // ??? check index range
                    if (char.IsDigit(nextCh) || nextCh == ' ')
                    {
                        sb.Append(c);
                        continue;
                    }

                    sb.Append(", ");
                }
                else
                    sb.Append(c);
            }

            return sb.ToString().Replace(" ,", ",");
        }

        public static string TrimToNull(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            var s2 = s.Trim();
            return string.IsNullOrWhiteSpace(s2) ? null : s2;
        }

        // A safe ToString() that does not throw a null reference exception but rather returns the null string
        public static string ToStringSafe(this object obj)
        {
            return obj == null ? null : obj.ToString();
        }

        public static string ToLowerSafe(this string s)
        {
            return s == null ? string.Empty : s.ToLower();
        }

        public static string StripNonAlphanumericChararcters(this string str)
        {
            if (str == null)
                return null;

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(str, "");
        }

        public static string StripNonAlphaSpaceChararcters(this string str)
        {
            if (str == null)
                return null;

            Regex rgx = new Regex("[^a-zA-Z ]");
            return rgx.Replace(str, "");
        }

        public static string Left(this string str, int length)
        {
            if (str == null)
                return null;

            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static bool IsZeroMeasurement(this string str)
        {
            var value = str.TakeOnlyFirstDecimalToken();
            return value == 0M;
        }

        public static bool IsInteger(this string str)
        {
            int result;
            return Int32.TryParse(str, out result);
        }

        public static bool IsDouble(this string str)
        {
            double result;
            return Double.TryParse(str, out result);
        }

        public static int TakeOnlyFirstIntegerToken(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            var sb = new StringBuilder("0");

            foreach (var c in input.Trim())
            {
                if (char.IsDigit(c))
                    sb.Append(c);
                else
                    break;
            }

            return int.Parse(sb.ToString());
        }

        public static int TakeOnlyLastIntegerToken(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            input = input.Trim();

            var sb = new StringBuilder("");
            for (var i = input.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(input[i]))
                    sb.Append(input[i]);
                else
                    break;
            }
            return int.Parse(new string(sb.ToString().Reverse().ToArray()));
        }

        public static decimal TakeOnlyFirstDecimalToken(this string input, int decimalPlaces = 4)
        {
            bool foundPoint = false;

            if (string.IsNullOrWhiteSpace(input))
                return 0;

            var sb = new StringBuilder("0");

            foreach (var c in input.Trim())
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                    continue;
                }

                if (c == '.' && !foundPoint)
                {
                    sb.Append(c);
                    foundPoint = true;
                    continue;
                }

                break;
            }

            return Math.Round(decimal.Parse(sb.ToString()), decimalPlaces);
        }

        public static bool ContainsDigit(this string input)
        {
            return input.Any(char.IsDigit);
        }

        public static bool IsOnlyDigits(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            foreach (var c in input)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        public static bool StartsWithDigit(this string input)
        {
            return Char.IsDigit(input.First());
        }

        public static bool EndsWithDigit(this string input)
        {
            return Char.IsDigit(input.Last());
        }

        public static string GetEverythingBeforeNewline(this string text)
        {
            return text.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries).First();
        }
    }
}