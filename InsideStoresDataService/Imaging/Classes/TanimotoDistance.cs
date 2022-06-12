using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsideStores.Imaging
{
    public class TanimotoDistance
    {

        // can either use static methods, or if going to compare a bunch to a single source,
        // can new TanimotoDistance() and pass in the descriptor, in which case it will be
        // pre-processed to help speed up later calculations.

        private float gTemp1 = 0, gTempCount3 = 0;
        private float[] gT1;


        /// <summary>
        /// Initializes a new instance of the <see cref="retrieval"/> class.
        /// </summary>
        /// <param name="descriptorQuery">The descriptor query.</param>
        public TanimotoDistance(float[] descriptorQuery)
        {
            SetQueryDescriptor(descriptorQuery);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="retrieval"/> class.
        /// </summary>
        /// <param name="descriptorQuery">The descriptor query.</param>
        public TanimotoDistance(byte[] descriptorQuery)
        {
            SetQueryDescriptor(descriptorQuery);
        }


        /// <param name="descriptorQuery">The descriptor query.</param>
        private void SetQueryDescriptor(float[] descriptorQuery)
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
        private void SetQueryDescriptor(byte[] descriptorQuery)
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
        /// GetDistance. Tanimoto classifier.
        /// </summary>
        /// <param name="Table1">The source descriptor.</param>
        /// <param name="Table2">The target descriptor.</param>
        /// <returns></returns>
        public static float GetDistance(byte[] Table1, byte[] Table2)
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
        /// GetDistance. Tanimoto classifier.
        /// </summary>
        /// <param name="Table1">The source descriptor.</param>
        /// <param name="Table2">The target descriptor.</param>
        /// <returns></returns>
        public static float GetDistance(float[] Table1, byte[] Table2)
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
        /// Compute distance (similarity) between local descriptor and target. Tanimoto classifier.
        /// </summary>
        /// <param name="descriptorTarget">The descriptor target.</param>
        /// <returns></returns>
        public float GetDistance(byte[] descriptorTarget)
        {
            // this is faster because it uses a pre-computed T1 for repeated calls relating 
            // to a single image.

            // it is caller's repsonsibility to ensure that the target descriptor matches up to source.
            
            return TanimotoClassifierSpeedy(descriptorTarget);
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

    }
}
