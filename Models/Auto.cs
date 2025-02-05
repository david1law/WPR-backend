using System.ComponentModel.DataAnnotations;

public class Auto {
    [Key]
    public string Kenteken { get; set; }

    [Required]
    public string Soort { get; set; }

    [Required]
    public string Merk { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    public decimal Huurkosten { get; set; }

    [Required]
    public decimal Borg { get; set; }

    [Required]
    public int Aanschafjaar { get; set; }

    [Required]
    public string Kleur { get; set; }

    [Required]
    public string Transmissie { get; set; }
}