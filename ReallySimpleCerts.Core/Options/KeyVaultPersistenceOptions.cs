namespace ReallySimpleCerts.Core
{
    public class KeyVaultPersistenceOptions
    {
        public string KeyVaultRootUrl { get; set; }
        public string KeyVaultConnectionString { get; set; }
        public bool UseManagedIdentity { get; set; }
        public ServicePrincipalCredentials ServicePrincipalCredentials { get; set; }
    }
}