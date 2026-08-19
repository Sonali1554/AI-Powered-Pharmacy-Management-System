using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Sale> Sales { get; set; }

        public DbSet<SaleItem> SaleItems { get; set; }

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
        }
    }
}