using PharmacyManagementSystem.Models;

namespace PharmacyManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any medicines.
            if (context.Medicines.Any())
            {
                return; // DB has been seeded
            }

            // 1. Categories
            var antibiotics = new MedicineCategory { CategoryName = "Antibiotics", Description = "Medicines that fight bacterial infections" };
            var analgesics = new MedicineCategory { CategoryName = "Analgesics", Description = "Pain relievers and anti-inflammatory medications" };
            var antihistamines = new MedicineCategory { CategoryName = "Antihistamines", Description = "Allergy and cold symptom relief" };
            var cardio = new MedicineCategory { CategoryName = "Cardiovascular", Description = "Heart and blood pressure management" };

            context.MedicineCategories.AddRange(antibiotics, analgesics, antihistamines, cardio);
            context.SaveChanges();

            // 2. Medicines
            var amoxicillin = new Medicine
            {
                MedicineName = "Amoxicillin 500mg",
                CategoryId = antibiotics.CategoryId,
                Manufacturer = "Cipla Ltd",
                BatchNumber = "AMX-8901",
                ExpiryDate = DateTime.Today.AddYears(2),
                PurchasePrice = 12.50m,
                SellingPrice = 22.00m,
                Quantity = 120,
                ReorderLevel = 30,
                Description = "Broad-spectrum antibacterial capsule"
            };

            var paracetamol = new Medicine
            {
                MedicineName = "Paracetamol 650mg",
                CategoryId = analgesics.CategoryId,
                Manufacturer = "Micro Labs",
                BatchNumber = "PCM-4412",
                ExpiryDate = DateTime.Today.AddYears(3),
                PurchasePrice = 2.20m,
                SellingPrice = 4.50m,
                Quantity = 250,
                ReorderLevel = 50,
                Description = "Pain relief and fever reducer"
            };

            var cetirizine = new Medicine
            {
                MedicineName = "Cetirizine 10mg",
                CategoryId = antihistamines.CategoryId,
                Manufacturer = "Sun Pharma",
                BatchNumber = "CTZ-3321",
                ExpiryDate = DateTime.Today.AddYears(1).AddMonths(6),
                PurchasePrice = 3.50m,
                SellingPrice = 7.50m,
                Quantity = 90,
                ReorderLevel = 20,
                Description = "Antihistamine for allergies"
            };

            var atorvastatin = new Medicine
            {
                MedicineName = "Atorvastatin 20mg",
                CategoryId = cardio.CategoryId,
                Manufacturer = "Zydus Cadila",
                BatchNumber = "ATV-7711",
                ExpiryDate = DateTime.Today.AddYears(2),
                PurchasePrice = 16.00m,
                SellingPrice = 30.00m,
                Quantity = 65,
                ReorderLevel = 15,
                Description = "Cholesterol lowering statin"
            };

            context.Medicines.AddRange(amoxicillin, paracetamol, cetirizine, atorvastatin);
            context.SaveChanges();

            // 3. Suppliers
            var supplierApex = new Supplier
            {
                SupplierName = "Apex Pharma Distributors",
                ContactPerson = "Rajesh Sharma",
                Email = "rajesh@apexpharma.in",
                Phone = "+91 98765 43210",
                Address = "Plot 45, MIDC Industrial Area, Andheri East",
                City = "Mumbai",
                State = "Maharashtra",
                GSTNumber = "27AABCA1234F1Z5",
                IsActive = true
            };

            var supplierMedlife = new Supplier
            {
                SupplierName = "MedLife Healthcare Supplies",
                ContactPerson = "Anita Verma",
                Email = "anita@medlifesupplies.com",
                Phone = "+91 98112 23344",
                Address = "Sector 18, Okhla Phase III",
                City = "New Delhi",
                State = "Delhi",
                GSTNumber = "07BBCCA5678G2Z1",
                IsActive = true
            };

            var supplierGlobal = new Supplier
            {
                SupplierName = "Global Health Logistics",
                ContactPerson = "Suresh Nair",
                Email = "suresh@globalhealth.in",
                Phone = "+91 99445 56677",
                Address = "Electronic City Phase 1",
                City = "Bengaluru",
                State = "Karnataka",
                GSTNumber = "29CCDD8901H3Z9",
                IsActive = true
            };

            context.Suppliers.AddRange(supplierApex, supplierMedlife, supplierGlobal);
            context.SaveChanges();

            // 4. Sample Purchase
            var samplePurchase = new Purchase
            {
                SupplierId = supplierApex.SupplierId,
                PurchaseDate = DateTime.Today.AddDays(-5),
                InvoiceNumber = "INV-2026-0801",
                PaymentStatus = PaymentStatus.Paid,
                Notes = "Monthly replenishment order",
                Discount = 50.00m,
                GSTAmount = 252.00m,
                TotalAmount = 2100.00m,
                NetAmount = 2302.00m,
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        MedicineId = amoxicillin.MedicineId,
                        BatchNumber = "AMX-8901",
                        ExpiryDate = DateTime.Today.AddYears(2),
                        Quantity = 100,
                        PurchasePrice = 12.50m,
                        SellingPrice = 22.00m,
                        Amount = 1250.00m
                    },
                    new PurchaseItem
                    {
                        MedicineId = paracetamol.MedicineId,
                        BatchNumber = "PCM-4412",
                        ExpiryDate = DateTime.Today.AddYears(3),
                        Quantity = 200,
                        PurchasePrice = 2.20m,
                        SellingPrice = 4.50m,
                        Amount = 440.00m
                    }
                }
            };

            context.Purchases.Add(samplePurchase);
            context.SaveChanges();

            // 5. Sample Prescriptions
            // Prescription 1: Pending (ready for pharmacist to Verify)
            var rxPending = new Prescription
            {
                PrescriptionNumber = $"RX-{DateTime.Today:yyyyMMdd}-0001",
                PatientName = "John Doe",
                PatientAge = 35,
                PatientGender = "Male",
                DoctorName = "Dr. R. K. Gupta",
                DoctorContact = "+91 98220 11223",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Pending,
                Notes = "Bacterial throat infection; patient allergic to sulfa drugs",
                PrescriptionItems = new List<PrescriptionItem>
                {
                    new PrescriptionItem
                    {
                        MedicineId = amoxicillin.MedicineId,
                        Dosage = "500mg",
                        Frequency = "Three times a day",
                        Duration = "5 days",
                        Quantity = 15,
                        Instructions = "Take after meals with plenty of water"
                    },
                    new PrescriptionItem
                    {
                        MedicineId = paracetamol.MedicineId,
                        Dosage = "650mg",
                        Frequency = "SOS (as needed for fever)",
                        Duration = "3 days",
                        Quantity = 6,
                        Instructions = "Take in case of temperature exceeding 100 F"
                    }
                }
            };

            // Prescription 2: Verified (ready for one-click Dispense)
            var rxVerified = new Prescription
            {
                PrescriptionNumber = $"RX-{DateTime.Today:yyyyMMdd}-0002",
                PatientName = "Sarah Connor",
                PatientAge = 29,
                PatientGender = "Female",
                DoctorName = "Dr. Priya Sen",
                DoctorContact = "+91 98450 67890",
                PrescriptionDate = DateTime.Today,
                Status = PrescriptionStatus.Verified,
                VerifiedBy = "Pharmacist Palak",
                VerifiedDate = DateTime.Now.AddMinutes(-30),
                Notes = "Seasonal allergic rhinitis",
                PrescriptionItems = new List<PrescriptionItem>
                {
                    new PrescriptionItem
                    {
                        MedicineId = cetirizine.MedicineId,
                        Dosage = "10mg",
                        Frequency = "Once daily at night",
                        Duration = "10 days",
                        Quantity = 10,
                        Instructions = "May cause mild drowsiness"
                    }
                }
            };

            // Prescription 3: Dispensed (for testing patient history)
            var rxDispensed = new Prescription
            {
                PrescriptionNumber = $"RX-{DateTime.Today.AddDays(-14):yyyyMMdd}-0001",
                PatientName = "John Doe",
                PatientAge = 35,
                PatientGender = "Male",
                DoctorName = "Dr. R. K. Gupta",
                DoctorContact = "+91 98220 11223",
                PrescriptionDate = DateTime.Today.AddDays(-14),
                Status = PrescriptionStatus.Dispensed,
                VerifiedBy = "Pharmacist Palak",
                VerifiedDate = DateTime.Today.AddDays(-14).AddHours(2),
                Notes = "Routine follow-up",
                PrescriptionItems = new List<PrescriptionItem>
                {
                    new PrescriptionItem
                    {
                        MedicineId = paracetamol.MedicineId,
                        Dosage = "650mg",
                        Frequency = "Twice daily",
                        Duration = "3 days",
                        Quantity = 6,
                        Instructions = "After food"
                    }
                }
            };

            context.Prescriptions.AddRange(rxPending, rxVerified, rxDispensed);
            context.SaveChanges();
        }
    }
}
