using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{

    public class AzureRmThirdPartyDomainCertificateHandler : ICertificateHandler, IHostNameHandler
    {
        private readonly AzureRmOptions options;
        private readonly ReallySimpleCertOptions rscOptions;
        private readonly ILogger<AzureRmThirdPartyDomainCertificateHandler> logger;
        private readonly IAzureConfigurationFactory azureConfigurationFactory;

        public AzureRmThirdPartyDomainCertificateHandler(IOptions<AzureRmOptions> options, 
                                                         IOptions<ReallySimpleCertOptions> rscOptions, 
                                                         ILogger<AzureRmThirdPartyDomainCertificateHandler> logger,
                                                         IAzureConfigurationFactory azureConfigurationFactory)
        {
            this.options = options?.Value ?? throw new ArgumentNullException("options cannot be null");
            this.rscOptions = rscOptions?.Value ?? throw new ArgumentNullException("rscOptions cannot be null");
            this.logger = logger ?? throw new ArgumentNullException("logger cannot be null");
            this.azureConfigurationFactory = azureConfigurationFactory ?? throw new ArgumentNullException("azureConfigurationFactory cannot be null");

            if (this.options.ServicePrincipalCredentials == null)
                this.options.ServicePrincipalCredentials = rscOptions.Value.DefaultAzureServicePrincipal;

            if (this.options.ServicePrincipalCredentials == null)
                throw new ArgumentNullException("ServicePrincipalCredentials cannot be null");
            if (this.rscOptions.CertificateInfo == null)
                throw new ArgumentNullException("CertificateInfo cannot be null");
        }

        public Task CertificateRestored(X509Certificate2 cert, byte[] pfx, string pfxpwd)
        {
            return NewCertificateCreated(cert, pfx, pfxpwd);
        }

        public async Task NewCertificateCreated(X509Certificate2 cert, byte[] pfx, string pfxpwd)
        {
            var webApp = await GetWebApp();

            var certExists = webApp.HostNameSslStates.ContainsKey(rscOptions.CertificateInfo.CommonName);
            if (certExists)
            {
                var certState = webApp.HostNameSslStates[rscOptions.CertificateInfo.CommonName];
                if (certState.Thumbprint == cert.Thumbprint)
                {
                    logger.LogTrace($"Cert {certState.Thumbprint} already installed");
                    return;
                }
            }

            await EnsureHostNameBinding(webApp);

            var certFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pfx");
            File.WriteAllBytes(certFilePath, pfx);
            await DefineSSLBinding(webApp, (certFilePath, pfxpwd), rscOptions.CertificateInfo.CommonName);
            File.Delete(certFilePath);
        }

        public async Task EnsureHostNameBinding()
        {
            await EnsureHostNameBinding(await GetWebApp());
        }

        private async Task<IWebApp> GetWebApp()
        {
            AzureCredentials credentials = SdkContext.AzureCredentialsFactory
                   .FromServicePrincipal(options.ServicePrincipalCredentials.ClientId, options.ServicePrincipalCredentials.Secret, options.ServicePrincipalCredentials.TenantId, AzureEnvironment.AzureGlobalCloud);

            var configure = azureConfigurationFactory.Configuration;

            var azure = string.IsNullOrWhiteSpace(options.SubscriptionId) ?
                configure
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription()
                        :
                configure
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithSubscription(options.SubscriptionId);

            var webApps = await azure.WebApps.ListByResourceGroupAsync(options.ResourceGroupName);
            var result = webApps.FirstOrDefault(x => x.Name == options.WebAppName);

            if (result == null)
            {
                throw new InvalidOperationException($"Web app {options.WebAppName} not found in resource group {options.ResourceGroupName}");
            }

            return result;
        }

        private async Task EnsureHostNameBinding(IWebApp webApp)
        {
            var hostNameIsSet = webApp.HostNames.Any(x => rscOptions.CertificateInfo.CommonName.Equals(x, StringComparison.InvariantCultureIgnoreCase));
            if (!hostNameIsSet)
            {
                await DefineHostBinding(webApp, rscOptions.CertificateInfo.CommonName);
            }
        }

        private async Task DefineHostBinding(IWebApp webApp, string hostName)
        {
            var split = hostName.Split('.');
            var subby = split[0];
            var domain = string.Join(".", split.Skip(1));

            await webApp
                    .Update()
                        .DefineHostnameBinding()
                            .WithThirdPartyDomain(domain)
                            .WithSubDomain(subby)
                            .WithDnsRecordType(options.DnsRecordType)
                            .Attach()
                    .ApplyAsync();

            var complete = false;
            do
            {
                var bindings = await webApp.GetHostNameBindingsAsync();
                complete = bindings?.ContainsKey(hostName) ?? false && bindings[hostName].HostNameType == HostNameType.Verified;
            }
            while (!complete);
        }

        private static async Task DefineSSLBinding(IWebApp webApp, (string Path, string Password) cert, string hostName)
        {
            await webApp
                    .Update()
                        .DefineSslBinding()
                            .ForHostname(hostName)
                            .WithPfxCertificateToUpload(cert.Path, cert.Password)
                            .WithSniBasedSsl()
                            .Attach()
                    .ApplyAsync();
        }
    }
}
