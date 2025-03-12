
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;



namespace TechStockWeb.Areas.Identity.Data
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(UserManager<TechStockWebUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            String[] UserRoles = { "Admin", "Support", "User" };

            foreach (String UserRole in UserRoles)
            {
                if (!await roleManager.RoleExistsAsync(UserRole))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRole));
                }
            }
            await CreateUserIfNotExists(userManager,"admin@verwee.be","Admin@123","Admin");
            await CreateUserIfNotExists(userManager, "support@verwee.be", "Support@123", "Support");
            await CreateUserIfNotExists(userManager, "user@verwee.be", "User@123", "User");
        }

        private static async Task CreateUserIfNotExists(UserManager<TechStockWebUser> userManager, string email,string password,string role)
        {
            var user = await userManager.FindByNameAsync(email);
            if (user == null)
            {
                user = new TechStockWebUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
