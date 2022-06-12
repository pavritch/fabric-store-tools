using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;



namespace cbir
{
    /// <summary>
    /// The Texture types 
    /// </summary>
    public enum TextureTypes {
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
        Diagonal_135Degree }

    /// <summary>
    /// Information about the image results
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// Gets or sets the image ID.
        /// </summary>
        /// <value>
        /// The image ID.
        /// </value>
        public int imageID { get; set; }
        /// <summary>
        /// Gets or sets the image similarity.
        /// </summary>
        /// <value>
        /// The image similarity.
        /// </value>
        public float similarity { get; set; }
        /// <summary>
        /// Gets or sets the image group ID.
        /// </summary>
        /// <value>
        /// The image group ID.
        /// </value>
        public int groupID { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInfo"/> class.
        /// </summary>
        /// <param name="imageID">The image ID.</param>
        /// <param name="similarity">The image similarity.</param>
        /// <param name="groupID">The image group ID.</param>
        public ImageInfo(int imageID, float similarity = 0, int groupID = 0)
        {
            this.imageID = imageID;
            this.similarity = similarity;
            this.groupID = groupID;
        }
    }

    /// <summary>
    /// The main operations class
    /// </summary>
    public class cbirMain
    {


        /// <summary>
        /// The history of the retrieval searches. Clear to reset them.
        /// </summary>
        public List<byte[]> History = new List<byte[]>();
        /// <summary>
        /// Enables the multi thread support
        /// </summary>
        public bool multiThread = true;

        /// <summary>
        /// Gets the retrieval results.
        /// </summary>
        /// <value>
        /// The retrieval results.
        /// </value>
        public ImageInfo[] getResults { get { return results; } }

        /// <summary>
        /// The original image data
        /// </summary>
        private Dictionary<int, byte[]> imgData;

        /// <summary>
        /// The final results
        /// </summary>
        private ImageInfo[] results;



        /// <summary>
        /// Initializes a new instance of the <see cref="cbirMain"/> class.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="multiThread">Enable or disable the multi thread operations.</param>
        public cbirMain(Dictionary<int, byte[]> imageData, bool multiThread = true)
        {
            this.imgData = imageData;
            results = new ImageInfo[imageData.Count()];
            int[] keys = imageData.Keys.ToArray();

            for (int i = 0; i < results.Length;i++ )
                results[i] = new ImageInfo(keys[i]);
            this.multiThread = multiThread;
        }

        #region Public Functions


        /// <summary>
        /// Calculates the image descriptor.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        public byte[] calculateDescriptor(BitmapSource myImage)
        {
            CEDD_Descriptor.CEDD myCEDD = new CEDD_Descriptor.CEDD();
            return myCEDD.getDescriptor(myImage);
        }

        #region Get Dominant Colors and Textures

        /// <summary>
        /// Gets the dominant colors.
        /// </summary>
        /// <param name="imageID">The image ID.</param>
        /// <param name="number">The number of dominant colors.</param>
        /// <returns></returns>
        public List<Color> getDominantColors(int imageID, int number = 2)
        {
            return getDominantColors(imgData[imageID], number);
        }

        /// <summary>
        /// Gets the dominant colors using a clustering approach.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <param name="number">The number of dominant colors.</param>
        /// <param name="similarity">Similarity of the colors allowed in the return color list (Range: 0 - 255)</param>
        /// <returns></returns>
        public List<Color> getDominantColors(BitmapSource myImage, int number = 2, int similarity = 0)
        {
            return getDominantColors(myImage, 0.02, 500, number, similarity);
            
        }

       

        /// <summary>
        /// Gets the dominant colors using a clustering approach. Very CPU costly but more accurate. 
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <param name="number">The number of dominant colors.</param>
        /// <param name="similarity">Similarity of the colors allowed in the return color list (Range: 0 - 255)</param>
        /// <returns></returns>
        public List<Color> getDominantColorsSlow(BitmapSource myImage, int number = 2, int similarity = 0)
        {
            return getDominantColors(myImage, 0.06, 1000, number, similarity);
        }


        /// <summary>
        /// Gets the dominant colors.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public List<Color> getDominantColors(byte[] descriptorSource, int number = 2)
        {
            Color[] descriptorColors = retrieval.getDescriptorColors();
            int[] colorsvalue = new int[24];
            for (int texture = 0; texture < 6; texture++)
                for (int color = 0; color < 24; color++)
                    colorsvalue[color] += descriptorSource[texture * 24 + color];

            var theColors = colorsvalue.Select(
                (x, index) => new { value = x, color = descriptorColors[index] }
                ).OrderByDescending(x => x.value).Take(number).Select(x => x.color);
            return theColors.ToList();
        }

        /// <summary>
        /// Gets the dominant textures.
        /// </summary>
        /// <param name="imageID">The image ID.</param>
        /// <param name="number">The number of dominant textures.</param>
        /// <returns></returns>
        public List<TextureTypes> getDominantTextures(int imageID, int number = 2)
        {
            return getDominantTextures(imgData[imageID], number);
        }


        /// <summary>
        /// Gets the dominant textures.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <param name="number">The number of dominant textures.</param>
        /// <returns></returns>
        public List<TextureTypes> getDominantTextures(BitmapSource myImage, int number = 2)
        {
            return getDominantTextures(calculateDescriptor(myImage), number);
        }

       
        /// <summary>
        /// Gets the dominant textures.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public List<TextureTypes> getDominantTextures(byte[] descriptorSource, int number = 2)
        {
            int[] texturevalues = new int[6];
            for (int texture = 0; texture < 6; texture++)
                for (int color = 0; color < 24; color++)
                    texturevalues[texture] += descriptorSource[texture * 24 + color];
            var textures = texturevalues.Select(
                (x, index) => new { value = x, indexvalue = index })
                .OrderByDescending(x => x.value).Take(number).Select(x => (TextureTypes)x.indexvalue);
            return textures.ToList();
        }
        
        #endregion

        #region Get Similar - classic retrieval
        /// <summary>
        /// Gets the similar images.
        /// </summary>
        /// <param name="imageDescriptor">The image descriptor.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilar(byte[] imageDescriptor)
        {
            if (History == null) History = new List<byte[]>();
            History.Add(imageDescriptor);
            AutomaticRelevanceFeedback arf = new AutomaticRelevanceFeedback(History[0]);
            for (int i = 1; i < History.Count; i++)
                arf.ApplyNewValues(History[i]);
            return getSimilar(arf.GetNewDescriptor());
        }

        /// <summary>
        /// Gets the similar images. Sometimes the query descriptor must be in float type.
        /// </summary>
        /// <param name="imageDescriptor">The image descriptor.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilar(float[] imageDescriptor)
        {

            int[] keys = imgData.Keys.ToArray();
            retrieval myretrieval = new retrieval(imageDescriptor);
            if (multiThread)
            {
                Parallel.For(0, keys.Length, i =>
                {
                    results[i].imageID = keys[i];
                    results[i].similarity = myretrieval.howSimilar(imgData[keys[i]]);
                });
            }
            else
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    results[i].imageID = keys[i];
                    results[i].similarity = myretrieval.howSimilar(imgData[keys[i]]);
                }
            }
            return (multiThread) ? results.AsParallel().OrderBy(x => x.similarity).ToArray() : results.OrderBy(x => x.similarity).ToArray();
        }

        /// <summary>
        /// Gets the similar images.
        /// </summary>
        /// <param name="imageID">The image ID.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilar(int imageID)
        {
            byte[] source = imgData[imageID];
            return getSimilar(source);
        }

        /// <summary>
        /// Gets the similar images.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilar(BitmapSource myImage)
        {
            return getSimilar(calculateDescriptor(myImage));
        }

        /// <summary>
        /// Gets the similar images.
        /// </summary>
        /// <param name="theTextures">The textures.</param>
        /// <param name="theColors">The colors.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilar(List<TextureTypes> theTextures, List<Color> theColors)
        {
            return getSimilar(createCustomDescriptor(theTextures, theColors));
        }
        
        #endregion

        #region Similar by Color or Texture

        /// <summary>
        /// Gets the similar images based on their color.
        /// </summary>
        /// <param name="imageID">The image ID.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilarbyColor(int imageID)
        {
            return getSimilarbyTransformation(imgData[imageID], transformToColor);
        }

        /// <summary>
        /// Gets the similar images based on their color.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilarbyColor(BitmapSource myImage)
        {
            return getSimilarbyTransformation(calculateDescriptor(myImage), transformToColor);
        }

        /// <summary>
        /// Gets the similar images based on their color.
        /// </summary>
        /// <param name="imageDescriptor">The image descriptor.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilarbyColor(byte[] imageDescriptor)
        {
            return getSimilarbyTransformation(imageDescriptor, transformToColor);
        }

        /// <summary>
        /// Gets the similar images based on their texture.
        /// </summary>
        /// <param name="imageID">The image ID.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilarbyTexture(int imageID)
        {
            return getSimilarbyTransformation(imgData[imageID], transformToTexture);
        }

        /// <summary>
        /// Gets the similar images based on their texture.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilarbyTexture(BitmapSource myImage)
        {
            return getSimilarbyTransformation(calculateDescriptor(myImage), transformToTexture);
        }

        /// <summary>
        /// Gets the similar images based on their texture.
        /// </summary>
        /// <param name="imageDescriptor">The image descriptor.</param>
        /// <returns></returns>
        public ImageInfo[] getSimilarbyTexture(byte[] imageDescriptor)
        {
            return getSimilarbyTransformation(imageDescriptor, transformToTexture);
        }
        
        #endregion


        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <param name="ncluster">The number of the clusters.</param>
        /// <param name="minError">The minimum error.</param>
        /// <param name="iterations">The maximum iterations.</param>
        /// <param name="clusterIsBased">Clustering based on color, texture or both</param>
        /// <param name="SOFMState">The state of the SOFM</param>
        /// <param name="trainDataLength">Length of the train data.</param>
        /// <param name="trainingData">The training data. If it is null then, the trainDataLength and imageData will be used.</param>
        /// <param name="debugLog">Enables or disables the debug log</param>
        /// <returns></returns>
        public ImageInfo[] getGroups(int ncluster, double minError, int iterations, ClusteringBased clusterIsBased, out object SOFMState, int trainDataLength = -1, byte[][] trainingData = null, bool debugLog = false)
        {
            SOFM mySOFM = new SOFM(imgData, ncluster, clusterIsBased, minError, iterations, trainDataLength, trainingData, debugLog);
            Dictionary<int,int> grouping = mySOFM.Run();
            int[] keys = grouping.Keys.ToArray();
            for (int i=0;i<keys.Length; i++)
            {
                results[i].imageID = keys[i];
                results[i].groupID = grouping[keys[i]];
            }
            SOFMState = mySOFM.SOFMState;
            return results;
        }

        /// <summary>
        /// Gets the groups without a training stage.
        /// </summary>
        /// <param name="SOFMState">The state of the SOFM</param>
        /// <param name="debugLog">Enables or disables the debug log</param>
        /// <returns></returns>
        public ImageInfo[] getGroups(object SOFMState, bool debugLog = false)
        {

            try
            {
                SOFM.Neuron[] mySOFMState = SOFMState as SOFM.Neuron[];
                SOFM mySOFM = new SOFM(imgData, mySOFMState, debugLog);
                Dictionary<int, int> grouping = mySOFM.RunClassification();
                int[] keys = grouping.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    results[i].imageID = keys[i];
                    results[i].groupID = grouping[keys[i]];
                }
                return results;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Saves the state of the SOFM.
        /// </summary>
        /// <param name="Filename">The filename that the SOFM state will be saved.</param>
        /// <param name="SOFMState">The state of the SOFM.</param>
        public void saveSOFMState(string Filename, object SOFMState)
        {
                Stream mystream = File.Open(Filename, FileMode.Create, FileAccess.Write);
                BinaryFormatter binFormat = new BinaryFormatter();
                binFormat.Serialize(mystream, SOFMState);
                mystream.Close();
        }

        /// <summary>
        /// Load the state of the SOFM.
        /// </summary>
        /// <param name="Filename">The filename that contains the SOFM sate.</param>
        /// <param name="nCluster">The number of the clusters.</param>
        /// <returns></returns>
        public object loadSOFMState(string Filename, out int nCluster)
        {
            Stream mystream = File.Open(Filename, FileMode.Open, FileAccess.Read);
            BinaryFormatter binFormat = new BinaryFormatter();
            var mySOFMState = binFormat.Deserialize(mystream) as SOFM.Neuron[];
            mystream.Close();
            nCluster = mySOFMState.Count();
            return mySOFMState;    
        }

       
        #endregion
        

        
        
        #region Private Functions
        
        /// <summary>
        /// Creates the custom descriptor.
        /// </summary>
        /// <param name="theTextures">The textures.</param>
        /// <param name="theColors">The colors.</param>
        /// <param name="normalize">if set to <c>true</c> [normalize].</param>
        /// <returns></returns>
        private float[] createCustomDescriptor(List<TextureTypes> theTextures, List<Color> theColors, bool normalize = true)
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

        /// <summary>
        /// Gets the dominant colors using a self organizing clustering technique.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <param name="samples">The samples.</param>
        /// <param name="interactions">The interactions.</param>
        /// <param name="number">The number.</param>
        /// <param name="similarity">Similarity of the colors allowed in the return color list (Range: 0 - 255)</param>
        /// <returns></returns>
        private List<Color> getDominantColors(BitmapSource myImage, double samples, int interactions, int number, int similarity)
        {
            int maxColors = 2 * number + 1;
            int bytesPerPixel = myImage.Format.BitsPerPixel / 8;
            int maxPixels = myImage.PixelWidth * myImage.PixelHeight;
            byte[] pixels = new byte[bytesPerPixel * myImage.PixelWidth * myImage.PixelHeight];
            int stride = myImage.PixelWidth * bytesPerPixel;
            myImage.CopyPixels(pixels, stride, 0);
            Dictionary<int, byte[]> classifying = new Dictionary<int, byte[]>();

            int maxTraining = (int)(samples * maxPixels);
            Random rnd = new Random();

            for (int i = 0; i < maxTraining; i++)
            {
                int k = rnd.Next(maxPixels);
                if (!classifying.ContainsKey(k)) classifying.Add(k, new byte[] { pixels[bytesPerPixel * k + 2], pixels[bytesPerPixel * k + 1], pixels[bytesPerPixel * k] });
            }

            SOFM myDomColors = new SOFM(classifying, maxColors, ClusteringBased.TextureColor, maxIterations: interactions, debugLog: false);
            myDomColors.colorMode = true;
            Dictionary<int, int> results = myDomColors.Run();
            SOFM.Neuron[] myCenters = myDomColors.SOFMState;
            int[] dominant = new int[maxColors];
            foreach (var r in results) dominant[r.Value]++;
            // implementing  Manhattan distance for computing the similarity score
            Func<Color, Color, int> manhattanDistance = (color1, color2) => (int)Math.Round((Math.Abs(color1.R - color2.R) +
                Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B)) / 3f);

            var stage1 = dominant.Select((x, index) => new
            {
                count = x,
                color = Color.FromRgb(
                (byte)myCenters[index].Weights[0],
                (byte)myCenters[index].Weights[1],
                (byte)myCenters[index].Weights[2])
            }).OrderByDescending(x => x.count).Take(number).ToList();

            var removedList = new Dictionary<Color, bool>();
            for (int i = 1; i < stage1.Count(); i++)
            {
                if (removedList.ContainsKey(stage1[i].color)) continue;
                for (int j = (i + 1); j < stage1.Count(); j++)
                {
                    if (manhattanDistance(stage1[i].color, stage1[j].color) < similarity)
                        if (!removedList.ContainsKey(stage1[j].color))
                            removedList.Add(stage1[j].color, true);
                }
            }



            return stage1.Where(y=> !removedList.ContainsKey(y.color)).Select(x => x.color).ToList();
        }


        /// <summary>
        /// Puts the color to descriptor.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="theColor">The color.</param>
        /// <returns></returns>
        private byte[] putColorToDescriptor(byte[] descriptorSource, Color theColor)
        {
            Color[] descriptorColors = retrieval.getDescriptorColors();
            double[] similarity = descriptorColors.Select(x => Math.Sqrt(Math.Pow(x.R - theColor.R, 2) + Math.Pow(x.G - theColor.G, 2) + Math.Pow(x.B - theColor.B, 2))).ToArray();
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
        /// Puts the colors to descriptor.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="theColors">The colors.</param>
        /// <returns></returns>
        private byte[] putColorsToDescriptor(byte[] descriptorSource, List<Color> theColors)
        {
            foreach (var c in theColors)
                descriptorSource = putColorToDescriptor(descriptorSource, c);
            return descriptorSource;
        }

        /// <summary>
        /// Puts the textures to descriptor.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <param name="theTextures">The textures.</param>
        /// <returns></returns>
        private byte[] putTexturesToDescriptor(byte[] descriptorSource, List<TextureTypes> theTextures)
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

        /// <summary>
        /// Transforms to texture.
        /// </summary>
        /// <param name="descriptorSource">The descriptor source.</param>
        /// <returns></returns>
        private byte[] transformToTexture(byte[] descriptorSource)
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
        private byte[] transformToColor(byte[] desciptorSource)
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

        /// <summary>
        /// Gets the similarby transformation.
        /// </summary>
        /// <param name="imageDescriptor">The image descriptor.</param>
        /// <param name="descriptorTranformation">The descriptor transformation.</param>
        /// <returns></returns>
        private ImageInfo[] getSimilarbyTransformation(byte[] imageDescriptor, Func<byte[], byte[]> descriptorTranformation)
        {
            int[] keys = imgData.Keys.ToArray();
            retrieval myretrieval = new retrieval(descriptorTranformation(imageDescriptor));
            if (multiThread)
            {
                Parallel.For(0, keys.Length, i =>
                {
                    results[i].imageID = keys[i];
                    results[i].similarity = myretrieval.howSimilar(descriptorTranformation(imgData[keys[i]]));
                });
            }
            else
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    results[i].imageID = keys[i];
                    results[i].similarity = myretrieval.howSimilar(descriptorTranformation(imgData[keys[i]]));
                }
            }


            return (multiThread) ? results.AsParallel().OrderBy(x => x.similarity).ToArray() : results.OrderBy(x => x.similarity).ToArray();
        }
        
        #endregion

        
    }
}
