using Microsoft.Azure.Management.AppService.Fluent.Models;

namespace ReallySimpleCerts.Core
{
    public class AzureRmOptions
    {
        public ServicePrincipalCredentials ServicePrincipalCredentials { get; set; }

        /// <summary>
        /// If null or empty the default subscription will be used.
        /// </summary>
        public string SubscriptionId { get; set; }
        public string ResourceGroupName { get; set; }
        public string WebAppName { get; set; }
        public CustomHostNameDnsRecordType DnsRecordType { get; set; }
    }
}
