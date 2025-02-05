using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WPR_backend.Models;
using Microsoft.EntityFrameworkCore;
using WPR_backend.Data;

namespace WPR_backend.Controllers {
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] // Alleen de admin kan deze API endpoint in
    public class AdminController : ControllerBase {
        private readonly UserManager<User> _userManager;
        private readonly ILookupNormalizer _normalizer;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<User> userManager, ILookupNormalizer normalizer, ApplicationDbContext context) {
            _userManager = userManager;
            _normalizer = normalizer;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers() {
            Console.WriteLine("Gebruikers ophalen...");

            var users = await _userManager.Users.ToListAsync();
            Console.WriteLine($"{users.Count} gebruikers in de database gevonden.");

            var userDTOs = new List<UserDTO>();

            foreach (var user in users) {
                var roles = await _userManager.GetRolesAsync(user);
                Console.WriteLine($"Gebruiker: {user.Email}, Rol: {string.Join(", ", roles)}");

                userDTOs.Add(new UserDTO {
                    Id = user.Id,
                    Voornaam = user.Voornaam,
                    Achternaam = user.Achternaam,
                    Email = user.Email,
                    Roles = roles.ToList() // Rollen worden in een lijst opgeslagen, zelfs als het er maar 1 is
                });
            }

            Console.WriteLine("Finished processing users.");
            return Ok(userDTOs);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Gebruiker niet gevonden." });

            var verhuurRecords = await _context.Verhuur.Where(v => v.UserId == id).ToListAsync();
            foreach (var verhuur in verhuurRecords) {
                verhuur.DeletedUserEmail = user.Email;
                verhuur.UserId = null;
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(new { message = "Fout bij verwijderen van gebruiker." });

            return Ok(new { message = "Gebruiker succesvol verwijderd." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO model) {
            if (model.Role != "Frontoffice" && model.Role != "Backoffice") return BadRequest(new { message = "Ongeldige rol. Kies 'Frontoffice' of 'Backoffice'." });

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
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Normalized email en update deze gelijk
            user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
            await _userManager.UpdateAsync(user); // Update de gebruiker in de database

            // Wijst de gekozen rol toe aan de gebruiker
            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new {
                message = "Gebruiker succesvol toegevoegd.",
                user = new {
                    user.Id,
                    user.Voornaam,
                    user.Achternaam,
                    user.Email,
                    Role = model.Role
                }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("userroles/{id}")]
        public async Task<IActionResult> GetUserRoles(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Gebruiker niet gevonden." });

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { roles });
        }
    }
}
