using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Data;
using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var purchases = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                purchases = purchases.Where(p =>
                    p.InvoiceNumber.Contains(searchString) ||
                    (p.Supplier != null && p.Supplier.SupplierName.Contains(searchString)));
            }

            ViewData["SearchString"] = searchString;

            return View(await purchases.OrderByDescending(p => p.PurchaseDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Medicine)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["SupplierId"] = new SelectList(
                await _context.Suppliers.Where(s => s.IsActive).ToListAsync(),
                "SupplierId",
                "SupplierName");

            ViewData["Medicines"] = new SelectList(
                await _context.Medicines.ToListAsync(),
                "MedicineId",
                "MedicineName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Calculate amounts for each item
                    foreach (var item in purchase.PurchaseItems)
                    {
                        item.Amount = item.Quantity * item.PurchasePrice;
                    }

                    // Calculate totals
                    purchase.TotalAmount = purchase.PurchaseItems.Sum(i => i.Amount);
                    purchase.NetAmount = purchase.TotalAmount - purchase.Discount + purchase.GSTAmount;

                    _context.Purchases.Add(purchase);
                    await _context.SaveChangesAsync();

                    // Update medicine stock for each purchase item
                    foreach (var item in purchase.PurchaseItems)
                    {
                        var medicine = await _context.Medicines.FindAsync(item.MedicineId);
                        if (medicine != null)
                        {
                            medicine.Quantity += item.Quantity;
                            medicine.BatchNumber = item.BatchNumber;
                            medicine.ExpiryDate = item.ExpiryDate;
                            medicine.PurchasePrice = item.PurchasePrice;
                            medicine.SellingPrice = item.SellingPrice;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            ViewData["SupplierId"] = new SelectList(
                await _context.Suppliers.Where(s => s.IsActive).ToListAsync(),
                "SupplierId",
                "SupplierName",
                purchase.SupplierId);

            ViewData["Medicines"] = new SelectList(
                await _context.Medicines.ToListAsync(),
                "MedicineId",
                "MedicineName");

            return View(purchase);
        }

        public async Task<IActionResult> Invoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Medicine)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Medicine)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase != null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Reverse the stock additions
                    foreach (var item in purchase.PurchaseItems)
                    {
                        var medicine = await _context.Medicines.FindAsync(item.MedicineId);
                        if (medicine != null)
                        {
                            medicine.Quantity = Math.Max(0, medicine.Quantity - item.Quantity);
                        }
                    }

                    _context.Purchases.Remove(purchase);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
