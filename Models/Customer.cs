using System.ComponentModel.DataAnnotations;

namespace PharmacyManagmentSystem.Models
{
	public class Customer
	{
		public int CustomerId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[Required]
		[Phone]
		public string Phone { get; set; } = string.Empty;

		[EmailAddress]
		public string? Email { get; set; }

		public string? Address { get; set; }

		public ICollection<Sale> Sales { get; set; } = new List<Sale>();
	}
}