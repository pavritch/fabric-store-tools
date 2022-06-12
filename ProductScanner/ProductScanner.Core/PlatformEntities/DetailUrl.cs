using System.ComponentModel.DataAnnotations.Schema;

namespace ProductScanner.Core.PlatformEntities
{
    public class DetailUrl
    {
        public int Id { get; set; }

        [Index]
        public int VariantId { get; set; }
        public int VendorId { get; set; }
        public string Url { get; set; }

        public string Store { get; set; }
    }
}