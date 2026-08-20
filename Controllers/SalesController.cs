using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Data;
using PharmacyManagmentSystem.Models;
using Microsoft.AspNetCore.SignalR;

namespace PharmacyManagmentSystem.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<PharmacyManagmentSystem.Hubs.AnalyticsHub> _hubContext;

        public SalesController(ApplicationDbContext context, Microsoft.AspNetCore.SignalR.IHubContext<PharmacyManagmentSystem.Hubs.AnalyticsHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Sales History
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return View(sales);
        }

        // Create Bill Page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Save Bill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Customer customer,
            List<SaleItem> saleItems,
            decimal discountPercentage,
            string paymentMethod)
        {
            // Check customer
            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                ModelState.AddModelError("", "Customer name is required.");
            }

            // Check medicines
            if (saleItems == null || saleItems.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one medicine.");
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            // Calculate every item's total
            foreach (var item in saleItems)
            {
                item.TotalPrice =
                    item.Quantity * item.UnitPrice;
            }

            // Calculate subtotal
            decimal subtotal = saleItems.Sum(
                item => item.TotalPrice);

            // Calculate discount
            decimal discountAmount =
                subtotal * discountPercentage / 100;

            // Calculate final amount
            decimal totalAmount =
                subtotal - discountAmount;

            // Save customer
            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            // Create Sale
            var sale = new Sale
            {
                CustomerId = customer.CustomerId,
                SaleDate = DateTime.Now,
                Subtotal = subtotal,
                DiscountPercentage = discountPercentage,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,

                SaleItems = saleItems
            };

            // Save sale + ALL sale items
            _context.Sales.Add(sale);

            await _context.SaveChangesAsync();

            // Notify connected dashboard clients to update in real-time
            await _hubContext.Clients.All.SendAsync("UpdateDashboard");

            // Go to invoice
            return RedirectToAction(
                nameof(Details),
                new { id = sale.SaleId });
        }

        // Invoice / Bill Details
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }
    }
}