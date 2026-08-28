using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManagmentSystem.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PharmacyManagmentSystem.Services.AIDemandPredictionService _aiPredictionService;

        public AnalyticsController(ApplicationDbContext context, PharmacyManagmentSystem.Services.AIDemandPredictionService aiPredictionService)
        {
            _context = context;
            _aiPredictionService = aiPredictionService;
        }

        // GET: Analytics/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaryStats()
        {
            var totalRevenue = await _context.Sales.SumAsync(s => s.TotalAmount);
            var totalSalesCount = await _context.Sales.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalDiscounts = await _context.Sales.SumAsync(s => s.DiscountAmount);
            var avgOrderValue = totalSalesCount > 0 ? totalRevenue / totalSalesCount : 0;

            return Json(new
            {
                totalRevenue,
                totalSalesCount,
                totalCustomers,
                avgOrderValue,
                totalDiscounts
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueTrends()
        {
            var sales = await _context.Sales
                .Select(s => new { s.SaleDate, s.TotalAmount, s.DiscountAmount })
                .ToListAsync();

            var trends = sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(s => s.TotalAmount),
                    Discounts = g.Sum(s => s.DiscountAmount)
                })
                .OrderBy(t => t.Date)
                .ToList();

            return Json(trends);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopMedicinesByVolume()
        {
            var topMedicines = await _context.SaleItems
                .GroupBy(si => si.MedicineName)
                .Select(g => new
                {
                    MedicineName = g.Key,
                    TotalQuantity = g.Sum(si => si.Quantity)
                })
                .OrderByDescending(m => m.TotalQuantity)
                .Take(10)
                .ToListAsync();

            return Json(topMedicines);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopMedicinesByRevenue()
        {
            var topMedicines = await _context.SaleItems
                .GroupBy(si => si.MedicineName)
                .Select(g => new
                {
                    MedicineName = g.Key,
                    TotalRevenue = g.Sum(si => si.TotalPrice)
                })
                .OrderByDescending(m => m.TotalRevenue)
                .Take(10)
                .ToListAsync();

            return Json(topMedicines);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopCustomers()
        {
            var topCustomers = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.Customer != null)
                .GroupBy(s => new { s.CustomerId, s.Customer.Name })
                .Select(g => new
                {
                    CustomerName = g.Key.Name,
                    TotalSpent = g.Sum(s => s.TotalAmount),
                    SalesCount = g.Count()
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToListAsync();

            return Json(topCustomers);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentMethodsBreakdown()
        {
            var breakdown = await _context.Sales
                .GroupBy(s => s.PaymentMethod)
                .Select(g => new
                {
                    PaymentMethod = string.IsNullOrEmpty(g.Key) ? "Unknown" : g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.TotalAmount)
                })
                .ToListAsync();

            return Json(breakdown);
        }

        [HttpGet]
        public async Task<IActionResult> GetReorderRecommendations()
        {
            var topMedicines = await _context.SaleItems
                .GroupBy(si => si.MedicineName)
                .Select(g => new { MedicineName = g.Key, TotalSold = g.Sum(si => si.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();

            var recommendations = new List<object>();

            foreach (var medicine in topMedicines)
            {
                var predictedDemand = await _aiPredictionService.PredictDemandAsync(medicine.MedicineName, 30);
                
                // If predicted demand for next 30 days is high
                if (predictedDemand > medicine.TotalSold * 0.1f) // Example threshold
                {
                    recommendations.Add(new
                    {
                        MedicineName = medicine.MedicineName,
                        Velocity = predictedDemand > medicine.TotalSold ? "Very High" : "High",
                        TotalSold = medicine.TotalSold,
                        Reason = $"AI predicts {predictedDemand:F0} units needed next month"
                    });
                }
            }

            return Json(recommendations);
        }

        [HttpGet]
        public async Task<IActionResult> GetAiDemandPredictions()
        {
            var topMedicines = await _context.SaleItems
                .GroupBy(si => si.MedicineName)
                .Select(g => g.Key)
                .Take(5)
                .ToListAsync();

            var predictions = new List<object>();

            foreach (var medName in topMedicines)
            {
                var predictedQuantity = await _aiPredictionService.PredictDemandAsync(medName, 30);
                predictions.Add(new
                {
                    MedicineName = medName,
                    PredictedDemand = Math.Round(predictedQuantity)
                });
            }

            return Json(predictions);
        }
    }
}
