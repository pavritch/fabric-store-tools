using System;
using System.Collections.Generic;
using System.Text;

namespace InsideStores.Imaging.Descriptors
{
    public class JCD
    {

        public static double[] MakeJointDescriptor(double[] CEDD, double[] FCTH)
        {

            double[] JointDescriptor = new double[168];

            double[] TempTable1 = new double[24];
            double[] TempTable2 = new double[24];
            double[] TempTable3 = new double[24];
            double[] TempTable4 = new double[24];

            for (int i = 0; i < 24; i++)
            {
                TempTable1[i] = FCTH[0 + i] + FCTH[96 + i];
                TempTable2[i] = FCTH[24 + i] + FCTH[120 + i];
                TempTable3[i] = FCTH[48 + i] + FCTH[144 + i];
                TempTable4[i] = FCTH[72 + i] + FCTH[168 + i];

            }

            // 

            for (int i = 0; i < 24; i++)
            {
                JointDescriptor[i] = (TempTable1[i] + CEDD[i]) / 2; //ok
                JointDescriptor[24 + i] = (TempTable2[i] + CEDD[48 + i]) / 2; //ok
                JointDescriptor[48 + i] = CEDD[96 + i]; //ok
                JointDescriptor[72 + i] = (TempTable3[i] + CEDD[72 + i]) / 2;//ok
                JointDescriptor[96 + i] = CEDD[120 + i]; //ok
                JointDescriptor[120 + i] = TempTable4[i];//ok
                JointDescriptor[144 + i] = CEDD[24 + i];//ok

            }

            return (JointDescriptor);

        }

    }
}
