using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using TechStockWeb.Data;
using Microsoft.AspNetCore.Identity;
using TechStockWeb.Areas.Identity.Data;
using TechStockWeb.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// --- Support de la localisation ---
builder.Services.AddLocalization(options => options.ResourcesPath = "LanguageResources");

// --- Configuration de la base de données et de l'identité ---
builder.Services.AddDbContext<TechStockContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TechStock")
    ?? throw new InvalidOperationException("Connection string 'TechStock' not found.")));

builder.Services.AddDefaultIdentity<TechStockWebUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TechStockContext>();

// --- Ajout du support de la localisation dans les vues et contrôleurs ---
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

var app = builder.Build();

// --- Définition des cultures supportées ---
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("fr"),
    new CultureInfo("nl")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"), // Langue par défaut
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

// --- Seed de la base de données et configuration d'identité ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<TechStockWebUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<TechStockContext>();

        // Création des rôles si non existants
        var roles = new[] { "Admin", "Support", "User" };
        foreach (var role in roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Attribuer le rôle "User" par défaut aux utilisateurs sans rôle
        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                await userManager.AddToRoleAsync(user, "User");
            }
        }

        // Ajouter les statuts (States) s'ils n'existent pas
        if (!dbContext.States.Any())
        {
            dbContext.States.AddRange(new List<States>
            {
                new States { Status = "New Product" },
                new States { Status = "Old Product" },
                new States { Status = "Product to repair" },
                new States { Status = "Broken Product" }
            });

            await dbContext.SaveChangesAsync();
            Console.WriteLine("Default states added successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error when seeding the database: {ex.Message}");
    }
}

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
