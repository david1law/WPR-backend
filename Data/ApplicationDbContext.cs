using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WPR_backend.Models;

namespace WPR_backend.Data {
    public class ApplicationDbContext : IdentityDbContext<User> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Auto> Autos { get; set; }
        public DbSet<Verhuur> Verhuur { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            // ✅ Force Id to be string
            builder.Entity<User>()
                .Property(u => u.Id)
                .HasColumnType("nvarchar(450)")
                .IsRequired();

                builder.Entity<Auto>()
                .HasKey(a => a.Kenteken);

            // ✅ Define Foreign Key Relationship in `Verhuur`
            builder.Entity<Verhuur>()
                .HasOne(v => v.Auto)
                .WithMany()
                .HasForeignKey(v => v.Kenteken)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Verhuur>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }

    }
}