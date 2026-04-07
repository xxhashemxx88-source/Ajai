using Microsoft.EntityFrameworkCore;

namespace AJAI_Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@factory.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("123456")
                }
            );

            // 2. إنشاء كاميرتين جاهزة
            modelBuilder.Entity<Camera>().HasData(
                new Camera
                {
                    Id = 1,
                    Email = "cam1@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("123"),
                    IsActive = false,
                    LastPing = new DateTime(2026, 1, 1, 0, 0, 0)
                },
                new Camera
                {
                    Id = 2,
                    Email = "cam2@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("123"),
                    IsActive = false,
                    LastPing = new DateTime(2026, 1, 1, 0, 0, 0)
                }
            );
        }
    }
}