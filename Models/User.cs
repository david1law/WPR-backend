using Microsoft.AspNetCore.Identity;

namespace WPR_backend.Models {
    public class User : IdentityUser {
        public string? Voornaam { get; set; }
        public string? Achternaam{ get; set; }
        public string Email { get; set; }
    }
}
