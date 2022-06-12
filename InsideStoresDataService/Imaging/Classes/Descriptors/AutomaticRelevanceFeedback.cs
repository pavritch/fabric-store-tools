using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InsideStores.Imaging.Descriptors
{
        // called like this - where a history of previous calls is built up, and
        // this module merges the descriptors together to come up with a synthetic descriptor
        // which represents the set of them. For a fresh query, starts out with only that
        // first descriptor, and that one is passed as is through the similarity function.

        //public ImageInfo[] getSimilar(byte[] imageDescriptor)
        //{
        //    if (History == null) History = new List<byte[]>();
        //    History.Add(imageDescriptor);
        //    AutomaticRelevanceFeedback arf = new AutomaticRelevanceFeedback(History[0]);
        //    for (int i = 1; i < History.Count; i++)
        //        arf.ApplyNewValues(History[i]);
        //    return getSimilar(arf.GetNewDescriptor());
        //}

    /// <summary>
    /// The Automatic Relevance Feedback
    /// </summary>
    public class AutomaticRelevanceFeedback
    {
        private const float Ef = 0.01f;// Final LearningRate;
        private const float Ei = 0.4f;// Initial LearningRate;
        private const int Tmax = 30; // Maximum number of iterations (epochs)

        /// <summary>
        /// winner
        /// </summary>
        private struct Winner
        {
            public int x;
            public int y;
            public int z;
        }

        /// <summary>
        /// descriptors type
        /// </summary>
        private enum Descriptors
        {
            CEDD,FCTH, CCEDD, CFCTH,JCD 
        }
        
        private float[,,] Weights;

        private float[,,,] WGrid1;   // Weight Vectors
        private int i,x,y,z;
        private int Time = 0; // Current epochs (number of searches)
        private Winner Win1 = new Winner();
        private int Number_X;
        private int Number_Y;
        private int Number_Z;
        private float[] x1;
        private float[] returnWeights;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticRelevanceFeedback" /> class.
        /// </summary>
        /// <param name="InitValues">The initial values.</param>
        public AutomaticRelevanceFeedback(byte[] InitValues)
        {
            Initialize(Descriptors.CEDD, InitValues.Select(x => Convert.ToSingle(x)).ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticRelevanceFeedback" /> class.
        /// </summary>
        /// <param name="InitValues">The initial values.</param>
        public AutomaticRelevanceFeedback(float[] InitValues)
        {
            Initialize(Descriptors.CEDD, InitValues);   
        }

        /// <summary>
        /// Initializes the specified descriptor.
        /// </summary>
        /// <param name="Descriptor">The descriptor.</param>
        /// <param name="InitValues">The init values.</param>
        private void Initialize(Descriptors Descriptor, float[] InitValues)
        {
            switch (Descriptor)
            {
                default:
                case Descriptors.CEDD:
                    Weights = new float[6, 8, 3];
                    WGrid1 = new float[6, 8, 3, 144];
                    break;
                case Descriptors.FCTH:
                    Weights = new float[8, 8, 3];
                    WGrid1 = new float[8, 8, 3, 192];
                    break;
                case Descriptors.CCEDD:
                    Weights = new float[6, 10, 1];
                    WGrid1 = new float[6, 10, 1, 60];
                    break;
                case Descriptors.CFCTH:
                    Weights = new float[8, 10, 1];
                    WGrid1 = new float[8, 10, 1, 80];
                    break;
                case Descriptors.JCD:
                    Weights = new float[7, 8, 3];
                    //define kohonen topology                    
                    WGrid1 = new float[7, 8, 3, 168];
                    //RGBColors = new int[,] { { 255, 255, 255 }, { 128, 128, 128 }, { 0, 0, 0 }, {255,0,0},
                    //    {255,140,0}, {255,255,0}, {0,255,0}, {0,255,255}, {0,0,255}, {255,0,255} };
                    break;
            }
            Number_X = WGrid1.GetLength(0);
            Number_Y = WGrid1.GetLength(1);
            Number_Z = WGrid1.GetLength(2);

            for (x = 0; x < Number_X; x++)
            {
                for (y = 0; y < Number_Y; y++)
                {
                    for (z = 0; z < Number_Z; z++)
                    {
                        WGrid1[x, y, z, x * Number_Y * Number_Z + y * Number_Z + z] = InitValues[x * Number_Y * Number_Z + y * Number_Z + z];
                    }
                }
            }
        }


        int GetX(int pos)
        {
            return pos / (Number_Y*Number_Z);
        }

        int GetY(int pos)
        {
            return (pos - GetX(pos) * Number_Y * Number_Z)/Number_Z;
        }

        int GetZ(int pos)
        {
            return (pos - (GetX(pos) * Number_Y * Number_Z + GetY(pos) * Number_Z));
        }

        /// <summary>
        /// Apply the new descriptor.
        /// </summary>
        /// <param name="Values">The new descriptor values.</param>
        public void ApplyNewValues(byte[] Values)
        {
            ApplyNewValues(Values.Select(x => Convert.ToSingle(x)).ToArray());
        }

        /// <summary>
        /// Apply the new descriptor.
        /// </summary>
        /// <param name="Values">The new descriptor values.</param>
        public void ApplyNewValues(float[] Values)
        {
            x1 = new float[Values.Length];
            Time++;
            for (i = 0; i < Values.Length; i++)
            {
                for (y = 0; y < Values.Length; y++)
                    x1[y] = 0;
                x1[i] = Values[i];
                Win1.x = GetX(i);
                Win1.y = GetY(i);
                Win1.z = GetZ(i);
                UpdateWeights();
            }
        }

        private void UpdateWeights()
        {

            float CurrentLR = LearningRate();
            WGrid1[Win1.x, Win1.y, Win1.z, Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z] += CurrentLR * (x1[Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z] - WGrid1[Win1.x, Win1.y, Win1.z, Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z]);
            
            for (x = 0; x < Number_X; x++)
                if (x != Win1.x)
                    WGrid1[x, Win1.y, Win1.z, x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z] += GetXDistanceVariance()*CurrentLR * (x1[Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z] - WGrid1[Win1.x, Win1.y, Win1.z, Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z]);
            
            if (Win1.y > 3)
                for (z = 0; z < Number_Z; z++)
                    if (z != Win1.z)
                        WGrid1[Win1.x, Win1.y, z, Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + z] += GetZDistanceVariance(Math.Abs(z - Win1.z)) * CurrentLR * (x1[Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z] - WGrid1[Win1.x, Win1.y, Win1.z, Win1.x * Number_Y * Number_Z + Win1.y * Number_Z + Win1.z]);
        }

        private float GetXDistanceVariance()
        {
            return (float)Number_Y *Number_Z/ 100;
        }

        private float GetZDistanceVariance(int distance)
        {
                return (float)(Number_Y / (100 *distance));
        }

        private int GetTexturePosition(int pos)
        {
            return pos / Number_Y;
        }
        
        private float GetColorVariable(int distance)
        {
            return 7 * Number_Y * distance / 765;
        }

        private float GetTextureVariable(int distance)
        {
            if (distance == 0)
                return 0;
            else
                return Number_Y / 2;
        }

        /// <summary>
        /// Gets the new descriptor.
        /// </summary>
        /// <returns></returns>
        public float[] GetNewDescriptor()
        {
            returnWeights = new float[Number_X*Number_Y*Number_Z];
            for (x = 0; x < Number_X; x++)
                for (y = 0; y < Number_Y; y++)
                    for (z = 0; z < Number_Z; z++)
                        returnWeights[x * Number_Y * Number_Z + y * Number_Z + z] = WGrid1[x, y, z, x * Number_Y * Number_Z + y * Number_Z + z];
            return returnWeights;
        }

        private float LearningRate()
        {
            return Convert.ToSingle(Ei * Math.Pow((Ef / Ei), (Time / (double)Tmax)));
        }
    }
}
