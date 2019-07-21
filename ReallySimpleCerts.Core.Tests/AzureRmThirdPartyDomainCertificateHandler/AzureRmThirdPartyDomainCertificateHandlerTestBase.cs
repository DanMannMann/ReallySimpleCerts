using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core.Tests.AzureRmThirdPartyDomainCertificateHandlerTests
{
    public abstract class AzureRmThirdPartyDomainCertificateHandlerTestBase
    {

        protected static void GetMocks(
            ReallySimpleCertOptions certOptsValue, AzureRmOptions optsValue,
            out Mock<ILogger<AzureRmThirdPartyDomainCertificateHandler>> mockLogger, out Mock<IOptions<AzureRmOptions>> mockOpts,
            out Mock<IOptions<ReallySimpleCertOptions>> mockCertOpts, out Mock<IWebApp> mockWebApp, out AzureMocks mocks,
            string webAppName, string clientId, string tenantId)
        {
            mockLogger = new Mock<ILogger<AzureRmThirdPartyDomainCertificateHandler>>();
            mockOpts = new Mock<IOptions<AzureRmOptions>>();
            mockOpts.Setup(m => m.Value).Returns(optsValue).Verifiable();

            mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value).Returns(certOptsValue).Verifiable();

            mockWebApp = new Mock<IWebApp>();
            mockWebApp.Setup(m => m.Name)
                      .Returns(webAppName)
                      .Verifiable();
            mocks = WrapMockWebApp(mockWebApp, clientId, tenantId);
        }

        protected static void WebAppHasHostNames(Mock<IWebApp> mockWebApp, params string[] hostnames)
        {
            mockWebApp.Setup(m => m.HostNames)
                      .Returns(new HashSet<string>(hostnames))
                      .Verifiable();
        }

        protected static AzureMocks WrapMockWebApp(Mock<IWebApp> mockWebApp, string clientId, string tenantId)
        {
            var mocks = new AzureMocks(mockWebApp);

            mocks.MockAzureFactory
                .Setup(m => m.Configuration)
                .Returns(mocks.MockConfigurableAzure.Object)
                .Verifiable();

            mocks.MockConfigurableAzure
                .Setup(m => m.WithLogLevel(It.IsAny<HttpLoggingDelegatingHandler.Level>()))
                .Returns(mocks.MockConfigurableAzure.Object)
                .Verifiable();

            mocks.MockConfigurableAzure
                .Setup(m => m.Authenticate(It.Is<AzureCredentials>(ac => ac.ClientId == clientId && ac.TenantId == tenantId)))
                .Returns(mocks.MockAuthenticatedAzure.Object)
                .Verifiable();

            mocks.MockAuthenticatedAzure
                .Setup(m => m.WithSubscription(It.IsAny<string>()))
                .Returns(mocks.MockAzure.Object)
                .Verifiable();

            mocks.MockAuthenticatedAzure
                .Setup(m => m.WithDefaultSubscription())
                .Returns(mocks.MockAzure.Object)
                .Verifiable();

            mocks.MockAzure
                .Setup(m => m.WebApps)
                .Returns(mocks.MockWebApps.Object)
                .Verifiable();

            mocks.MockWebApps
                    .Setup(m => m.ListByResourceGroupAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(mocks.MockWebAppPaged.Object))
                    .Verifiable();


            mocks.MockWebAppPaged
                .Setup(m => m.GetEnumerator())
                .Returns(new List<IWebApp> { mockWebApp.Object }.GetEnumerator())
                .Verifiable();

            return mocks;
        }
    }
}
