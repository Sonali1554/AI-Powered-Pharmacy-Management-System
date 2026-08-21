using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Models;

namespace PharmacyManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedicineCategory> MedicineCategories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique index on Purchase.InvoiceNumber
            modelBuilder.Entity<Purchase>()
                .HasIndex(p => p.InvoiceNumber)
                .IsUnique();

            // Unique index on Prescription.PrescriptionNumber
            modelBuilder.Entity<Prescription>()
                .HasIndex(p => p.PrescriptionNumber)
                .IsUnique();

            // Supplier -> Purchases: Restrict delete (prevent deleting supplier with purchases)
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Purchase -> PurchaseItems: Cascade delete
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // PurchaseItem -> Medicine: Restrict delete
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Medicine)
                .WithMany()
                .HasForeignKey(pi => pi.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> PrescriptionItems: Cascade delete
            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Prescription)
                .WithMany(p => p.PrescriptionItems)
                .HasForeignKey(pi => pi.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // PrescriptionItem -> Medicine: Restrict delete
            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Medicine)
                .WithMany()
                .HasForeignKey(pi => pi.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}