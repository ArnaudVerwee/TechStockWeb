using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechStockWeb.Data;
using Microsoft.AspNetCore.Identity;
using TechStockWeb.Areas.Identity.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TechStockContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TechStock") ?? throw new InvalidOperationException("Connection string 'TechStock' not found.")));

builder.Services.AddDefaultIdentity<TechStockWebUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<TechStockContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<TechStockWebUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Vérifier si les rôles existent, sinon les créer
        var roles = new[] { "Admin", "Support", "User" };
        foreach (var role in roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Attribuer le rôle "User" par défaut à tous les utilisateurs qui n'ont pas encore de rôle
        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                // Si l'utilisateur n'a aucun rôle, lui attribuer le rôle "User"
                await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error when seeding the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")

    .WithStaticAssets();

app.MapRazorPages()
.WithStaticAssets();

app.Run();
