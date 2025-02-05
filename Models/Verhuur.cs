using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WPR_backend.Models;

public class Verhuur {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();  // Auto-generated unieke ID

    [Required]
    [ForeignKey("Auto")] // Foreign key naar Autos
    public string Kenteken { get; set; }
    
    public virtual Auto? Auto { get; set; }  // Nullable

    [ForeignKey("User")]  // Foreign key naar AspNetUsers
    public string? UserId { get; set; }

    public virtual User? User { get; set; }  // Nullable

    public string Status { get; set; }

    [Required]
    public string Rijbewijs { get; set; }

    [Required]
    public string Startdatum { get; set; }

    [Required]
    public string Einddatum { get; set; }

    [Required]
    public string Ophaallocatie { get; set; }

    [Required]
    public string Inleverlocatie { get; set; }

    public string Opmerkingen { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public double Huurkosten { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public double Borg { get; set; }
    
    [Column(TypeName = "nvarchar(255)")]
    public string? DeletedUserEmail { get; set; } // Dit extra veld is nodig voor het bewaren van de verhuur informatie als de gebruiker wordt verwijderd
}