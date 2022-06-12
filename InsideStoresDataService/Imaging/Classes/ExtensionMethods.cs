using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ColorMine.ColorSpaces;
using InsideStores.Imaging.Descriptors;

namespace InsideStores.Imaging
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// Calculates the image descriptor.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        public static byte[] CalculateDescriptor(this BitmapSource myImage)
        {
            CEDD myCEDD = new CEDD();
            return myCEDD.MakeDescriptor(myImage);
        }

        /// <summary>
        /// Calculates the image descriptor.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        [Obsolete]
        public static byte[] CalculateDescriptor(this Bitmap myImage)
        {
            CEDD myCEDD = new CEDD();
            return myCEDD.MakeDescriptor(myImage);
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

        /// <summary>
        /// Euclidian distance of two byte[]
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static double EuclidianDistance(this byte[] vector1, byte[] vector2)
        {
            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow((double)vector1[i] - (double)vector2[i], 2);
            }
            return Math.Sqrt(value);
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
        /// BitmapSource to its flat data bytes.
        /// </summary>
        /// <param name="bms"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this BitmapSource bms)
        {
            if (bms == null)
                throw new ArgumentNullException("Null BitmapSource.");

            var w = bms.PixelWidth;
            var h = bms.PixelHeight;

            int nStride = (w * bms.Format.BitsPerPixel + 7) / 8;
            byte[] pixelByteArray = new byte[h * nStride];
            bms.CopyPixels(pixelByteArray, nStride, 0);
            return pixelByteArray;
        }

        /// <summary>
        /// Bitmap to its flat data bytes.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("Null bitmap.");

            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                bitmap.UnlockBits(bmpdata);

                return bytedata;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);

                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
                throw;
            }
        }


        /// <summary>
        /// Convert a (A)RGB string to a Color object.
        /// </summary>
        /// <param name="s">An RGB or an ARGB string.</param>
        /// <returns>A Color object.</returns>
        public static System.Windows.Media.Color ToColor(this string s)
        {
            s = s.Replace("#", string.Empty);

            byte a = System.Convert.ToByte("ff", 16);

            byte pos = 0;

            if (s.Length == 8)
            {
                a = System.Convert.ToByte(s.Substring(pos, 2), 16);
                pos = 2;
            }

            byte r = System.Convert.ToByte(s.Substring(pos, 2), 16);

            pos += 2;

            byte g = System.Convert.ToByte(s.Substring(pos, 2), 16);

            pos += 2;

            byte b = System.Convert.ToByte(s.Substring(pos, 2), 16);

            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }

        public static string ToRGBColorsString(this List<System.Windows.Media.Color> colors)
        {
            var sb = new StringBuilder();

            bool isFirst = true;

            Func<System.Windows.Media.Color, string> colorToString = (c) =>
            {
                // save as #RRGGB
                return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
            };

            foreach (var color in colors)
            {
                if (!isFirst)
                    sb.Append(";");
                sb.AppendFormat(colorToString(color));
                isFirst = false;
            }

            return sb.ToString();
        }

        public static string ToRGBColorsString(this System.Windows.Media.Color color)
        {
            // save as #RRGGB
            return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        public static Lab ToLabColor(this System.Windows.Media.Color color)
        {
            var rgb = new ColorMine.ColorSpaces.Rgb() { R = color.R, G = color.G, B = color.B };
            var lab = rgb.To<Lab>();
            return lab;
        }

        public static List<Lab> ToLabColors(this IEnumerable<System.Windows.Media.Color> colors)
        {
            if (colors == null)
                return  new List<Lab>();

            var list = new List<Lab>();
            foreach (var color in colors)
                list.Add(color.ToLabColor());

#if false
            foreach(var color in colors)
            {
                var targetLabColor = color.ToLabColor();
                Debug.WriteLine(string.Format("color {6}: RGB({0}, {1}, {2}) = LabColor = L:{3:N3}  A:{4:N3}  B:{5:N3}", color.R, color.G, color.B, targetLabColor.L, targetLabColor.A, targetLabColor.B, color.ToRGBColorsString()));

            }
#endif
            return list;
        }

        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert Bitmap to BitmapSource.
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this Bitmap bmp)
        {
            IntPtr ptr = bmp.GetHbitmap(); //obtain the Hbitmap

            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(ptr); //release the HBitmap
            return bs;
        }


        public static Bitmap FromImageByteArrayToBitmap(this byte[] ContentData)
        {
            try
            {
                if (ContentData.Length > 0)
                {
                    using (var stream = new MemoryStream(ContentData.Length))
                    {
                        stream.Write(ContentData, 0, ContentData.Length);
                        stream.Position = 0;

                        var bmp = new Bitmap(stream);
                        return bmp;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return null;
        }


        public static BitmapSource FromImageByteArrayToBitmapSource(this byte[] ContentData)
        {
            try
            {
                if (ContentData.Length > 0)
                {
                    using (var bmp = ContentData.FromImageByteArrayToBitmap())
                    {
                        var bmsrc = bmp.ToBitmapSource();
                        return bmsrc;
                    }

                    // throws excepton - so using the above approach
                    // Key cannot be null.
                    // Parameter name: key
                    //
                    //using (var stream = new MemoryStream(ContentData))
                    //{
                    //    var bitmap = new BitmapImage();
                    //    bitmap.BeginInit();
                    //    bitmap.StreamSource = stream;
                    //    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    //    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    //    bitmap.EndInit();
                    //    bitmap.Freeze();

                    //    return bitmap;
                    //}

                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return null;
        }


        public static System.Drawing.Color CalculateAverageColor(this Bitmap bm, int minDiversion=15)
        {
            // http://stackoverflow.com/questions/6177499/how-to-determine-the-background-color-of-document-when-there-are-3-options-usin/6185448#6185448

            // minDiversion:  drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
            // quickly tried changing from 5 to 25, didn't see any difference on the single target image tried.

            // cutting corners, will fail on anything else but 32 and 24 bit images
            Debug.Assert(bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int width = bm.Width;
            int height = bm.Height;
            int red = 0;
            int green = 0;
            int blue = 0;
            
            int dropped = 0; // keep track of dropped pixels
            long[] totals = new long[] { 0, 0, 0 };
            int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? 3 : 4;

            var pixelBytes = bm.ToByteArray();
            var pixelCount = pixelBytes.Length / bppModifier;

            for (int i = 0; i < pixelCount; i++)
            {
                int idx = i * bppModifier;
                red = pixelBytes[idx + 2];
                green = pixelBytes[idx + 1];
                blue = pixelBytes[idx];

                if (Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion || Math.Abs(green - blue) > minDiversion)
                {
                    totals[2] += red;
                    totals[1] += green;
                    totals[0] += blue;
                }
                else
                {
                    dropped++;
                }
            }

            int count = width * height - dropped;
            int avgR = (int)(totals[2] / count);
            int avgG = (int)(totals[1] / count);
            int avgB = (int)(totals[0] / count);

            return System.Drawing.Color.FromArgb(avgR, avgG, avgB);
        }
    }
}
