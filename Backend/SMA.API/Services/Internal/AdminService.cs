using Microsoft.EntityFrameworkCore;
using SMA.API.Data;
using SMA.API.Enums;
using SMA.API.Utilities;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UserManagerConsole")]
namespace SMA.API.Services.Internal
{
    internal static class AdminService
    {
        public static async Task CreateSuperAdminAsync(string connectionString, string email, string firstName, string? lastName, string password)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var db = new AppDbContext(options);

            if (await db.Users.AnyAsync(u => u.Role == UserRoles.SuperAdmin.ToString()))
                throw new InvalidOperationException("A SuperAdmin already exists.");

            if (await db.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("Email already exists.");

            var user = new Entities.User
            {
                Email = email,
                PasswordHash = PasswordHasher.HashPassword(password),
                FirstName = firstName,
                LastName = lastName ?? "",
                Role = UserRoles.SuperAdmin.ToString(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        public static async Task<Entities.User?> GetSuperAdminByEmailAsync(string connectionString, string email)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var db = new AppDbContext(options);
            return await db.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.SuperAdmin.ToString() && u.Email == email);
        }

        public static async Task UpdateSuperAdminAsync(string connectionString, string email, string? newEmail, string? newFirstName, string? newLastName, string? newPassword)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var db = new AppDbContext(options);

            var user = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.SuperAdmin.ToString() && u.Email == email);
            if (user == null) throw new InvalidOperationException("SuperAdmin not found.");

            if (!string.IsNullOrWhiteSpace(newEmail))
            {
                if (await db.Users.AnyAsync(u => u.Email == newEmail && u.Id != user.Id))
                    throw new InvalidOperationException("Email already in use.");
                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(newFirstName)) user.FirstName = newFirstName;
            if (!string.IsNullOrWhiteSpace(newLastName)) user.LastName = newLastName;
            if (!string.IsNullOrWhiteSpace(newPassword)) user.PasswordHash = PasswordHasher.HashPassword(newPassword);

            user.UpdatedAt = DateTime.UtcNow;
            db.Users.Update(user);
            await db.SaveChangesAsync();
        }

        public static async Task CreateAdminAsync(string connectionString, string email, string password)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var db = new AppDbContext(options);

            if (await db.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("Email already exists.");

            var user = new Entities.User
            {
                Email = email,
                PasswordHash = PasswordHasher.HashPassword(password),
                Role = UserRoles.Admin.ToString(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        public static async Task<bool> DeleteAdminAsync(string connectionString, string email)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var db = new AppDbContext(options);

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == UserRoles.Admin.ToString());
            if (user == null) return false;

            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
