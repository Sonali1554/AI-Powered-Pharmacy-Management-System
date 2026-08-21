using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        // Existing team tables
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Sale> Sales { get; set; }

        public DbSet<SaleItem> SaleItems { get; set; }

        // Inventory tables
        public DbSet<Medicine> Medicines { get; set; }

        public DbSet<MedicineCategory> MedicineCategories { get; set; }

        public DbSet<Batch> Batches { get; set; }

        public DbSet<StockHistory> StockHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sale decimal fields
            modelBuilder.Entity<Sale>()
                .Property(s => s.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sale>()
                .Property(s => s.DiscountPercentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Sale>()
                .Property(s => s.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasPrecision(18, 2);

            // SaleItem decimal fields
            modelBuilder.Entity<SaleItem>()
                .Property(s => s.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SaleItem>()
                .Property(s => s.TotalPrice)
                .HasPrecision(18, 2);

            // Inventory Medicine decimal fields
            modelBuilder.Entity<Medicine>()
                .Property(m => m.PurchasePrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Medicine>()
                .Property(m => m.SellingPrice)
                .HasPrecision(10, 2);
        }
    }
}