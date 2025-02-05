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
            _normalizer = normalizer; // ✅ Initialize normalizer
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDTO model) {
            var user = new User { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // ✅ Assign role (must be "Frontoffice" or "Backoffice" since "Klant" is automatic)
            if (model.Role != "Frontoffice" && model.Role != "Backoffice")
                return BadRequest("Invalid role. Only Frontoffice or Backoffice users can be created by admin.");

            await _userManager.AddToRoleAsync(user, model.Role);
            return Ok(new { message = $"User {model.Email} created successfully with role {model.Role}" });
        }

        [HttpPost("registreren")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Deze e-mail is al in gebruik." });

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Voornaam = model.Voornaam,
                Achternaam = model.Achternaam,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // ✅ Normalize email and manually update it
            user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
            await _userManager.UpdateAsync(user); // ✅ Explicitly update user in the database

            // ✅ Assign default role
            await _userManager.AddToRoleAsync(user, "Particulier");

            return Ok(new { message = "Gebruiker succesvol geregistreerd." });
        }

        // Allow OPTIONS requests for CORS preflight
        [HttpOptions]
        public IActionResult Preflight() {
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model) {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password)) {
                Console.WriteLine("❌ Error: Email or password is empty.");
                return BadRequest("Email and password are required.");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password)) {
                Console.WriteLine($"❌ Error: No user found with email '{model.Email}'.");
                return Unauthorized("Invalid credentials");
            }

            Console.WriteLine($"✅ Found user: {user.Email} (ID: {user.Id})");

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid) {
                Console.WriteLine($"❌ Error: Incorrect password for {user.Email}");
                return Unauthorized("Invalid credentials");
            }

            var token = await GenerateJwtToken(user);
            Console.WriteLine($"✅ Token generated successfully for {user.Email}");
            // 🔍 Debugging: Decode and log the JWT payload
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(token);
            Console.WriteLine($"🔹 Token Payload: {decodedToken}");
            return Ok(new { token });
        }

        private async Task<string> GenerateJwtToken(User user) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JwtSettings:Secret"];
            
            if (string.IsNullOrEmpty(secretKey)) {
                throw new Exception("❌ ERROR: JWT Secret Key is missing from appsettings.json!");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            // ✅ Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count == 0) {
                Console.WriteLine($"❌ User {user.Email} has no assigned roles!");
            } else {
                Console.WriteLine($"✅ User {user.Email} has roles: {string.Join(", ", roles)}");
            }

            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email)
            };
            
            claims.AddRange(roleClaims); // ✅ Add role claims to token

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}