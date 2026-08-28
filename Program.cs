using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyManagmentSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// DATABASE
// ===============================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found."
    );

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)
);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ===============================
// IDENTITY + ROLES
// ===============================

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC
builder.Services.AddControllersWithViews();

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Register AI Demand Prediction Service
builder.Services.AddScoped<PharmacyManagmentSystem.Services.AIDemandPredictionService>();

// ===============================
// BUILD APP
// ===============================

var app = builder.Build();

// ===============================
// DATABASE + ADMIN SETUP
// ===============================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync();

    // ===============================
    // SEED MOCK DATA
    // ===============================

    if (!db.Customers.Any())
    {
        var random = new Random();

        var customers = new List<PharmacyManagmentSystem.Models.Customer>();

        for (int i = 1; i <= 5; i++)
        {
            customers.Add(new PharmacyManagmentSystem.Models.Customer
            {
                Name = $"Customer {i}",
                Phone = $"555-010{i}",
                Email = $"customer{i}@example.com",
                Address = $"{i} Main St"
            });
        }

        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        var medicines = new[]
        {
            "Paracetamol",
            "Amoxicillin",
            "Ibuprofen",
            "Omeprazole",
            "Azithromycin"
        };

        var paymentMethods = new[]
        {
            "Cash",
            "Credit Card",
            "Debit Card"
        };

        for (int i = 0; i < 25; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var saleItems = new List<PharmacyManagmentSystem.Models.SaleItem>();

            decimal subtotal = 0;
            int numItems = random.Next(1, 4);

            for (int j = 0; j < numItems; j++)
            {
                var medicine = medicines[random.Next(medicines.Length)];
                var quantity = random.Next(1, 5);
                var unitPrice = (decimal)(random.NextDouble() * 20 + 5);
                var totalPrice = quantity * unitPrice;

                subtotal += totalPrice;

                saleItems.Add(new PharmacyManagmentSystem.Models.SaleItem
                {
                    MedicineName = medicine,
                    Quantity = quantity,
                    UnitPrice = Math.Round(unitPrice, 2),
                    TotalPrice = Math.Round(totalPrice, 2)
                });
            }

            var discountPercentage = (decimal)random.Next(0, 15);
            var discountAmount =
                Math.Round(subtotal * (discountPercentage / 100), 2);

            var totalAmount = subtotal - discountAmount;

            var sale = new PharmacyManagmentSystem.Models.Sale
            {
                CustomerId = customer.CustomerId,
                SaleDate = DateTime.Now.AddDays(-random.Next(0, 30)),
                Subtotal = Math.Round(subtotal, 2),
                DiscountPercentage = discountPercentage,
                DiscountAmount = discountAmount,
                TotalAmount = Math.Round(totalAmount, 2),
                PaymentMethod =
                    paymentMethods[random.Next(paymentMethods.Length)],
                SaleItems = saleItems
            };

            db.Sales.Add(sale);
        }

        await db.SaveChangesAsync();
    }

    // Call the new centralized DbInitializer to seed missing data for other tables
    PharmacyManagmentSystem.Data.DbInitializer.Initialize(db);

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<IdentityUser>>();

    // ===============================
    // CREATE ROLES
    // ===============================

    string[] roles =
    {
        "Admin",
        "Pharmacist",
        "Staff"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result =
                await roleManager.CreateAsync(
                    new IdentityRole(role)
                );

            if (!result.Succeeded)
            {
                throw new Exception(
                    "Could not create role " + role + ": " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)
                    )
                );
            }
        }
    }

    // ===============================
    // CREATE ADMIN USER
    // ===============================

    var adminEmail = "admin@test.com";
    var adminPassword = "Admin@123";

    var adminUser =
        await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result =
            await userManager.CreateAsync(
                adminUser,
                adminPassword
            );

        if (!result.Succeeded)
        {
            throw new Exception(
                "Could not create admin user: " +
                string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description)
                )
            );
        }
    }

    // ===============================
    // GIVE ADMIN ROLE
    // ===============================

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var result =
            await userManager.AddToRoleAsync(
                adminUser,
                "Admin"
            );

        if (!result.Succeeded)
        {
            throw new Exception(
                "Could not assign Admin role: " +
                string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description)
                )
            );
        }
    }
}

// ===============================
// HTTP PIPELINE
// ===============================

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
)
.WithStaticAssets();

app.MapHub<PharmacyManagmentSystem.Hubs.AnalyticsHub>("/analyticsHub");

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
