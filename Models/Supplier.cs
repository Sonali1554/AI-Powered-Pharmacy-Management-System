using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "GST Number")]
        public string? GSTNumber { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
