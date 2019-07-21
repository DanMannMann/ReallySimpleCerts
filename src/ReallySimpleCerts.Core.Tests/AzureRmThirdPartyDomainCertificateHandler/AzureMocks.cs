using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Moq;

namespace ReallySimpleCerts.Core.Tests.AzureRmThirdPartyDomainCertificateHandlerTests
{

    public sealed class AzureMocks
    {
        public AzureMocks(Mock<IWebApp> webApp)
        {
            MockWebApp = webApp;
        }

        public Mock<IAzureConfigurationFactory> MockAzureFactory { get; } = new Mock<IAzureConfigurationFactory>();
        public Mock<Azure.IConfigurable> MockConfigurableAzure { get; }  = new Mock<Azure.IConfigurable>();
        public Mock<Azure.IAuthenticated> MockAuthenticatedAzure { get; } = new Mock<Azure.IAuthenticated>();
        public Mock<IAzure> MockAzure { get; } = new Mock<IAzure>();
        public Mock<IWebApps> MockWebApps { get; } = new Mock<IWebApps>();
        public Mock<IPagedCollection<IWebApp>> MockWebAppPaged { get; } = new Mock<IPagedCollection<IWebApp>>();
        public Mock<IWebApp> MockWebApp{ get; }
    }
}
