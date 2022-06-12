using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTester
{
    public static class ExtensionMethods
    {

        public static byte[] ReadBinaryFile(this string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var ImageData = new byte[fs.Length];
                fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));
                return ImageData;
            }
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

        public static bool HasEmbeddedWhiteRectangle(this Bitmap bmp, Rectangle rct, float whiteThresholdAvg=0.998f, float whiteThresholdLowLimit = 0.90f)
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
        public static bool HasWhiteSpaceAroundImage(this Bitmap bmp, int cornerSize, float whiteThresholdAvg, float whiteThresholdLowLimit = 0.90f, int minCorners=4)
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
                    for (int row = 0; row < r.Height; row++ )
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



    }
}
