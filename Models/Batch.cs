using System.ComponentModel.DataAnnotations;

namespace PharmacyManagmentSystem.Models
{
    public class Batch
    {
        public int BatchID { get; set; }

        public int MedicineID { get; set; }

        [Required]
        public string BatchNumber { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int MinimumStock { get; set; }

        public DateTime ManufacturingDate { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}