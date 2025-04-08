using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using TechStockWeb.Data;
using Microsoft.AspNetCore.Identity;
using TechStockWeb.Areas.Identity.Data;
using TechStockWeb.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Localization;
using TechStockWeb.Resources;
using TechStockWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration des services de localisation ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var assembly = Assembly.GetExecutingAssembly();
Debug.WriteLine($"🔍 Assembly en cours : {assembly.FullName}");

var resourceNames = assembly.GetManifestResourceNames();
foreach (var name in resourceNames)
{
    Debug.WriteLine($"📂 Ressource trouvée : {name}");
}

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

// --- Ajout des services pour l'API ---
builder.Services.AddControllers();  // Permet d'ajouter les contrôleurs API

builder.Services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
builder.Services.AddSingleton<IStringLocalizer>(provider =>
{
    var factory = provider.GetRequiredService<IStringLocalizerFactory>();
    return factory.Create(typeof(SharedResource));
});

// --- Configuration des options de localisation ---
var supportedCultures = new[] { "en", "fr", "nl" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToArray();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToArray();
});

// --- Configuration de l'email ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// --- Middleware de gestion des cultures ---
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// --- Initialisation de la base de données et des rôles ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<TechStockWebUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<TechStockContext>();

        await SeedRolesAndUsers(roleManager, userManager);
        await SeedStates(dbContext);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Erreur lors du seed de la base de données : {ex.Message}");
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

// Ajout de ton middleware de logging
app.UseMiddleware<TechStockWeb.Middleware.LoggingMiddleware>();

app.UseAuthorization();

// --- Mappe les contrôleurs API ---
// Cette ligne permet de mapper les contrôleurs de ton API
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// --- Fonctions auxiliaires ---
static async Task SeedRolesAndUsers(RoleManager<IdentityRole> roleManager, UserManager<TechStockWebUser> userManager)
{
    string[] roles = { "Admin", "Support", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var users = await userManager.Users.ToListAsync();
    foreach (var user in users)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Any())
        {
            await userManager.AddToRoleAsync(user, "User");
        }
    }
}

static async Task SeedStates(TechStockContext dbContext)
{
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
        Debug.WriteLine("✅ États par défaut ajoutés avec succès.");
    }
}
