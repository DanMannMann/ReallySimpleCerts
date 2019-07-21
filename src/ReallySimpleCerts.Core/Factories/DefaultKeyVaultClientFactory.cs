using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace ReallySimpleCerts.Core
{
    public class DefaultKeyVaultClientFactory : IKeyVaultClientFactory
    {
        public IKeyVaultClient GetClient(ServicePrincipalCredentials creds)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider(creds == null ? null :
                $"RunAs=App;AppId={creds.ClientId};TenantId={creds.TenantId};AppKey={creds.Secret}");
            return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        }
    }
}
