using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EposRetail.Models;
using Microsoft.Extensions.Configuration;

namespace EposRetail.Services
{
    public class AzureKeyVaultSecretProvider
    {
#if DEBUG
        private const string TeyaConfigurationSection = "TeyaSandbox";
#else
        private const string TeyaConfigurationSection = "TeyaProduction";
#endif

        private readonly IConfiguration _configuration;
        private readonly SecretClient? _secretClient;

        public AzureKeyVaultSecretProvider(IConfiguration configuration)
        {
            _configuration = configuration;

            var keyVaultUri = GetTeyaSetting("PartnerCredentials:KeyVaultUri");
            if (!string.IsNullOrWhiteSpace(keyVaultUri))
            {
                _secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            }
        }

        public async Task<TeyaPartnerCredentials> GetTeyaPartnerCredentialsAsync()
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException($"Azure Key Vault is not configured. Set {TeyaConfigurationSection}:PartnerCredentials:KeyVaultUri in Appsettings.json.");
            }

            var clientIdSecretName = GetTeyaSetting("PartnerCredentials:ClientIdSecretName");
            var clientSecretSecretName = GetTeyaSetting("PartnerCredentials:ClientSecretSecretName");

            if (string.IsNullOrWhiteSpace(clientIdSecretName) || string.IsNullOrWhiteSpace(clientSecretSecretName))
            {
                throw new InvalidOperationException("Teya partner secret names are missing from configuration.");
            }

            var clientId = await _secretClient.GetSecretAsync(clientIdSecretName);
            var clientSecret = await _secretClient.GetSecretAsync(clientSecretSecretName);

            return new TeyaPartnerCredentials
            {
                ClientId = clientId.Value.Value,
                ClientSecret = clientSecret.Value.Value
            };
        }

        private string? GetTeyaSetting(string settingName) =>
            _configuration[$"{TeyaConfigurationSection}:{settingName}"];
    }
}
