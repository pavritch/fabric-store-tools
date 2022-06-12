using System.IO;
using System.IO.Compression;
using System.Text;

namespace Utilities
{
    public static class GZipHelper
    {
        private static byte[] GZipMemory(this byte[] buffer)
        {
            using (var ms = new MemoryStream())
            {
                using (var gZip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gZip.Write(buffer, 0, buffer.Length);
                }
                return ms.ToArray();
            }
        }

        private static byte[] GZipMemory(this string input)
        {
            return GZipMemory(Encoding.UTF8.GetBytes(input));
        }

        private static byte[] UnGZipMemory(this byte[] input)
        {
            //Prepare for decompress
            using (var msIn = new MemoryStream(input))
            {
                using (var gz = new GZipStream(msIn, CompressionMode.Decompress))
                {
                    //Copy the decompression stream into the output file.
                    var buffer = new byte[4096];
                    using (var msOut = new MemoryStream())
                    {
                        int numRead;
                        while ((numRead = gz.Read(buffer, 0, buffer.Length)) != 0)
                            msOut.Write(buffer, 0, numRead);

                        return msOut.ToArray();
                    }
                }
            }
        }
    }
}