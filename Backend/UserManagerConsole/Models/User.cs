using System;

namespace Models
{
    public enum UserRole
    {
        Customer = 1,
        Admin,
        SuperAdmin
    }

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
