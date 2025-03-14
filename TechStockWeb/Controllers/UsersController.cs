using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Data;
using TechStockWeb.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechStockWeb.Areas.Identity.Data;

[Authorize(Roles = "Admin")] // Seuls les admins peuvent gérer les utilisateurs
public class UsersController : Controller
{
    private readonly UserManager<TechStockWebUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<TechStockWebUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Affiche la liste des utilisateurs et leurs rôles
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new List<UserRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles.Add(new UserRolesViewModel { UserName = user.UserName, Roles = roles.ToList() });
        }

        return View(userRoles);
    }

    // Modifier les rôles d'un utilisateur
    [HttpGet]
    public async Task<IActionResult> ManageRoles(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return NotFound();

        var model = new UserRolesViewModel
        {
            UserName = user.UserName,
            Roles = (await _userManager.GetRolesAsync(user)).ToList()
        };

        ViewBag.AllRoles = new MultiSelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Roles);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ManageRoles(UserRolesViewModel model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (model.Roles != null)
            await _userManager.AddToRolesAsync(user, model.Roles);

        return RedirectToAction("Index");
    }
}
