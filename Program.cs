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
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    )
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

    // Apply pending migrations
    await db.Database.MigrateAsync();

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

// IMPORTANT
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
)
.WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();