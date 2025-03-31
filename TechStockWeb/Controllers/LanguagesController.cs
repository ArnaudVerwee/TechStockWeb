using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace TechStockWeb.Controllers
{
    public class LanguagesController : Controller
    {
        public IActionResult ChangeLanguage(string id, string returnUrl)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = "en"; // valeur par défaut si rien n'est envoyé
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(id)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
