using System.Text.Json;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataHandlerLibrary.Data
{
    public sealed class DatabaseInitializationDesignTimeFactory : IDesignTimeDbContextFactory<DatabaseInitialization>
    {
        public DatabaseInitialization CreateDbContext(string[] args)
        {
            var appsettingsPath = FindEposAppsettingsPath();

            using var document = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
            var root = document.RootElement;

            var encryptedConnectionString = root.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString();
            var encryptionKey = root.GetProperty("Encryption").GetProperty("Key").GetString();

            if (string.IsNullOrWhiteSpace(encryptedConnectionString))
            {
                throw new InvalidOperationException($"ConnectionStrings:DefaultConnection missing in {appsettingsPath}");
            }

            if (string.IsNullOrWhiteSpace(encryptionKey))
            {
                throw new InvalidOperationException($"Encryption:Key missing in {appsettingsPath}");
            }

            var aes = new AESEncryptDecryptServices();
            var decryptedConnectionString = aes.Decrypt(encryptedConnectionString, encryptionKey);

            var options = new DbContextOptionsBuilder<DatabaseInitialization>()
                .UseNpgsql(decryptedConnectionString)
                .Options;

            return new DatabaseInitialization(options);
        }

        private static string FindEposAppsettingsPath()
        {
            var currentDir = Directory.GetCurrentDirectory();
            for (int i = 0; i < 8; i++)
            {
                var candidate = Path.Combine(currentDir, "KaninifyRetailEpos", "Appsettings.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                var parent = Directory.GetParent(currentDir);
                if (parent == null)
                {
                    break;
                }

                currentDir = parent.FullName;
            }

            throw new InvalidOperationException("Could not find KaninifyRetailEpos\\Appsettings.json by walking up from the current directory.");
        }
    }
}

