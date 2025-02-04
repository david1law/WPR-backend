using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WPR_backend.Data;
using WPR_backend.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var secretKey = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(secretKey)) throw new Exception("❌ ERROR: JWT Secret Key is missing from appsettings.json!");

// Add CORS Policy
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with Custom User Model
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
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

// Ensure Roles & Admin Account Exist on Startup
using (var scope = app.Services.CreateScope()) {
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var keyNormalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();

    string[] roles = { "Admin", "Particulier", "Frontoffice", "Backoffice" };
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

    // ✅ Retrieve the Admin user again to ensure they exist
    adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null) {
        Console.WriteLine("❌ ERROR: Admin user could not be found after creation.");
        return;
    }

    // ✅ Ensure the Admin Role exists before assigning
    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (adminRole == null) {
        Console.WriteLine("❌ ERROR: Admin role does not exist. Creating it now...");
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // ✅ Prevent Duplicate Role Assignment Before Adding
    var rolesForAdmin = await userManager.GetRolesAsync(adminUser);
    if (!rolesForAdmin.Contains("Admin")) {
        Console.WriteLine($"🔍 Assigning 'Admin' role to {adminEmail}");
        var roleAssignmentResult = await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(adminEmail), "Admin");

        if (!roleAssignmentResult.Succeeded) {
            Console.WriteLine($"❌ Error assigning role: {string.Join(", ", roleAssignmentResult.Errors.Select(e => e.Description))}");
        } else {
            Console.WriteLine($"✅ Successfully assigned 'Admin' role to {adminEmail}");
        }
    } else {
        Console.WriteLine($"✅ {adminEmail} already has 'Admin' role.");
    }
}

app.UseCors("AllowReactApp");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();