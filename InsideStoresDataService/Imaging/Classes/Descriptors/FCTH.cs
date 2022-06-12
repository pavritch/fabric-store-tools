
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace InsideStores.Imaging.Descriptors
{


    /// <summary>
    /// Invert an image
    /// </summary>
    public class FCTH
    {

        /// <summary>
        /// Correlation filter
        /// </summary>
        public class variables
        {

            // Constructor

            public struct WaveletMatrix
            {
                public double F1;
                public double F2;
                public double F3;

            }

            public struct WaveletMatrixPlus
            {
                public double F1;
                public double F2;
                public double F3;
                public double Entropy;

            }

            public struct SpatiaWaveletMatrix
            {
                public double X;
                public double Y;
                public double Z0;
                public double Z1;
                public double Z2;

            }


        }

        // Method   0 = LOM
        //          1 = Multi Equal Participate
        //          2 = Multi Participate

        public int  Method = 2; // review of PDF by Savvas and LIRE code shows this to be the best setting.
        public bool Compact = false;

        // Constructor
        public FCTH()
        {


        }


        // Apply filter
        public byte[] MakeDescriptor(Bitmap srcImg)
        {
            return (
                from x in (IEnumerable<double>)this.Apply(srcImg, 2)
                select Convert.ToByte(x)).ToArray<byte>();
        }

        // Apply filter
        private double[] Apply(Bitmap srcImg, int Method)
        {
            Fuzzy10Bin Fuzzy10 = new Fuzzy10Bin(false);
            Fuzzy24Bin Fuzzy24 = new Fuzzy24Bin(false);
            FuzzyFCTHpart FuccyFCTH = new FuzzyFCTHpart();


            double[] Fuzzy10BinResultTable = new double[10];
            double[] Fuzzy24BinResultTable = new double[24];
            double[] FuzzyHistogram192 = new double[192];


            this.Method = Method;
            int width = srcImg.Width;
            int height = srcImg.Height;
           

            for (int R = 0; R < 192; R++)
            {
                FuzzyHistogram192[R] = 0;

            }


            RGB2HSV HSVConverter = new RGB2HSV();
            int[] HSV = new int[3];

            variables.WaveletMatrixPlus Matrix = new variables.WaveletMatrixPlus();


            PixelFormat fmt = (srcImg.PixelFormat == PixelFormat.Format8bppIndexed) ?
                        PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;

            BitmapData srcData = srcImg.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, fmt);

          

            int offset = srcData.Stride - ((fmt == PixelFormat.Format8bppIndexed) ? width : width * 3);
           // int offsetBnr = BnrData.Stride - ((fmt == PixelFormat.Format8bppIndexed) ? BnrData.Width : BnrData.Width * 3);

            double[,] ImageGrid = new double[width, height];
            int[,] ImageGridRed = new int[width, height];
            int[,] ImageGridGreen = new int[width, height];
            int[,] ImageGridBlue = new int[width, height];
            double[,] WaveletImageGrid = new double[width, height];


            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();
               

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src += 3)
                    {


                        int mean = (int)(0.114 * src[0] + 0.587 * src[1] + 0.299 * src[2]);


                        ImageGrid[x, y] = mean;
                        ImageGridRed[x, y] = (int)src[2];
                        ImageGridGreen[x, y] = (int)src[1];
                        ImageGridBlue[x, y] = (int)src[0];


                    }

                    src += offset;
                 

                }

            }

            srcImg.UnlockBits(srcData);
         
            // Allages
            int NumberOfBlocks = 1600; 
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


            if (Step_Y < 4) Step_Y = 4;
            if (Step_X < 4) Step_X = 4;
            ///
            // Filter

            for (int y = 0; y < height - Step_Y; y += Step_Y)
            {
                for (int x = 0; x < width - Step_X; x += Step_X)
                {
                    int[,] BinaryBlock = new int[4, 4];
                    double[,] Block = new double[4, 4];
                    int[,] BlockR = new int[4, 4];
                    int[,] BlockG = new int[4, 4];
                    int[,] BlockB = new int[4, 4];
                    int[,] BlockCount = new int[4, 4];

                    int[] CororRed = new int[Step_Y * Step_X];
                    int[] CororGreen = new int[Step_Y * Step_X];
                    int[] CororBlue = new int[Step_Y * Step_X];

                    int[] CororRedTemp = new int[Step_Y * Step_X];
                    int[] CororGreenTemp = new int[Step_Y * Step_X];
                    int[] CororBlueTemp = new int[Step_Y * Step_X];
                   
                    int MeanRed = 0;
                    int MeanGreen = 0;
                    int MeanBlue = 0;
                    int CurrentPixelX = 0;
                    int CurrentPixelY = 0;
                    // an tha ginei allagh telika olh h douleia mpenei edo

                    #region 
                    for (int i = 0; i < 4; i++)
                    {

                        for (int j = 0; j < 4; j++)
                        {
                            Block[i, j] = 0;
                            BlockCount[i, j] = 0;
                        }
                    } 
                    #endregion

                    int TempSum = 0;
                    for (int i = 0; i < Step_X; i++)
                    {
                        for (int j = 0; j < Step_Y; j++)
                        {
                            CurrentPixelX = 0;
                            CurrentPixelY = 0;
            
                            if (i >=  (Step_X / 4)) CurrentPixelX = 1;
                            if (i >=  (Step_X / 2)) CurrentPixelX = 2;
                            if (i >= (3 * Step_X / 4)) CurrentPixelX = 3;                            
                           
                            if (j >= (Step_Y / 4)) CurrentPixelY = 1;
                            if (j >= (Step_Y / 2)) CurrentPixelY = 2;
                            if (j >= (3 * Step_Y / 4)) CurrentPixelY = 3;

                            Block[CurrentPixelX, CurrentPixelY] += ImageGrid[x + i, y + j] ;
                            BlockCount[CurrentPixelX, CurrentPixelY]++;

                            BlockR[CurrentPixelX, CurrentPixelY] =  ImageGridRed[x + i, y + j] ;
                            BlockG[CurrentPixelX, CurrentPixelY] =  ImageGridGreen[x + i, y + j] ;
                            BlockB[CurrentPixelX, CurrentPixelY] =  ImageGridBlue[x + i, y + j] ;

                            CororRed[TempSum] = BlockR[CurrentPixelX, CurrentPixelY];
                            CororGreen[TempSum] = BlockG[CurrentPixelX, CurrentPixelY];
                            CororBlue[TempSum] = BlockB[CurrentPixelX, CurrentPixelY];

                            CororRedTemp[TempSum] = BlockR[CurrentPixelX, CurrentPixelY];
                            CororGreenTemp[TempSum] = BlockG[CurrentPixelX, CurrentPixelY];
                            CororBlueTemp[TempSum] = BlockB[CurrentPixelX, CurrentPixelY];


                            TempSum++;
                        }
                    }


                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            Block[i, j] = Block[i, j] / BlockCount[i, j];
                        }
                    }

                    Matrix = singlePassThreshold(Block, 1);


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

                    if (Compact == false)
                    {
                        Fuzzy10BinResultTable = Fuzzy10.ApplyFilter(HSV[0], HSV[1], HSV[2], Method);
                        Fuzzy24BinResultTable = Fuzzy24.ApplyFilter(HSV[0], HSV[1], HSV[2], Fuzzy10BinResultTable, Method);
                        FuzzyHistogram192 = FuccyFCTH.ApplyFilter(Matrix.F3, Matrix.F2, Matrix.F1, Fuzzy24BinResultTable, Method,24);

                    }
                    else
                    {
                        Fuzzy10BinResultTable = Fuzzy10.ApplyFilter(HSV[0], HSV[1], HSV[2], Method);
                        FuzzyHistogram192 = FuccyFCTH.ApplyFilter(Matrix.F3, Matrix.F2, Matrix.F1, Fuzzy10BinResultTable, Method,10);

                    }

                }


            }


            // end of the filter
            double TotalSum = 0;

            for (int i = 0; i < 192; i++)
            {


                TotalSum += FuzzyHistogram192[i];


            }

            for (int i = 0; i < 192; i++)
            {


                FuzzyHistogram192[i] = FuzzyHistogram192[i] / TotalSum;


            }

            FuzzyHistogram192 = FCTHQuant.Apply(FuzzyHistogram192);

            return FuzzyHistogram192;



        }

        private variables.WaveletMatrixPlus singlePassThreshold(double[,] inputMatrix, int level)
        {


            variables.WaveletMatrixPlus TempMatrix;
            level = (int)Math.Pow(2.0, level - 1);



            double[,] resultMatrix = new double[inputMatrix.GetLength(0), inputMatrix.GetLength(1)];

            int xOffset = inputMatrix.GetLength(0) / 2 / level;

            int yOffset = inputMatrix.GetLength(1) / 2 / level;

            int currentPixel = 0;

            double size = inputMatrix.GetLength(0) * inputMatrix.GetLength(1);

            double multiplier = 0;



            for (int y = 0; y < inputMatrix.GetLength(1); y++)
            {

                for (int x = 0; x < inputMatrix.GetLength(0); x++)
                {

                    if ((y < inputMatrix.GetLength(1) / 2 / level) && (x < inputMatrix.GetLength(0) / 2 / level))
                    {

                        currentPixel++;

                        resultMatrix[x, y] = (inputMatrix[2 * x, 2 * y] + inputMatrix[2 * x + 1, 2 * y] + inputMatrix[2 * x, 2 * y + 1] + inputMatrix[2 * x + 1, 2 * y + 1]) / 4;

                        double vertDiff = (-inputMatrix[2 * x, 2 * y] - inputMatrix[2 * x + 1, 2 * y] + inputMatrix[2 * x, 2 * y + 1] + inputMatrix[2 * x + 1, 2 * y + 1]);

                        double horzDiff = (inputMatrix[2 * x, 2 * y] - inputMatrix[2 * x + 1, 2 * y] + inputMatrix[2 * x, 2 * y + 1] - inputMatrix[2 * x + 1, 2 * y + 1]);

                        double diagDiff = (-inputMatrix[2 * x, 2 * y] + inputMatrix[2 * x + 1, 2 * y] + inputMatrix[2 * x, 2 * y + 1] - inputMatrix[2 * x + 1, 2 * y + 1]);




                        resultMatrix[x + xOffset, y] = (int)(byte)(multiplier + Math.Abs(vertDiff));

                        resultMatrix[x, y + yOffset] = (int)(byte)(multiplier + Math.Abs(horzDiff));

                        resultMatrix[x + xOffset, y + yOffset] = (int)(byte)(multiplier + Math.Abs(diagDiff));




                    }

                    else
                    {

                        if ((x >= inputMatrix.GetLength(0) / level) || (y >= inputMatrix.GetLength(1) / level))

                        { resultMatrix[x, y] = inputMatrix[x, y]; }

                    }

                }

            }

            double Temp1 = 0;
            double Temp2 = 0;
            double Temp3 = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Temp1 += 0.25 * Math.Pow(resultMatrix[2 + i, j], 2);
                    Temp2 += 0.25 * Math.Pow(resultMatrix[i, 2 + j], 2);
                    Temp3 += 0.25 * Math.Pow(resultMatrix[2 + i, 2 + j], 2);




                }

            }

            double[] MatrixResults = new double[4];
           

            TempMatrix.F1 = Math.Sqrt(Temp1);
            TempMatrix.F2 = Math.Sqrt(Temp2);
            TempMatrix.F3 = Math.Sqrt(Temp3);

            TempMatrix.Entropy  = 0;

            return TempMatrix;

        }

    }
}
