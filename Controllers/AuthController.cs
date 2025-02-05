using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WPR_backend.Models;
using WPR_backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace WPR_backend.Controllers {
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILookupNormalizer _normalizer;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, ILookupNormalizer normalizer, IConfiguration configuration) {
            _userManager = userManager;
            _normalizer = normalizer;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDTO model) {
            var user = new User { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Rol toewijzen (moet "Frontoffice" of "Backoffice" zijn aangezien "Klant" automatisch is voor nieuwe gebruikers)
            if (model.Role != "Frontoffice" && model.Role != "Backoffice")
                return BadRequest("Rol ongeldig. Alleen Front- en Backoffice kan worden aangemaakt door Admin.");

            await _userManager.AddToRoleAsync(user, model.Role);
            return Ok(new { message = $"Gebruiker {model.Email} succesvol aangemaakt met rol {model.Role}" });
        }

        [HttpPost("registreren")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null) return BadRequest(new { message = "Deze e-mail is al in gebruik." });

            var user = new User {
                UserName = model.Email,
                Email = model.Email,
                Voornaam = model.Voornaam,
                Achternaam = model.Achternaam,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Email handmatig normalizen
            user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
            await _userManager.UpdateAsync(user); // User updaten in de database

            // Default rol toewijzen
            await _userManager.AddToRoleAsync(user, "Particulier");

            return Ok(new { message = "Gebruiker succesvol geregistreerd." });
        }

        // Laat OPTIONS requests toe voor CORS preflight
        [HttpOptions]
        public IActionResult Preflight() {
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model) {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password)) {
                Console.WriteLine("Error: Email of wachtwoord is leeg.");
                return BadRequest("Email en wachtwoord zijn vereist.");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password)) {
                Console.WriteLine($"Error: Geen gebruiker gevonden me email '{model.Email}'.");
                return Unauthorized("Ongeldige inloggegevens");
            }

            Console.WriteLine($"Gebruiker gevonden: {user.Email} (ID: {user.Id})");

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid) {
                Console.WriteLine($"Error: Ongeldig wachtwoord voor {user.Email}");
                return Unauthorized("Ongeldige inloggegevens");
            }

            var token = await GenerateJwtToken(user);
            Console.WriteLine($"Token succesvol gegenereerd voor {user.Email}");
            // Token debugging bij errors
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(token);
            Console.WriteLine($"Token Payload: {decodedToken}");
            return Ok(new { token });
        }

        private async Task<string> GenerateJwtToken(User user) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JwtSettings:Secret"];
            
            if (string.IsNullOrEmpty(secretKey)) {
                throw new Exception("Error: JWT Secret Key is missing from appsettings.json!");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count == 0) {
                Console.WriteLine($"Gebruiker {user.Email} heeft geen rol toegewezen");
            } else {
                Console.WriteLine($"Gebruiker {user.Email} heeft rol: {string.Join(", ", roles)}");
            }

            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email)
            };
            
            claims.AddRange(roleClaims);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}