using PharmacyManagmentSystem.Models;

namespace PharmacyManagmentSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.MedicineCategories.Any())
            {
                return;   // DB has been seeded
            }

            var categories = new MedicineCategory[]
            {
                new MedicineCategory{CategoryName="Antibiotics", Description="Used to treat bacterial infections"},
                new MedicineCategory{CategoryName="Painkillers", Description="Analgesics for pain relief"},
                new MedicineCategory{CategoryName="Vitamins", Description="Dietary supplements"},
                new MedicineCategory{CategoryName="Antihistamines", Description="Allergy medications"}
            };
            foreach (var c in categories)
            {
                context.MedicineCategories.Add(c);
            }
            context.SaveChanges();

            var medicines = new Medicine[]
            {
                new Medicine{MedicineName="Amoxicillin", CategoryId=categories[0].CategoryId, BatchNumber="B001", ExpiryDate=DateTime.Parse("2027-12-31"), PurchasePrice=10.00m, SellingPrice=15.00m, Quantity=50, ReorderLevel=20, Manufacturer="PharmaCorp"},
                new Medicine{MedicineName="Ibuprofen", CategoryId=categories[1].CategoryId, BatchNumber="B002", ExpiryDate=DateTime.Parse("2027-12-31"), PurchasePrice=5.00m, SellingPrice=8.00m, Quantity=100, ReorderLevel=30, Manufacturer="HealthInc"},
                new Medicine{MedicineName="Vitamin C", CategoryId=categories[2].CategoryId, BatchNumber="B003", ExpiryDate=DateTime.Parse("2026-09-15"), PurchasePrice=8.00m, SellingPrice=12.00m, Quantity=200, ReorderLevel=50, Manufacturer="NutriLife"}
            };
            foreach (var m in medicines)
            {
                context.Medicines.Add(m);
            }
            context.SaveChanges();

            var batches = new Batch[]
            {
                new Batch{MedicineID=medicines[0].MedicineId, BatchNumber="B001", Quantity=50, MinimumStock=20, ExpiryDate=DateTime.Parse("2027-12-31"), ManufacturingDate=DateTime.Parse("2025-01-01")},
                new Batch{MedicineID=medicines[1].MedicineId, BatchNumber="B002", Quantity=100, MinimumStock=30, ExpiryDate=DateTime.Parse("2027-12-31"), ManufacturingDate=DateTime.Parse("2025-01-01")},
                new Batch{MedicineID=medicines[2].MedicineId, BatchNumber="B003", Quantity=200, MinimumStock=50, ExpiryDate=DateTime.Parse("2026-09-15"), ManufacturingDate=DateTime.Parse("2025-01-01")}
            };
            foreach (var b in batches)
            {
                context.Batches.Add(b);
            }
            context.SaveChanges();
            
            var suppliers = new Supplier[]
            {
                new Supplier{SupplierName="Global Pharma Distributors", ContactPerson="John Smith", Email="john@globalpharma.com", Phone="555-0101", IsActive=true},
                new Supplier{SupplierName="MedSupply Inc", ContactPerson="Jane Doe", Email="jane@medsupply.com", Phone="555-0102", IsActive=true}
            };
            foreach (var s in suppliers)
            {
                context.Suppliers.Add(s);
            }
            context.SaveChanges();

            var customers = new Customer[]
            {
                new Customer{Name="Alice Johnson", Phone="555-1001", Email="alice@example.com"},
                new Customer{Name="Bob Smith", Phone="555-1002", Email="bob@example.com"}
            };
            foreach (var c in customers)
            {
                context.Customers.Add(c);
            }
            context.SaveChanges();
        }
    }
}
