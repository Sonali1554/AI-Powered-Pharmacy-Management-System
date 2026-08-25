using Microsoft.AspNetCore.Mvc;
using PharmacyManagmentSystem.Data;
using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var batches = _context.Batches.ToList();

            var stockHistory = _context.StockHistories
                .OrderByDescending(h => h.Date)
                .ToList();

            ViewBag.StockHistory = stockHistory;

            return View(batches);
        }

        [HttpPost]
        public IActionResult AddStock(
            string medicineName,
            string batchNumber,
            int quantity,
            DateTime expiryDate,
            int minimumStock)
        {
            var batch = new Batch
            {
                BatchNumber = batchNumber,
                Quantity = quantity,
                ExpiryDate = expiryDate,
                MinimumStock = minimumStock
            };

            _context.Batches.Add(batch);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateStock(
            string batchNumber,
            int amount,
            string action)
        {
            var batch = _context.Batches
                .FirstOrDefault(b => b.BatchNumber == batchNumber);

            if (batch != null)
            {
                if (action == "add")
                {
                    batch.Quantity += amount;
                }
                else if (action == "remove")
                {
                    batch.Quantity -= amount;

                    if (batch.Quantity < 0)
                    {
                        batch.Quantity = 0;
                    }
                }

                var history = new StockHistory
                {
                    BatchNumber = batchNumber,
                    QuantityChange = amount,
                    Action = action,
                    Date = DateTime.Now
                };

                _context.StockHistories.Add(history);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
