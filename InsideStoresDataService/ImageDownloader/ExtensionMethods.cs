using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageDownloader
{
    public static class ExtensionMethods
    {

        public static string SHA256Digest(this string input)
        {
            return Hashing.Hash(input, Hashing.HashingTypes.SHA256);
        }

        public static class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random Local;

            public static Random ThisThreadsRandom
            {
                get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
            }
        }


        public static void Shuffle<T>(this IList<T> list)
        {
            if (list.Count() < 4)
                return;

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        public static byte[] ReadBinaryFile(this string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var ImageData = new byte[fs.Length];
                fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));
                return ImageData;
            }
        }

        public static void WriteBinaryFile(this byte[] data, string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Preambles for well-known image formats.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/List_of_file_signatures
        /// </remarks>
        private static Dictionary<string, List<byte[]>> KnownImageBytePreambles = new Dictionary<string, List<byte[]>>() 
            {
                {".jpg", new List<byte[]> {new byte[] {0xFF, 0xD8, 0xFF, 0xE0},  new byte[] {0xFF, 0xD8, 0xFF, 0xE1}}},
                {".jpeg", new List<byte[]> {new byte[] {0xFF, 0xD8, 0xFF, 0xE0},  new byte[] {0xFF, 0xD8, 0xFF, 0xE1}}},

                {".tif", new List<byte[]> {new byte[] {0x49, 0x49, 0x2A, 0x00},  new byte[] {0x4D, 0x4D, 0x00, 0x2A}}},
                {".tiff", new List<byte[]> {new byte[] {0x49, 0x49, 0x2A, 0x00},  new byte[] {0x4D, 0x4D, 0x00, 0x2A}}},

                {".png", new List<byte[]> {new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}}},
            };

        /// <summary>
        /// Review the first few bytes of the image data to see if represents a jpg file
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static bool HasJpgImagePreamble(this byte[] imageBytes)
        {
            bool found = false;
            foreach (var sequence in KnownImageBytePreambles[".jpg"])
            {
                for (int i = 0; i < sequence.Length; i++)
                {
                    found = true;
                    if (imageBytes[i] != sequence[i])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return true;
            }

            return false;
        }


        public static bool HasImagePreamble(this byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 10)
                return false;

            // https://en.wikipedia.org/wiki/List_of_file_signatures

            // ignore file types, we just want to know if if matches any well-known preamble

            foreach (var item in KnownImageBytePreambles.Values)
            {
                bool found = false;
                foreach (var sequence in item)
                {
                    for (int i = 0; i < sequence.Length; i++)
                    {
                        found = true;
                        if (imageBytes[i] != sequence[i])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Determine if the bytes correspond to an image of the indicated type by file extension.
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool HasImagePreamble(this byte[] imageBytes, string Url)
        {
            if (imageBytes == null || imageBytes.Length < 10)
                return false;

            var extension = Path.GetExtension(Url).ToLower();

            List<byte[]> allowedBytes;
            if (KnownImageBytePreambles.TryGetValue(extension, out allowedBytes))
            {
                bool found = false;
                foreach (var sequence in allowedBytes)
                {
                    for (int i = 0; i < sequence.Length; i++)
                    {
                        found = true;
                        if (imageBytes[i] != sequence[i])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Decompresses a byte array using gzip compression.
        /// </summary>
        /// <param name="bytes">Byte array to decompress.</param>
        /// <returns>A decompressed byte array.</returns>
        public static byte[] Decompress(this byte[] bytes)
        {
            MemoryStream ms = new MemoryStream();
            int msgLength = BitConverter.ToInt32(bytes, 0);
            ms.Write(bytes, 4, bytes.Length - 4);

            byte[] buffer = new byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public static byte[] GetImageFromWeb(this string Url)
        {
            var client = new WebClient();
            var image = client.DownloadData(Url);
            return image;
        }
    }
}
