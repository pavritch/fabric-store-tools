using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.Core.PlatformEntities
{
    public class VendorData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public StoreType Store { get; set; }
        public string Name { get; set; }
        public VendorStatus Status { get; set; }
    }
}
