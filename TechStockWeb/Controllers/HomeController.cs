using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Models;
using TechStockWeb.Data;

namespace TechStockWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly TechStockContext _context;

        // Constructeur
        public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer, TechStockContext context)
        {
            _logger = logger;
            _localizer = localizer;
            _context = context;
        }

        // Page d'accueil
        public IActionResult Index()
        {
            return View();
        }

        // Page de confidentialité
        public IActionResult Privacy()
        {
            return View();
        }

        // Gestion de l'erreur
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult ChangeLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return BadRequest("Culture is required");
            }

            var supportedCultures = new[] { "en", "fr", "nl" };
            if (!supportedCultures.Contains(culture))
            {
                return BadRequest("Culture not supported");
            }

            // Ajout d'un log pour voir la culture qui a été passée
            _logger.LogInformation($"Changing language to {culture}");

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }



        // 📊 Obtenir les statistiques des produits (Total / Assignés / Non Assignés)
        public async Task<IActionResult> GetProductStats()
        {
            var totalProducts = await _context.Products.CountAsync();
            var assignedProducts = await _context.MaterialManagement
                .Where(mm => mm.ProductId != null)
                .CountAsync();
            var unassignedProducts = totalProducts - assignedProducts;

            return Json(new
            {
                totalProducts,
                assignedProducts,
                unassignedProducts
            });
        }

        // 📊 Obtenir la performance des utilisateurs (Produits reçus)
        public async Task<IActionResult> UserPerformanceStats()
        {
            try
            {
                var userStats = await _context.MaterialManagement
                    .GroupBy(m => m.User.UserName)
                    .Select(group => new
                    {
                        userName = group.Key,
                        productCount = group.Count()
                    })
                    .OrderByDescending(u => u.productCount)
                    .ToListAsync();

                return Json(userStats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user performance stats: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
