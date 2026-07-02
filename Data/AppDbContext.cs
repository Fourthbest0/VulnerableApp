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
            // Contraseñas en texto plano (solo para referencia del equipo, NUNCA en producción):
            // admin -> "admin" | user1 -> "123456" | user2 -> "password"
            var adminHash = "$2a$11$waU2kZKN9Df4PqJJHrjk6.tcdKKZslX3p/KInASg/piP3FwTxoZBu";
            var user1Hash = "$2a$11$y0FWsUlOCUO2SQ8n44nliO2.ePXA38Ro6eeozfewJjfwNIyzd6LW6";
            var user2Hash = "$2a$11$8aifJEV0jF83hyqVLs6/w.a.FkXZIzcnjUkqOfaeqHQ9opQHg9ZEi";

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