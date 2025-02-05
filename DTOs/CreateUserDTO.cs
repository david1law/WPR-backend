public class CreateUserDTO {
    public string Voornaam { get; set; }
    public string Achternaam { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } // Expected values: "Frontoffice" or "Backoffice"
}