using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TechStockWeb.Areas.Identity.Data;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<TechStockWebUser> _userManager;

    public ConfirmEmailModel(UserManager<TechStockWebUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync(string userId, string token)
    {
        if (userId == null || token == null)
        {
            return BadRequest("Paramètres invalides");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Utilisateur avec l'ID '{userId}' non trouvé.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Page() : BadRequest("Échec de la confirmation.");
    }
}
