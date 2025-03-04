using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Areas.Identity.Data;

namespace TechStockWeb.Data;

public class TechStock : IdentityDbContext<TechStockWebUser>
{
    public TechStock(DbContextOptions<TechStock> options)
        : base(options)
    {
    }
    public DbSet<TechStockWeb.Models.User> Users { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
