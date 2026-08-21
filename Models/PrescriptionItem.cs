using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementSystem.Models
{
    public class PrescriptionItem
    {
        [Key]
        public int PrescriptionItemId { get; set; }

        [Required]
        public int PrescriptionId { get; set; }

        [Required]
        [Display(Name = "Medicine")]
        public int MedicineId { get; set; }

        [StringLength(50)]
        public string? Dosage { get; set; }

        [StringLength(50)]
        public string? Frequency { get; set; }

        [StringLength(50)]
        public string? Duration { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public string? Instructions { get; set; }

        [ForeignKey("PrescriptionId")]
        public Prescription? Prescription { get; set; }

        [ForeignKey("MedicineId")]
        public Medicine? Medicine { get; set; }
    }
}
