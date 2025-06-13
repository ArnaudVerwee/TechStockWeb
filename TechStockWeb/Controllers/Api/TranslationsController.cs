using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.AspNetCore.Mvc;

namespace TechStockWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllTranslations(string culture = "fr")
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($" GetAllTranslations for culture: {culture}");

                var translations = new Dictionary<string, string>();
                var assembly = Assembly.GetExecutingAssembly();
                var cultureInfo = new CultureInfo(culture);

                var allResourceNames = assembly.GetManifestResourceNames()
                    .Where(name => name.EndsWith(".resources"))
                    .Select(name => name.Replace(".resources", ""))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($" {allResourceNames.Count} resources found automatically");

                foreach (var resourceName in allResourceNames)
                {
                    try
                    {
                        var resourceManager = new ResourceManager(resourceName, assembly);
                        var resourceSet = resourceManager.GetResourceSet(cultureInfo, true, false);

                        if (resourceSet != null)
                        {
                            var resourceCount = 0;
                            foreach (System.Collections.DictionaryEntry entry in resourceSet)
                            {
                                if (entry.Key != null && entry.Value != null)
                                {
                                    var key = entry.Key.ToString();
                                    var value = entry.Value.ToString();

                                    if (!translations.ContainsKey(key))
                                    {
                                        translations[key] = value;
                                        resourceCount++;
                                    }
                                }
                            }

                            if (resourceCount > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($" {resourceName}: {resourceCount} translations");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($" Resource error {resourceName}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($" TOTAL: {translations.Count} unique translations for {culture}");

                foreach (var trans in translations.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"   {trans.Key}: {trans.Value}");
                }

                return Ok(translations);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" GetAllTranslations error: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error loading translations",
                    error = ex.Message
                });
            }
        }

        [HttpGet("debug")]
        public IActionResult GetDebugInfo()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName().Name;

                var manifestResourceNames = assembly.GetManifestResourceNames();

                var info = new
                {
                    AssemblyName = assemblyName,
                    BaseDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    ManifestResourceNames = manifestResourceNames,
                    SuggestedResourceManagerPaths = new[]
                    {
                        $"{assemblyName}.Resources.SharedResource",
                        $"{assemblyName}.SharedResource",
                        $"{assemblyName}.Resources.Resource",
                        $"{assemblyName}.Resource"
                    }
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}