using System.ComponentModel.DataAnnotations;

namespace PharmacyManagmentSystem.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        [StringLength(20)]
        [Display(Name = "Prescription Number")]
        public string PrescriptionNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [Display(Name = "Patient Age")]
        [Range(0, 150)]
        public int? PatientAge { get; set; }

        [StringLength(10)]
        [Display(Name = "Gender")]
        public string? PatientGender { get; set; }

        [StringLength(100)]
        [Display(Name = "Doctor Name")]
        public string? DoctorName { get; set; }

        [StringLength(20)]
        [Display(Name = "Doctor Contact")]
        public string? DoctorContact { get; set; }

        [Required]
        [Display(Name = "Prescription Date")]
        [DataType(DataType.Date)]
        public DateTime PrescriptionDate { get; set; } = DateTime.Today;

        [Required]
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;

        [StringLength(100)]
        [Display(Name = "Verified By")]
        public string? VerifiedBy { get; set; }

        [Display(Name = "Verified Date")]
        [DataType(DataType.Date)]
        public DateTime? VerifiedDate { get; set; }

        public string? Notes { get; set; }

        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}
