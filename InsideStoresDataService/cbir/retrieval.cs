using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace cbir
{

    /// <summary>
    /// Elementary image retrieval functions
    /// </summary>
    public class retrieval
    {
        
        private Func<byte[], float> distance;
        private float gTemp1 = 0, gTempCount3 = 0;
        private float[] gT1;


        /// <summary>
        /// Initializes a new instance of the <see cref="retrieval"/> class.
        /// </summary>
        /// <param name="descriptorQuery">The descriptor query.</param>
        public retrieval(float[] descriptorQuery)
        {
            putQueryDescriptor(descriptorQuery);
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="retrieval"/> class.
        /// </summary>
        /// <param name="descriptorQuery">The descriptor query.</param>
        public retrieval(byte[] descriptorQuery)
        {
            putQueryDescriptor(descriptorQuery);
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="retrieval"/> class. Not ready for retrieval operations needs to call putQueryDescriptor function.
        /// </summary>
        public retrieval() // 
        {
            
            Initialize();
        }

        /// <summary>
        /// Gets the approximately descriptor colors.
        /// </summary>
        /// <returns></returns>
        public static Color[] getDescriptorColors()
        {
            Color[] descriptorColors = new Color[24];
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
            descriptorColors[0] = Colors.White;
            descriptorColors[1] = Colors.Gray;
            descriptorColors[2] = Colors.Black;
            descriptorColors[3] = Color.FromArgb(255, 255, 204, 204);
            descriptorColors[4] = Colors.Red;
            descriptorColors[5] = Colors.DarkRed;
            descriptorColors[6] = Color.FromArgb(255, 255, 213, 142);
            descriptorColors[7] = Colors.Orange;
            descriptorColors[8] = Colors.DarkOrange;
            descriptorColors[9] = Colors.LightYellow;
            descriptorColors[10] = Colors.Yellow;
            descriptorColors[11] = Color.FromArgb(255, 150, 150, 0);
            descriptorColors[12] = Colors.LightGreen;
            descriptorColors[13] = Colors.Green;
            descriptorColors[14] = Colors.DarkGreen;
            descriptorColors[15] = Colors.LightCyan;
            descriptorColors[16] = Colors.Cyan;
            descriptorColors[17] = Colors.DarkCyan;
            descriptorColors[18] = Colors.LightBlue;
            descriptorColors[19] = Colors.Blue;
            descriptorColors[20] = Colors.DarkBlue;
            descriptorColors[21] = Color.FromArgb(255, 255, 127, 255);
            descriptorColors[22] = Colors.Magenta;
            descriptorColors[23] = Colors.DarkMagenta;
            return descriptorColors;
        }
        
        #region internal Methods

        /// <summary>
        /// Puts the query descriptor.
        /// </summary>
        /// <param name="descriptorQuery">The descriptor query.</param>
        public void putQueryDescriptor(float[] descriptorQuery)
        {
            for (int i = 0; i < descriptorQuery.Length; i++)
                gTemp1 += descriptorQuery[i];
            gT1 = new float[descriptorQuery.Length];
            for (int i = 0; i < descriptorQuery.Length; i++)
            {
                gT1[i] = descriptorQuery[i] / gTemp1;
                gTempCount3 += gT1[i] * gT1[i];
            }
        }

        /// <summary>
        /// Puts the query descriptor.
        /// </summary>
        /// <param name="descriptorQuery">The descriptor query.</param>
        public void putQueryDescriptor(byte[] descriptorQuery)
        {
            for (int i = 0; i < descriptorQuery.Length; i++)
                gTemp1 += descriptorQuery[i];
            gT1 = new float[descriptorQuery.Length];
            for (int i = 0; i < descriptorQuery.Length; i++)
            {
                gT1[i] = descriptorQuery[i] / gTemp1;
                gTempCount3 += gT1[i] * gT1[i];
            }
        }

        /// <summary>
        /// Image Similarity.
        /// </summary>
        /// <param name="descriptorTarget">The descriptor target.</param>
        /// <returns></returns>
        public float howSimilar(byte[] descriptorTarget)
        {
            return distance(descriptorTarget);
        }
                
        #endregion


        #region Private Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            distance = TanimotoClassifierSpeedy;
        }

        /// <summary>
        /// Tanimoto classifier.
        /// </summary>
        /// <param name="Table1">The source descriptor.</param>
        /// <param name="Table2">The target descriptor.</param>
        /// <returns></returns>
        private float TanimotoClassifier(byte[] Table1, byte[] Table2)
        {
            float Result = 0, Temp1 = 0, Temp2 = 0;
            float TempCount1 = 0, TempCount2 = 0, TempCount3 = 0;
            int i;

            for (i = 0; i < Table1.Length; i++)
            {
                Temp1 += Table1[i];
                Temp2 += Table2[i];
            }

            if (Temp1 == 0 || Temp2 == 0) Result = 100;
            if (Temp1 == 0 && Temp2 == 0) Result = 0;

            if (Temp1 > 0 && Temp2 > 0)
            {
                for (i = 0; i < Table1.Length; i++)
                {
                    TempCount1 += (Table1[i] / Temp1) * (Table2[i] / Temp2);
                    TempCount2 += (Table2[i] / Temp2) * (Table2[i] / Temp2);
                    TempCount3 += (Table1[i] / Temp1) * (Table1[i] / Temp1);

                }

                Result = TempCount1 / (TempCount2 + TempCount3 - TempCount1); //Tanimoto
            }
            return (1 - Result);
        }

        /// <summary>
        ///Tanimoto classifier.
        /// </summary>
        /// <param name="Table1">The source descriptor.</param>
        /// <param name="Table2">The target descriptor.</param>
        /// <returns></returns>
        private float TanimotoClassifier(float[] Table1, byte[] Table2)
        {
            float Result = 0, Temp1 = 0, Temp2 = 0;
            float TempCount1 = 0, TempCount2 = 0, TempCount3 = 0;
            float T1 = 0, T2 = 0;
            int i;

            for (i = 0; i < Table1.Length; i++)
            {
                Temp1 += Table1[i];
                Temp2 += Table2[i];
            }

            if (Temp1 == 0 || Temp2 == 0)
            {
                Result = 100;
                if (Temp1 == 0 && Temp2 == 0) Result = 0;
                return Result;
            }


            for (i = 0; i < Table1.Length; i++)
            {
                T1 = Table1[i] / Temp1;
                T2 = Table2[i] / Temp2;
                TempCount1 += T1 * T2;
                TempCount2 += T2 * T2;
                TempCount3 += T1 * T1;

            }

            Result = TempCount1 / (TempCount2 + TempCount3 - TempCount1); //Tanimoto

            return (1 - Result);
        }

        /// <summary>
        /// Tanimoto classifier (speedy).
        /// </summary>
        /// <param name="Table2">The target descriptor.</param>
        /// <returns></returns>
        private float TanimotoClassifierSpeedy(byte[] Table2)
        {
            float Result = 0, Temp2 = 0;
            float TempCount1 = 0, TempCount2 = 0;
            float T2 = 0;
            int i;

            for (i = 0; i < Table2.Length; i++)
                Temp2 += Table2[i];

            if (gTemp1 == 0 || Temp2 == 0)
            {
                Result = 100;
                if (gTemp1 == 0 && Temp2 == 0) Result = 0;
                return Result;
            }


            for (i = 0; i < Table2.Length; i++)
            {
                T2 = Table2[i] / Temp2;

                TempCount1 += gT1[i] * T2;
                TempCount2 += T2 * T2;

            }

            Result = TempCount1 / (TempCount2 + gTempCount3 - TempCount1); //Tanimoto

            return (1 - Result);
        } 
        #endregion
    }

}
