using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Domain.Entities.Users;
using FinitiGlossary.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FinitiGlossary.Infrastructure.Seeding
{
    public class AdminSeeder
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher _hasher;

        public AdminSeeder(AppDbContext db, IConfiguration config, IPasswordHasher hasher)
        {
            _db = db;
            _config = config;
            _hasher = hasher;
        }

        public async Task SeedAsync()
        {
            // ---- 1. Check if admin exists ----
            var existingAdmin = await _db.Users
                .FirstOrDefaultAsync(u => u.Role == "Admin");

            if (existingAdmin != null)
                return; // Already seeded

            // ---- 2. Load values from appsettings ----
            var username = _config["AdminSeed:Username"];
            var email = _config["AdminSeed:Email"];
            var password = _config["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("AdminSeed settings are missing in appsettings.json.");
            }

            // ---- 3. Create admin user ----
            var admin = new User
            {
                Username = username,
                Email = email,
                PasswordHash = _hasher.Hash(password),
                Role = "Admin",
                IsAdmin = true,
                MustChangePassword = true,
                MustUpdateProfile = true,
                CreatedAt = DateTime.UtcNow
            };

            // ---- 4. Save to DB ----
            await _db.Users.AddAsync(admin);
            await _db.SaveChangesAsync();
        }
    }
}
