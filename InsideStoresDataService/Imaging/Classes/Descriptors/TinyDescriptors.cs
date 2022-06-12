using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace InsideStores.Imaging.Descriptors
{

    public class TinyDescriptors
    {
        /// <summary>
        /// Turn a standard CEDD descriptor into Haar wavelet transform for fast fuzzy distance metrics.
        /// </summary>
        /// <remarks>
        /// To tell the difference between two tiny decriptors, take the XOR and count the resulting bits.
        /// </remarks>
        /// <param name="cedd"></param>
        /// <returns></returns>
        public static uint MakeTinyDescriptor(byte[] cedd)
        {
            uint tinyCEDD = 0;
            bool isFirstTime = true;

            Func<List<byte[]>, List<byte[]>> splitVector = (fragmentGroup) =>
                {
                    var splitList = new List<byte[]>();

                    // all should be the same, so take first
                    var inputCount = fragmentGroup[0].Length; 
                    var splitHalfCount = inputCount/2;

                    foreach(var fragment in fragmentGroup)
                    {
                        byte[] left = fragment.Take(splitHalfCount).ToArray();
                        byte[] right = fragment.Skip(splitHalfCount).ToArray();

                        splitList.Add(left);
                        splitList.Add(right);

                        if (!isFirstTime)
                            tinyCEDD <<= 1;

                        if (left.Sum(e => e) >= right.Sum(e => e))
                            tinyCEDD |= 1;

                        isFirstTime = false;
                    }

                    return splitList;
                };

            var list = new List<byte[]>() { cedd };

            for (var i = 0; i < 4; i++ )
                list = splitVector(list);

            return tinyCEDD;
        }

        public static int Distance(uint d1, uint d2)
        {
            var distance = 0;
            var bits = d1 ^ d2;
            for(var i=0; i < 16; i++)
            {
                if ((bits & 1) > 0)
                    distance++;
                bits >>= 1;
            }
            return distance;
        }
    }
}
