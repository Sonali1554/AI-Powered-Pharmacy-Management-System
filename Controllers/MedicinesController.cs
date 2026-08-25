using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Data;
using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Controllers
{
    public class MedicinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var medicines = _context.Medicines
                .Include(m => m.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                medicines = medicines.Where(m =>
                    m.MedicineName.Contains(searchString) ||
                    m.BatchNumber.Contains(searchString) ||
                    (m.Manufacturer != null && m.Manufacturer.Contains(searchString)));
            }

            ViewData["SearchString"] = searchString;

            return View(await medicines.ToListAsync());
        }

        public async Task<IActionResult> Details(int? medicineid)
        {
            if (medicineid == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MedicineId == medicineid);

            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(
                await _context.MedicineCategories.ToListAsync(),
                "CategoryId",
                "CategoryName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MedicineId,MedicineName,CategoryId,Manufacturer,BatchNumber,ExpiryDate,PurchasePrice,SellingPrice,Quantity,ReorderLevel,Description")]
            Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                _context.Medicines.Add(medicine);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(
                await _context.MedicineCategories.ToListAsync(),
                "CategoryId",
                "CategoryName",
                medicine.CategoryId);

            return View(medicine);
        }

        public async Task<IActionResult> Edit(int? medicineid)
        {
            if (medicineid == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines.FindAsync(medicineid);

            if (medicine == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(
                await _context.MedicineCategories.ToListAsync(),
                "CategoryId",
                "CategoryName",
                medicine.CategoryId);

            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int medicineid,
            [Bind("MedicineId,MedicineName,CategoryId,Manufacturer,BatchNumber,ExpiryDate,PurchasePrice,SellingPrice,Quantity,ReorderLevel,Description")]
            Medicine medicine)
        {
            if (medicineid != medicine.MedicineId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicineExists(medicine.MedicineId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(
                await _context.MedicineCategories.ToListAsync(),
                "CategoryId",
                "CategoryName",
                medicine.CategoryId);

            return View(medicine);
        }

        public async Task<IActionResult> Delete(int? medicineid)
        {
            if (medicineid == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MedicineId == medicineid);

            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int medicineid)
        {
            var medicine = await _context.Medicines.FindAsync(medicineid);

            if (medicine != null)
            {
                _context.Medicines.Remove(medicine);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MedicineExists(int medicineid)
        {
            return _context.Medicines.Any(e => e.MedicineId == medicineid);
        }
    }
}
