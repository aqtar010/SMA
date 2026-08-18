using Services;
using SMA.API.Services.Internal;
class Program
{
    static async Task<int> Main()
    {
        // Use same connection string as API. Prefer setting env var "DefaultConnection".
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection") ?? "Host=localhost;Port=7878;Database=sma;Username=postgres;Password=A@1234";

        Console.WriteLine("User Manager Console (type 'help')");
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLowerInvariant();

            try
            {
                switch (cmd)
                {
                    case "help":
                        Console.WriteLine("SuperAdmin Commands:");
                        Console.WriteLine(" create-superadmin   - interactively create the SuperAdmin (email, firstName, [lastName], password)");
                        Console.WriteLine(" update-superadmin   - interactively update an existing SuperAdmin");
                        Console.WriteLine(" get-superadmin      - retrieve SuperAdmin information by email");
                        Console.WriteLine();
                        Console.WriteLine("Admin Commands:");
                        Console.WriteLine(" create-admin        - interactively create an Admin (email, password)");
                        Console.WriteLine(" update-admin        - interactively update an existing Admin");
                        Console.WriteLine(" get-admin           - retrieve Admin information by email");
                        Console.WriteLine(" list-admins         - list all Admins");
                        Console.WriteLine(" delete-admin        - delete an Admin by email");
                        Console.WriteLine();
                        Console.WriteLine("Other Commands:");
                        Console.WriteLine(" exit");
                        break;

                    case "create-superadmin":
                        await CreateSuperAdmin(connectionString);
                        break;

                    case "update-superadmin":
                        await UpdateSuperAdmin(connectionString, parts);
                        break;

                    case "get-superadmin":
                        await GetSuperAdmin(connectionString, parts);
                        break;

                    case "create-admin":
                        await CreateAdmin(connectionString);
                        break;

                    case "update-admin":
                        await UpdateAdmin(connectionString, parts);
                        break;

                    case "get-admin":
                        await GetAdmin(connectionString, parts);
                        break;

                    case "list-admins":
                        await ListAdmins(connectionString);
                        break;

                    case "delete-admin":
                        await DeleteAdmin(connectionString, parts);
                        break;

                    case "exit":
                        return 0;

                    default:
                        Console.WriteLine("Unknown command. Type 'help'.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static async Task CreateSuperAdmin(string connectionString)
    {
        string email;
        while (true)
        {
            Console.Write("Email: ");
            email = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                Console.WriteLine("Please enter a valid email address.");
                continue;
            }
            break;
        }

        string firstName;
        while (true)
        {
            Console.Write("First name: ");
            firstName = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(firstName))
            {
                Console.WriteLine("First name is required.");
                continue;
            }
            break;
        }

        Console.Write("Last name (optional): ");
        var lastName = Console.ReadLine();

        string password;
        while (true)
        {
            Console.Write("Password (min 6 chars): ");
            password = Console.ReadLine() ?? string.Empty;
            if (password.Length < 6)
            {
                Console.WriteLine("Password must be at least 6 characters.");
                continue;
            }
            break;
        }

        await AdminService.CreateSuperAdminAsync(connectionString, email, firstName, string.IsNullOrWhiteSpace(lastName) ? null : lastName, password);
        Console.WriteLine("✓ SuperAdmin created successfully.");
    }

    static async Task UpdateSuperAdmin(string connectionString, string[] parts)
    {
        string targetEmail;
        if (parts.Length >= 2)
        {
            targetEmail = parts[1];
        }
        else
        {
            Console.Write("Email of SuperAdmin to update: ");
            targetEmail = Console.ReadLine() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(targetEmail))
        {
            Console.WriteLine("Email is required to locate the SuperAdmin.");
            return;
        }

        var existing = await AdminService.GetSuperAdminByEmailAsync(connectionString, targetEmail);
        if (existing == null)
        {
            Console.WriteLine("SuperAdmin not found.");
            return;
        }

        Console.WriteLine($"Current Email: {existing.Email}");
        Console.WriteLine($"Current FirstName: {existing.FirstName}");
        Console.WriteLine($"Current LastName: {existing.LastName}");

        Console.Write("New email (leave empty to keep): ");
        var newEmail = Console.ReadLine();
        Console.Write("New first name (leave empty to keep): ");
        var newFirst = Console.ReadLine();
        Console.Write("New last name (leave empty to keep): ");
        var newLast = Console.ReadLine();

        string? newPassword = null;
        Console.Write("Change password? (y/N): ");
        var pwChoice = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(pwChoice) && pwChoice.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            while (true)
            {
                Console.Write("New password (min 6 chars): ");
                var p = Console.ReadLine() ?? string.Empty;
                if (p.Length < 6)
                {
                    Console.WriteLine("Password must be at least 6 characters.");
                    continue;
                }
                newPassword = p;
                break;
            }
        }

        await AdminService.UpdateSuperAdminAsync(
            connectionString,
            targetEmail,
            string.IsNullOrWhiteSpace(newEmail) ? null : newEmail,
            string.IsNullOrWhiteSpace(newFirst) ? null : newFirst,
            string.IsNullOrWhiteSpace(newLast) ? null : newLast,
            newPassword
        );

        Console.WriteLine("✓ SuperAdmin updated successfully.");
    }

    static async Task GetSuperAdmin(string connectionString, string[] parts)
    {
        string emailToGet;
        if (parts.Length >= 2)
        {
            emailToGet = parts[1];
        }
        else
        {
            Console.Write("Email of SuperAdmin to retrieve: ");
            emailToGet = Console.ReadLine() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(emailToGet))
        {
            Console.WriteLine("Email is required.");
            return;
        }

        var superAdmin = await AdminService.GetSuperAdminByEmailAsync(connectionString, emailToGet);
        if (superAdmin == null)
        {
            Console.WriteLine("SuperAdmin not found.");
        }
        else
        {
            Console.WriteLine($"Email: {superAdmin.Email}");
            Console.WriteLine($"First Name: {superAdmin.FirstName}");
            Console.WriteLine($"Last Name: {superAdmin.LastName}");
            Console.WriteLine($"Active: {superAdmin.IsActive}");
        }
    }

    static async Task CreateAdmin(string connectionString)
    {
        string email;
        string firstName;
        string lastName;
        while (true)
        {
            Console.Write("Email: ");
            email = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                Console.WriteLine("Please enter a valid email address.");
                continue;
            }
            break;
        }
        
        Console.Write("First Name: ");
        firstName = Console.ReadLine() ?? string.Empty;

        Console.Write("Last Name: ");
        lastName = Console.ReadLine() ?? string.Empty;

        string password;
        while (true)
        {
            Console.Write("Password (min 6 chars): ");
            password = Console.ReadLine() ?? string.Empty;
            if (password.Length < 6)
            {
                Console.WriteLine("Password must be at least 6 characters.");
                continue;
            }
            break;
        }

        await AdminService.CreateAdminAsync(connectionString, email, firstName, lastName, password);
        Console.WriteLine("✓ Admin created successfully.");
    }

    static async Task UpdateAdmin(string connectionString, string[] parts)
    {
        string targetEmail;
        if (parts.Length >= 2)
        {
            targetEmail = parts[1];
        }
        else
        {
            Console.Write("Email of Admin to update: ");
            targetEmail = Console.ReadLine() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(targetEmail))
        {
            Console.WriteLine("Email is required to locate the Admin.");
            return;
        }

        var existing = await AdminService.GetAdminByEmailAsync(connectionString, targetEmail);
        if (existing == null)
        {
            Console.WriteLine("Admin not found.");
            return;
        }

        Console.WriteLine($"Current Email: {existing.Email}");

        Console.Write("New email (leave empty to keep): ");
        var newEmail = Console.ReadLine();

        string? newPassword = null;
        Console.Write("Change password? (y/N): ");
        var pwChoice = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(pwChoice) && pwChoice.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            while (true)
            {
                Console.Write("New password (min 6 chars): ");
                var p = Console.ReadLine() ?? string.Empty;
                if (p.Length < 6)
                {
                    Console.WriteLine("Password must be at least 6 characters.");
                    continue;
                }
                newPassword = p;
                break;
            }
        }

        await AdminService.UpdateAdminAsync(
            connectionString,
            targetEmail,
            string.IsNullOrWhiteSpace(newEmail) ? null : newEmail,
            newPassword
        );

        Console.WriteLine("✓ Admin updated successfully.");
    }

    static async Task GetAdmin(string connectionString, string[] parts)
    {
        string emailToGet;
        if (parts.Length >= 2)
        {
            emailToGet = parts[1];
        }
        else
        {
            Console.Write("Email of Admin to retrieve: ");
            emailToGet = Console.ReadLine() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(emailToGet))
        {
            Console.WriteLine("Email is required.");
            return;
        }

        var admin = await AdminService.GetAdminByEmailAsync(connectionString, emailToGet);
        if (admin == null)
        {
            Console.WriteLine("Admin not found.");
        }
        else
        {
            Console.WriteLine($"Email: {admin.Email}");
            Console.WriteLine($"Active: {admin.IsActive}");
            Console.WriteLine($"Created: {admin.CreatedAt}");
        }
    }

    static async Task ListAdmins(string connectionString)
    {
        var admins = await AdminService.ListAdminsAsync(connectionString);
        if (admins.Count == 0)
        {
            Console.WriteLine("No admins found.");
            return;
        }

        Console.WriteLine($"Total Admins: {admins.Count}");
        Console.WriteLine("─────────────────────────────────────────");
        foreach (var admin in admins)
        {
            Console.WriteLine($"Email: {admin.Email}");
            Console.WriteLine($"  Active: {admin.IsActive}");
            Console.WriteLine($"  Created: {admin.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }
    }

    static async Task DeleteAdmin(string connectionString, string[] parts)
    {
        string emailToDelete;
        if (parts.Length >= 2)
        {
            emailToDelete = parts[1];
        }
        else
        {
            Console.Write("Email of Admin to delete: ");
            emailToDelete = Console.ReadLine() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(emailToDelete))
        {
            Console.WriteLine("Email is required.");
            return;
        }

        Console.Write($"Are you sure you want to delete admin '{emailToDelete}'? (y/N): ");
        var confirmation = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(confirmation) || !confirmation.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Delete cancelled.");
            return;
        }

        var success = await AdminService.DeleteAdminAsync(connectionString, emailToDelete);
        if (success)
        {
            Console.WriteLine("✓ Admin deleted successfully.");
        }
        else
        {
            Console.WriteLine("Admin not found.");
        }
    }
}
