using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WPR_backend.Models;

namespace WPR_backend.Data {
    public class ApplicationDbContext : IdentityDbContext<User> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            // ✅ Force Id to be string
            builder.Entity<User>()
                .Property(u => u.Id)
                .HasColumnType("nvarchar(450)")
                .IsRequired();
        }
    }
}