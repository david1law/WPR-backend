using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WPR_backend.Data;
using WPR_backend.Models;

[Route("api/verhuur")]
[ApiController]
public class VerhuurController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VerhuurController(ApplicationDbContext context) {
        _context = context;
    }

    // POST: api/verhuur
    [HttpPost("create")]
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
        } catch (Exception ex) {
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

    [HttpGet("all")]
    [Authorize(Roles = "Backoffice, Frontoffice")]
    public async Task<IActionResult> GetAllVerhuur(
        [FromQuery] bool afwachting = false,
        [FromQuery] bool goedgekeurd = false,
        [FromQuery] bool afgekeurd = false,
        [FromQuery] bool verhuurd = false,
        [FromQuery] bool ingeleverd = false) {
        try {
            IQueryable<Verhuur> query = _context.Verhuur
                .Include(v => v.Auto)
                .Include(v => v.User);

            if (afwachting) query = query.Where(v => v.Status == "aanvraag in afwachting");
            if (goedgekeurd) query = query.Where(v => v.Status == "goedgekeurd");
            if (afgekeurd) query = query.Where(v => v.Status == "afgekeurd");
            if (verhuurd) query = query.Where(v => v.Status == "in verhuur");
            if (ingeleverd) query = query.Where(v => v.Status == "ingeleverd");
 
            var verhuurList = await query.ToListAsync();
            return Ok(verhuurList);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error retrieving verhuur records: {ex.Message}");
        }
    }

    [HttpPut("update/{id}")]
    [Authorize(Roles = "Backoffice, Frontoffice")]
    public async Task<IActionResult> UpdateVerhuurStatus(Guid id, [FromBody] VerhuurStatusUpdateDto updateDto) {
        if (updateDto == null) {
            return BadRequest("Invalid request data.");
        }

        // Find the Verhuur request by ID
        var verhuur = await _context.Verhuur.FindAsync(id);
        if (verhuur == null) {
            return NotFound("Verhuur request not found.");
        }

        // ✅ Update only the status and opmerkingen
        verhuur.Status = updateDto.Status;
        if (!string.IsNullOrEmpty(updateDto.Opmerkingen)) {
            verhuur.Opmerkingen = updateDto.Opmerkingen;
        }

        try {
            await _context.SaveChangesAsync();
            return Ok(new { message = "Verhuur status updated successfully." });
        } catch (Exception ex) {
            return StatusCode(500, $"Error updating verhuur: {ex.Message}");
        }
    }

    // ✅ DTO to ensure only Status and Opmerkingen can be updated
    public class VerhuurStatusUpdateDto {
        public string Status { get; set; }
        public string? Opmerkingen { get; set; }
    }
}