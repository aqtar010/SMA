using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Models;

namespace Services
{
    public class UserStore
    {
        private readonly string _filePath;
        public List<User> Users { get; private set; } = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public UserStore(string filePath)
        {
            _filePath = filePath;
        }

        public async Task LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                Users = new List<User>();
                return;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            Users = JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(Users, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task CreateSuperAdminAsync(string username, string password)
        {
            if (Users.Any(u => u.Role == UserRole.SuperAdmin))
                throw new InvalidOperationException("A SuperAdmin already exists.");

            if (Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Username already exists.");

            var user = new User
            {
                Username = username,
                PasswordHash = Hash(password),
                Role = UserRole.SuperAdmin
            };
            Users.Add(user);
            await SaveAsync();
            Console.WriteLine("SuperAdmin created.");
        }

        public async Task CreateUserAsync(string username, string password, UserRole role)
        {
            if (Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Username already exists.");

            var user = new User
            {
                Username = username,
                PasswordHash = Hash(password),
                Role = role
            };
            Users.Add(user);
            await SaveAsync();
            Console.WriteLine($"{role} created.");
        }

        public User? Authenticate(string username, string password)
        {
            var hash = Hash(password);
            return Users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash == hash);
        }

        public async Task<bool> DeleteAdminAsync(string username)
        {
            var user = Users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Role == UserRole.Admin);

            if (user == null) return false;
            Users.Remove(user);
            await SaveAsync();
            return true;
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }

    public class Session
    {
        public User? CurrentUser { get; set; }
    }
}
