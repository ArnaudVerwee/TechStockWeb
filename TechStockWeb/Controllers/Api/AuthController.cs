using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechStockWeb.Areas.Identity.Data;

namespace TechStockWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<TechStockWebUser> _userManager;
        private readonly SignInManager<TechStockWebUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<TechStockWebUser> userManager,
            SignInManager<TechStockWebUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Données invalides" });
                }

                // Rechercher l'utilisateur par email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                // Vérifier le mot de passe
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                // Générer le token JWT
                var token = await GenerateJwtTokenAsync(user);

                return Ok(new LoginResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.UserName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }

        // POST: api/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Données invalides" });
                }

                // Vérifier si l'utilisateur existe déjà
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Un utilisateur avec cet email existe déjà" });
                }

                // Créer le nouvel utilisateur
                var user = new TechStockWebUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true // Pour simplifier, on confirme automatiquement
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { message = $"Erreur lors de la création: {errors}" });
                }

                // Ajouter un rôle par défaut si nécessaire
                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new { message = "Utilisateur créé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }

        // GET: api/Auth/ValidateToken
        [HttpGet("ValidateToken")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new
            {
                isValid = true,
                userId = userId,
                email = email
            });
        }

        // POST: api/Auth/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Pour un token JWT, la déconnexion se fait côté client
            // Ici on pourrait ajouter le token à une blacklist si nécessaire
            return Ok(new { message = "Déconnexion réussie" });
        }

        // GET: api/Auth/Profile
        [HttpGet("Profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                roles = roles
            });
        }

        private async Task<string> GenerateJwtTokenAsync(TechStockWebUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourVeryLongSecretKeyHere123456789"; // Clé par défaut
            var issuer = jwtSettings["Issuer"] ?? "TechStockAPI";
            var audience = jwtSettings["Audience"] ?? "TechStockMaui";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Ajouter les rôles comme claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token valide 7 jours
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTOs pour les requêtes
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}