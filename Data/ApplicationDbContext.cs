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
    }
}