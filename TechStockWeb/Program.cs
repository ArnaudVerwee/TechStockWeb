using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
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

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var assembly = Assembly.GetExecutingAssembly();
Debug.WriteLine($" Assembly ongoing : {assembly.FullName}");

var resourceNames = assembly.GetManifestResourceNames();
foreach (var name in resourceNames)
{
    Debug.WriteLine($" Ressource found : {name}");
}

builder.Services.AddDbContext<TechStockContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TechStock")
    ?? throw new InvalidOperationException("Connection string 'TechStock' not found.")));


builder.Services.AddDefaultIdentity<TechStockWebUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TechStockContext>();


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourVeryLongSecretKeyHere123456789";

builder.Services.AddAuthentication()  
    .AddJwtBearer("Bearer", options =>  
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "TechStockAPI",
            ValidAudience = jwtSettings["Audience"] ?? "TechStockMaui",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddControllers();

builder.Services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
builder.Services.AddSingleton<IStringLocalizer>(provider =>
{
    var factory = provider.GetRequiredService<IStringLocalizerFactory>();
    return factory.Create(typeof(SharedResource));
});

var supportedCultures = new[] { "en", "fr", "nl" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToArray();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToArray();
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost",
                "https://localhost",
                "http://10.0.2.2:5000",
                "https://10.0.2.2:7237",
                "http://localhost:5000",
                "https://localhost:7237"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.WebHost.UseUrls(
    "http://0.0.0.0:7236",     
    "https://localhost:7237"    
);
var app = builder.Build();

var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

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
        Debug.WriteLine($" Error while seeding the database:{ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseMiddleware<TechStockWeb.Middleware.LoggingMiddleware>();

app.UseCors("AllowMauiApp");


app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

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
        Debug.WriteLine(" Default states were added successfully.");
    }
}