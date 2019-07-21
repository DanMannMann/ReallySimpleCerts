using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public class KeyVaultPersistence : IReallySimpleCertPersistence
    {
        private readonly KeyVaultPersistenceOptions options;
        private readonly IKeyVaultClient keyVaultClient;
        private readonly ILogger<KeyVaultPersistence> logger;

        public KeyVaultPersistence(IOptions<KeyVaultPersistenceOptions> options, 
                                   IOptions<ReallySimpleCertOptions> baseOptions, 
                                   ILogger<KeyVaultPersistence> logger,
                                   IKeyVaultClientFactory clientFactory)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if (baseOptions?.Value == null) throw new ArgumentNullException(nameof(baseOptions));
            if (this.options.ServicePrincipalCredentials == null)
                this.options.ServicePrincipalCredentials = baseOptions.Value.DefaultAzureServicePrincipal;

            keyVaultClient = clientFactory?.GetClient(this.options.UseManagedIdentity ? null : this.options.ServicePrincipalCredentials) ?? throw new ArgumentNullException(nameof(clientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(string authz, Uri location)> GetAuthz(string token)
        {
            var secretIdentifier = $"{options.KeyVaultRootUrl.TrimEnd('/')}/secrets/authz-{SecretNameEncode(token)}";
            var secret = await GetSecret(secretIdentifier);
            return secret == null ? default : JsonConvert.DeserializeObject<(string authz, Uri location)>(secret);
        }

        public async Task<string> GetPemKey(string email)
        {
            var secretIdentifier = $"{options.KeyVaultRootUrl.TrimEnd('/')}/secrets/pem-{SecretNameEncode(email)}";
            return await GetSecret(secretIdentifier);
        }

        public async Task<byte[]> GetPfx(string nakedUrl)
        {
            var secretIdentifier = $"{options.KeyVaultRootUrl.TrimEnd('/')}/secrets/pfx-{SecretNameEncode(nakedUrl)}";
            var secret = await GetSecret(secretIdentifier);
            return secret == null ? default : Convert.FromBase64String(secret);
        }

        public async Task<string> GetPfxPassword(string nakedUrl)
        {
            var secretIdentifier = $"{options.KeyVaultRootUrl.TrimEnd('/')}/secrets/pfxpwd-{SecretNameEncode(nakedUrl)}";
            return await GetSecret(secretIdentifier);
        }

        public async Task StoreAuthz(string token, (string authz, Uri location) value)
        {
            await keyVaultClient.SetSecretAsync(options.KeyVaultRootUrl, $"authz-{SecretNameEncode(token)}", JsonConvert.SerializeObject(value));
        }

        public async Task StorePemKey(string email, string pemKey)
        {
            await keyVaultClient.SetSecretAsync(options.KeyVaultRootUrl, $"pem-{SecretNameEncode(email)}", pemKey);
        }

        public async Task StorePfx(string nakedUrl, byte[] pfx)
        {
            await keyVaultClient.SetSecretAsync(options.KeyVaultRootUrl, $"pfx-{SecretNameEncode(nakedUrl)}", Convert.ToBase64String(pfx));
        }

        public async Task StorePfxPassword(string nakedUrl, string password)
        {
            await keyVaultClient.SetSecretAsync(options.KeyVaultRootUrl, $"pfxpwd-{SecretNameEncode(nakedUrl)}", password);
        }

        private string SecretNameEncode(string input)
        {
            return new string(input.Where(x => char.IsLetterOrDigit(x) || x == '-').ToArray());
        }

        private async Task<string> GetSecret(string secretIdentifier)
        {
            try
            {
                var secret = await keyVaultClient.GetSecretAsync(secretIdentifier);
                return secret.Value;
            }
            catch (KeyVaultErrorException ex)
            when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (KeyVaultErrorException ex)
            {
                logger.LogError("Unexpected KeyVaultErrorException: {ex}", ex);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError("Unexpected exception: {ex}", ex);
                throw;
            }
        }
    }
}
