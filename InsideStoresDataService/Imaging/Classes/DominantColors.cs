using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InsideStores.Imaging.Descriptors;

namespace InsideStores.Imaging
{
    public class DominantColors
    {
        #region Simple Method using Only Descriptor

        /// <summary>
        /// Gets the dominant colors from descriptor.
        /// </summary>
        /// <remarks>
        /// Return the N most dominant colors from descriptor, which is constrained
        /// to the 24 basic colors used in descriptors. Essentially using as a tiny
        /// histogram and taking the N with the highest frequency.
        /// </remarks>
        /// <param name="cedd">The descriptor source.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public static List<Color> GetDominantColorsFromDescriptor(byte[] cedd, int number = 2)
        {
            var histogram = CEDD.GetColorHistogram(cedd);

            var theColors = histogram.Select(
                (x, index) => new { value = x, color = CEDD.DescriptorColors[index] }
                ).OrderByDescending(x => x.value).Take(number).Select(x => x.color);
            return theColors.ToList();
        }

        /// <summary>
        /// Gets the dominant textures from descriptor.
        /// </summary>
        /// <remarks>
        /// Uses the descriptor table to build a tiny histogram of the 6 kinds of tracked textures,
        /// and then returnes the N most frequent.
        /// </remarks>
        /// <param name="cedd">The descriptor source.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public List<CEDD.TextureTypes> GetDominantTexturesFromDescriptor(byte[] cedd, int number = 2)
        {
            var histogram = CEDD.GetTextureHistogram(cedd);

            var textures = histogram.Select(
                (x, index) => new { value = x, indexvalue = index })
                .OrderByDescending(x => x.value).Take(number).Select(x => (CEDD.TextureTypes)x.indexvalue);
            return textures.ToList();
        }
  
        #endregion

        #region Advanced Method using Image and SOM

        /// <summary>
        /// Gets the dominant colors using a clustering approach.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <param name="number">The number of dominant colors.</param>
        /// <param name="similarity">Similarity of the colors allowed in the return color list (Range: 0 - 255)</param>
        /// <returns></returns>
        public static List<Color> GetDominantColors(BitmapSource myImage, int number = 4, int similarity = 20)
        {
            // prototype used number = 8, similarity = 25

            return GetDominantColors(myImage, 0.02, 500, number, similarity);
        }

        /// <summary>
        /// Gets the dominant colors using a clustering approach. Very CPU costly but more accurate. 
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <param name="number">The number of dominant colors.</param>
        /// <param name="similarity">Similarity of the colors allowed in the return color list (Range: 0 - 255)</param>
        /// <returns></returns>
        public static List<Color> GetDominantColorsSlow(BitmapSource myImage, int number = 4, int similarity = 20)
        {
            // prototype used number = 8, similarity = 20
            return GetDominantColors(myImage, 0.5, 1000, number, similarity);
        }

        /// <summary>
        /// Gets the dominant colors using a self organizing clustering technique.
        /// </summary>
        /// <param name="myImage">My image.</param>
        /// <param name="samples">The samples. A factor, percentage to calc the Number of pixels to sample from the image before trying to cluster.</param>
        /// <param name="interactions">The interactions.</param>
        /// <param name="number">The number.</param>
        /// <param name="similarity">Similarity of the colors allowed in the return color list (Range: 0 - 255)</param>
        /// <returns></returns>
        public static List<Color> GetDominantColors(BitmapSource myImage, double samples, int interactions, int number, int similarity)
        {
            int maxColors = 2 * number + 1; // the number of clusters to make (K)
            int bytesPerPixel = myImage.Format.BitsPerPixel / 8;
            int maxPixels = myImage.PixelWidth * myImage.PixelHeight;
            byte[] pixels = new byte[bytesPerPixel * myImage.PixelWidth * myImage.PixelHeight];
            int stride = myImage.PixelWidth * bytesPerPixel;
            myImage.CopyPixels(pixels, stride, 0);
            Dictionary<int, byte[]> classifying = new Dictionary<int, byte[]>();

            // assuming samples is less than one, we take a percentage of image pixels to be used for training.
            int maxTraining = (int)(samples * maxPixels); 

            Random rnd = new Random();

            // build up a random subset of the pixels in the image to be used to perform the clustering.

            // for each of the N training pixels, get a byte[] with it's RGB value and add to classifying dic with
            // the key as the pixel number, and the value as byte[]. Do not add the same pixel index more than once.

            if (samples < 1.0)
            {
                for (int i = 0; i < maxTraining; i++)
                {
                    int k = rnd.Next(maxPixels);// randomly pick one of the pixels from the image
                    if (!classifying.ContainsKey(k)) classifying.Add(k, new byte[] { pixels[bytesPerPixel * k + 2], pixels[bytesPerPixel * k + 1], pixels[bytesPerPixel * k] });
                }

            }
            else
            {
                // all of them
                for (int i = 0; i < maxPixels; i++)
                    classifying.Add(i, new byte[] { pixels[bytesPerPixel * i + 2], pixels[bytesPerPixel * i + 1], pixels[bytesPerPixel * i] });
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

            // takes the N most frequent neurons (based on how many items in that bucket) and makes colors from them

            var stage1 = dominant.Select((x, index) => new
            {
                count = x,
                color = Color.FromRgb(
                (byte)myCenters[index].Weights[0],
                (byte)myCenters[index].Weights[1],
                (byte)myCenters[index].Weights[2])
            }).OrderByDescending(x => x.count).Take(number).ToList();

            // remove colors that are too close to each other

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

            // what is being returned is the color of the center of neurons, which might not be exactly
            // same pixels as in the actual image

            return stage1.Where(y => !removedList.ContainsKey(y.color)).Select(x => x.color).ToList();
        }

        #endregion

#if false
        #region Another K-Means method

        public class DominantColor
        {
	        public Color color;
	        public float percentage;

	        public DominantColor(Color color, float percentage)
            {
		        this.color = color;
		        this.percentage = percentage;
	        }
        }

        // https://github.com/jfeinstein10/DominantColors/blob/master/src/com/dominantcolors/DominantColors.java

	    private const int DEFAULT_NUM_COLORS = 3;
	    private const double DEFAULT_MIN_DIFF = 0.5f;

#if false
        public static DominantColor[] getDominantColors(Bitmap bitmap)
        {
		    return getDominantColors(bitmap, DEFAULT_NUM_COLORS);
	    }


	    public static DominantColor[] getDominantColors(Bitmap bitmap, int numColors)
        {
		    return getDominantColors(bitmap, numColors, DEFAULT_MIN_DIFF);
	    }

	    public static DominantColor[] getDominantColors(Bitmap bitmap, int numColors, double minDiff)
        {
		    // scale down while maintaining aspect ratio
		    bitmap = resizeToFitInSquare(bitmap, SIDE_SIZE);

		    int[] c = kmeans(bitmap, numColors);
		    
		    DominantColor[] colors = new DominantColor[numColors];
		    for (int i = 0; i < numColors; i++) {
			    colors[i] = new DominantColor(c[i], 1);
		    }
		    return colors;
	    }
	
	    private static DominantColor[] getMeanShift(Bitmap bitmap, float radius)
        {
		    int[] c = meanShift(bitmap, radius);
		    DominantColor[] colors = new DominantColor[c.length];
		    for (int i = 0; i < c.length; i++) {
			    colors[i] = new DominantColor(c[i], 1);
		    }
		    return colors;
	    }


	    private static DominantColor[] kMeans(int[] points, int numColors, double minDiff)
        {
		    // create the clusters
		    int[] middles = getRandomMiddles(points, numColors);
		    DominantColor[] colors = new DominantColor[numColors];

		    while (true) {
			    // resample and resort the points
			    ArrayList<Integer>[] newClusters = new ArrayList[numColors];
			    for (int i = 0; i < numColors; i++)
				    newClusters[i] = new ArrayList<Integer>();

			    for (int point : points) {
				    double minDist = Double.MAX_VALUE;
				    int minId = 0;
				    for (int i = 0; i < middles.length; i++) {
					    double dist = calculateDistance(point, middles[i]);
					    if (dist < minDist) {
						    minDist = dist;
						    minId = i;
					    }
				    }
				    newClusters[minId].add(point);
			    }
			    // copy the new cluster data into the old clusters
			    double diff = 0;
			    for (int i = 0; i < middles.length; i++) {
				    int newCenter = calculateCenter(newClusters[i]);
				    diff = Math.max(diff, calculateDistance(newCenter, middles[i]));
				    middles[i] = newCenter;
			    }
			    if (diff < minDiff) {
				    for (int i = 0; i < middles.length; i++)
					    colors[i] = new DominantColor(middles[i], (float) newClusters[i].size() / (float) points.length);
				    break;
			    }
		    }

		    Arrays.sort(colors, new Comparator<DominantColor>()
            {
			    @Override
			    public int compare(DominantColor lhs, DominantColor rhs) {
				    return (int)(100 * (lhs.percentage - rhs.percentage));
			    }			
		    });

		    return colors;
	    }

#endif

        private static Color[] getRandomMiddles(Color[] points, int numColors)
        {
		    var indices = new List<int>();
		    for (int i = 0; i < points.Count(); i++)
			    indices.Add(i);

		    // Collections.shuffle(indices);

		    var midArray = new List<Color>();
		    Color[] middles = new Color[numColors];
		    int index = 0;

		    while (midArray.Count() < numColors)
            {
			    var val = points[indices[index++]];
			    if (!midArray.Contains(val))
                {
				    middles[midArray.Count()] = val;
				    midArray.Add(val);
			    }
		    }
		    return middles;
	    }

	    private static Color calculateCenter(List<Color> points)
        {
		    int rSum, gSum, bSum;
		    rSum = gSum = bSum = 0;

		    foreach (var i in points)
            {
			    rSum += i.R;
			    gSum += i.G;
			    bSum += i.B;
		    }

            if (points.Count() == 0)
                return Color.FromArgb(255, 0, 0, 0);

            return Color.FromArgb(255, (byte)(rSum / points.Count()), (byte)(gSum / points.Count()), (byte)(bSum / points.Count()));
	    }

	    private static double calculateDistance(Color c1, Color c2)
        {
		    return Math.Sqrt(
				    0.9 * Math.Pow(c1.R - c2.R, 2) +  
				    1.2 * Math.Pow(c1.G - c2.G, 2) +
				    0.9 * Math.Pow(c1.B - c2.B, 2));
	    }


        #endregion
#endif    
    }
}
