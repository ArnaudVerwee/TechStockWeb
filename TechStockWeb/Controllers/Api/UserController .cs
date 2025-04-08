using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Areas.Identity.Data;
using TechStockWeb.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin")]  
public class UserController : ControllerBase  
{
    private readonly UserManager<TechStockWebUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(UserManager<TechStockWebUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRolesViewModel>>> GetUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new List<UserRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles.Add(new UserRolesViewModel { UserName = user.UserName, Roles = roles.ToList() });
        }

        return Ok(userRoles);
    }

    
    [HttpGet("{userName}")]
    public async Task<ActionResult<UserRolesViewModel>> GetUser(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return NotFound();
        }

        var model = new UserRolesViewModel
        {
            UserName = user.UserName,
            Roles = (await _userManager.GetRolesAsync(user)).ToList()
        };

        return Ok(model);
    }

    
    [HttpPost("manageRoles")]
    public async Task<IActionResult> ManageRoles([FromBody] UserRolesViewModel model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (model.Roles != null)
        {
            await _userManager.AddToRolesAsync(user, model.Roles);
        }

        return NoContent();
    }
}
