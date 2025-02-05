using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateVerhuur([FromBody] Verhuur verhuur) {
        if (verhuur == null) {
            return BadRequest("Ongeldige verhuur aanvraag.");
        }

        // Controleren of de gekozen auto in de database bestaat
        var existingAuto = await _context.Autos.FindAsync(verhuur.Kenteken);
        if (existingAuto == null) {
            return NotFound("Auto niet gevonden.");
        }

        // Controleren of de gebruiker in de database bestaat
        var existingUser = await _context.Users.FindAsync(verhuur.UserId);
        if (existingUser == null) {
            return NotFound("Gebruiker niet gevonden.");
        }

        try {
            verhuur.Id = Guid.NewGuid();  // Zorgt voor een unieke ID
            verhuur.Status = "aanvraag in afwachting";  // Default status

            // Voegt foreign key references toe zonder de navigation properties nodig te hebben
            verhuur.Auto = null;
            verhuur.User = null;

            _context.Verhuur.Add(verhuur);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVerhuurById), new { id = verhuur.Id }, verhuur);
        } catch (Exception ex) {
            return StatusCode(500, $"Error bij het opslaan van het verhuur: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVerhuurById(Guid id) {
        var verhuur = await _context.Verhuur.FindAsync(id);
        if (verhuur == null) return NotFound();
        return Ok(verhuur);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Backoffice, Frontoffice")]
    public async Task<IActionResult> GetAllVerhuur(
        [FromQuery] bool afwachting = false, // De filter parameters uit de URL bv. /verhuur?goedgekeurd=true
        [FromQuery] bool goedgekeurd = false,
        [FromQuery] bool afgekeurd = false,
        [FromQuery] bool verhuurd = false,
        [FromQuery] bool ingeleverd = false) {
        try {
            IQueryable<Verhuur> query = _context.Verhuur
                .Include(v => v.Auto)
                .Include(v => v.User);

            // Afhankelijk van waar de request vandaan komt, wordt de gepaste filter toegepast op het ophalen van de lijst met verhuur
            if (afwachting) query = query.Where(v => v.Status == "aanvraag in afwachting");
            if (goedgekeurd) query = query.Where(v => v.Status == "goedgekeurd");
            if (afgekeurd) query = query.Where(v => v.Status == "afgekeurd");
            if (verhuurd) query = query.Where(v => v.Status == "in verhuur");
            if (ingeleverd) query = query.Where(v => v.Status == "ingeleverd");
 
            var verhuurList = await query.ToListAsync();
            return Ok(verhuurList);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error bij het ophalen van verhuur: {ex.Message}");
        }
    }

    [HttpPut("update/{id}")]
    [Authorize(Roles = "Backoffice, Frontoffice")]
    public async Task<IActionResult> UpdateVerhuurStatus(Guid id, [FromBody] VerhuurStatusUpdateDto updateDto) {
        if (updateDto == null) return BadRequest("Ongeldige request data.");

        // Find by ID voor het verhuur
        var verhuur = await _context.Verhuur.FindAsync(id);
        if (verhuur == null) {
            return NotFound("Verhuur aanvraag niet gevonden.");
        }

        // Alleen de status en opmerkingen worden bijgewerkt
        verhuur.Status = updateDto.Status;
        if (!string.IsNullOrEmpty(updateDto.Opmerkingen)) verhuur.Opmerkingen = updateDto.Opmerkingen;

        try {
            await _context.SaveChangesAsync();
            return Ok(new { message = "Verhuur status succesvol bijgewerkt." });
        } catch (Exception ex) {
            return StatusCode(500, $"Error bij het updaten van verhuur: {ex.Message}");
        }
    }

    // DTO om ervoor te zorgen dat de status en opmerkingen bijgewerkt kunnen worden
    public class VerhuurStatusUpdateDto {
        public string Status { get; set; }
        public string? Opmerkingen { get; set; }
    }
}