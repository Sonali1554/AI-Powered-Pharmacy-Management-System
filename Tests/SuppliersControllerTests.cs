using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Controllers;
using PharmacyManagementSystem.Data;
using PharmacyManagementSystem.Models;
using Xunit;

namespace PharmacyManagementSystem.Tests
{
    public class SuppliersControllerTests
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

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfSuppliers()
        {
            // Arrange
            var context = GetDbContext("SuppliersIndex");
            context.Suppliers.AddRange(
                new Supplier { SupplierName = "Supplier A", IsActive = true },
                new Supplier { SupplierName = "Supplier B", IsActive = false }
            );
            await context.SaveChangesAsync();

            var controller = new SuppliersController(context);

            // Act
            var result = await controller.Index(null!);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Supplier>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Index_WithSearchString_FiltersResults()
        {
            // Arrange
            var context = GetDbContext("SuppliersSearch");
            context.Suppliers.AddRange(
                new Supplier { SupplierName = "MedPharma", City = "Mumbai" },
                new Supplier { SupplierName = "HealthCare", City = "Delhi" }
            );
            await context.SaveChangesAsync();

            var controller = new SuppliersController(context);

            // Act
            var result = await controller.Index("MedPharma");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Supplier>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("MedPharma", model.First().SupplierName);
        }

        [Fact]
        public async Task Create_Post_AddsSupplierAndRedirects()
        {
            // Arrange
            var context = GetDbContext("SuppliersCreate");
            var controller = new SuppliersController(context);
            var supplier = new Supplier
            {
                SupplierName = "New Supplier",
                ContactPerson = "John",
                Email = "john@test.com",
                Phone = "1234567890",
                City = "Mumbai",
                State = "Maharashtra",
                IsActive = true
            };

            // Act
            var result = await controller.Create(supplier);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, await context.Suppliers.CountAsync());
            Assert.Equal("New Supplier", (await context.Suppliers.FirstAsync()).SupplierName);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetDbContext("SuppliersDetailsNull");
            var controller = new SuppliersController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsSupplier_WithPurchaseCount()
        {
            // Arrange
            var context = GetDbContext("SuppliersDetailsValid");
            var supplier = new Supplier { SupplierName = "Test Supplier" };
            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            var controller = new SuppliersController(context);

            // Act
            var result = await controller.Details(supplier.SupplierId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Supplier>(viewResult.Model);
            Assert.Equal("Test Supplier", model.SupplierName);
            Assert.Empty(model.Purchases);
        }

        [Fact]
        public async Task Edit_Post_UpdatesSupplier()
        {
            // Arrange
            var context = GetDbContext("SuppliersEdit");
            var supplier = new Supplier { SupplierName = "Old Name", City = "Mumbai" };
            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();
            context.Entry(supplier).State = EntityState.Detached;

            var controller = new SuppliersController(context);

            var updatedSupplier = new Supplier
            {
                SupplierId = supplier.SupplierId,
                SupplierName = "Updated Name",
                City = "Delhi"
            };

            // Act
            var result = await controller.Edit(supplier.SupplierId, updatedSupplier);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var savedSupplier = await context.Suppliers.FindAsync(supplier.SupplierId);
            Assert.Equal("Updated Name", savedSupplier!.SupplierName);
            Assert.Equal("Delhi", savedSupplier.City);
        }

        [Fact]
        public async Task Delete_BlockedWhenSupplierHasPurchases()
        {
            // Arrange
            var context = GetDbContext("SuppliersDeleteBlocked");
            var supplier = new Supplier { SupplierName = "Has Purchases" };
            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            context.Purchases.Add(new Purchase
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = "INV-001",
                PurchaseDate = DateTime.Today
            });
            await context.SaveChangesAsync();

            var controller = new SuppliersController(context);

            // Act
            var result = await controller.DeleteConfirmed(supplier.SupplierId);

            // Assert — supplier should NOT be deleted
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(await context.Suppliers.AnyAsync(s => s.SupplierId == supplier.SupplierId));
        }

        [Fact]
        public async Task Delete_RemovesSupplierWhenNoPurchases()
        {
            // Arrange
            var context = GetDbContext("SuppliersDeleteSuccess");
            var supplier = new Supplier { SupplierName = "No Purchases" };
            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            var controller = new SuppliersController(context);

            // Act
            var result = await controller.DeleteConfirmed(supplier.SupplierId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.False(await context.Suppliers.AnyAsync(s => s.SupplierId == supplier.SupplierId));
        }
    }
}
