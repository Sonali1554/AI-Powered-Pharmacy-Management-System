using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Models
{
    public class MedicineCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    }
}