using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WPR_backend.Models;
using Microsoft.EntityFrameworkCore;
using WPR_backend.Data;

namespace WPR_backend.Controllers {
    [ApiController]
    [Route("api/wpb")]
    [Authorize(Roles = "Wagenparkbeheerder")] // Alleen een Wagenparkbeheerder kan deze API gebruiken
    public class WpbController : ControllerBase {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public WpbController(UserManager<User> userManager, ApplicationDbContext context) {
            _userManager = userManager;
            _context = context;
        }

        // Haalt alleen gebruikers met de rol 'Zakelijk' op
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers() {
            Console.WriteLine("Gebruikers ophalen...");

            var users = await _userManager.Users.ToListAsync();
            var userDTOs = new List<UserDTO>();

            foreach (var user in users) {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Zakelijk")) {  // Filter alleen zakelijke gebruikers
                    userDTOs.Add(new UserDTO {
                        Id = user.Id,
                        Voornaam = user.Voornaam,
                        Achternaam = user.Achternaam,
                        Email = user.Email,
                        Roles = roles.ToList()
                    });
                }
            }

            Console.WriteLine($"{userDTOs.Count} zakelijke gebruikers gevonden.");
            return Ok(userDTOs);
        }

        // WPB mag alleen gebruikers met de rol 'Zakelijk' verwijderen
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Gebruiker niet gevonden." });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Zakelijk")) {
                return Forbid("Wagenparkbeheerder mag alleen zakelijke gebruikers verwijderen.");
            }

            var verhuurRecords = await _context.Verhuur.Where(v => v.UserId == id).ToListAsync();
            foreach (var verhuur in verhuurRecords) {
                verhuur.DeletedUserEmail = user.Email;
                verhuur.UserId = null;
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(new { message = "Fout bij verwijderen van gebruiker." });

            return Ok(new { message = "Zakelijke gebruiker succesvol verwijderd." });
        }

        // WPB mag alleen 'Zakelijk' gebruikers aanmaken
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO model) {
            if (model.Role != "Zakelijk") return BadRequest(new {
                message = "Wagenparkbeheerder mag alleen gebruikers met de rol 'Zakelijk' aanmaken."
            });

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
            await _userManager.UpdateAsync(user);

            // WPB kan alleen 'Zakelijk' rol toewijzen
            await _userManager.AddToRoleAsync(user, "Zakelijk");

            return Ok(new {
                message = "Zakelijke gebruiker succesvol toegevoegd.",
                user = new {
                    user.Id,
                    user.Voornaam,
                    user.Achternaam,
                    user.Email,
                    Role = "Zakelijk"
                }
            });
        }

        // WPB kan alleen de rollen van zakelijke gebruikers ophalen
        [HttpGet("userroles/{id}")]
        public async Task<IActionResult> GetUserRoles(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Gebruiker niet gevonden." });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Zakelijk")) {
                return Forbid("Wagenparkbeheerder mag alleen zakelijke gebruikers beheren.");
            }

            return Ok(new { roles });
        }
    }
}