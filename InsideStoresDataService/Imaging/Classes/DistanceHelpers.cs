using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ColorMine.ColorSpaces.Comparisons;

namespace InsideStores.Imaging
{
    public static class DistanceHelpers
    {

        public enum DistanceMeasurementMethod
        {
            Euclidean,
            Tanimoto,
        }



        public static double Distance(string file1, string file2, DistanceMeasurementMethod method)
        {
            var file1Bytes = file1.ReadBinaryFile();
            var file2Bytes = file2.ReadBinaryFile();

            var bmp1 = file1Bytes.FromImageByteArrayToBitmap();
            var bmp2 = file2Bytes.FromImageByteArrayToBitmap();

            return Distance(bmp1, bmp2, method);
        }


        public static double Distance(Bitmap bmp1, Bitmap bmp2, DistanceMeasurementMethod method)
        {
            Debug.Assert(bmp1.Width == bmp2.Width);
            Debug.Assert(bmp1.Height == bmp2.Height);

            var bmp1Bytes = bmp1.ToByteArray();
            var bmp2Bytes = bmp2.ToByteArray();

            return CalcDistance(bmp1Bytes, bmp2Bytes, method);
        }

        public static double Distance(BitmapSource bmp1, BitmapSource bmp2, DistanceMeasurementMethod method)
        {
            Debug.Assert(bmp1.PixelWidth == bmp2.PixelWidth);
            Debug.Assert(bmp1.PixelHeight == bmp2.PixelHeight);

            var bmp1Bytes = bmp1.ToByteArray();
            var bmp2Bytes = bmp2.ToByteArray();

            return CalcDistance(bmp1Bytes, bmp2Bytes, method);
        }

        public static double Distance(Bitmap bmp1, BitmapSource bmp2, DistanceMeasurementMethod method)
        {
            Debug.Assert(bmp1.Width == bmp2.PixelWidth);
            Debug.Assert(bmp1.Height == bmp2.PixelHeight);

            var bmp1Bytes = bmp1.ToByteArray();
            var bmp2Bytes = bmp2.ToByteArray();

            return CalcDistance(bmp1Bytes, bmp2Bytes, method);
        }

        public static double Distance(System.Windows.Media.Color c1, System.Windows.Media.Color c2)
        {
            var comp = new Cie1976Comparison();

            var a = new ColorMine.ColorSpaces.Rgb() { R = c1.R, G = c1.G, B = c1.B };
            var b = new ColorMine.ColorSpaces.Rgb() { R = c2.R, G = c2.G, B = c2.B }; 

            double distance = comp.Compare(a, b);

            return distance;
        }


        /// <summary>
        /// Calc the distance between two descriptors taking all color/texture features into account.
        /// </summary>
        /// <param name="cedd1"></param>
        /// <param name="cedd2"></param>
        /// <returns></returns>
        public static double CalcDistance(byte[] cedd1, byte[] cedd2)
        {
            Debug.Assert(cedd1 != null && cedd1.Length == 144);
            Debug.Assert(cedd2 != null && cedd2.Length == 144);

            return CalcDistance(cedd1, cedd2, DistanceMeasurementMethod.Tanimoto);
        }

        private static double CalcDistance(byte[] vector1, byte[] vector2, DistanceMeasurementMethod method)
        {
            Debug.Assert(vector1.Length == vector2.Length);

            if (vector1.Length != vector2.Length)
                throw new Exception("Distance vectors are not the same length.");

            double dist;

            switch (method)
            {
                case DistanceMeasurementMethod.Euclidean:
                    dist = vector1.EuclidianDistance(vector2);
                    break;

                case DistanceMeasurementMethod.Tanimoto:
                    dist = InsideStores.Imaging.TanimotoDistance.GetDistance(vector1, vector2);
                    break;

                default:
                    throw new Exception("Invalid distance parameter.");
            }

            return dist;
        }
    }
}
