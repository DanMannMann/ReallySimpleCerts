using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Azure.Storage.Blob;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Logging;

namespace ReallySimpleCerts.Core.Tests.Persistence.KeyVault
{
    public class KeyVaultPersistenceTestBase
    {
        protected KeyVaultPersistence Setup(Mock<IOptions<KeyVaultPersistenceOptions>> mockOpts, 
                                            Mock<IOptions<ReallySimpleCertOptions>> mockCertOpts, 
                                            out Mock<IKeyVaultClient> mockClient, 
                                            out Mock<IKeyVaultClientFactory> mockClientFactory,
                                            out Mock<ILogger<KeyVaultPersistence>> mockLogger)
        {
            mockClient = new Mock<IKeyVaultClient>(MockBehavior.Strict);

            mockClientFactory = new Mock<IKeyVaultClientFactory>();
            mockClientFactory.Setup(m => m.GetClient(It.IsAny<ServicePrincipalCredentials>()))
                                .Returns(mockClient.Object)
                                .Verifiable();

            mockLogger = new Mock<ILogger<KeyVaultPersistence>>();
            var subject = new KeyVaultPersistence(mockOpts?.Object, mockCertOpts?.Object, mockLogger.Object, mockClientFactory.Object);
            Assert.IsNotNull(subject);
            return subject;
        }
    }
}
