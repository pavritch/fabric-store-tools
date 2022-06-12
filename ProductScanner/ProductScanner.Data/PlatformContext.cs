using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using ProductScanner.Core.PlatformEntities;

namespace ProductScanner.Data
{
    public class PlatformContext : DbContext
    {
        public PlatformContext() : base("name=PlatformContext")
        {
            Database.SetInitializer<PlatformContext>(null);
        }

        public virtual IDbSet<VendorData> VendorDatas { get; set; }
        public virtual IDbSet<DetailUrl> DetailUrls { get; set; }
        public virtual IDbSet<ScannerCommit> ScannerCommits { get; set; }
        public virtual IDbSet<ScanLog> ScanLogs { get; set; }
        public virtual IDbSet<ScannerCheckpoint> ScannerCheckpoints { get; set; }
        public virtual IDbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}