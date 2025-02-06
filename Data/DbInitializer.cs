using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WPR_backend.Data;
using WPR_backend.Models;

public static class DbInitializer {
    public static void SeedAutos(ApplicationDbContext context) {
        var autos = new List<Auto> {
            new Auto { Kenteken = "F-237-GK", Soort = "Auto", Merk = "Volkswagen", Type = "ID.4", Huurkosten = 110.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "G-357-SS", Soort = "Auto", Merk = "Toyota", Type = "Corolla", Huurkosten = 75.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "J-204-PP", Soort = "Auto", Merk = "Audi", Type = "Q5", Huurkosten = 130.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "K-174-SP", Soort = "Auto", Merk = "Mazda", Type = "CX-5", Huurkosten = 84.50M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "K-977-ST", Soort = "Auto", Merk = "Lexus", Type = "IS300", Huurkosten = 120.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "L-928-TX", Soort = "Auto", Merk = "Porsche", Type = "Cayenne", Huurkosten = 220.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "P-237-ZT", Soort = "Auto", Merk = "Hyundai", Type = "Ioniq 5", Huurkosten = 120.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "S-238-TL", Soort = "Auto", Merk = "Audi", Type = "A7", Huurkosten = 144.95M, Borg = 450.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "S-241-GP", Soort = "Auto", Merk = "Volvo", Type = "V60", Huurkosten = 80.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "S-476-DS", Soort = "Auto", Merk = "Renault", Type = "Arkana", Huurkosten = 97.51M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "S-967-KS", Soort = "Auto", Merk = "Polestar", Type = "2", Huurkosten = 120.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "X-466-XT", Soort = "Auto", Merk = "Volkswagen", Type = "T-Roc", Huurkosten = 105.00M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "X-587-SV", Soort = "Auto", Merk = "Mercedes-Benz", Type = "E-Klasse", Huurkosten = 159.50M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "T-492-KS", Soort = "Auto", Merk = "Peugeot", Type = "508", Huurkosten = 159.50M, Borg = 400.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" },
            new Auto { Kenteken = "Z-991-XK", Soort = "Auto", Merk = "BMW", Type = "7 Serie", Huurkosten = 174.25M, Borg = 500.00M, Aanschafjaar = 2024, Kleur = "zwart", Transmissie = "automaat" }
        };

        foreach (var auto in autos) {
            // Bestaande entries overslaan
            if (!context.Autos.Any(a => a.Kenteken == auto.Kenteken)) {
                context.Autos.Add(auto);
            }
        }

        context.SaveChanges(); // Slaat de nieuwe autos op
    }
}