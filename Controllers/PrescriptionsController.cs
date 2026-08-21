using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Data;
using PharmacyManagementSystem.Models;

namespace PharmacyManagementSystem.Controllers
{
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var prescriptions = _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                prescriptions = prescriptions.Where(p =>
                    p.PatientName.Contains(searchString) ||
                    p.PrescriptionNumber.Contains(searchString) ||
                    (p.DoctorName != null && p.DoctorName.Contains(searchString)));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<PrescriptionStatus>(statusFilter, out var status))
            {
                prescriptions = prescriptions.Where(p => p.Status == status);
            }

            ViewData["SearchString"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            return View(await prescriptions.OrderByDescending(p => p.PrescriptionDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(pi => pi.Medicine)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Medicines"] = new SelectList(
                await _context.Medicines.ToListAsync(),
                "MedicineId",
                "MedicineName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            if (ModelState.IsValid)
            {
                // Auto-generate prescription number
                var today = DateTime.Today;
                var countToday = await _context.Prescriptions
                    .CountAsync(p => p.PrescriptionDate.Date == today);
                prescription.PrescriptionNumber = $"RX-{today:yyyyMMdd}-{(countToday + 1):D4}";

                prescription.Status = PrescriptionStatus.Pending;

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["Medicines"] = new SelectList(
                await _context.Medicines.ToListAsync(),
                "MedicineId",
                "MedicineName");

            return View(prescription);
        }

        public async Task<IActionResult> Verify(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(pi => pi.Medicine)
                        .ThenInclude(m => m!.Category)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            if (prescription.Status != PrescriptionStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(prescription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyConfirmed(int id, string verifiedBy)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            if (prescription.Status != PrescriptionStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            prescription.Status = PrescriptionStatus.Verified;
            prescription.VerifiedBy = verifiedBy;
            prescription.VerifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispense(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            if (prescription.Status != PrescriptionStatus.Verified)
            {
                TempData["ErrorMessage"] = "Only verified prescriptions can be dispensed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Deduct stock for each prescription item
                foreach (var item in prescription.PrescriptionItems)
                {
                    var medicine = await _context.Medicines.FindAsync(item.MedicineId);
                    if (medicine != null)
                    {
                        if (medicine.Quantity < item.Quantity)
                        {
                            TempData["ErrorMessage"] = $"Insufficient stock for {medicine.MedicineName}. Available: {medicine.Quantity}, Required: {item.Quantity}";
                            return RedirectToAction(nameof(Details), new { id });
                        }

                        medicine.Quantity -= item.Quantity;
                    }
                }

                prescription.Status = PrescriptionStatus.Dispensed;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> History(string patientName)
        {
            if (string.IsNullOrWhiteSpace(patientName))
            {
                return View(new List<Prescription>());
            }

            var prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(pi => pi.Medicine)
                .Where(p => p.PatientName.Contains(patientName))
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            ViewData["PatientName"] = patientName;

            return View(prescriptions);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(pi => pi.Medicine)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
