using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementSystem.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(50)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "GST Amount")]
        public decimal GSTAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "Net Amount")]
        public decimal NetAmount { get; set; }

        [Required]
        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string? Notes { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
