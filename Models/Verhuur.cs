using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WPR_backend.Models; // ✅ Ensure this is correct

public class Verhuur
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();  // ✅ Auto-generated unique ID

    [Required]
    [ForeignKey("Auto")]  // ✅ Foreign key to `Autos`
    public string Kenteken { get; set; }
    
    public virtual Auto Auto { get; set; }  // ✅ Navigation Property

    [ForeignKey("User")]  // ✅ Foreign key to `AspNetUsers`
    public string UserId { get; set; }

    public virtual User User { get; set; }  // ✅ Navigation Property

    public string Status { get; set; } // ✅ Status can now be NULL

    [Required]
    public string Rijbewijs { get; set; }

    [Required]
    public DateTime Startdatum { get; set; }

    [Required]
    public DateTime Einddatum { get; set; }

    [Required]
    public string Ophaallocatie { get; set; }

    [Required]
    public string Inleverlocatie { get; set; }

    public string Opmerkingen { get; set; }
}