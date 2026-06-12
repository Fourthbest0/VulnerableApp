using Microsoft.EntityFrameworkCore;
using VulnerableApp.Models;

namespace VulnerableApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var adminHash = "$2a$11$YsGP5q5QzYgHqJr5oJqZUOHMWGnMQzSRpLcpJbp0g1HkgQLv3WMaG";
            var user1Hash = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi";
            var user2Hash = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi";

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = adminHash,
                    Email = "admin@test.com", Balance = 1000m, CreatedAt = new DateTime(2024, 1, 1) },
                new User { Id = 2, Username = "user1", PasswordHash = user1Hash,
                    Email = "user@test.com", Balance = 500m, CreatedAt = new DateTime(2024, 1, 1) },
                new User { Id = 3, Username = "user2", PasswordHash = user2Hash,
                    Email = "user2@test.com", Balance = 750m, CreatedAt = new DateTime(2024, 1, 1) }
            );
        }
    }
}