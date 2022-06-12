using System.IO;
using InsideFabric.Data;

namespace Jaipur
{
    public class ImageInfo
    {
        public string Filename { get; set; }
        public string ImageUrl { get; set; }
        public ImageVariantType Type { get; set; }

        public ImageInfo(string filename, string imageUrl, ImageVariantType type)
        {
            Filename = filename;
            ImageUrl = imageUrl;
            Type = type;
        }

        public string GetFullUrl()
        {
            return Path.Combine(ImageUrl, Filename);
        }

        public string GetSku()
        {
            return Filename.Replace(".jpg", "");
        }
    }
}