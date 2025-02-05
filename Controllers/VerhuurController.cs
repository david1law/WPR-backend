using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WPR_backend.Data;
using WPR_backend.Models;

[Route("api")]
[ApiController]
public class VerhuurController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VerhuurController(ApplicationDbContext context) {
        _context = context;
    }

    // POST: api/verhuur
    [HttpPost("verhuur")]
    [Authorize]
    public async Task<IActionResult> CreateVerhuur([FromBody] Verhuur verhuur) {
        if (verhuur == null) {
            return BadRequest("Invalid verhuur request.");
        }

        // Ensure that the provided auto (kenteken) exists in the database
        var existingAuto = await _context.Autos.FindAsync(verhuur.Kenteken);
        if (existingAuto == null) {
            return NotFound("Auto not found.");
        }

        // Ensure that the provided user exists in the database
        var existingUser = await _context.Users.FindAsync(verhuur.UserId);
        if (existingUser == null) {
            return NotFound("User not found.");
        }

        try {
            verhuur.Id = Guid.NewGuid();  // Ensure unique ID
            verhuur.Status = "aanvraag in afwachting";  // Default status

            // Attach the foreign key references without requiring navigation properties
            verhuur.Auto = null;
            verhuur.User = null;

            _context.Verhuur.Add(verhuur);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVerhuurById), new { id = verhuur.Id }, verhuur);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error saving verhuur: {ex.Message}");
        }
    }

    // GET: api/verhuur/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVerhuurById(Guid id) {
        var verhuur = await _context.Verhuur.FindAsync(id);
        if (verhuur == null) {
            return NotFound();
        }
        return Ok(verhuur);
    }
}