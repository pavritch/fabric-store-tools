using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using InsideStores.Imaging;
using InsideStores.Imaging.Descriptors;
using cbir;

//using Emgu.CV.Structure;
//using Emgu.CV;

namespace ImageTester
{
    class Program
    {
        #region Links and References
        // color tools
        // colormine.org + nuget package
        // https://github.com/THEjoezack/ColorMine


        // color palettes
        // ColorImageQuantizer Class - to make palette of N colors
        // http://www.aforgenet.com/framework/docs/html/f5216194-b2d6-be30-929a-dd91706d1173.htm
        
        #endregion


        static void Main(string[] args)
        {
            Console.WriteLine("Test Program Started.");

            var files = new string[] 
            {
                // all true
                @"c:\temp\rugmarket-e164b64d-2ce8-493a-ba27-7d06fa37331e.jpg", 
                @"c:\temp\rugmarket-db7b8451-841e-46ac-a01f-13aef4b45cba.jpg", 
                @"c:\temp\rugmarket-8c4f041e-538c-4c34-929e-a67f68e3cbce.jpg", 
                @"c:\temp\rugmarket-50f8cbb0-9c98-4d91-a1d5-a280745cfb4d.jpg", 

                // all false
                @"c:\temp\oval-0ee10a6c-a1ee-4d02-b705-245a2cf27845.jpg", 
                @"c:\temp\oval-7e9a73c8-158f-4e3d-860d-d440f4655350.jpg", 
                @"c:\temp\oval-88834452-0345-43c9-b52e-d5126d2aa015.jpg", 
                @"c:\temp\oval-32427d54-407c-4680-aa3b-a4d4efe85609.jpg", 
            };

            foreach (var file in files)
            {
                var hasCopyright = CheckForCopyrightNotice(file);
                var msg = string.Format("File {0} has copyright: {1}", file, hasCopyright.ToString());
                Debug.WriteLine(msg);
                Console.WriteLine(msg);
            }

#if false
            var files = new string[] 
            {
                @"c:\temp\oval-0ee10a6c-a1ee-4d02-b705-245a2cf27845.jpg", 
                @"c:\temp\oval-7e9a73c8-158f-4e3d-860d-d440f4655350.jpg", 
                @"c:\temp\oval-88834452-0345-43c9-b52e-d5126d2aa015.jpg", 
                @"c:\temp\oval-32427d54-407c-4680-aa3b-a4d4efe85609.jpg", 
            };

            foreach(var file in files)
            {
                var shape = FindShape(file);
                var msg = string.Format("Shape of {0} is: {1}", file, shape);
                Debug.WriteLine(msg);
                Console.WriteLine(msg);
            }
#endif

#if false
            //var domColorGen = new DomColorsGenerator();
            //domColorGen.Run(200);

            var phraseProcessor = new PhraseProcessor();
            //phraseProcessor.Run(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Data\CombinedListOfColors.txt", @"c:\temp\distinct-colors.txt");
            phraseProcessor.Run(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Data\CombinedListOfMaterials.txt", @"c:\temp\distinct-materials.txt");
            phraseProcessor.Run(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Data\CombinedListOfTags.txt", @"c:\temp\distinct-tags.txt");
            phraseProcessor.Run(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Data\CombinedListOfWeaves.txt", @"c:\temp\distinct-weaves.txt");
            phraseProcessor.Run(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Data\CombinedListOfBacking.txt", @"c:\temp\distinct-backing.txt");
#endif

            Console.WriteLine("Test Program ended.");
            return;



            //TestFeedbackDescriptor();
            #region Region 1

            //TestColorSearch();


            //FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            //var img = new System.Windows.Media.Imaging.BitmapImage();
            //img.BeginInit();
            //img.StreamSource = fileStream;
            //img.EndInit();

            //var filenames = new string[] {
            //    "SampeImageCompressionA-png24.png",
            //    "SampeImageCompressionA-jpg100.jpg",
            //    "SampeImageCompressionA-jpg95.jpg",
            //    "SampeImageCompressionA-jpg90.jpg",
            //    "SampeImageCompressionA-jpg80.jpg",
            //    "SampeImageCompressionA-jpg75.jpg",
            //    "SampeImageCompressionA-jpg50.jpg",
            //};

            //Func<string, string> makePath = (s) =>
            //    {
            //        return Path.Combine(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Samples", s);
            //    };


            //var imgPng100 = new BitmapImage(new Uri(makePath(filenames[0])));
            //var descriptorPng100 = calculateDescriptor(imgPng100);
            //foreach(var f in filenames)
            //{
            //    var img = new BitmapImage(new Uri(makePath(f)));
            //    var desc= calculateDescriptor(img);
            //    //Debug.WriteLine(string.Format("Distince({0}) = {1}", f, InsideStores.Imaging.Tanimoto.GetDistance(descriptorPng100, desc)));

            //    Debug.WriteLine(string.Format("DistinceEx({0}) = {1}", f, Distance(makePath(filenames[0]), makePath(f))));


            //}

            // InsideStores.Imaging.Tanimoto.GetDistance()
            //Distince(SampeImageCompression-png24.png) = 0
            //Distince(SampeImageCompression-jpg100.jpg) = 0.005239248
            //Distince(SampeImageCompression-jpg95.jpg) = 0.01123595
            //Distince(SampeImageCompression-jpg90.jpg) = 0
            //Distince(SampeImageCompression-jpg80.jpg) = 0.01037759
            //Distince(SampeImageCompression-jpg75.jpg) = 0.005682051
            //Distince(SampeImageCompression-jpg50.jpg) = 0.04691905

            // EuclidianDistance 150x150
            //DistinceEx(SampeImageCompression-png24.png) = 0
            //DistinceEx(SampeImageCompression-jpg100.jpg) = 314.677612803962
            //DistinceEx(SampeImageCompression-jpg95.jpg) = 470.786575849397
            //DistinceEx(SampeImageCompression-jpg90.jpg) = 692.344567394011
            //DistinceEx(SampeImageCompression-jpg80.jpg) = 1099.55763832552
            //DistinceEx(SampeImageCompression-jpg75.jpg) = 1450.47923115086
            //DistinceEx(SampeImageCompression-jpg50.jpg) = 4727.49087783361

            // EuclidianDistance 150x150
            //DistinceEx(SampeImageCompressionA-png24.png) = 0
            //DistinceEx(SampeImageCompressionA-jpg100.jpg) = 305.126203397873
            //DistinceEx(SampeImageCompressionA-jpg95.jpg) = 431.983795992396
            //DistinceEx(SampeImageCompressionA-jpg90.jpg) = 579.852567468663
            //DistinceEx(SampeImageCompressionA-jpg80.jpg) = 856.089948545128
            //DistinceEx(SampeImageCompressionA-jpg75.jpg) = 1029.21280598329
            //DistinceEx(SampeImageCompressionA-jpg50.jpg) = 1974.6655413006

            // Conclusion here is that Euclidean distance from jpg100 would be a good path to take. 
            // Not sure if can use percentages...


            //var filepath = @"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Samples\SampeImage001.jpg";
            //var imageBytes = filepath.ReadBinaryFile();
            //var bmp = imageBytes.FromImageByteArrayToBitmap();

            //ImageStatistics imageStats = new ImageStatistics(bmp);

            // needs review, does not clean up correctly
            //var avgColor = bmp.CalculateAverageColor();  


            // instantiate the images' color quantization class
            //ColorImageQuantizer ciq = new ColorImageQuantizer( new MedianCutQuantizer( ) );
            //// get 16 color palette for a given image
            //Color[] colorTable = ciq.CalculatePalette(bmp, 16);

            //MakeImageFeatures(filepath);

            //// create the color quantization algorithm
            //IColorQuantizer quantizer = new MedianCutQuantizer();
            //// process colors (taken from image for example)
            //for (int i = 0; i < pixelsToProcess; i++)
            //{
            //    quantizer.AddColor( /* pixel color */ );
            //}
            //// get palette reduced to 16 colors
            //Color[] palette = quantizer.GetPalette(16);
            
            #endregion
#if false
            // this works - but the Nuget package has been removed since not planning to need it

            // NOTES:
            // When installing Emgu 2.4.10 x86 from NuGet - although it did install the OpenCV files
            // in an x86 folder, it did not mark the project files as Copy Always (need to do that manually).

            // The VS2010  MSVCRT 10.0 is needed. If VS2010 has not been installed, deploy manually from these links.
            // Microsoft Visual C++ 2010 SP1 Redistributable Package (x86)
            // http://www.microsoft.com/en-us/download/details.aspx?id=5555
            // Microsoft Visual C++ 2010 SP1 Redistributable Package (x64)
            // https://www.microsoft.com/en-ca/download/details.aspx?id=13523

            // Emgu.CV
            float[] BlueHist;
            float[] GreenHist;
            float[] RedHist;

            Image<Bgr, Byte> img = new Image<Bgr, byte>(filepath);

            DenseHistogram Histo = new DenseHistogram(255, new RangeF(0, 255));

            Image<Gray, Byte> img2Blue = img[0];
            Image<Gray, Byte> img2Green = img[1];
            Image<Gray, Byte> img2Red = img[2];


            Histo.Calculate(new Image<Gray, Byte>[] { img2Blue }, true, null);
            //The data is here
            //Histo.MatND.ManagedArray
            BlueHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(BlueHist, 0);

            Histo.Clear();

            Histo.Calculate(new Image<Gray, Byte>[] { img2Green }, true, null);
            GreenHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(GreenHist, 0);

            Histo.Clear();

            Histo.Calculate(new Image<Gray, Byte>[] { img2Red }, true, null);
            RedHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(RedHist, 0);
#endif            


#if false
            var filepath2 = @"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Samples\SampeImage009.jpg";

            //Debug.WriteLine(string.Format("DistinceEx(file1, file2) = {0}", Distance(filepath, filepath2)));

            var image = new BitmapImage(new Uri(filepath));
            var started = DateTime.Now;

            var descriptor = calculateDescriptor(image);
            Debug.WriteLine(string.Format("Sum: {0}, time: {1}", descriptor.Sum(e => (int)e), DateTime.Now - started));


            var image1a = new BitmapImage(new Uri(filepath2));
            started = DateTime.Now;

            var descriptor1a = calculateDescriptor(image1a);
            Debug.WriteLine(string.Format("Distince(#2,#9) = {0}", InsideStores.Imaging.Tanimoto.GetDistance(descriptor, descriptor1a)));


            var started2 = DateTime.Now;
            var imageBytes = filepath.ReadBinaryFile();
            var bmp = imageBytes.FromImageByteArrayToBitmap();

            var descriptor2 = calculateDescriptor(bmp);
            Debug.WriteLine(string.Format("Sum2: {0}, time: {1}", descriptor2.Sum(e => (int)e), DateTime.Now - started2));

            var started3 = DateTime.Now;
            var bmpSrc3 = CreateBitmapSourceFromBitmap(bmp);
            var descriptor3 = calculateDescriptor(bmpSrc3);
            Debug.WriteLine(string.Format("Sum3: {0}, time: {1}", descriptor3.Sum(e => (int)e), DateTime.Now - started3));

            Debug.WriteLine(string.Format("Distince(1,3) = {0}", InsideStores.Imaging.Tanimoto.GetDistance(descriptor, descriptor3)));
            Debug.WriteLine(string.Format("Distince(1,2) = {0}", InsideStores.Imaging.Tanimoto.GetDistance(descriptor, descriptor2)));

            Debug.WriteLine(string.Format("Distince(2,3) = {0}", InsideStores.Imaging.Tanimoto.GetDistance(descriptor2, descriptor3)));

            Debug.WriteLine(string.Format("Distince(Bitmap,BitmapSource) = {0}", DistanceHelpers.Distance(bmp, image, DistanceHelpers.DistanceMeasurementMethod.Euclidean)));
#endif
            Debug.WriteLine("Done");

            #region Region 2

            // comparing descriptor distances using Tanimoto between bitmap vs bitmap source on 8 samples

            //1:
            //Distince(1,3) = 0.01257867
            //Distince(1,2) = 0.01257867
            //Distince(2,3) = 0


            //2:
            //Distince(1,3) = 0
            //Distince(1,2) = 0.007916331
            //Distince(2,3) = 0.007916331

            //3:
            //Distince(1,3) = 0
            //Distince(1,2) = 0.01233089
            //Distince(2,3) = 0.01233089

            //4:
            //Distince(1,3) = 0
            //Distince(1,2) = 0.009935379
            //Distince(2,3) = 0.009935379

            //5:
            //Distince(1,3) = 0
            //Distince(1,2) = 0
            //Distince(2,3) = 0

            //6:
            //Distince(1,3) = 0
            //Distince(1,2) = 0.01055211
            //Distince(2,3) = 0.01055211


            //7:
            //Distince(1,3) = 0
            //Distince(1,2) = 0
            //Distince(2,3) = 0

            //8:
            //Distince(1,3) = 0
            //Distince(1,2) = 0.01441729
            //Distince(2,3) = 0.01441729

            //Distince(#1,#2) = 0.8698527
            //Distince(#1,#3) = 0.7988077
            //Distince(#1,#4) = 0.4392869
            //Distince(#2,#4) = 0.9232903
            //Distince(#5,#7) = 1
            //Distince(#8,#3) = 0.948727
            //Distince(#3,#6) = 0.786884
            //Distince(#2,#9) = 0.9311521

            // comparing distance from when bytes come from straight BMP or through BitmapSource

            //Euclidean distance on 8 samples
            //1=Distince(Bitmap,BitmapSource) =  88200.3117851632
            //2=Distince(Bitmap,BitmapSource) = 143399.165175394
            //3=Distince(Bitmap,BitmapSource) =  18713.9843165479
            //4=Distince(Bitmap,BitmapSource) =  59024.4701712773
            //5=Distince(Bitmap,BitmapSource) = 137677.236916638
            //6=Distince(Bitmap,BitmapSource) = 183426.266941243
            //7=Distince(Bitmap,BitmapSource) =  66038.2035567292
            //8=Distince(Bitmap,BitmapSource) = 153897.71631509

            // Tanimoto distance on 8 samples
            //1=Distince(Bitmap,BitmapSource) = 0.126613914966583
            //2=Distince(Bitmap,BitmapSource) = 0.274705111980438
            //3=Distince(Bitmap,BitmapSource) = 0.00208741426467896
            //4=Distince(Bitmap,BitmapSource) = 0.207042276859283
            //5=Distince(Bitmap,BitmapSource) = 0.406567096710205
            //6=Distince(Bitmap,BitmapSource) = 0.416404545307159
            //7=Distince(Bitmap,BitmapSource) = 0.577146172523499
            //8=Distince(Bitmap,BitmapSource) = 0.382739245891571
            
            #endregion

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }


        public static bool CheckForCopyrightNotice(string filepath)
        {
            var CroppedImage = filepath.ReadBinaryFile();

            using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
            {
                var left = (int)(bmp.Width * .07);
                var top = (int)(bmp.Height * .02);

                var stripeWidth = (int)(bmp.Width * .05);
                var stripeHeight = (int)(bmp.Height * .95);

                var r = new Rectangle(left, top, stripeWidth, stripeHeight);

                return bmp.HasEmbeddedWhiteRectangle(r);
            }
        }


        public static string FindShape(string filepath)
        {
            string Shape = "Unknown";

            var CroppedImage = filepath.ReadBinaryFile();

            int CroppedWidth;
            int CroppedHeight;

            using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
            {
                CroppedWidth = bmp.Width;
                CroppedHeight = bmp.Height;
            }

            // for any remaining shapes (Rectangular, Square, Oval, Round, Runner), perform
            // some tests to really try to get it right

            // the key here is in the borders

            Func<bool> hasGenerallySquareDimensions = () =>
            {
                var ratio = (double)CroppedWidth / (double)CroppedHeight;
                return Math.Abs(ratio - 1.0) < .05;
            };

            Func<bool> hasGenerallyRunnerDimensions = () =>
            {
                var ratio = (double)CroppedWidth / (double)CroppedHeight;
                return ratio <= .45;
            };

            Func<bool> hasGenerallyRectangularDimensions = () =>
            {
                var ratio = (double)CroppedWidth / (double)CroppedHeight;
                return ratio >= .5;
            };

            if (hasGenerallySquareDimensions())
            {
                // could be square or round, determined by white corners

                using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                {
                    // the actual touching square corner would be closer to .14, but we'll trim that
                    // back to be extra safe.

                    int cornerSize = (int)(CroppedWidth * .10);
                    var isRound = ExtensionMethods.HasWhiteSpaceAroundImage(bmp, cornerSize, 0.998f);

                    Shape = isRound ? "Round" : "Square";
                    return Shape;
                }
            }

            if (hasGenerallyRunnerDimensions())
            {
                // likely a runner
                // for sure, anything with a ratio of under .42 seems to always be a runner
                // so for now, the test is assumed to be pretty accurate
                Shape = "Runner";
                return Shape;
            }

            if (hasGenerallyRectangularDimensions())
            {
                // oval or rectangular, determined by white corners

                using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                {
                    int cornerSize = (int)(CroppedWidth * .08);
                    var isOval = ExtensionMethods.HasWhiteSpaceAroundImage(bmp, cornerSize, 0.998f, 0.90f, 3);

                    Shape = isOval ? "Oval" : "Rectangular";
                    return Shape;
                }
            }

            return Shape;
        }


        public static void TestTanimoto()
        {
            Console.WriteLine("TestTanimoto - Start");

            var rnd = new Random();
            var testData = new List<byte[]>();

            // create 1000 test arrays
            for(int i=0; i < 1000; i++)
            {
                var el = new byte[144];
                for (int j = 0; j < el.Length; j++)
                    el[j] = (byte)rnd.Next(0, 100);
                testData.Add(el);
            }

            for(int k=0; k < 10000; k++)
            {
                var indexA = rnd.Next(0, testData.Count());
                var indexB = rnd.Next(0, testData.Count());

                var dataA = testData[indexA];
                var dataB = testData[indexB];

                var dataAfloat = dataA.Select(e => (float) e).ToArray();
                var dataBfloat = dataB.Select(e => (float) e).ToArray();

                var dist1 = TanimotoDistance.GetDistance(dataA, dataB);

                var td = new TanimotoDistance(dataAfloat);
                var dist2 = td.GetDistance(dataB);

                var dist3 = TanimotoDistance.GetDistance(dataAfloat, dataB);

                Debug.Assert(dist1 == dist2);

                Debug.Assert(dist1 == dist3);
            }

            Console.WriteLine("TestTanimoto - Done");
        }


        public static void CompareTanimoto()
        {
            Console.WriteLine("TestTanimoto - Start");

            var rnd = new Random();
            var testData = new List<byte[]>();

            // create 1000 test arrays
            for (int i = 0; i < 1000; i++)
            {
                var el = new byte[144];
                for (int j = 0; j < el.Length; j++)
                    el[j] = (byte)rnd.Next(0, 100);
                testData.Add(el);
            }

            for (int k = 0; k < 100000; k++)
            {
                var indexA = rnd.Next(0, testData.Count());
                var indexB = rnd.Next(0, testData.Count());

                var dataA = testData[indexA];
                var dataB = testData[indexB];

                var dataAfloat = dataA.Select(e => (float)e).ToArray();

                var r = new retrieval(dataAfloat);
                var td = new TanimotoDistance(dataAfloat);


                var dist1 = td.GetDistance(dataB);
                var dist2 = r.howSimilar(dataB);

                Debug.Assert(dist1 == dist2);

            }

            Console.WriteLine("TestTanimoto - Done");
        }

        public static void CompareDescriptors()
        {
            var filenames = new string[] {
                "SampeImage001.jpg",
                "SampeImage002.jpg",
                "SampeImage003.jpg",
                "SampeImage004.jpg",
                "SampeImage005.jpg",
                "SampeImage006.jpg",
                "SampeImage007.jpg",
                "SampeImage008.jpg",
                "SampeImage009.jpg",
            };

            Func<string, string> makePath = (s) =>
                {
                    return Path.Combine(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Samples", s);
                };

            Func<BitmapSource, byte[]> zCalculateDescriptor = (myImage) =>
            {
                CEDD_Descriptor.CEDD myCEDD = new CEDD_Descriptor.CEDD();
                return myCEDD.getDescriptor(myImage);
            };

            Func<BitmapSource, byte[]> pCalculateDescriptor = (myImage) =>
            {
                var cedd = myImage.CalculateDescriptor();
                return cedd;
            };

            foreach(var filename in filenames)
            {
                var filepath = makePath(filename);

                BitmapSource myImage = new BitmapImage(new Uri(filepath));

                var zDescriptor = zCalculateDescriptor(myImage);

                var pDescriptor = pCalculateDescriptor(myImage);

                Debug.Assert(zDescriptor.Sum(e => e) == pDescriptor.Sum(e => e));

                var distance = TanimotoDistance.GetDistance(zDescriptor, pDescriptor);

                Debug.WriteLine(string.Format("Distance for {0}: {1}", filename, distance));
            }
        }


        public static void CompareDescriptors2()
        {

            var filenames = Directory.GetFiles(@"D:\InsideRugs-Dev\images\product\icon", "*.jpg");

            Debug.WriteLine(string.Format("Found {0:N0} files.", filenames.Count()));

            Func<BitmapSource, byte[]> zCalculateDescriptor = (myImage) =>
            {
                CEDD_Descriptor.CEDD myCEDD = new CEDD_Descriptor.CEDD();
                return myCEDD.getDescriptor(myImage);
            };

            Func<BitmapSource, byte[]> pCalculateDescriptor = (myImage) =>
            {
                var cedd = myImage.CalculateDescriptor();
                return cedd;
            };


            //Func<Bitmap, byte[]> pCalculateDescriptorBMP = (myImage) =>
            //{
            //    var cedd = myImage.CalculateDescriptor();
            //    return cedd;
            //};


            int count = 0;

            //byte[] refDescriptor = zCalculateDescriptor(new BitmapImage(new Uri(filenames[0]))); ;

            foreach (var filepath in filenames)
            {
                var filename = Path.GetFileName(filepath);

                var mediumPath = Path.Combine(@"D:\InsideRugs-Dev\images\product\small", filename);

                BitmapSource myImage = new BitmapImage(new Uri(filepath));
                BitmapSource myImageMedium = new BitmapImage(new Uri(mediumPath));

                //Bitmap bmpImage = new Bitmap(filepath);

                var zDescriptor = zCalculateDescriptor(myImage);
                var zDescriptorMedium = zCalculateDescriptor(myImageMedium);

                //var pDescriptor = pCalculateDescriptor(myImage);
                //var pDescriptorBMP = pCalculateDescriptorBMP(bmpImage);


                var distance = TanimotoDistance.GetDistance(zDescriptor, zDescriptorMedium);

                Debug.WriteLine(string.Format("Distance for {0}: {1}", filename, distance));

                count++;

                if (count == 1000)
                    break;
            }
        }

        /// <summary>
        /// Transforms to texture.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <returns></returns>
        private static byte[] transformToTexture(byte[] descriptorSource)
        {
            // new 6-bin histogram
            byte[] textureHistogram = new byte[6];
            for (int color = 0; color < 24; color++)
                for (int texture = 0; texture < 6; texture++)
                    textureHistogram[texture] += descriptorSource[texture * 24 + color];
            return textureHistogram;
        }



        /// <summary>
        /// Transforms to color.
        /// </summary>
        /// <param name="desciptorSource">The descriptor source.</param>
        /// <returns></returns>
        private static byte[] transformToColor(byte[] desciptorSource)
        {
            //new 24-bin histogram
            byte[] colorHistogram = new byte[24];
            /*   0 - White
             *   1 - Gray
             *   2 - Black
             *   3 - Light Red
             *   4 - Dark Red
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
             *   17  - Dark Cyan
             *   18 - Light Blue
             *   19 - Blue
             *   20 - Dark blue
             *   21 - Light Magenta
             *   22 - Magenta
             *   23 - Dark Magenta
             */
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 24; j++)
                {
                    colorHistogram[j] += desciptorSource[i * 24 + j];
                }
            return colorHistogram;
        }


        private static void CompareImagesFromSamePattern()
        {

            //select ProductID, Imagefilenameoverride from Product where ManufacturerPartNumber = 
            //(select ManufacturerPartNumber from Product where SKU = 'SV-CHT720A')

            var filenames = new string[] {
                "44cf6745-5063-5784-9bf4-9b66358eabec.jpg",
                "082bf0c5-3881-598f-843c-f1f645c37c58.jpg",
                "a49b617c-1dfa-5323-a663-2ffa87195f44.jpg",
                "a1684ec2-e6b1-5a92-b53f-6922492ebb8e.jpg",
                "e5d00873-90d4-52df-8e69-b3d120f39c73.jpg",
                "fab85542-b7ce-5064-9429-ae2b662d9c2e.jpg",
                "3d870c08-9584-5cea-abbc-c29c22717a48.jpg",
                "a23c77e4-1c5d-5a9f-87c6-e03b5f2f6c07.jpg",
            };

            Func<BitmapSource, byte[]> zCalculateDescriptor = (myImage) =>
            {
                CEDD_Descriptor.CEDD myCEDD = new CEDD_Descriptor.CEDD();
                return myCEDD.getDescriptor(myImage);
            };

            Func<string, string> makePath = (s) =>
                {
                    return Path.Combine(@"D:\InsideRugs-Dev\images\product\medium", s);
                };

            var descriptors = new List<byte[]>();

            foreach(var filename in filenames)
            {
                BitmapSource myImage = new BitmapImage(new Uri(makePath(filename)));
                var cedd = zCalculateDescriptor(myImage);

                var texture = transformToTexture(cedd);
                var color = transformToColor(cedd);

                descriptors.Add(texture);
            }

            for(int i=0; i < descriptors.Count(); i++)
            {
                var dist = TanimotoDistance.GetDistance(descriptors[0], descriptors[i]);
                Debug.WriteLine(string.Format("Distance: {0}", dist));
            }
        }


        private static void TestFeedbackDescriptor()
        {

            //select ProductID, Imagefilenameoverride from Product where ManufacturerPartNumber = 
            //(select ManufacturerPartNumber from Product where SKU = 'SV-CHT720A')

            var filenames = new string[] {
                "44cf6745-5063-5784-9bf4-9b66358eabec.jpg",
                "082bf0c5-3881-598f-843c-f1f645c37c58.jpg",
                "a49b617c-1dfa-5323-a663-2ffa87195f44.jpg",
                "a1684ec2-e6b1-5a92-b53f-6922492ebb8e.jpg",
                "e5d00873-90d4-52df-8e69-b3d120f39c73.jpg",
                "fab85542-b7ce-5064-9429-ae2b662d9c2e.jpg",
                "3d870c08-9584-5cea-abbc-c29c22717a48.jpg",
                "a23c77e4-1c5d-5a9f-87c6-e03b5f2f6c07.jpg",
            };

            Func<BitmapSource, byte[]> zCalculateDescriptor = (myImage) =>
            {
                CEDD_Descriptor.CEDD myCEDD = new CEDD_Descriptor.CEDD();
                return myCEDD.getDescriptor(myImage);
            };

            Func<byte[], float[]> feedbackDescriptor = (cedd) =>
            {
                var arf = new cbir.AutomaticRelevanceFeedback(cedd);
                return arf.GetNewDescriptor();
            };

            Func<string, string> makePath = (s) =>
            {
                return Path.Combine(@"D:\InsideRugs-Dev\images\product\medium", s);
            };

            var descriptors = new List<byte[]>();

            foreach (var filename in filenames)
            {
                BitmapSource myImage = new BitmapImage(new Uri(makePath(filename)));
                var cedd = zCalculateDescriptor(myImage);
                var arf = feedbackDescriptor(cedd);

                var td = new TanimotoDistance(arf);
                var dist = td.GetDistance(cedd);

                Debug.WriteLine(string.Format("Distance: {0}", dist));
            }

        }



        #region Test ColorMine

        public static void TestColorSearch()
        {
            Console.WriteLine("Data preparation.");
            // setup - create bunch of random RGB values

            var itemCount = 100000;

            var list = new List<Lab>();


            var rnd = new Random();


            Func<System.Windows.Media.Color> makeRandomColor = () =>
                {
                    var r = (byte)rnd.Next(0, 256);
                    var g = (byte)rnd.Next(0, 256);
                    var b = (byte)rnd.Next(0, 256);
                    return System.Windows.Media.Color.FromArgb(0xFF, r, g, b);
                };


            Func<Lab> makeRandomLabColor = () =>
                {
                    var r = (byte)rnd.Next(0, 256);
                    var g = (byte)rnd.Next(0, 256);
                    var b = (byte)rnd.Next(0, 256);

                    var rgb = new ColorMine.ColorSpaces.Rgb() { R = r, G = g, B = g };
                    var lab = rgb.To<Lab>();

                    return lab;
                };

            for (int i = 0; i < itemCount; i++)
                list.Add(makeRandomLabColor());

            var targets = new List<Lab>();
            for (int i = 0; i < 100000; i++)
                targets.Add(makeRandomLabColor());


            Console.WriteLine("Begin test.");

            var dtStart = DateTime.Now;

            var results = new List<double>();

            var lockObj = new object();

            double maxDistance = 0.0;

            for (int j = 0; j < targets.Count(); j++)
            {
                var c1 = targets[j];

                Parallel.ForEach(list, (item) =>
                {
                    var distance = c1.Compare(item, new Cie1976Comparison());
                    if (distance > maxDistance)
                    {
                        lock (lockObj)
                        {
                            maxDistance = distance;
                        }
                    }
                });
            }

            //for (int j = 0; j < targets.Count(); j++)
            //{
            //    var c1 = targets[j];

            //    Parallel.ForEach(list, (item) => {

            //        var distance = c1.Compare(item, new Cie1976Comparison());

            //        lock (lockObj)
            //        {
            //            results.Add(distance);
            //        }
            //    });
            //}

            //var sorted = results.AsParallel().OrderBy(e => e);

            var dtEnd = DateTime.Now;
            Console.WriteLine("end test.");
            Console.WriteLine(string.Format("Max Distance = {0}", maxDistance));
            Console.WriteLine(string.Format("Time to process: {0}", dtEnd - dtStart));
            //Console.WriteLine(string.Format("Time to process {0:N0} items: {1}", results.Count(), dtEnd - dtStart));
            Console.ReadLine();
        }

        
        #endregion

        #region Static Helpers

        /// <summary>
        /// Euclidian distance.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static double EuclidianDistance(byte[] vector1, byte[] vector2)
        {
            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow((double)vector1[i] - (double)vector2[i], 2);
            }
            return Math.Sqrt(value);
        }

        /// <summary>
        /// Create an image features property set for the given filename.
        /// </summary>
        /// <remarks>
        /// The file is expected to already exist in the suite of various-sized folders.
        /// Will prefer to use the 800x (large) image.
        /// </remarks>
        /// <returns></returns>
        private static void MakeImageFeatures(string filepath)
        {
            try
            {
                var filename = Path.GetFileName(filepath);
                var startTime = DateTime.Now;

                if (!File.Exists(filepath))
                    return;

                var imageBytes = filepath.ReadBinaryFile();
                var bmp = imageBytes.FromImageByteArrayToBitmap();
                var bmsrc = bmp.ToBitmapSource();
                var cedd = bmsrc.CalculateDescriptor();
                var tinyCEDD = TinyDescriptors.MakeTinyDescriptor(cedd);

                var filepath2 = @"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Samples\SampeImageCompression-jpg90.jpg";
                var imageBytes2 = filepath2.ReadBinaryFile();
                var bmp2 = imageBytes2.FromImageByteArrayToBitmap();
                var bmsrc2 = bmp2.ToBitmapSource();
                var domColors = DominantColors.GetDominantColorsSlow(bmsrc2);

                var cedd2 = bmsrc2.CalculateDescriptor();
                var tinyCEDD2 = TinyDescriptors.MakeTinyDescriptor(cedd2);
                var dist2 = TinyDescriptors.Distance(tinyCEDD, tinyCEDD2);

                var fcth = new FCTH().MakeDescriptor(bmp);
                //var jcd = JCD.MakeJointDescriptor(cedd.Cast<double>().ToArray<double>(), fcth.Cast<double>().ToArray<double>());

                var endTime = DateTime.Now;
                Debug.WriteLine(string.Format("Image features for {0}: {1}", filename, endTime - startTime));


            }
            catch
            {
            }
        }




        /// <summary>
        /// Calculates the image descriptor.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        public static byte[] calculateDescriptor(BitmapSource myImage)
        {
            CEDD myCEDD = new CEDD();
            return myCEDD.MakeDescriptor(myImage);
        }

        /// <summary>
        /// Calculates the image descriptor.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        //public static byte[] calculateDescriptor(Bitmap myImage)
        //{
        //    CEDD myCEDD = new CEDD();
        //    return myCEDD.MakeDescriptor(myImage);
        //}

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            if (Dispatcher.CurrentDispatcher == null)
                return null; // Is it possible?

            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // You need to specify the image format to fill the stream. 
                    // I'm assuming it is PNG
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread
                    if (InvokeRequired)
                        return (BitmapSource)Dispatcher.CurrentDispatcher.Invoke(
                            new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
                            DispatcherPriority.Normal,
                            memoryStream);

                    return CreateBitmapSourceFromBitmap(memoryStream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool InvokeRequired
        {
            get { return false; }
        }

        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();

            return writable;
        }
        
        #endregion
    }
}
