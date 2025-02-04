using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WPR_backend.Models;
using Microsoft.EntityFrameworkCore;


namespace WPR_backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] // ✅ Only Admins can access
    public class AdminController : ControllerBase {
        private readonly UserManager<User> _userManager;

        public AdminController(UserManager<User> userManager) {
            _userManager = userManager;
        }

        // ✅ Get all users with their roles
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Voornaam = u.Voornaam,
                    Achternaam = u.Achternaam,
                    Email = u.Email
                })
                .ToListAsync(); // ✅ Now ToListAsync() works!

            return Ok(users);
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

    }
}
