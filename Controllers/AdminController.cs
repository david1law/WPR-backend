using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WPR_backend.Models;
using Microsoft.EntityFrameworkCore;


namespace WPR_backend.Controllers {
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] // ✅ Only Admins can access
    public class AdminController : ControllerBase {
        private readonly UserManager<User> _userManager;
        private readonly ILookupNormalizer _normalizer;

        public AdminController(UserManager<User> userManager, ILookupNormalizer normalizer) {
            _userManager = userManager;
            _normalizer = normalizer; // ✅ Initialize normalizer
        }

        // ✅ Get all users with their roles
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers() {
            Console.WriteLine("🔍 Fetching users...");

            var users = await _userManager.Users.ToListAsync();
            Console.WriteLine($"✅ Retrieved {users.Count} users from the database.");

            var userDTOs = new List<UserDTO>();

            foreach (var user in users) {
                var roles = await _userManager.GetRolesAsync(user);
                Console.WriteLine($"🔍 User: {user.Email}, Roles: {string.Join(", ", roles)}");

                userDTOs.Add(new UserDTO {
                    Id = user.Id,
                    Voornaam = user.Voornaam,
                    Achternaam = user.Achternaam,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            Console.WriteLine("✅ Finished processing users.");
            return Ok(userDTOs);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound(new { message = "Gebruiker niet gevonden." }); // ✅ Return a clear message
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) {
                return BadRequest(new { message = "Fout bij verwijderen van gebruiker." });
            }

            return Ok(new { message = "Gebruiker succesvol verwijderd." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO model)
        {
            if (model.Role != "Frontoffice" && model.Role != "Backoffice")
                return BadRequest(new { message = "Ongeldige rol. Kies 'Frontoffice' of 'Backoffice'." });

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

            // ✅ Assign the selected role
            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new
            {
                message = "Gebruiker succesvol toegevoegd.",
                user = new
                {
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
