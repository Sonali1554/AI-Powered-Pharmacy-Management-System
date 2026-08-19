using System.ComponentModel.DataAnnotations;

namespace PharmacyManagmentSystem.Models
{
    public class Sale
    {
        public int SaleId { get; set; }

        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Range(0, double.MaxValue)]
        public decimal Subtotal { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; } = "Cash";

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}