using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace cbir
{

    /// <summary>
    /// Defines  what it will be used for the clustering
    /// </summary>
    public enum ClusteringBased { 
        /// <summary>
        /// Clustering based on Color
        /// </summary>
        Color,
        /// <summary>
        /// Clustering based on Texture
        /// </summary>
        Texture, 
        /// <summary>
        /// Clustering based on both Color and Texture
        /// </summary>
        TextureColor
    }
    
    /// <summary>
    /// Self-Organizing Feature Map
    /// </summary>
    [Serializable]
    public class SOFM
    {




        /// <summary>
        /// The output neurons
        /// </summary>
        private Neuron[] outputs;
        /// <summary>
        /// Current iteration
        /// </summary>
        private int iteration;

        /// <summary>
        /// The length of the output grid
        /// </summary>
        private int length;

        /// <summary>
        /// Number of input dimensions
        /// </summary>
        private int dimensions;

        /// <summary>
        /// The data
        /// </summary>
        private Dictionary<int, byte[]> classifingdata;
        private byte[][] trainingData;
        private Dictionary<int, int> results;

        /// <summary>
        /// The stream file for the output debug log options
        /// </summary>
        private StreamWriter sw;
        /// <summary>
        /// The data length
        /// </summary>
        private int dataTrainLength;

        private ClusteringBased myClusterBased;

        /// <summary>
        /// Gets or sets the minimum error.
        /// </summary>
        /// <value>
        /// The minimum error.
        /// </value>
        public double minError { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum iterations.
        /// </summary>
        /// <value>
        /// The maximum iterations.
        /// </value>
        public int maxIterations { get; set; }
        
        /// <summary>
        /// Gets the classified results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public Dictionary<int, int> Results { get { return results; } }
        
        /// <summary>
        /// Gets or sets a value indicating whether to produce a debug log.
        /// </summary>
        /// <value>
        ///   <c>true</c> if wand to produce a debug log; otherwise, <c>false</c>.
        /// </value>
        public bool debugLog { get; set; }

        public Neuron[] SOFMState
        {
            get
            {
                return outputs;
            }
        }

        public bool colorMode = false;



        /// <summary>
        /// Initializes a new instance of the <see cref="SOFM" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="nCluster">The number of clusters.</param>
        /// <param name="clusterIsBased">Clustering based on color, texture or both</param>
        /// <param name="minError">The minimum error.</param>
        /// <param name="maxIterations">The maximum iterations.</param>
        /// <param name="dataTrainLength">Length of the training data.</param>
        /// <param name="trainingData">The training data. If it is null then, data and dataTrainLength will be used.</param>
        /// <param name="debugLog">Enable or disable a debug log.</param>
        public SOFM(Dictionary<int, byte[]> data, int nCluster,ClusteringBased clusterIsBased, double minError = 0.1, int maxIterations = 10000, 
            int dataTrainLength = -1, byte[][] trainingData = null, bool debugLog = false)
        {

            this.dataTrainLength = dataTrainLength > 0 ? dataTrainLength : data.Count();
            this.length = nCluster;
            switch (clusterIsBased)
            {
                case ClusteringBased.TextureColor:
                    this.dimensions = 144;
                    this.classifingdata = data;
                    this.trainingData = (trainingData == null) ?
                data.Take(this.dataTrainLength).Select(x => x.Value).ToArray() :
                trainingData.ToArray();
                    break;
                case ClusteringBased.Color:
                    this.dimensions = 24;
                    this.classifingdata = convertColorDescriptor(data);
                    this.trainingData = (trainingData == null) ?
                data.Take(this.dataTrainLength).Select(x => transformToColor(x.Value)).ToArray() :
                trainingData.Select(x => transformToColor(x)).ToArray();
                    break;
                case ClusteringBased.Texture:
                    this.dimensions = 6;
                    this.classifingdata = convertTextureDescriptor(data);
                    this.trainingData = (trainingData == null) ?
                data.Take(this.dataTrainLength).Select(x => transformToTexture(x.Value)).ToArray() :
                trainingData.Select(x => transformToTexture(x)).ToArray();

                    break;
            }
            
            this.maxIterations = maxIterations;
            this.minError = minError;
            this.debugLog = debugLog;
            this.myClusterBased = clusterIsBased;
            if (debugLog) sw = new StreamWriter("logClustering.txt", false);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SOFM"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="SOFMState">State of the SOFM.</param>
        /// <param name="debugLog">Enable or disable a debug log.</param>
        public SOFM(Dictionary<int,byte[]> data, Neuron[] SOFMState, bool debugLog = false)
        {
            
            this.length = SOFMState.Length;
            this.debugLog = debugLog;
            this.outputs = SOFMState;
            this.dimensions = outputs[0].Weights.Length;
            switch (dimensions)
            {
                case 144:
                    this.myClusterBased = ClusteringBased.TextureColor;
                    this.classifingdata = data;
                    break;
                case 24:
                    this.myClusterBased = ClusteringBased.Color;
                    this.classifingdata = convertColorDescriptor(data);
                    break;
                case 6:
                    this.myClusterBased = ClusteringBased.Texture;
                    this.classifingdata = convertTextureDescriptor(data);
                    break;
            }
            if (debugLog) sw = new StreamWriter("logClustering.txt", false);
        }


        //  
        /// <summary>
        /// Cleaning up the streamwriter object if something bad happen. Finalizes an instance of the <see cref="SOFM"/> class.
        /// </summary>
        ~SOFM() 
        {
            if (sw != null)
            { sw.Close(); sw = null; }
            
        }

        /// <summary>
        /// Closing the log file manually.
        /// </summary>
        public void ClosingLogFile()
        {
            sw.Close(); sw = null;
        }

        /// <summary>
        /// Runs the SOFM.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> Run()
        {
            if (this.colorMode)
            { this.dimensions = 3; InitialiseColor(); }
            else { Initialise(); }
            Train(minError);
            results = classifyData();
            if (debugLog)
            { sw.Close(); sw = null; }
            return Results;
        }

        /// <summary>
        /// Runs the data classification without training.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> RunClassification()
        {
            results = classifyData();
            if (debugLog)
            { sw.Close(); sw = null; }
            return Results;
        }

        private Dictionary<int,byte[]> convertTextureDescriptor (Dictionary<int,byte[]> initData) {
            Dictionary<int, byte[]> convertData = new Dictionary<int, byte[]>();
            foreach (var d in initData)
                convertData.Add(d.Key, transformToTexture(d.Value));
            return convertData;
        }

        private Dictionary<int, byte[]> convertColorDescriptor(Dictionary<int, byte[]> initData)
        {
            Dictionary<int, byte[]> convertData = new Dictionary<int, byte[]>();
            foreach (var d in initData)
                convertData.Add(d.Key, transformToColor(d.Value));
            return convertData;
        }

        private void Initialise()
        {

            outputs = new Neuron[length];
            Random rnd = new Random();
            for (int i = 0; i < length; i++)
            {
                outputs[i] = new Neuron(i, length);
                outputs[i].Weights = new double[dimensions];
                for (int k = 0; k < dimensions; k++)
                {
                    outputs[i].Weights[k] = rnd.NextDouble();
                }
            }

        }


        private void InitialiseColor()
        {
            outputs = new Neuron[length];
            int[] keys = classifingdata.Keys.ToArray();

            Random rnd = new Random();
            for (int i = 0; i < length; i++)
            {
                outputs[i] = new Neuron(i, length);
                outputs[i].Weights = new double[dimensions];
                int v = keys[rnd.Next(keys.Length)];
                for (int k = 0; k < dimensions; k++)
                {
                    outputs[i].Weights[k] = classifingdata[v][k];
                }
            }

        }


        private void Train(double maxError)
        {

            double currentError = double.MaxValue;
            //randomly shuffle them
            
            trainingData = trainingData.OrderBy(x => Guid.NewGuid()).ToArray();
            if (debugLog) sw.WriteLine("{0}: Start Training...", DateTime.Now);
            while (currentError > maxError && iteration < maxIterations)
            {
                currentError = 0;
                object locker = new object();
                Parallel.For(0, trainingData.Length, i =>
                    {
                        var localerror = TrainPattern(trainingData[i]);
                        lock (locker)
                        {
                            currentError += localerror;
                        }

                    });

                iteration++;
                if (debugLog)
                {
                    sw.WriteLine(string.Format("Current Error:{0:0.00000}\t Current Iteration: {1}", currentError, iteration));
                    sw.Flush();
                }
                    
            }
        }



        private double TrainPattern(byte[] pattern)
        {

            double error = 0;
            Neuron winner = Winner(pattern);
            for (int i = 0; i < length; i++)
            {
                error += outputs[i].UpdateWeights(pattern, winner, iteration);
            }
            
            return Math.Abs(error / (length * length));
        }



        private Dictionary<int, int> classifyData()
        {
            int[] keys = classifingdata.Keys.ToArray();
            Dictionary<int, int> r = new Dictionary<int, int>();
            if (debugLog)
                sw.WriteLine("{0}: DONE Training! Start Classifying...", DateTime.Now);
            for (int i = 0; i < keys.Length; i++)
            {
                Neuron n = Winner(classifingdata[keys[i]]);
                r.Add(keys[i], n.X);
            }
            if (debugLog)
                sw.WriteLine("{0}: Done Classifying!", DateTime.Now);
            return r;
        }



        private Neuron Winner(byte[] pattern)
        {

            Neuron winner = null;
            double min = double.MaxValue;
            for (int i = 0; i < length; i++)
            {
                double d = Distance(pattern, outputs[i].Weights);
                if (d < min)
                {
                    min = d;
                    winner = outputs[i];
                }
            }
            if (winner == null)
                new Exception("Something bad happen. Winner Neuron is null");
            return winner;
        }

        

        private double Distance(byte[] vector1, double[] vector2)
        {
            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow((vector1[i] - vector2[i]), 2);
            }
            return Math.Sqrt(value);
        }

        private byte[] transformToTexture(byte[] descriptorSource)
        {
            // new 6-bin histogram
            byte[] textureHistogram = new byte[6];
            for (int color = 0; color < 24; color++)
                for (int texture = 0; texture < 6; texture++)
                    textureHistogram[texture] += descriptorSource[texture * 24 + color];
            return textureHistogram;
        }

        private byte[] transformToColor(byte[] desciptorSource)
        {
            //new 24-bin histogram
            byte[] colorHistogram = new byte[24];
           
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 24; j++)
                {
                    colorHistogram[j] += desciptorSource[i * 24 + j];
                }
            return colorHistogram;
        }



        [Serializable]
        public class Neuron
        {

            public double[] Weights;
            public int X;
            private int length;
            private double nf;



            public Neuron(int x, int length)
            {
                X = x;
                this.length = length;
                nf = Convert.ToSingle(1000 / Math.Log(length));
            }



            private double Gauss(Neuron win, int it)
            {

                //double distance = Math.Sqrt(Math.Pow(win.X - X, 2) + Math.Pow(win.Y - Y, 2));
                double distance = Math.Abs(win.X - X);
                return (Math.Pow(Strength(it), 2) == 0) ? 0 : Math.Exp(-Math.Pow(distance, 2) / (Math.Pow(Strength(it), 2)));
            }



            private double LearningRate(int it)
            {
                return Math.Exp(-it / 1000) * 0.1;
            }



            private double Strength(int it)
            {
                return Math.Exp(-it / nf) * length;
            }



            public double UpdateWeights(byte[] pattern, Neuron winner, int it)
            {

                double sum = 0;
                for (int i = 0; i < Weights.Length; i++)
                {
                    double delta = LearningRate(it) * Gauss(winner, it) * (pattern[i] - Weights[i]);
                    Weights[i] += delta;
                    sum += delta;
                }
                return sum / Weights.Length;
            }

        }
    }

}
