using Microsoft.AspNetCore.Identity;

namespace WPR_backend.Models {
    public class User : IdentityUser {
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }

        // De email wordt hier genormalized voor de database
        public override string NormalizedEmail {
            get => base.NormalizedEmail;
            set => base.NormalizedEmail = value?.ToUpper();
        }
    }
}