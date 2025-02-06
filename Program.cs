using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WPR_backend.Data;
using WPR_backend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var secretKey = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(secretKey)) throw new Exception("Error: JWT Secret Key is missing from appsettings.json!");

// CORS Policy toevoegen
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// Database configureren
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity opzetten
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authenticatie configureren
var key = Encoding.UTF8.GetBytes(secretKey);
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Roles en Admin account aanmaken bij startup
using (var scope = app.Services.CreateScope()) {
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var keyNormalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate(); // Controleert of de db up to date is
    DbInitializer.SeedAutos(context); // Roept de Autos seed op

    string[] roles = { "Admin", "Particulier", "Zakelijk", "Frontoffice", "Backoffice", "Wagenparkbeheerder" };
    foreach (var role in roles) {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    string adminEmail = "admin@hhs.nl";
    string adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null) {
        Console.WriteLine($"🔍 Creating admin user: {adminEmail}");

        adminUser = new User {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Voornaam = "Admin",
            Achternaam = "User",
            NormalizedEmail = keyNormalizer.NormalizeName(adminEmail),
            NormalizedUserName = keyNormalizer.NormalizeName(adminEmail)
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded) {
            Console.WriteLine($"✅ Admin user created: {adminEmail}");
            await dbContext.SaveChangesAsync();
        } else {
            Console.WriteLine($"❌ Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return;
        }
    }

    // Kijkt of de Admin account bestaat
    adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null) {
        Console.WriteLine("Error: Admin user niet gevonden na het aanmaken.");
        return;
    }

    // Zorgt dat de rol Admin bestaat voor het toewijzen aan de gebruiker
    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (adminRole == null) {
        Console.WriteLine("Error: Admin rol bestaat niet. Aanmaken...");
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Voorkomt het dubbel toewijzen van rollen
    var rolesForAdmin = await userManager.GetRolesAsync(adminUser);
    if (!rolesForAdmin.Contains("Admin")) {
        Console.WriteLine($"Admin rol toevoegen aan {adminEmail}");
        var roleAssignmentResult = await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(adminEmail), "Admin");

        if (!roleAssignmentResult.Succeeded) {
            Console.WriteLine($"Error bij rol toewijzen: {string.Join(", ", roleAssignmentResult.Errors.Select(e => e.Description))}");
        } else {
            Console.WriteLine($"Succesvol Admin rol toegewezen aan {adminEmail}");
        }
    } else {
        Console.WriteLine($"{adminEmail} heeft al Admin rol.");
    }
}

app.UseCors("AllowReactApp"); // Laat de app CORS gebruiken

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();