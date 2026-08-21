using PharmacyManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagmentSystem.Models
{
    public class Medicine
    {
        [Key]
        public int MedicineId { get; set; }

        [Required]
        [StringLength(100)]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SellingPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int ReorderLevel { get; set; }

        public string? Description { get; set; }

        [ForeignKey("CategoryId")]
        public MedicineCategory? Category { get; set; }
    }
}