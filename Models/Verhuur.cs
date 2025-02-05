using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WPR_backend.Models;

public class Verhuur {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();  // ✅ Auto-generated unique ID

    [Required]
    [ForeignKey("Auto")]  // ✅ Foreign key to `Autos`
    public string Kenteken { get; set; }
    
    public virtual Auto? Auto { get; set; }  // ✅ Make Nullable

    [ForeignKey("User")]  // ✅ Foreign key to `AspNetUsers`
    public string? UserId { get; set; }

    public virtual User? User { get; set; }  // ✅ Make Nullable

    public string Status { get; set; } // ✅ Status can now be NULL

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
    public double Huurkosten { get; set; } // ✅ New field for rental cost

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public double Borg { get; set; } // ✅ New field for deposit
    
    [Column(TypeName = "nvarchar(255)")]
    public string? DeletedUserEmail { get; set; }
}