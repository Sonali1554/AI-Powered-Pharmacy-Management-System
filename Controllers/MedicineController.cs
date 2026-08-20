using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Data;
using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Controllers
{
    [Authorize]
    public class MedicineController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicineController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Medicine
        public async Task<IActionResult> Index()
        {
            var medicines = await _context.Medicines
                .OrderBy(m => m.Name)
                .ToListAsync();
            return View(medicines);
        }

        // GET: Medicine/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Medicine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Category,Price,StockQuantity,ExpiryDate")] Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(medicine);
        }

        // We can add Edit/Delete methods later if requested, but for now Create is requested.
    }
}