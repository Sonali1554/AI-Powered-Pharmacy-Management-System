using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Controllers;
using PharmacyManagementSystem.Data;
using PharmacyManagementSystem.Models;
using Xunit;

namespace PharmacyManagementSystem.Tests
{
    public class PurchasesControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private async Task<(Supplier supplier, MedicineCategory category, Medicine medicine)> SeedBaseData(ApplicationDbContext context)
        {
            var category = new MedicineCategory { CategoryName = "Antibiotics" };
            context.MedicineCategories.Add(category);
            await context.SaveChangesAsync();

            var supplier = new Supplier { SupplierName = "Test Supplier", IsActive = true };
            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            var medicine = new Medicine
            {
                MedicineName = "Paracetamol",
                CategoryId = category.CategoryId,
                BatchNumber = "OLD-001",
                ExpiryDate = DateTime.Today.AddYears(1),
                PurchasePrice = 5.00m,
                SellingPrice = 10.00m,
                Quantity = 50,
                ReorderLevel = 10
            };
            context.Medicines.Add(medicine);
            await context.SaveChangesAsync();

            return (supplier, category, medicine);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithPurchases()
        {
            // Arrange
            var context = GetDbContext("PurchasesIndex");
            var (supplier, _, _) = await SeedBaseData(context);

            context.Purchases.Add(new Purchase
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = "INV-001",
                PurchaseDate = DateTime.Today,
                TotalAmount = 100
            });
            await context.SaveChangesAsync();

            var controller = new PurchasesController(context);

            // Act
            var result = await controller.Index(null!);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Purchase>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_Post_SavesPurchaseAndAddsStock()
        {
            // Arrange
            var context = GetDbContext("PurchasesCreateStock");
            var (supplier, _, medicine) = await SeedBaseData(context);

            var initialStock = medicine.Quantity; // 50

            var controller = new PurchasesController(context);

            var purchase = new Purchase
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = "INV-NEW-001",
                PurchaseDate = DateTime.Today,
                PaymentStatus = PaymentStatus.Paid,
                Discount = 0,
                GSTAmount = 0,
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = medicine.MedicineId,
                        BatchNumber = "BATCH-NEW",
                        ExpiryDate = DateTime.Today.AddYears(2),
                        Quantity = 100,
                        PurchasePrice = 8.00m,
                        SellingPrice = 15.00m
                    }
                }
            };

            // Act
            var result = await controller.Create(purchase);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify purchase was saved
            Assert.Equal(1, await context.Purchases.CountAsync());
            var savedPurchase = await context.Purchases.Include(p => p.PurchaseItems).FirstAsync();
            Assert.Equal("INV-NEW-001", savedPurchase.InvoiceNumber);
            Assert.Single(savedPurchase.PurchaseItems);

            // Verify stock was updated
            var updatedMedicine = await context.Medicines.FindAsync(medicine.MedicineId);
            Assert.Equal(initialStock + 100, updatedMedicine!.Quantity); // 50 + 100 = 150

            // Verify batch/expiry updated
            Assert.Equal("BATCH-NEW", updatedMedicine.BatchNumber);
        }

        [Fact]
        public async Task Create_Post_CalculatesAmountsCorrectly()
        {
            // Arrange
            var context = GetDbContext("PurchasesAmounts");
            var (supplier, _, medicine) = await SeedBaseData(context);
            var controller = new PurchasesController(context);

            var purchase = new Purchase
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = "INV-CALC-001",
                PurchaseDate = DateTime.Today,
                Discount = 10.00m,
                GSTAmount = 18.00m,
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = medicine.MedicineId,
                        BatchNumber = "B-001",
                        ExpiryDate = DateTime.Today.AddYears(1),
                        Quantity = 10,
                        PurchasePrice = 20.00m,
                        SellingPrice = 30.00m
                    }
                }
            };

            // Act
            await controller.Create(purchase);

            // Assert
            var saved = await context.Purchases.FirstAsync();
            Assert.Equal(200.00m, saved.TotalAmount); // 10 * 20
            Assert.Equal(208.00m, saved.NetAmount);   // 200 - 10 + 18
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetDbContext("PurchasesDetailsNull");
            var controller = new PurchasesController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Invoice_ReturnsPurchaseWithItems()
        {
            // Arrange
            var context = GetDbContext("PurchasesInvoice");
            var (supplier, _, medicine) = await SeedBaseData(context);

            var purchase = new Purchase
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = "INV-INV-001",
                PurchaseDate = DateTime.Today,
                TotalAmount = 500,
                NetAmount = 500,
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = medicine.MedicineId,
                        BatchNumber = "B-INV",
                        ExpiryDate = DateTime.Today.AddYears(1),
                        Quantity = 50,
                        PurchasePrice = 10,
                        SellingPrice = 15,
                        Amount = 500
                    }
                }
            };
            context.Purchases.Add(purchase);
            await context.SaveChangesAsync();

            var controller = new PurchasesController(context);

            // Act
            var result = await controller.Invoice(purchase.PurchaseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Purchase>(viewResult.Model);
            Assert.Equal("INV-INV-001", model.InvoiceNumber);
            Assert.Single(model.PurchaseItems);
        }

        [Fact]
        public async Task Delete_ReversesStockAdditions()
        {
            // Arrange
            var context = GetDbContext("PurchasesDeleteStock");
            var (supplier, _, medicine) = await SeedBaseData(context);

            // Simulate a purchase that added stock
            medicine.Quantity = 150; // was 50, purchase added 100
            await context.SaveChangesAsync();

            var purchase = new Purchase
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = "INV-DEL-001",
                PurchaseDate = DateTime.Today,
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = medicine.MedicineId,
                        BatchNumber = "B-DEL",
                        ExpiryDate = DateTime.Today.AddYears(1),
                        Quantity = 100,
                        PurchasePrice = 8,
                        SellingPrice = 15,
                        Amount = 800
                    }
                }
            };
            context.Purchases.Add(purchase);
            await context.SaveChangesAsync();

            var controller = new PurchasesController(context);

            // Act
            var result = await controller.DeleteConfirmed(purchase.PurchaseId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Purchase should be deleted
            Assert.Equal(0, await context.Purchases.CountAsync());

            // Stock should be reversed: 150 - 100 = 50
            var updatedMedicine = await context.Medicines.FindAsync(medicine.MedicineId);
            Assert.Equal(50, updatedMedicine!.Quantity);
        }
    }
}
