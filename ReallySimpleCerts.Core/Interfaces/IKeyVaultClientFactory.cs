using Microsoft.Azure.KeyVault;

namespace ReallySimpleCerts.Core
{
    public interface IKeyVaultClientFactory
    {
        IKeyVaultClient GetClient(ServicePrincipalCredentials creds); 
    }
}
