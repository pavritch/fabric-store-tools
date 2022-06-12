using System.Data.Entity;
using ProductScanner.Core.DataEntities.Store;

namespace ProductScanner.StoreData
{

    public partial class StoreContext : DbContext
    {
        public StoreContext(string connectionStringName)
            : base(connectionStringName)
        {
            Database.SetInitializer<StoreContext>(null);
            this.Database.CommandTimeout = 180;
        }

        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Manufacturer> Manufacturer { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCategory> ProductCategory { get; set; }
        public virtual DbSet<ProductManufacturer> ProductManufacturer { get; set; }
        public virtual DbSet<ProductVariant> ProductVariant { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(e => e.UpsellProductDiscountPercentage)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Product>()
                .Property(e => e.ImageFilenameOverride)
                .IsUnicode(false);

            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.SalePrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.Weight)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.MSRP)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.Cost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.ImageFilenameOverride)
                .IsUnicode(false);
        }
    }
}
