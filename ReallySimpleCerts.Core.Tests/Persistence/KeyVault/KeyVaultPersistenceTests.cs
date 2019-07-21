using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ReallySimpleCerts.Core.Tests.Persistence.KeyVault
{

    [TestClass]
    public class KeyVaultPersistenceTests : KeyVaultPersistenceTestBase
    {
        [TestMethod]
        public void CanConstruct()
        {
            var mockOpts = new Mock<IOptions<KeyVaultPersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new KeyVaultPersistenceOptions
                    {

                    });

            var mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value)
                    .Returns(new ReallySimpleCertOptions
                    {

                    });

            Setup(mockOpts, mockCertOpts, out var mockClient, out var mockContainer, out var mockContainerFactory);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ThrowsIfFactoryIsNull()
        {
            var mockOpts = new Mock<IOptions<KeyVaultPersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new KeyVaultPersistenceOptions
                    {

                    });

            var mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value)
                    .Returns(new ReallySimpleCertOptions
                    {

                    });

            var mockLogger = new Mock<ILogger<KeyVaultPersistence>>();

            Assert.ThrowsException<ArgumentNullException>(() => new KeyVaultPersistence(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, null));
        }

        [TestMethod]
        public void ThrowsIfOptionsIsNull()
        {
            var mockOpts = new Mock<IOptions<KeyVaultPersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns((KeyVaultPersistenceOptions)null);

            var mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value)
                    .Returns(new ReallySimpleCertOptions
                    {

                    });

            var mockClientFactory = new Mock<IKeyVaultClientFactory>();
            var mockLogger = new Mock<ILogger<KeyVaultPersistence>>();

            Assert.ThrowsException<ArgumentNullException>(() => new KeyVaultPersistence(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mockClientFactory.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new KeyVaultPersistence(null, mockCertOpts.Object, mockLogger.Object, mockClientFactory.Object));
        }

        [TestMethod]
        public void ThrowsIfCertOptionsIsNull()
        {
            var mockOpts = new Mock<IOptions<KeyVaultPersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new KeyVaultPersistenceOptions
                    {

                    });

            var mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value)
                    .Returns((ReallySimpleCertOptions)null);

            var mockClientFactory = new Mock<IKeyVaultClientFactory>();
            var mockLogger = new Mock<ILogger<KeyVaultPersistence>>();

            Assert.ThrowsException<ArgumentNullException>(() => new KeyVaultPersistence(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mockClientFactory.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new KeyVaultPersistence(mockOpts.Object, null, mockLogger.Object, mockClientFactory.Object));
        }
    }
}
