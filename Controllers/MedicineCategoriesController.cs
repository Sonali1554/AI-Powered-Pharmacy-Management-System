using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Data;
using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Controllers
{
    public class MedicineCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicineCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.MedicineCategories.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicineCategory medicineCategory)
        {
            if (ModelState.IsValid)
            {
                _context.MedicineCategories.Add(medicineCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(medicineCategory);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicineCategory = await _context.MedicineCategories.FindAsync(id);

            if (medicineCategory == null)
            {
                return NotFound();
            }

            return View(medicineCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicineCategory medicineCategory)
        {
            if (id != medicineCategory.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(medicineCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(medicineCategory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicineCategory = await _context.MedicineCategories
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (medicineCategory == null)
            {
                return NotFound();
            }

            return View(medicineCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicineCategory = await _context.MedicineCategories.FindAsync(id);

            if (medicineCategory != null)
            {
                _context.MedicineCategories.Remove(medicineCategory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
