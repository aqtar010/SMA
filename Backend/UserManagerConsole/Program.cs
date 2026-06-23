using SMA.API.Services.Internal;
class Program
{
    static async Task<int> Main()
    {
        // Use same connection string as API. Prefer setting env var "DefaultConnection".
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
            ?? "Host=localhost;Database=sma;Username=postgres;Password=A@1234";

        Console.WriteLine("Commands:");
        Console.WriteLine(" create-superadmin   - interactively create the SuperAdmin (email, firstName, [lastName], password)");
        Console.WriteLine(" update-superadmin   - interactively update an existing SuperAdmin");
        Console.WriteLine(" exit");
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
                        Console.WriteLine("Commands:");
                        Console.WriteLine(" create-superadmin   - interactively create the SuperAdmin (email, firstName, [lastName], password)");
                        Console.WriteLine(" update-superadmin   - interactively update an existing SuperAdmin");
                        Console.WriteLine(" exit");
                        break;

                    case "create-superadmin":
                        // Turn-based interactive input for required fields
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
                        Console.WriteLine("SuperAdmin created.");
                        break;

                    case "update-superadmin":
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
                            break;
                        }

                        var existing = await AdminService.GetSuperAdminByEmailAsync(connectionString, targetEmail);
                        if (existing == null)
                        {
                            Console.WriteLine("SuperAdmin not found.");
                            break;
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

                        Console.WriteLine("SuperAdmin updated.");
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
}
