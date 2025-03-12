using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Areas.Identity.Data;
using TechStockWeb.Models;

namespace TechStockWeb.Data;

public class TechStockContext : IdentityDbContext<TechStockWebUser>
{
    public TechStockContext(DbContextOptions<TechStockContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

public DbSet<TechStockWeb.Models.TypeArticle> TypeArticle { get; set; } = default!;

public DbSet<TechStockWeb.Models.Supplier> Supplier { get; set; } = default!;

public DbSet<TechStockWeb.Models.States> States { get; set; } = default!;

public DbSet<TechStockWeb.Models.Product> Product { get; set; } = default!;

public DbSet<TechStockWeb.Models.MaterialManagement> MaterialManagement { get; set; } = default!;
}

