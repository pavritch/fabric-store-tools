using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Drawing;
using System.IO.Compression;
using BitMiracle.LibJpeg;
using System.ComponentModel;
using Gen4.Util.Misc;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace Website
{
    public static class JsonExtensionMethods
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };

        public static string ToJSON(this object obj, JsonSerializerSettings settings=null)
        {
            return JsonConvert.SerializeObject(obj, settings ?? SerializerSettings);
        }

        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static T JSONtoList<T>(this string jsonObj)
        {
            var jsserializer = new JavaScriptSerializer();
            jsserializer.MaxJsonLength = 999999999;
            return jsserializer.Deserialize<T>(jsonObj);
        }

        public static T FromJSON<T>(this string jsonObj)
        {
            return JsonConvert.DeserializeObject<T>(jsonObj);
        }

        public static object FromJSON(this string jsonObj)
        {
            return JsonConvert.DeserializeObject(jsonObj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
        }
    }

    public static class ExtensionMethods
    {

        public static HashSet<int> Clone(this HashSet<int> original)
        {
            HashSet<int> clone = new HashSet<int>(original);
            return clone;
        }

        public static class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random Local;

            public static Random ThisThreadsRandom
            {
                get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
            }
        }


        /// <summary>
        /// Decompresses a byte array using gzip compression.
        /// </summary>
        /// <param name="bytes">Byte array to decompress.</param>
        /// <returns>A decompressed byte array.</returns>
        public static byte[] Decompress(this byte[] bytes)
        {
            MemoryStream ms = new MemoryStream();
            int msgLength = BitConverter.ToInt32(bytes, 0);
            ms.Write(bytes, 4, bytes.Length - 4);

            byte[] buffer = new byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public static byte[] ReadBinaryFile(this string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var ImageData = new byte[fs.Length];
                fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));
                return ImageData;
            }
        }

        public static void WriteBinaryFile(this byte[] data, string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public static string StreamToString(this Stream st)
        {
            return StreamToString(st, Encoding.UTF8);
        }

        public static string StreamToString(this Stream st, Encoding encoding)
        {
            using (var reader = new StreamReader(st))
            {
                string value = reader.ReadToEnd();
                return value;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            if (list.Count() < 4)
                return;

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string SHA256Digest(this string input)
        {
            return Hashing.Hash(input, Hashing.HashingTypes.SHA256);
        }

        public static int ToSeed(this string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            var hashString = new SHA1Managed();
            int result = 0;

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                result += x;

            return result;
        }


        public static void DeterministicShuffle<T>(this List<T> list, string seed)
        {
            if (string.IsNullOrEmpty(seed))
                DeterministicShuffle(list, 12345);
            else
                DeterministicShuffle(list, seed.ToSeed());
        }

        public static void DeterministicShuffle<T>(this List<T> list, int seed)
        {
            if (list.Count() < 4)
                return;

            var r = new Random(seed);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Euclidian distance of two double[].
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static double EuclidianDistance(this double[] vector1, double[] vector2)
        {
            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow(vector1[i] - vector2[i], 2);
            }
            return Math.Sqrt(value);
        }

        /// <summary>
        /// Returns Description attribute of an Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static string ToMaterialString(this Dictionary<string, int?> input)
        {
            if (input == null || input.Count() == 0)
                return string.Empty;

            var sb = new StringBuilder(100);

            if (input.Count() > 1)
            {
                bool isFirst = true;
                foreach (var item in input.OrderByDescending(e => e.Value.GetValueOrDefault(100)))
                {
                    if (string.IsNullOrWhiteSpace(item.Key))
                        continue;

                    if (!isFirst)
                        sb.Append(", ");

                    if (item.Value.HasValue)
                    {
                        sb.AppendFormat("{0} {1}%", item.Key, item.Value);
                    }
                    else
                    {
                        sb.Append(item.Key);
                    }
                    isFirst = false;
                }
            }
            else
            {
                var item = input.First();
                if (item.Value.HasValue)
                {
                    sb.AppendFormat("{0}% {1}", item.Value, item.Key);
                }
                else
                {
                    sb.Append(item.Key);
                }
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


        public static string HtmlEncode(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return HttpUtility.HtmlEncode(s);
        }

        public static string HtmlDecode(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return HttpUtility.HtmlDecode(s);
        }

        public static List<string> ParseCommaDelimitedList(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return new List<string>();

            try
            {

                var list = new List<string>();

                string[] phrases = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var phrase in phrases)
                {
                    if (string.IsNullOrWhiteSpace(phrase))
                        continue;

                    var cleanPhrase = phrase.Trim();
                    if (string.IsNullOrWhiteSpace(cleanPhrase))
                        continue;

                    list.Add(cleanPhrase);
                }

                return list;
            }
            catch
            {
                return new List<string>();
            }
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


        public static string ToCommaDelimitedList(this IEnumerable<int> values, bool noSpaces = false)
        {
            if (values == null)
                return string.Empty;

            var sb = new StringBuilder(500);
            int count = 0;
            foreach (var v in values)
            {
                if (count > 0)
                    sb.Append(noSpaces ? "," : ", ");

                sb.AppendFormat("{0}", v);
                count++;
            }

            return sb.ToString();
        }

        public static string ConvertToLines(this IEnumerable<string> input)
        {
            var sb = new StringBuilder(512);

            foreach (var item in input)
                sb.AppendLine(item); 

            return sb.ToString();
        }

        /// <summary>
        /// Given the input string, return split up into lines.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> ConvertToListOfLines(this string input)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return list;

            StringReader strReader = new StringReader(input);
            while (true)
            {
                var line = strReader.ReadLine();
                if (line == null)
                    break;

                if (!string.IsNullOrWhiteSpace(line))
                    list.Add(line);
            }

            return list;
        }


        public static string TitleCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            var sTrim = s.Trim();

            var tokens = sTrim.Split(' ');

            if (tokens.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(s.Length);

            foreach (string token in tokens)
            {
                var trimToken = token.Trim();
                if (string.IsNullOrWhiteSpace(trimToken))
                    continue;

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

                sb.Append(" ");
            }

            return sb.ToString().Trim();
        }


        public static bool ContainsIgnoreCase(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return false;

            return (s1.IndexOf(s2, StringComparison.OrdinalIgnoreCase) >= 0);
        }


        public static decimal Median(this IEnumerable<decimal> source)
        {
            var cached = source.ToList();               // Cache the sequence
            int decimals = cached.Count();              // Use the cached version
            if (decimals != 0)
            {
                var midpoint = (decimals - 1) / 2;
                var sorted = cached.OrderBy(n => n);    // Use the cached version
                var median = sorted.ElementAt(midpoint);

                if (decimals % 2 == 0)
                {
                    median = (median + sorted.ElementAt(midpoint + 1)) / 2;
                }

                return median;
            }
            else
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
        }


        /// <summary>
        /// Compute teh Levenshtein distance between two strings.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns>cost or distance (steps) to go from one to the other.</returns>
        public static int LevenshteinDistance (this string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }



        public static bool IsValidAbsoluteUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("ftp", StringComparison.OrdinalIgnoreCase))
                return false;

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Uri tempValue;
                return (Uri.TryCreate(url, UriKind.Absolute, out tempValue));
            }

            return false;
        }


        public static bool IsImageAvailable(this string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 1000 * 10;
                request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15";

                var response = (HttpWebResponse)request.GetResponse();

                bool result = false;

                if (response.StatusCode == HttpStatusCode.OK && response.ContentType.ToLower().Contains("image"))
                    result = true;

                response.Close();

                return result;
            }
            catch
            {
                return false;
            }
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

        public static Bitmap FromImageByteArrayToBitmap(this byte[] ContentData)
        {
            try
            {
                if (ContentData.Length > 0)
                {
                    return new Bitmap(new MemoryStream(ContentData));
                    //using (var stream = new MemoryStream(ContentData.Length))
                    //{
                    //    stream.Write(ContentData, 0, ContentData.Length);
                    //    stream.Position = 0;

                    //    var bmp = new Bitmap(stream);
                    //    return bmp;
                    //}
                }
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Preambles for well-known image formats.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/List_of_file_signatures
        /// </remarks>
        private static Dictionary<string, List<byte[]>> KnownImageBytePreambles = new Dictionary<string, List<byte[]>>() 
            {
                {".jpg", new List<byte[]> {new byte[] {0xFF, 0xD8}}},
                {".jpeg", new List<byte[]> {new byte[] {0xFF, 0xD8}}},

                //{".jpg", new List<byte[]> {new byte[] {0xFF, 0xD8, 0xFF, 0xE0},  new byte[] {0xFF, 0xD8, 0xFF, 0xE1}}},
                //{".jpeg", new List<byte[]> {new byte[] {0xFF, 0xD8, 0xFF, 0xE0},  new byte[] {0xFF, 0xD8, 0xFF, 0xE1}}},

                {".tif", new List<byte[]> {new byte[] {0x49, 0x49, 0x2A, 0x00},  new byte[] {0x4D, 0x4D, 0x00, 0x2A}}},
                {".tiff", new List<byte[]> {new byte[] {0x49, 0x49, 0x2A, 0x00},  new byte[] {0x4D, 0x4D, 0x00, 0x2A}}},

                {".png", new List<byte[]> {new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}}},
            };

        /// <summary>
        /// Review the first few bytes of the image data to see if represents a jpg file
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static bool HasJpgImagePreamble(this byte[] imageBytes)
        {
            if (imageBytes == null)
                return false;

            bool found = false;
            foreach (var sequence in KnownImageBytePreambles[".jpg"])
            {
                for (int i = 0; i < sequence.Length; i++)
                {
                    found = true;
                    if (imageBytes[i] != sequence[i])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return true;
            }

            return false;
        }


        public static bool HasImagePreamble(this byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 10)
                return false;

            // https://en.wikipedia.org/wiki/List_of_file_signatures

            // ignore file types, we just want to know if if matches any well-known preamble

            foreach (var item in KnownImageBytePreambles.Values)
            {
                bool found = false;
                foreach (var sequence in item)
                {
                    for (int i = 0; i < sequence.Length; i++)
                    {
                        found = true;
                        if (imageBytes[i] != sequence[i])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Determine if the bytes correspond to an image of the indicated type by file extension.
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool HasImagePreamble(this byte[] imageBytes, string Url, string contentType=null)
        {
            if (imageBytes == null || imageBytes.Length < 10)
                return false;

            string extension;

            // if content type input, then use that as super strong hint about the kind of image, since
            // some URLs may be parameterized and not contain a true extension

            if (contentType != null)
            {
                switch(contentType)
                {
                    case "image/jpeg":
                    case "image/jpg":
                        extension = ".jpg";
                        break;

                    case "image/tiff":
                    case "image/tif":
                        extension = ".tif";
                        break;

                    case "image/png":
                        extension = ".png";
                        break;

                    default:
                        extension = Path.GetExtension(Url).ToLower();
                        break;
                }
            }
            else
                extension = Path.GetExtension(Url).ToLower();

            List<byte[]> allowedBytes;
            if (KnownImageBytePreambles.TryGetValue(extension, out allowedBytes))
            {
                bool found = false;
                foreach (var sequence in allowedBytes)
                {
                    for (int i = 0; i < sequence.Length; i++)
                    {
                        found = true;
                        if (imageBytes[i] != sequence[i])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Get the image (http or ftp) and return bytes, else null if any kind of exception or not an image.
        /// </summary>
        /// <remarks>
        /// Does not throw.
        /// </remarks>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static byte[] GetImageFromWeb(this string Url)
        {
            // Note: below line of code is in Application_Start to prevent some exceptions when processing HTTPS images.
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

            try
            {
                if (Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var client = new WebClientEx();

                    var image = client.DownloadData(Url);

                    var contentType = client.ResponseHeaders["Content-Type"];
                    if (contentType == null || (contentType.IndexOf("image") == -1) || !HasImagePreamble(image, Url, contentType))
                        return null;

                    return image;
                }
                else if (Url.StartsWith("ftp", StringComparison.OrdinalIgnoreCase))
                {
                    // the login must be passed on the URL, special chars like # must be escaped.
                    // ftp://dealers:Brewster%231@ftpimages.brewsterhomefashions.com/WallpaperBooks/Dollhouse VIII/Images/300dpi/Patterns/487-68832.jpg

                    try
                    {
                        WebClient client = new WebClient();

                        var imageBytes = client.DownloadData(Url);

                        // perform a series of checks to ensure we have something good back

                        if (imageBytes == null || imageBytes.Length < 10)
                            return null;

                        if (!HasImagePreamble(imageBytes, Url))
                            return null;

                        return imageBytes;
                    }
                    catch(WebException Ex)
                    {
                        // FTP for bad file gives 550 error
                        //if (Ex.Message.ContainsIgnoreCase("file not found"))
                        //    throw new Exception("file not found");

                        Debug.WriteLine(Ex.Message);
                        return null;
                    }
                    catch(Exception Ex)
                    {
                        Debug.WriteLine(Ex.Message);
                        return null;
                    }
                }
                else
                    return null;

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }

            return null;
        }




        /// <summary>
        /// Crop white borders using a threshold to help guide the tolerence.
        /// </summary>
        /// <remarks>
        /// White (for the border) is considered anything greater than or equal to 
        /// the threshold. The brightness ranges from 0.0 through 1.0, where 0.0
        /// represents black and 1.0 represents white.
        /// </remarks>
        /// <param name="bmp"></param>
        /// <param name="whiteThreshold"></param>
        /// <returns></returns>
        public static Bitmap CropWhiteSpace(this Bitmap bmp, float whiteThresholdAvg, float whiteThresholdLowLimit = 0.90f)
        {
            // http://www.itdevspace.com/2012/07/c-crop-white-space-from-around-image.html

            int w = bmp.Width;
            int h = bmp.Height;

            Func<int, bool> allWhiteRow = r =>
            {
                float sum = 0.0f;
                int pixelCount = 0;

                for (int i = 0; i < w; i++)
                {
                    var pixelBrightness = bmp.GetPixel(i, r).GetBrightness();
                    if (pixelBrightness < whiteThresholdLowLimit)
                        return false;

                    sum += pixelBrightness;
                }

                var avg = sum / (float)pixelCount;

                if (avg >= whiteThresholdAvg)
                    return true;

                return false;
            };

            Func<int, bool> allWhiteColumn = c =>
            {
                float sum = 0.0f;
                int pixelCount = 0;

                for (int i = 0; i < h; ++i)
                {
                    var pixelBrightness = bmp.GetPixel(c, i).GetBrightness();
                    if (pixelBrightness < whiteThresholdLowLimit)
                        return false;

                    sum += pixelBrightness;
                }

                var avg = sum / (float)pixelCount;

                if (avg >= whiteThresholdAvg)
                    return true;

                return false;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (!allWhiteRow(row))
                    break;
                topmost = row;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (!allWhiteRow(row))
                    break;
                bottommost = row;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (!allWhiteColumn(col))
                    break;
                leftmost = col;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (!allWhiteColumn(col))
                    break;
                rightmost = col;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}",
                  topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }


        public static byte[] ResizeImageAsSquare(this byte[] originalContent, int newDimension, int jpgCompression = 80)
        {
            if (originalContent == null)
                return originalContent;

            // the new dimension is used for both width and height

            // we want to return an image which uses the full dimension in each direction - so a non-square image will still
            // return a square

            int? Width;
            int? Height;

            GetImageDimensions(originalContent, out Width, out Height);

            var imgElem = new Neodynamic.WebControls.ImageDraw.ImageElement { Source = Neodynamic.WebControls.ImageDraw.ImageSource.Binary, SourceBinary = originalContent, PreserveMetaData=false };

            var imgDraw = new Neodynamic.WebControls.ImageDraw.ImageDraw();
            imgDraw.Canvas.AutoSize = true;

            if (Width != Height)
            {
                var smallestDimension = Math.Min(Width.GetValueOrDefault(), Height.GetValueOrDefault());

                var crop = new Neodynamic.WebControls.ImageDraw.Crop
                {
                    Width = smallestDimension,
                    Height = smallestDimension,
                };
                imgElem.Actions.Add(crop);
            }

            //Create an instance of Resize class
            var resize = new Neodynamic.WebControls.ImageDraw.Resize
            {
                Width = newDimension,
                LockAspectRatio = Neodynamic.WebControls.ImageDraw.LockAspectRatio.WidthBased
            };
            imgElem.Actions.Add(resize);


            imgDraw.Elements.Add(imgElem);
            imgDraw.ImageFormat = Neodynamic.WebControls.ImageDraw.ImageDrawFormat.Jpeg;
            imgDraw.JpegCompressionLevel = jpgCompression;

            var resizedImage = imgDraw.GetOutputImageBinary();
            return resizedImage;
        }

        public static byte[] ResizeImage(this byte[] originalContent, int newWidth, int quality=80)
        {
            int? Width;
            int? Height;

            GetImageDimensions(originalContent, out Width, out Height);

            // if original content already smaller than desired size, no need to resize smaller, keep what we have

            if (Width.HasValue && Width.Value <= newWidth)
            {
#if DEBUG
                //Debug.WriteLine(string.Format("Want {2}, Not resized: {0}x{1}", Width.Value, Height.Value, newWidth));
#endif
                return originalContent;
            }
            var imgElem = new Neodynamic.WebControls.ImageDraw.ImageElement { Source = Neodynamic.WebControls.ImageDraw.ImageSource.Binary, SourceBinary = originalContent, PreserveMetaData = false };

            var imgDraw = new Neodynamic.WebControls.ImageDraw.ImageDraw();
            imgDraw.Canvas.AutoSize = true;

            //Create an instance of Resize class
            var resize = new Neodynamic.WebControls.ImageDraw.Resize
            {
                Width = newWidth,
                //Height = imageSize,
                LockAspectRatio = Neodynamic.WebControls.ImageDraw.LockAspectRatio.WidthBased
            };

            //Apply the action on the ImageElement
            imgElem.Actions.Add(resize);

            imgDraw.Elements.Add(imgElem);
            imgDraw.ImageFormat = Neodynamic.WebControls.ImageDraw.ImageDrawFormat.Jpeg;
            imgDraw.JpegCompressionLevel = quality;

            var resizedImage = imgDraw.GetOutputImageBinary();
#if false
            int? WidthAfter;
            int? HeightAfter;

            GetImageDimensions(resizedImage, out WidthAfter, out HeightAfter);
            Debug.WriteLine(string.Format("Want {4}, Resized from {0}x{1} to {2}x{3}", Width.Value, Height.Value, WidthAfter.Value, HeightAfter.Value, newWidth));
#endif
            return resizedImage;
        }

        public static byte[] MakeOptimizedSquareImage(byte[] originalImage, int width, int[] compressionRatios, int[] acceptableSizeKB)
        {
            byte[] resizedImage = null;

            for (int attempt = 0; attempt < compressionRatios.Length; attempt++)
            {
                var imgBytes = originalImage.ResizeImageAsSquare(width, compressionRatios[attempt]);
                if (imgBytes.Length < (1024 * acceptableSizeKB[attempt]))
                {
                    resizedImage = imgBytes;
                    break;
                }
            }

            return resizedImage;
        }

        public static byte[] MakeIconImage150(this byte[] originalImage)
        {
            // returns a 150 square icon to our specifications

            var compressionRatios = new int[] {95, 90, 85, 80, 70, 60, 40, 10 };
            var acceptableSizeKB = new int[]  { 5, 12, 15, 20, 30, 50, 50, 50 };
            return MakeOptimizedSquareImage(originalImage, 150, compressionRatios, acceptableSizeKB);
        }

        public static byte[] MakeMicroImage50(this byte[] originalImage)
        {
            // returns a 50 square icon to our specifications

            var compressionRatios = new int[] {  85, 80, 75, 70, 60 };
            var acceptableSizeKB = new int[]  {   2,  2,  3,  4,  5 };

            return MakeOptimizedSquareImage(originalImage, 50, compressionRatios, acceptableSizeKB);
        }

        public static byte[] MakeMiniImage100(this byte[] originalImage)
        {
            // returns a 100 square icon to our specifications

            var compressionRatios = new int[] { 95, 90, 85, 80, 75, 70, 65 };
            var acceptableSizeKB = new int[] {   4,  4,  5,  6,  7, 10, 12 };

            return MakeOptimizedSquareImage(originalImage, 100, compressionRatios, acceptableSizeKB);
        }


        public static byte[] MakeSmallImage225(this byte[] originalImage)
        {
            // returns a 225 square icon to our specifications

            var compressionRatios = new int[] {90, 85, 80, 70, 60, 40, 10 };
            var acceptableSizeKB = new int[] { 20, 25, 25, 30, 50, 50, 50 };

            return MakeOptimizedSquareImage(originalImage, 225, compressionRatios, acceptableSizeKB);
        }

        public static bool HasWhiteSpaceAroundImage(this byte[] image)
        {
            try
            {
                if (image == null)
                    return false;

                using (var bmp = image.FromImageByteArrayToBitmap())
                {
                    return HasWhiteSpaceAroundImage(bmp);
                }
            }
            catch
            {}

            return false;
        }

        public static bool HasWhiteSpaceAroundImage(this Bitmap bmp)
        {
            return HasWhiteSpaceAroundImage(bmp, 20, 0.998f);
        }

        /// <summary>
        /// Checks to see if the given rect within the image is all white.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="r"></param>
        /// <param name="whiteThresholdAvg"></param>
        /// <param name="whiteThresholdLowLimit"></param>
        /// <returns></returns>
        public static bool HasEmbeddedWhiteRectangle(this Bitmap bmp, Rectangle rct, float whiteThresholdAvg = 0.998f, float whiteThresholdLowLimit = 0.90f)
        {

            Func<Rectangle, int, bool> allWhiteRow = (r, rowNumber) =>
            {
                float sum = 0.0f;
                int pixelCount = 0;

                for (int i = 0; i < r.Width; i++)
                {
                    var pixelBrightness = bmp.GetPixel(r.X + i, r.Y + rowNumber).GetBrightness();
                    if (pixelBrightness < whiteThresholdLowLimit)
                        return false;

                    sum += pixelBrightness;
                }

                var avg = sum / (float)pixelCount;

                if (avg >= whiteThresholdAvg)
                    return true;

                return false;
            };

            Func<Rectangle, bool> allWhiteRectangle = (r) =>
            {
                for (int row = 0; row < r.Height; row++)
                {
                    if (!allWhiteRow(r, row))
                        return false;
                }
                return true;
            };

            return allWhiteRectangle(rct);
        }


        /// <summary>
        /// Checks four corners to be white(ish), then the middle to not be white.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="cornerSize"></param>
        /// <param name="whiteThresholdAvg"></param>
        /// <param name="whiteThresholdLowLimit"></param>
        /// <returns></returns>
        public static bool HasWhiteSpaceAroundImage(this Bitmap bmp, int cornerSize, float whiteThresholdAvg, float whiteThresholdLowLimit = 0.90f, int minCorners = 4)
        {
            if (bmp == null || cornerSize >= bmp.Width || cornerSize >= bmp.Height)
                return false;

            Func<Rectangle, int, bool> allWhiteCornerRow = (corner1, r) =>
            {
                float sum = 0.0f;
                int pixelCount = 0;

                for (int i = 0; i < corner1.Width; i++)
                {
                    var pixelBrightness = bmp.GetPixel(corner1.X + i, corner1.Y + r).GetBrightness();
                    if (pixelBrightness < whiteThresholdLowLimit)
                        return false;

                    sum += pixelBrightness;
                }

                var avg = sum / (float)pixelCount;

                if (avg >= whiteThresholdAvg)
                    return true;

                return false;
            };

            Func<Rectangle, bool> allWhiteRectangle = (r) =>
            {
                for (int row = 0; row < r.Height; row++)
                {
                    if (!allWhiteCornerRow(r, row))
                        return false;
                }
                return true;
            };


            var corners = new List<Rectangle>()
            {
                new Rectangle(0, 0, cornerSize, cornerSize), // top left
                new Rectangle(bmp.Width - cornerSize, 0, cornerSize, cornerSize), // top right
                new Rectangle(0, bmp.Height - cornerSize, cornerSize, cornerSize), // bottom left
                new Rectangle(bmp.Width - cornerSize, bmp.Height - cornerSize, cornerSize, cornerSize), // bottom right
            };

            int countWhiteCorners = 0;
            foreach (var corner in corners)
            {
                if (allWhiteRectangle(corner))
                    countWhiteCorners++;
            }

            if (countWhiteCorners < minCorners)
                return false;

            // all the corners are white(ish); now make sure the image has some non-white interior to
            // ensure we don't get fooled by some all white image

            if (allWhiteRectangle(new Rectangle(bmp.Width / 4, bmp.Height / 4, bmp.Width / 2, bmp.Height / 2)))
                return false;

            return true;
        }


        /// <summary>
        /// Crop N pixels from left and right sides of image.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="cropPixels"></param>
        /// <returns></returns>
        public static Bitmap CropSides(this Bitmap bmp, int cropPixels)
        {
            var croppedWidth = bmp.Width - cropPixels * 2;

            var target = new Bitmap(croppedWidth, bmp.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(bmp,
                    new RectangleF(0, 0, croppedWidth, bmp.Height),
                    new RectangleF(cropPixels, 0, croppedWidth, bmp.Height),
                    GraphicsUnit.Pixel);
            }
            return target;

        }

        /// <summary>
        /// Crop N pixels from top and bottom of image.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="cropPixels"></param>
        /// <returns></returns>
        public static Bitmap CropTopAndBottom(this Bitmap bmp, int cropPixels)
        {
            var croppedHeight = bmp.Height - cropPixels * 2;

            var target = new Bitmap(bmp.Width, croppedHeight);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(bmp,
                    new RectangleF(0, 0, bmp.Width, croppedHeight),
                    new RectangleF(cropPixels, 0, bmp.Width, croppedHeight),
                    GraphicsUnit.Pixel);
            }
            return target;

        }
        /// <summary>
        /// Crop N pixels from left, right, top, and bottom sides of image.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="cropPixels"></param>
        /// <returns></returns>
        public static Bitmap CropBorder(this Bitmap bmp, int cropPixels)
        {
            var croppedWidth = bmp.Width - cropPixels * 2;
            var croppedHeight = bmp.Height - cropPixels * 2;

            var target = new Bitmap(croppedWidth, croppedHeight);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(bmp,
                    new RectangleF(0, 0, croppedWidth, croppedHeight),
                    new RectangleF(cropPixels, cropPixels, croppedWidth, croppedHeight),
                    GraphicsUnit.Pixel);
            }
            return target;

        }

        public static Bitmap CropDropShadow(this Bitmap bmp, int fromBottom, int fromRight)
        {
            var croppedWidth = bmp.Width - fromRight;
            var croppedHeight = bmp.Height - fromBottom;
            
            var target = new Bitmap(croppedWidth, croppedHeight);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(bmp,
                    new RectangleF(0, 0, croppedWidth, croppedHeight),
                    new RectangleF(0, 0, croppedWidth, croppedHeight),
                    GraphicsUnit.Pixel);
            }
            return target;
        }

        /// <summary>
        /// Crop out white borders, but must be EXACTLY white.
        /// </summary>
        /// <remarks>
        /// This is the original version.
        /// </remarks>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap CropWhiteSpace(this Bitmap bmp)
        {
            // http://www.itdevspace.com/2012/07/c-crop-white-space-from-around-image.html

            int w = bmp.Width;
            int h = bmp.Height;
            int white = 0xffffff;

            Func<int, bool> allWhiteRow = r =>
            {
                for (int i = 0; i < w; ++i)
                    if ((bmp.GetPixel(i, r).ToArgb() & white) != white)
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = c =>
            {
                for (int i = 0; i < h; ++i)
                    if ((bmp.GetPixel(c, i).ToArgb() & white) != white)
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (!allWhiteRow(row))
                    break;
                topmost = row;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (!allWhiteRow(row))
                    break;
                bottommost = row;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (!allWhiteColumn(col))
                    break;
                leftmost = col;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (!allWhiteColumn(col))
                    break;
                rightmost = col;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}",
                  topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }


        public static bool IsSquareImage(this byte[] originalContent, int? dimension)
        {
            if (originalContent == null)
                return false;

            int? Width;
            int? Height;

            GetImageDimensions(originalContent, out Width, out Height);

            var isSquare = (Width.GetValueOrDefault() == Height.GetValueOrDefault());

            return dimension.HasValue ? isSquare && dimension.Value == Height.Value : isSquare;
        }


        public static void GetImageDimensions(byte[] ContentData, out int? Width, out int? Height)
        {
            Width = null;
            Height = null;

            if (ContentData.Length > 0)
            {
                try
                {
                    using (var bmp = ContentData.FromImageByteArrayToBitmap())
                    {
                        Width = bmp.Width;
                        Height = bmp.Height;
                    }
                }
                catch
                {
                }
            }
        }


        public static bool HasKravetDropShadow(this byte[] image)
        {
            try
            {
                if (image == null)
                    return false;

                using (var bmp = image.FromImageByteArrayToBitmap())
                {
                    return HasKravetDropShadow(bmp);
                }
            }
            catch
            { }

            return false;
        }


        public static bool HasKravetDropShadow(this Bitmap bmp)
        {
            // looking for a signature for KR drop shadow
            // these pixels form the lower right corner of the shadow, which when
            // detected, means we want to crop 12px from the bottom, and 8px from the right
            // F1F1F1, F4F4F4
            // F6F6F6, F8F8F8

            if (bmp == null)
                return false;

            var width = bmp.Width;
            var height = bmp.Height;

            if (width < 500 || height < 500)
                return false;

#if true
            // draw a diagonal line up from the bottom right corner
            // and see that all the brightness levels get darker

            Func<bool> checkRightBottomCorner = () =>
                {
                    // logically works, but didn't detect all the shadows 
                    // due to many not exactly matching the target values

                    // search at most within 10px of bottom right corner

                    var detectSize = 10;

                    var pxRightBottom = "F8F8F8".ToColor();
                    var pxLeftBottom = "F6F6F6".ToColor();
                    var pxRightTop = "F4F4F4".ToColor();
                    var pxLeftTop = "F1F1F1".ToColor();

                    Func<int, int, bool> isShadowBlock = (x, y) =>
                    {
                        // passed in X,Y must be lower right pixel
                        // of box to check

                        if (bmp.GetPixel(x, y) != pxRightBottom)
                            return false;

                        if (bmp.GetPixel(x - 1, y) != pxLeftBottom)
                            return false;

                        if (bmp.GetPixel(x, y - 1) != pxRightTop)
                            return false;

                        if (bmp.GetPixel(x - 1, y - 1) != pxLeftTop)
                            return false;

                        return true;
                    };

                    for (int x = width - 1; x > width - detectSize; x--)
                    {
                        for (int y = height - 1; y > height - detectSize; y--)
                        {
                            if (isShadowBlock(x, y))
                                return true;
                        }
                    }

                    return false;

                };

            Func<bool> checkDiagUpFromBottomRightCorner = () =>
                {
                    // typical

                    //delta[1]=0.02352941
                    //delta[2]=0.03921568
                    //delta[3]=0.05490196
                    //delta[4]=0.09019607
                    //delta[5]=0.09019607
                    //delta[6]=0.0862745

                    //delta[1]=0.02745098
                    //delta[2]=0.04313725
                    //delta[3]=0.05098039
                    //delta[4]=0.0862745
                    //delta[5]=0.0745098
                    //delta[6]=0.07843137

                    //delta[1]=0.02745098
                    //delta[2]=0.04313725
                    //delta[3]=0.05490196
                    //delta[4]=0.07843137
                    //delta[5]=0.07843137
                    //delta[6]=0.07843137

                    //delta[1]=0.01568627
                    //delta[2]=0.04313725
                    //delta[3]=0.05490196
                    //delta[4]=0.07843137
                    //delta[5]=0.0745098
                    //delta[6]=0.0862745


                    var lengthLine = 7;

                    // lower indexes in array should be lighter
                    var aryPixels = new float[lengthLine];

                    for (int i = 0; i < lengthLine; i++)
                    {
                        var x = (width - 1) - i;
                        var y = (height - 1) - i;
                        aryPixels[i] = bmp.GetPixel(x, y).GetBrightness();
                    }

                    // each index in the array must be darker than the one before it

                    // index of J, so first does not count
                    var minDeltas = new float[] { 0.0f, 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.05f};
                    var maxDeltas = new float[] { 0.0f, 0.05f, 0.06f, 0.08f, 0.10f, 0.11f, 0.12f};

                    // skip first pixel
                    for (int j = 1; j < aryPixels.Length; j++)
                    {
                        var delta = aryPixels[j - 1] - aryPixels[j];
                        //Debug.WriteLine(string.Format("delta[{0}]={1}", j, delta));
                        if (delta < minDeltas[j] || delta > maxDeltas[j])
                            return false;
                    }

                    return true;
                };

            Func<int, bool> checkLineUpFromBottom = (x) =>
            {
                // pass in X coord for where the line up from the bottom is checked

                // read some pixels in a short line from the bottom row up a few, all at the same X
                // if is a drop shadow, they will get darker

                // typical
                //delta[1]=0.04705882
                //delta[2]=0.05490196
                //delta[3]=0.07058823
                //delta[4]=0.08235294

                //delta[1]=0.04705882
                //delta[2]=0.05882353
                //delta[3]=0.06666666
                //delta[4]=0.08235294

                //delta[1]=0.04705882
                //delta[2]=0.04705882
                //delta[3]=0.06274509
                //delta[4]=0.09019607

                //delta[1]=0.04313725
                //delta[2]=0.05490196
                //delta[3]=0.06666666
                //delta[4]=0.0745098

                var lengthLine = 5;

                // lower indexes in array should be lighter
                var aryPixels = new float[lengthLine];

                for (int i = 0; i < lengthLine; i++)
                {
                    var y = (height - 1) - i;
                    aryPixels[i] = bmp.GetPixel(x, y).GetBrightness();
                }

                // each index in the array must be darker than the one before it

                // index of J, so first does not count
                var minDeltas = new float[] { 0.0f, 0.01f, 0.02f, 0.03f, 0.04f};
                var maxDeltas = new float[] { 0.0f, 0.065f, 0.085f, 0.10f, 0.11f};

                // skip first pixel
                for (int j = 1; j < aryPixels.Length; j++)
                {
                    var delta = aryPixels[j - 1] - aryPixels[j];
                    //Debug.WriteLine(string.Format("delta[{0}]={1}", j, delta));

                    if (delta < minDeltas[j] || delta > maxDeltas[j])
                        return false;
                }

                return true;
            };


            // has to pass both the diag and the line up test

            if (checkDiagUpFromBottomRightCorner() && checkLineUpFromBottom(50) && checkLineUpFromBottom(150))
                return true;

            // or, has specific pixels in bottom right corner
            if (checkRightBottomCorner())
                return true;

            // no shadow signature detected

            return false;
#else

#endif
        }

        public static bool HasGrayCorners(this Bitmap bmp)
        {
            var width = bmp.Width;
            var height = bmp.Height;

            var topLeft = bmp.GetPixel(0, 0);
            var topRight = bmp.GetPixel(width - 1, 0);
            var bottomRight = bmp.GetPixel(width - 1, height - 1);
            var bottomLeft = bmp.GetPixel(0, height - 1);

            return (
                (topLeft.R == topLeft.G && topLeft.G == topLeft.B && topLeft.R > 70 && topLeft.R < 180) &&
                (topRight.R == topRight.G && topRight.G == topRight.B && topRight.R > 70 && topRight.R < 180) &&
                (bottomRight.R == bottomRight.G && bottomRight.G == bottomRight.B && bottomRight.R > 70 && bottomRight.R < 180) &&
                (bottomLeft.R == bottomLeft.G && bottomLeft.G == bottomLeft.B && bottomLeft.R > 70 && bottomLeft.R < 180));
        }

        public static byte[] ToJpeg(this byte[] imageBytes, int quality = 80)
        {
            var imgElem = new Neodynamic.WebControls.ImageDraw.ImageElement { Source = Neodynamic.WebControls.ImageDraw.ImageSource.Binary, SourceBinary = imageBytes, PreserveMetaData = false };

            var imgDraw = new Neodynamic.WebControls.ImageDraw.ImageDraw();
            imgDraw.Canvas.AutoSize = true;

            imgDraw.Elements.Add(imgElem);
            imgDraw.ImageFormat = Neodynamic.WebControls.ImageDraw.ImageDrawFormat.Jpeg;
            imgDraw.JpegCompressionLevel = quality;

            var resizedImage = imgDraw.GetOutputImageBinary();

            return resizedImage;
        }



        public static byte[] ToJpeg(this Bitmap bmp, int quality = 80)
        {
#if true
            Byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                bmp.Save(memoryStream, ImageFormat.Bmp);
                data = memoryStream.ToArray();
            }

            return data.ToJpeg(quality);
#else
            //  very slow - like a few seconds for a big image

            var jpg = new JpegImage(bmp);

            using (var ms = new MemoryStream())
            {
                var compression = new CompressionParameters
                {
                    Quality = quality
                };

                jpg.WriteJpeg(ms, compression);

                var ary = ms.ToArray();

                return ary;
            }
#endif
        }

        public static ProductGroup? ToProductGroup(this string input)
        {
            // note that we're matching on description since that is what is used by SQL.

            if (string.IsNullOrWhiteSpace(input))
                return null;

            foreach (var v in LibEnum.GetValues<ProductGroup>())
            {
                if (LibEnum.GetDescription(v).Equals(input, StringComparison.OrdinalIgnoreCase))
                    return v;
            }

            return null;
        }

        public static string ToInchesMeasurement(this double inches)
        {
            if (inches == 0.0)
                return string.Empty;

            return string.Format("{0} {1}", inches.ToString("#.####"), (inches == 1.0 ? "inch" : "inches"));
        }

    }
}