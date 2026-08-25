using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Controllers;
using PharmacyManagementSystem.Data;
using PharmacyManagementSystem.Models;
using Xunit;

namespace PharmacyManagementSystem.Tests
{
    public class PrescriptionsControllerTests
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

        private PrescriptionsController CreateController(ApplicationDbContext context)
        {
            var controller = new PrescriptionsController(context);
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new TestTempDataProvider());
            controller.TempData = tempData;
            return controller;
        }

        /// <summary>
        /// Minimal ITempDataProvider for unit testing.
        /// </summary>
        private class TestTempDataProvider : ITempDataProvider
        {
            private IDictionary<string, object?> _data = new Dictionary<string, object?>();

            public IDictionary<string, object?> LoadTempData(HttpContext context) => _data;

            public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
            {
                _data = values;
            }
        }


        private async Task<(MedicineCategory category, Medicine medicine)> SeedMedicine(ApplicationDbContext context, int quantity = 100)
        {
            var category = new MedicineCategory { CategoryName = "General" };
            context.MedicineCategories.Add(category);
            await context.SaveChangesAsync();

            var medicine = new Medicine
            {
                MedicineName = "Amoxicillin",
                CategoryId = category.CategoryId,
                BatchNumber = "AMX-001",
                ExpiryDate = DateTime.Today.AddYears(1),
                PurchasePrice = 5.00m,
                SellingPrice = 10.00m,
                Quantity = quantity,
                ReorderLevel = 10
            };
            context.Medicines.Add(medicine);
            await context.SaveChangesAsync();

            return (category, medicine);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithPrescriptions()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsIndex");
            context.Prescriptions.Add(new Prescription
            {
                PrescriptionNumber = "RX-001",
                PatientName = "John Doe",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Pending
            });
            await context.SaveChangesAsync();

            var controller = new PrescriptionsController(context);

            // Act
            var result = await controller.Index(null!, null!);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Prescription>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Index_FiltersbyStatus()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsStatusFilter");
            context.Prescriptions.AddRange(
                new Prescription { PrescriptionNumber = "RX-P1", PatientName = "A", PrescriptionDate = DateTime.Today, Status = PrescriptionStatus.Pending },
                new Prescription { PrescriptionNumber = "RX-V1", PatientName = "B", PrescriptionDate = DateTime.Today, Status = PrescriptionStatus.Verified },
                new Prescription { PrescriptionNumber = "RX-D1", PatientName = "C", PrescriptionDate = DateTime.Today, Status = PrescriptionStatus.Dispensed }
            );
            await context.SaveChangesAsync();

            var controller = new PrescriptionsController(context);

            // Act
            var result = await controller.Index(null!, "Verified");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Prescription>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("B", model.First().PatientName);
        }

        [Fact]
        public async Task Create_Post_GeneratesPrescriptionNumber()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsCreate");
            var (_, medicine) = await SeedMedicine(context);

            var controller = new PrescriptionsController(context);

            var prescription = new Prescription
            {
                PatientName = "Jane Doe",
                PatientAge = 30,
                PatientGender = "Female",
                DoctorName = "Dr. Smith",
                PrescriptionDate = DateTime.Today,
                PrescriptionItems = new List<PrescriptionItem>
                {
                    new PrescriptionItem
                    {
                        MedicineId = medicine.MedicineId,
                        Dosage = "500mg",
                        Frequency = "Twice daily",
                        Duration = "7 days",
                        Quantity = 14
                    }
                }
            };

            // Act
            var result = await controller.Create(prescription);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var saved = await context.Prescriptions.Include(p => p.PrescriptionItems).FirstAsync();
            Assert.StartsWith("RX-", saved.PrescriptionNumber);
            Assert.Contains(DateTime.Today.ToString("yyyyMMdd"), saved.PrescriptionNumber);
            Assert.Equal(PrescriptionStatus.Pending, saved.Status);
            Assert.Single(saved.PrescriptionItems);
        }

        [Fact]
        public async Task Verify_UpdatesStatusAndRecordsVerifier()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsVerify");
            var prescription = new Prescription
            {
                PrescriptionNumber = "RX-VER-001",
                PatientName = "Test Patient",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Pending
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();

            var controller = new PrescriptionsController(context);

            // Act
            var result = await controller.VerifyConfirmed(prescription.PrescriptionId, "Pharmacist A");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);

            var updated = await context.Prescriptions.FindAsync(prescription.PrescriptionId);
            Assert.Equal(PrescriptionStatus.Verified, updated!.Status);
            Assert.Equal("Pharmacist A", updated.VerifiedBy);
            Assert.NotNull(updated.VerifiedDate);
        }

        [Fact]
        public async Task Verify_SkipsIfNotPending()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsVerifySkip");
            var prescription = new Prescription
            {
                PrescriptionNumber = "RX-SKIP-001",
                PatientName = "Test",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Dispensed
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();

            var controller = new PrescriptionsController(context);

            // Act
            var result = await controller.VerifyConfirmed(prescription.PrescriptionId, "Someone");

            // Assert — should redirect without changing status
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            var updated = await context.Prescriptions.FindAsync(prescription.PrescriptionId);
            Assert.Equal(PrescriptionStatus.Dispensed, updated!.Status);
        }

        [Fact]
        public async Task Dispense_DeductsStockFromMedicines()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsDispense");
            var (_, medicine) = await SeedMedicine(context, quantity: 100);

            var prescription = new Prescription
            {
                PrescriptionNumber = "RX-DISP-001",
                PatientName = "Dispense Test",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Verified,
                PrescriptionItems = new List<PrescriptionItem>
                {
                    new PrescriptionItem
                    {
                        MedicineId = medicine.MedicineId,
                        Dosage = "500mg",
                        Frequency = "TID",
                        Duration = "5 days",
                        Quantity = 15
                    }
                }
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Dispense(prescription.PrescriptionId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);

            var updated = await context.Prescriptions.FindAsync(prescription.PrescriptionId);
            Assert.Equal(PrescriptionStatus.Dispensed, updated!.Status);

            var updatedMedicine = await context.Medicines.FindAsync(medicine.MedicineId);
            Assert.Equal(85, updatedMedicine!.Quantity); // 100 - 15
        }

        [Fact]
        public async Task Dispense_FailsIfInsufficientStock()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsDispenseFail");
            var (_, medicine) = await SeedMedicine(context, quantity: 5);

            var prescription = new Prescription
            {
                PrescriptionNumber = "RX-FAIL-001",
                PatientName = "Fail Test",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Verified,
                PrescriptionItems = new List<PrescriptionItem>
                {
                    new PrescriptionItem
                    {
                        MedicineId = medicine.MedicineId,
                        Quantity = 20 // More than available (5)
                    }
                }
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Dispense(prescription.PrescriptionId);

            // Assert — should redirect with error, status unchanged
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            var updated = await context.Prescriptions.FindAsync(prescription.PrescriptionId);
            Assert.Equal(PrescriptionStatus.Verified, updated!.Status); // Not dispensed

            var updatedMedicine = await context.Medicines.FindAsync(medicine.MedicineId);
            Assert.Equal(5, updatedMedicine!.Quantity); // Stock unchanged
        }

        [Fact]
        public async Task History_ReturnsPatientPrescriptions()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsHistory");
            context.Prescriptions.AddRange(
                new Prescription { PrescriptionNumber = "RX-H1", PatientName = "Alice Smith", PrescriptionDate = DateTime.Today.AddDays(-10), Status = PrescriptionStatus.Dispensed },
                new Prescription { PrescriptionNumber = "RX-H2", PatientName = "Alice Smith", PrescriptionDate = DateTime.Today, Status = PrescriptionStatus.Pending },
                new Prescription { PrescriptionNumber = "RX-H3", PatientName = "Bob Jones", PrescriptionDate = DateTime.Today, Status = PrescriptionStatus.Pending }
            );
            await context.SaveChangesAsync();

            var controller = new PrescriptionsController(context);

            // Act
            var result = await controller.History("Alice");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Prescription>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.All(model, p => Assert.Contains("Alice", p.PatientName));
        }

        [Fact]
        public async Task History_ReturnsEmpty_WhenNoPatientName()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsHistoryEmpty");
            var controller = new PrescriptionsController(context);

            // Act
            var result = await controller.History(null!);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Prescription>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Delete_RemovesPrescription()
        {
            // Arrange
            var context = GetDbContext("PrescriptionsDelete");
            var prescription = new Prescription
            {
                PrescriptionNumber = "RX-DEL-001",
                PatientName = "Delete Test",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Pending
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();

            var controller = new PrescriptionsController(context);

            // Act
            var result = await controller.DeleteConfirmed(prescription.PrescriptionId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(0, await context.Prescriptions.CountAsync());
        }
    }
}
