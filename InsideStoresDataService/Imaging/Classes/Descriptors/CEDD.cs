using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Diagnostics;

// 
/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * 
 * Savvas Chatzichristofis
 * Download the latest version from http://www.chatzichristofis.info
 * 
 * Details regarding these descriptors can be found at the following papers: (in other words, if you use these descriptors in your scientific work, we kindly ask you to cite one or more of the following papers  )
 *
 * S. A. Chatzichristofis and Y. S. Boutalis, “CEDD: COLOR AND EDGE DIRECTIVITY DESCRIPTOR – A COMPACT DESCRIPTOR FOR IMAGE INDEXING AND RETRIEVAL.”, «6th International Conference in advanced research on Computer Vision Systems (ICVS)», Lecture Notes in Computer Science (LNCS), pp.312-322, May 12 to 15, 2008, Santorini, Greece.
 *
 * S. A. Chatzichristofis, Y. S. Boutalis and M. Lux, “SELECTION OF THE PROPER COMPACT COMPOSIDE DESCRIPTOR FOR IMPROVING CONTENT BASED IMAGE RETRIEVAL.”, «The Sixth IASTED International Conference on Signal Processing, Pattern Recognition and Applications (SPPRA)», ACTA PRESS, pp.134-140, February 17 to 19, 2009, Innsbruck, Austria.
 * 
 * 
 * 
 * VER 1.01 April 1st 2013
 * schatzic@ee.duth.gr
 *

 */

namespace InsideStores.Imaging.Descriptors
{
    public  class CEDD
    {
        /// <summary>
        /// The Texture types 
        /// </summary>
        public enum TextureTypes
        {
            /// <summary>
            /// Solid Textures
            /// </summary>
            NonEdge,
            /// <summary>
            /// Multi - Directional Lines
            /// </summary>
            NonDirectionalEdge,
            /// <summary>
            /// Horizontal Lines
            /// </summary>
            HorizontalEdge,
            /// <summary>
            /// Vertical Lines
            /// </summary>
            VerticalEdge,
            /// <summary>
            /// Diagonal 45 Degree Lines
            /// </summary>
            Diagonal_45Degree,
            /// <summary>
            /// Diagonal 135 Degree Lines
            /// </summary>
            Diagonal_135Degree
        }


        public struct MaskResults
        {
            public double Mask1;
            public double Mask2;
            public double Mask3;
            public double Mask4;
            public double Mask5;

        }

        public struct Neighborhood 
        {
            public double Area1;
            public double Area2;
            public double Area3;
            public double Area4;
            
            // Area 1       Area2
            // Area 3       Area4

        }

        private static System.Windows.Media.Color[] descriptorColors;
        private static string[] descriptorColorNames;


        public double T0;
        public double T1;
        public double T2;
        public double T3;
        public bool Compact;

        // we do not use this one!
        public CEDD(double Th0, double Th1, double Th2, double Th3,bool CompactDescriptor)
        {
            this.T0 = Th0;
            this.T1 = Th1;
            this.T2 = Th2;
            this.T3 = Th3;
            this.Compact = CompactDescriptor;
        }


        static CEDD()
        {
            #region Init Static Color and Texture Data
            descriptorColors = new System.Windows.Media.Color[24];
            /*   0 - White
             *   1 - Gray
             *   2 - Black
             *   3 - Light Red
             *   4 - Red
             *   5 - Dark Red
             *   6 - Light Orange
             *   7 - Orange
             *   8 - Dark Orange
             *   9 - Light Yellow
             *   10 - Yellow
             *   11 - Dark Yellow
             *   12  - Light Green
             *   13 - Green
             *   14 - Dark Green
             *   15 - Light Cyan
             *   16 - Cyan
             *   17  - Dark Syan
             *   18 - Light Blue
             *   19 - Blue
             *   20 - Dark blue
             *   21 - Light Magenta
             *   22 - Magenta
             *   23 - Dark Magenta
             */
            descriptorColors[0] = System.Windows.Media.Colors.White;
            descriptorColors[1] = System.Windows.Media.Colors.Gray;
            descriptorColors[2] = System.Windows.Media.Colors.Black;
            descriptorColors[3] = System.Windows.Media.Color.FromArgb(255, 255, 204, 204);
            descriptorColors[4] = System.Windows.Media.Colors.Red;
            descriptorColors[5] = System.Windows.Media.Colors.DarkRed;
            descriptorColors[6] = System.Windows.Media.Color.FromArgb(255, 255, 213, 142);
            descriptorColors[7] = System.Windows.Media.Colors.Orange;
            descriptorColors[8] = System.Windows.Media.Colors.DarkOrange;
            descriptorColors[9] = System.Windows.Media.Colors.LightYellow;
            descriptorColors[10] = System.Windows.Media.Colors.Yellow;
            descriptorColors[11] = System.Windows.Media.Color.FromArgb(255, 150, 150, 0);
            descriptorColors[12] = System.Windows.Media.Colors.LightGreen;
            descriptorColors[13] = System.Windows.Media.Colors.Green;
            descriptorColors[14] = System.Windows.Media.Colors.DarkGreen;
            descriptorColors[15] = System.Windows.Media.Colors.LightCyan;
            descriptorColors[16] = System.Windows.Media.Colors.Cyan;
            descriptorColors[17] = System.Windows.Media.Colors.DarkCyan;
            descriptorColors[18] = System.Windows.Media.Colors.LightBlue;
            descriptorColors[19] = System.Windows.Media.Colors.Blue;
            descriptorColors[20] = System.Windows.Media.Colors.DarkBlue;
            descriptorColors[21] = System.Windows.Media.Color.FromArgb(255, 255, 127, 255);
            descriptorColors[22] = System.Windows.Media.Colors.Magenta;
            descriptorColors[23] = System.Windows.Media.Colors.DarkMagenta;


            descriptorColorNames = new string[24];
            descriptorColorNames[0] = "White";
            descriptorColorNames[1] = "Gray";
            descriptorColorNames[2] = "Black";
            descriptorColorNames[3] = "Light Red";
            descriptorColorNames[4] = "Red";
            descriptorColorNames[5] = "Dark Red";
            descriptorColorNames[6] = "Light Orange";
            descriptorColorNames[7] = "Orange";
            descriptorColorNames[8] = "Dark Orange";
            descriptorColorNames[9] = "Light Yellow";
            descriptorColorNames[10] = "Yellow";
            descriptorColorNames[11] = "Dark Yellow";
            descriptorColorNames[12] = "Light Green";
            descriptorColorNames[13] = "Green";
            descriptorColorNames[14] = "Dark Green";
            descriptorColorNames[15] = "Light Cyan";
            descriptorColorNames[16] = "Cyan";
            descriptorColorNames[17] = "Dark Cyan";
            descriptorColorNames[18] = "Light Blue";
            descriptorColorNames[19] = "Blue";
            descriptorColorNames[20] = "Dark blue";
            descriptorColorNames[21] = "Light Magenta";
            descriptorColorNames[22] = "Magenta";
            descriptorColorNames[23] = "Dark Magenta";
            #endregion
        }
        

        public CEDD()
        {
            this.T0 = 14;
            this.T1 = 0.68;
            this.T2 = 0.98;
            this.T3 = 0.98;
            this.Compact = false;
        }


        /// <summary>
        /// Gets the approximately descriptor colors.
        /// </summary>
        public static System.Windows.Media.Color[] DescriptorColors
        {
            get
            {
                return descriptorColors;
            }
        }

        /// <summary>
        /// Gets the approximately descriptor colors.
        /// </summary>
        public static string[] DescriptorColorNames
        {
            get
            {
                return descriptorColorNames;
            }
        } 

        public static int ColorCount
        {
            get
            {
                return 24; // number of color units in this CEDD
            }
        }

        public static int TextureCount
        {
            get
            {
                return 6; // number of texture units in this CEDD
            }
        }

        // http://www.codeproject.com/Articles/104929/Bitmap-to-BitmapSource

        public byte[] MakeDescriptor(BitmapSource srcImg)
        {
            // both apply() methods return the same result, but there could be a super tiny difference
            // due to jpg decoding. Noticed one or two bits. Bitmap input is faster.

            return (
                from x in (IEnumerable<double>)this.Apply(srcImg)
                select Convert.ToByte(x)).ToArray<byte>();
        }


        [Obsolete]
        public byte[] MakeDescriptor(Bitmap srcImg)
        {
            // Aug 24, 2015
            // in testing same images....there was about a .01 to .02 distance difference between
            // the descriptors produced between Bitmap and BitmapSource. The original used only 
            // bitmap source - suggest sticking with that since have verified our results exactly
            // match the original prototype project (descriptors, tanimoto and histograms).

            // both apply() methods return the same result, but there could be a super tiny difference
            // due to jpg decoding. Noticed one or two bits. Bitmap input is faster.

            return (
                from x in (IEnumerable<double>)this.Apply(srcImg)
                select Convert.ToByte(x)).ToArray<byte>();
        }


        /// <summary>
        /// Returns int[24] histogram with color values.
        /// </summary>
        /// <remarks>
        /// Order of colors matches descriptorColors list.
        /// </remarks>
        /// <param name="cedd"></param>
        /// <returns></returns>
        public static int[] GetColorHistogram(byte[] cedd)
        {
            Debug.Assert(cedd != null && cedd.Length == 144);

            int[] histogram = new int[24];
            for (int texture = 0; texture < 6; texture++)
                for (int color = 0; color < 24; color++)
                    histogram[color] += cedd[texture * 24 + color];

            return histogram;
        }


        /// <summary>
        /// returns int[6] histogram with texture values.
        /// </summary>
        /// <remarks>
        /// Order of bins matches TextureTypes enum.
        /// </remarks>
        /// <param name="cedd"></param>
        /// <returns></returns>
        public static int[] GetTextureHistogram(byte[] cedd)
        {
            Debug.Assert(cedd != null && cedd.Length == 144);

            int[] histogram = new int[6];
            for (int texture = 0; texture < 6; texture++)
                for (int color = 0; color < 24; color++)
                    histogram[texture] += cedd[texture * 24 + color];

            return histogram;
        }

        /// <summary>
        /// Creates the custom descriptor from a set of textures and colors.
        /// </summary>
        /// <param name="theTextures">The textures.</param>
        /// <param name="theColors">The colors.</param>
        /// <param name="normalize">if set to <c>true</c> [normalize].</param>
        /// <returns></returns>
        public static float[] CreateCustomDescriptor(List<TextureTypes> theTextures, List<System.Windows.Media.Color> theColors, bool normalize = true)
        {
            byte[] textureDescriptor = new byte[144];
            byte[] colorDescriptor = new byte[144];
            textureDescriptor = putTexturesToDescriptor(textureDescriptor, theTextures);
            colorDescriptor = putColorsToDescriptor(colorDescriptor, theColors);
            float[] descriptor = new float[144];
            for (int i = 0; i < 144; i++)
                descriptor[i] = textureDescriptor[i] + colorDescriptor[i];
            if (normalize)
            {
                float max = descriptor.Max();
                for (int i = 0; i < 144; i++)
                    descriptor[i] = 7 * descriptor[i] / max;

            }
            else
            {
                for (int i = 0; i < 144; i++)
                    if (descriptor[i] > 7) descriptor[i] = 7;
            }
            return descriptor;
        }

        #region Local Methods
        private double[] Apply(BitmapSource srcImg)
        {
            CEDD.MaskResults mask1 = new CEDD.MaskResults();
            CEDD.Neighborhood area1 = new CEDD.Neighborhood();
            Fuzzy10Bin fuzzy10Bin = new Fuzzy10Bin(false);
            Fuzzy24Bin fuzzy24Bin = new Fuzzy24Bin(false);
            RGB2HSV rGB2HSV = new RGB2HSV();
            int[] numArray = new int[3];
            double[] numArray1 = new double[10];
            double[] numArray2 = new double[24];
            double[] numArray3 = new double[144];
            int pixelWidth = srcImg.PixelWidth;
            int pixelHeight = srcImg.PixelHeight;
            double[,] numArray4 = new double[pixelWidth, pixelHeight];
            double[,] numArray5 = new double[2, 2];
            int[,] numArray6 = new int[pixelWidth, pixelHeight];
            int[,] numArray7 = new int[pixelWidth, pixelHeight];
            int[,] numArray8 = new int[pixelWidth, pixelHeight];
            int num = 0x640;
            int num1 = (int)Math.Floor((double)pixelWidth / Math.Sqrt((double)num));
            int num2 = (int)Math.Floor((double)pixelHeight / Math.Sqrt((double)num));
            if (num1 % 2 != 0)
            {
                num1--;
            }
            if (num2 % 2 != 0)
            {
                num2--;
            }
            if (num2 < 2)
            {
                num2 = 2;
            }
            if (num1 < 2)
            {
                num1 = 2;
            }
            int[] numArray9 = new int[6];
            for (int i = 0; i < 144; i++)
            {
                numArray3[i] = 0;
            }
            int num3 = 4 * pixelWidth;
            byte[] numArray10 = new byte[num3 * pixelHeight];
            srcImg.CopyPixels(numArray10, num3, 0);
            for (int j = 0; j < pixelHeight; j++)
            {
                for (int k = 0; k < pixelWidth; k++)
                {
                    int num4 = numArray10[4 * k + j * num3 + 2];
                    int num5 = numArray10[4 * k + j * num3 + 1];
                    int num6 = numArray10[4 * k + j * num3];
                    int num7 = (int)(0.114 * (double)num6 + 0.587 * (double)num5 + 0.299 * (double)num4);
                    numArray4[k, j] = (double)num7;
                    numArray6[k, j] = num4;
                    numArray7[k, j] = num5;
                    numArray8[k, j] = num6;
                }
            }
            int[] numArray11 = new int[num2 * num1];
            int[] numArray12 = new int[num2 * num1];
            int[] numArray13 = new int[num2 * num1];
            int[] numArray14 = new int[num2 * num1];
            int[] numArray15 = new int[num2 * num1];
            int[] numArray16 = new int[num2 * num1];
            int num8 = 0;
            for (int l = 0; l < pixelHeight - num2; l = l + num2)
            {
                for (int m = 0; m < pixelWidth - num1; m = m + num1)
                {
                    int num9 = 0;
                    int num10 = 0;
                    num8 = 0;
                    area1.Area1 = 0;
                    area1.Area2 = 0;
                    area1.Area3 = 0;
                    area1.Area4 = 0;
                    numArray9[0] = -1;
                    numArray9[1] = -1;
                    numArray9[2] = -1;
                    numArray9[3] = -1;
                    numArray9[4] = -1;
                    numArray9[5] = -1;
                    for (int n = 0; n < 2; n++)
                    {
                        for (int o = 0; o < 2; o++)
                        {
                            numArray5[n, o] = 0;
                        }
                    }
                    int num11 = 0;
                    for (int p = l; p < l + num2; p++)
                    {
                        for (int q = m; q < m + num1; q++)
                        {
                            numArray11[num11] = numArray6[q, p];
                            numArray12[num11] = numArray7[q, p];
                            numArray13[num11] = numArray8[q, p];
                            numArray14[num11] = numArray6[q, p];
                            numArray15[num11] = numArray7[q, p];
                            numArray16[num11] = numArray8[q, p];
                            num11++;
                            if (q < m + num1 / 2 && p < l + num2 / 2)
                            {
                                area1.Area1 = area1.Area1 + 4 * numArray4[q, p] / (double)(num1 * num2);
                            }
                            if (q >= m + num1 / 2 && p < l + num2 / 2)
                            {
                                area1.Area2 = area1.Area2 + 4 * numArray4[q, p] / (double)(num1 * num2);
                            }
                            if (q < m + num1 / 2 && p >= l + num2 / 2)
                            {
                                area1.Area3 = area1.Area3 + 4 * numArray4[q, p] / (double)(num1 * num2);
                            }
                            if (q >= m + num1 / 2 && p >= l + num2 / 2)
                            {
                                area1.Area4 = area1.Area4 + 4 * numArray4[q, p] / (double)(num1 * num2);
                            }
                        }
                    }
                    mask1.Mask1 = Math.Abs(area1.Area1 * 2 + area1.Area2 * -2 + area1.Area3 * -2 + area1.Area4 * 2);
                    mask1.Mask2 = Math.Abs(area1.Area1 * 1 + area1.Area2 * 1 + area1.Area3 * -1 + area1.Area4 * -1);
                    mask1.Mask3 = Math.Abs(area1.Area1 * 1 + area1.Area2 * -1 + area1.Area3 * 1 + area1.Area4 * -1);
                    mask1.Mask4 = Math.Abs(area1.Area1 * Math.Sqrt(2) + area1.Area2 * 0 + area1.Area3 * 0 + area1.Area4 * -Math.Sqrt(2));
                    mask1.Mask5 = Math.Abs(area1.Area1 * 0 + area1.Area2 * Math.Sqrt(2) + area1.Area3 * -Math.Sqrt(2) + area1.Area4 * 0);
                    double num12 = Math.Max(mask1.Mask1, Math.Max(mask1.Mask2, Math.Max(mask1.Mask3, Math.Max(mask1.Mask4, mask1.Mask5))));
                    mask1.Mask1 = mask1.Mask1 / num12;
                    mask1.Mask2 = mask1.Mask2 / num12;
                    mask1.Mask3 = mask1.Mask3 / num12;
                    mask1.Mask4 = mask1.Mask4 / num12;
                    mask1.Mask5 = mask1.Mask5 / num12;
                    int num13 = -1;
                    if (num12 >= this.T0)
                    {
                        num13 = -1;
                        if (mask1.Mask1 > this.T1)
                        {
                            num13++;
                            numArray9[num13] = 1;
                        }
                        if (mask1.Mask2 > this.T2)
                        {
                            num13++;
                            numArray9[num13] = 2;
                        }
                        if (mask1.Mask3 > this.T2)
                        {
                            num13++;
                            numArray9[num13] = 3;
                        }
                        if (mask1.Mask4 > this.T3)
                        {
                            num13++;
                            numArray9[num13] = 4;
                        }
                        if (mask1.Mask5 > this.T3)
                        {
                            num13++;
                            numArray9[num13] = 5;
                        }
                    }
                    else
                    {
                        numArray9[0] = 0;
                        num13 = 0;
                    }
                    for (int r = 0; r < num2 * num1; r++)
                    {
                        num9 = num9 + numArray11[r];
                        num10 = num10 + numArray12[r];
                        num8 = num8 + numArray13[r];
                    }
                    num9 = Convert.ToInt32(num9 / (num2 * num1));
                    num10 = Convert.ToInt32(num10 / (num2 * num1));
                    num8 = Convert.ToInt32(num8 / (num2 * num1));
                    numArray = rGB2HSV.ApplyFilter(num9, num10, num8);
                    if (this.Compact)
                    {
                        numArray1 = fuzzy10Bin.ApplyFilter((double)numArray[0], (double)numArray[1], (double)numArray[2], 2);
                        for (int s = 0; s <= num13; s++)
                        {
                            for (int t = 0; t < 10; t++)
                            {
                                if (numArray1[t] > 0)
                                {
                                    numArray3[10 * numArray9[s] + t] = numArray3[10 * numArray9[s] + t] + numArray1[t];
                                }
                            }
                        }
                    }
                    else
                    {
                        numArray1 = fuzzy10Bin.ApplyFilter((double)numArray[0], (double)numArray[1], (double)numArray[2], 2);
                        numArray2 = fuzzy24Bin.ApplyFilter((double)numArray[0], (double)numArray[1], (double)numArray[2], numArray1, 2);
                        for (int u = 0; u <= num13; u++)
                        {
                            for (int v = 0; v < 24; v++)
                            {
                                if (numArray2[v] > 0)
                                {
                                    numArray3[24 * numArray9[u] + v] = numArray3[24 * numArray9[u] + v] + numArray2[v];
                                }
                            }
                        }
                    }
                }
            }
            double num14 = 0;
            for (int w = 0; w < 144; w++)
            {
                num14 = num14 + numArray3[w];
            }
            for (int x = 0; x < 144; x++)
            {
                numArray3[x] = numArray3[x] / num14;
            }
            numArray3 = CEDDQuant.Apply(numArray3);
            return numArray3;
        }


        // would need to verify these return the same results before 
        // simply allowing both

        // Extract the descriptor
        private double[] Apply(Bitmap srcImg)
        {
            Fuzzy10Bin Fuzzy10 = new Fuzzy10Bin(false);
            Fuzzy24Bin Fuzzy24 = new Fuzzy24Bin(false);
            RGB2HSV HSVConverter = new RGB2HSV();
            int[] HSV = new int[3];

            double[] Fuzzy10BinResultTable = new double[10];
            double[] Fuzzy24BinResultTable = new double[24];
            double[] CEDD = new double[144];


            int width = srcImg.Width;
            int height = srcImg.Height;


            double[,] ImageGrid = new double[width, height];
            double[,] PixelCount = new double[2, 2];
            int[,] ImageGridRed = new int[width, height];
            int[,] ImageGridGreen = new int[width, height];
            int[,] ImageGridBlue = new int[width, height];
            int NumberOfBlocks = 1600; // blocks
            int Step_X = (int)Math.Floor(width / Math.Sqrt(NumberOfBlocks));
            int Step_Y = (int)Math.Floor(height / Math.Sqrt(NumberOfBlocks));

            if ((Step_X % 2) != 0)
            {
                Step_X = Step_X - 1;
            }
            if ((Step_Y % 2) != 0)
            {
                Step_Y = Step_Y - 1;
            }


            if (Step_Y < 2) Step_Y = 2;
            if (Step_X < 2) Step_X = 2;

            int[] Edges = new int[6];

            MaskResults MaskValues;
            Neighborhood PixelsNeighborhood;

            PixelFormat fmt = (srcImg.PixelFormat == PixelFormat.Format8bppIndexed) ?
                         PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;


            BitmapData srcData = srcImg.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, fmt);


            int offset = srcData.Stride - ((fmt == PixelFormat.Format8bppIndexed) ? width : width * 3);

            for (int i = 0; i < 144; i++)
            {

                CEDD[i] = 0;

            }


            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();


                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src += 3)
                    {


                        ImageGrid[x, y] = (0.299f * src[2] + 0.587f * src[1] + 0.114f * src[0]);
                        ImageGridRed[x, y] = (int)src[2];
                        ImageGridGreen[x, y] = (int)src[1];
                        ImageGridBlue[x, y] = (int)src[0];


                    }

                    src += offset;


                }

            }

            srcImg.UnlockBits(srcData);


            int[] CororRed = new int[Step_Y * Step_X];
            int[] CororGreen = new int[Step_Y * Step_X];
            int[] CororBlue = new int[Step_Y * Step_X];

            int[] CororRedTemp = new int[Step_Y * Step_X];
            int[] CororGreenTemp = new int[Step_Y * Step_X];
            int[] CororBlueTemp = new int[Step_Y * Step_X];

            int MeanRed, MeanGreen, MeanBlue = 0;
            int T = -1;


            int TempSum = 0;
            double Max = 0;

            int TemoMAX_X = Step_X * (int)Math.Sqrt(NumberOfBlocks);
            int TemoMAX_Y = Step_Y * (int)Math.Sqrt(NumberOfBlocks); ;


            for (int y = 0; y < TemoMAX_Y; y += Step_Y)
            {

                for (int x = 0; x < TemoMAX_X; x += Step_X)
                {


                    MeanRed = 0;
                    MeanGreen = 0;
                    MeanBlue = 0;
                    PixelsNeighborhood.Area1 = 0;
                    PixelsNeighborhood.Area2 = 0;
                    PixelsNeighborhood.Area3 = 0;
                    PixelsNeighborhood.Area4 = 0;
                    Edges[0] = -1;
                    Edges[1] = -1;
                    Edges[2] = -1;
                    Edges[3] = -1;
                    Edges[4] = -1;
                    Edges[5] = -1;

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            PixelCount[i, j] = 0;
                        }

                    }

                    TempSum = 0;

                    for (int i = y; i < y + Step_Y; i++)
                    {
                        for (int j = x; j < x + Step_X; j++)
                        {
                            // Color Information
                            CororRed[TempSum] = ImageGridRed[j, i];
                            CororGreen[TempSum] = ImageGridGreen[j, i];
                            CororBlue[TempSum] = ImageGridBlue[j, i];

                            CororRedTemp[TempSum] = ImageGridRed[j, i];
                            CororGreenTemp[TempSum] = ImageGridGreen[j, i];
                            CororBlueTemp[TempSum] = ImageGridBlue[j, i];

                            TempSum++;


                            // Texture Information

                            if (j < (x + Step_X / 2) && i < (y + Step_Y / 2)) PixelsNeighborhood.Area1 += (ImageGrid[j, i]);
                            if (j >= (x + Step_X / 2) && i < (y + Step_Y / 2)) PixelsNeighborhood.Area2 += (ImageGrid[j, i]);
                            if (j < (x + Step_X / 2) && i >= (y + Step_Y / 2)) PixelsNeighborhood.Area3 += (ImageGrid[j, i]);
                            if (j >= (x + Step_X / 2) && i >= (y + Step_Y / 2)) PixelsNeighborhood.Area4 += (ImageGrid[j, i]);



                        }

                    }


                    PixelsNeighborhood.Area1 = (int)(PixelsNeighborhood.Area1 * (4.0 / (Step_X * Step_Y)));

                    PixelsNeighborhood.Area2 = (int)(PixelsNeighborhood.Area2 * (4.0 / (Step_X * Step_Y)));

                    PixelsNeighborhood.Area3 = (int)(PixelsNeighborhood.Area3 * (4.0 / (Step_X * Step_Y)));

                    PixelsNeighborhood.Area4 = (int)(PixelsNeighborhood.Area4 * (4.0 / (Step_X * Step_Y)));


                    MaskValues.Mask1 = Math.Abs(PixelsNeighborhood.Area1 * 2 + PixelsNeighborhood.Area2 * -2 + PixelsNeighborhood.Area3 * -2 + PixelsNeighborhood.Area4 * 2);
                    MaskValues.Mask2 = Math.Abs(PixelsNeighborhood.Area1 * 1 + PixelsNeighborhood.Area2 * 1 + PixelsNeighborhood.Area3 * -1 + PixelsNeighborhood.Area4 * -1);
                    MaskValues.Mask3 = Math.Abs(PixelsNeighborhood.Area1 * 1 + PixelsNeighborhood.Area2 * -1 + PixelsNeighborhood.Area3 * 1 + PixelsNeighborhood.Area4 * -1);
                    MaskValues.Mask4 = Math.Abs(PixelsNeighborhood.Area1 * Math.Sqrt(2) + PixelsNeighborhood.Area2 * 0 + PixelsNeighborhood.Area3 * 0 + PixelsNeighborhood.Area4 * -Math.Sqrt(2));
                    MaskValues.Mask5 = Math.Abs(PixelsNeighborhood.Area1 * 0 + PixelsNeighborhood.Area2 * Math.Sqrt(2) + PixelsNeighborhood.Area3 * -Math.Sqrt(2) + PixelsNeighborhood.Area4 * 0);


                    Max = Math.Max(MaskValues.Mask1, Math.Max(MaskValues.Mask2, Math.Max(MaskValues.Mask3, Math.Max(MaskValues.Mask4, MaskValues.Mask5))));

                    MaskValues.Mask1 = MaskValues.Mask1 / Max;
                    MaskValues.Mask2 = MaskValues.Mask2 / Max;
                    MaskValues.Mask3 = MaskValues.Mask3 / Max;
                    MaskValues.Mask4 = MaskValues.Mask4 / Max;
                    MaskValues.Mask5 = MaskValues.Mask5 / Max;


                    T = -1;

                    if (Max < T0)
                    {
                        Edges[0] = 0;
                        T = 0;
                    }
                    else
                    {
                        T = -1;

                        if (MaskValues.Mask1 > T1)
                        {
                            T++;
                            Edges[T] = 1;
                        }
                        if (MaskValues.Mask2 > T2)
                        {
                            T++;
                            Edges[T] = 2;
                        }
                        if (MaskValues.Mask3 > T2)
                        {
                            T++;
                            Edges[T] = 3;
                        }
                        if (MaskValues.Mask4 > T3)
                        {
                            T++;
                            Edges[T] = 4;
                        }
                        if (MaskValues.Mask5 > T3)
                        {
                            T++;
                            Edges[T] = 5;
                        }

                    }




                    for (int i = 0; i < (Step_Y * Step_X); i++)
                    {
                        MeanRed += CororRed[i];
                        MeanGreen += CororGreen[i];
                        MeanBlue += CororBlue[i];
                    }

                    MeanRed = Convert.ToInt32(MeanRed / (Step_Y * Step_X));
                    MeanGreen = Convert.ToInt32(MeanGreen / (Step_Y * Step_X));
                    MeanBlue = Convert.ToInt32(MeanBlue / (Step_Y * Step_X));

                    HSV = HSVConverter.ApplyFilter(MeanRed, MeanGreen, MeanBlue);



                    if (this.Compact == false)
                    {
                        Fuzzy10BinResultTable = Fuzzy10.ApplyFilter(HSV[0], HSV[1], HSV[2], 2);
                        Fuzzy24BinResultTable = Fuzzy24.ApplyFilter(HSV[0], HSV[1], HSV[2], Fuzzy10BinResultTable, 2);


                        for (int i = 0; i <= T; i++)
                        {
                            for (int j = 0; j < 24; j++)
                            {

                                if (Fuzzy24BinResultTable[j] > 0) CEDD[24 * Edges[i] + j] += Fuzzy24BinResultTable[j];

                            }

                        }
                    }
                    else
                    {

                        Fuzzy10BinResultTable = Fuzzy10.ApplyFilter(HSV[0], HSV[1], HSV[2], 2);

                        for (int i = 0; i <= T; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {

                                if (Fuzzy10BinResultTable[j] > 0) CEDD[10 * Edges[i] + j] += Fuzzy10BinResultTable[j];

                            }

                        }
                    }






                }

            }



            double Sum = 0;

            for (int i = 0; i < 144; i++)
            {


                Sum += CEDD[i];
            }

            for (int i = 0; i < 144; i++)
            {


                CEDD[i] = CEDD[i] / Sum;
            }

            CEDD = CEDDQuant.Apply(CEDD);

            return (CEDD);

        }


        /// <summary>
        /// Puts the colors to descriptor.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="theColors">The colors.</param>
        /// <returns></returns>
        private static byte[] putColorsToDescriptor(byte[] descriptorSource, List<System.Windows.Media.Color> theColors)
        {
            foreach (var c in theColors)
                descriptorSource = putColorToDescriptor(descriptorSource, c);
            return descriptorSource;
        }

        /// <summary>
        /// Puts the color to descriptor.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="theColor">The color.</param>
        /// <returns></returns>
        private static byte[] putColorToDescriptor(byte[] descriptorSource, System.Windows.Media.Color theColor)
        {
            double[] similarity = DescriptorColors.Select(x => Math.Sqrt(Math.Pow(x.R - theColor.R, 2) + Math.Pow(x.G - theColor.G, 2) + Math.Pow(x.B - theColor.B, 2))).ToArray();
            double[] maxThreeColors = similarity.OrderBy(x => x).Take(3).ToArray();

            //Anonymous function to put the color
            var putColor = new Func<int, int, byte[], byte[]>((colorIndex, value, descriptor) =>
            {
                for (int texture = 0; texture < 6; texture++)
                    descriptor[texture * 24 + colorIndex] += (byte)value;
                return descriptor;
            });

            //diffuse the colors
            var diffuseColors = new Func<int, byte[], byte[]>((index, des) =>
            {
                const int value = 3;
                switch (index)
                {
                    case 0:
                        putColor(1, value, des);
                        break;
                    case 1:
                        putColor(0, value, des);
                        putColor(2, value, des);
                        break;
                    case 2:
                        putColor(1, value, des);
                        break;
                    case 3:
                        putColor(4, value, des);
                        putColor(5, value, des);
                        break;
                    case 4:
                        putColor(3, value, des);
                        putColor(5, value, des);
                        break;
                    case 5:
                        putColor(4, value, des);
                        putColor(3, value, des);
                        break;
                    case 6:
                        putColor(7, value, des);
                        putColor(8, value, des);
                        break;
                    case 7:
                        putColor(6, value, des);
                        putColor(8, value, des);
                        break;
                    case 8:
                        putColor(6, value, des);
                        putColor(7, value, des);
                        break;
                    case 9:
                        putColor(10, value, des);
                        putColor(11, value, des);
                        break;
                    case 10:
                        putColor(9, value, des);
                        putColor(11, value, des);
                        break;
                    case 11:
                        putColor(9, value, des);
                        putColor(10, value, des);
                        break;
                    case 12:
                        putColor(13, value, des);
                        putColor(14, value, des);
                        break;
                    case 13:
                        putColor(12, value, des);
                        putColor(14, value, des);
                        break;
                    case 14:
                        putColor(12, value, des);
                        putColor(13, value, des);
                        break;
                    case 15:
                        putColor(16, value, des);
                        putColor(17, value, des);
                        break;
                    case 16:
                        putColor(15, value, des);
                        putColor(17, value, des);
                        break;
                    case 17:
                        putColor(15, value, des);
                        putColor(16, value, des);
                        break;
                    case 18:
                        putColor(20, value, des);
                        putColor(19, value, des);
                        break;
                    case 19:
                        putColor(18, value, des);
                        putColor(20, value, des);
                        break;
                    case 20:
                        putColor(18, value, des);
                        putColor(19, value, des);
                        break;
                    case 21:
                        putColor(22, value, des);
                        putColor(23, value, des);
                        break;
                    case 22:
                        putColor(21, value, des);
                        putColor(23, value, des);
                        break;
                    case 23:
                        putColor(21, value, des);
                        putColor(22, value, des);
                        break;
                }
                return des;
            });

            for (int i = 0; i < similarity.Length; i++)
            {
                if (similarity[i] == maxThreeColors[0])
                    descriptorSource = diffuseColors(i, putColor(i, 7, descriptorSource));
            }
            return descriptorSource;
        }

        /// <summary>
        /// Puts the textures to descriptor.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="theTextures">The textures.</param>
        /// <returns></returns>
        private static byte[] putTexturesToDescriptor(byte[] descriptorSource, List<TextureTypes> theTextures)
        {
            var putTextureToDescriptor = new Func<TextureTypes, int, byte[], byte[]>((texture, value, des) =>
            {
                for (int color = 0; color < 24; color++)
                    des[24 * (int)texture + color] += (byte)value;
                return des;
            });

            foreach (var t in theTextures)
                descriptorSource = putTextureToDescriptor(t, 7, descriptorSource);
            return descriptorSource;
        }


        #endregion
    }
}
