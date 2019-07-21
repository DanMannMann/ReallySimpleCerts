using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace ReallySimpleCerts.Core.Tests.FactoryTests
{
    [TestClass]
    public sealed class FactoryTests
    {
        [TestInitialize]
        public void Prep()
        {

        }

        [TestMethod]
        public void DefaultAzureConfigurationFactoryTest()
        {
            var subject = new DefaultAzureConfigurationFactory();
            Assert.IsTrue(subject.Configuration != null);
        }

        [TestMethod]
        public void DefaultBlobContainerFactoryTest()
        {
            var mockOpts = new Mock<IOptions<BlobStorePersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new BlobStorePersistenceOptions
                    {
                        StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=faketestaccount;AccountKey=9DVhn5FU6C9ZyvAub/nCijiT0WDnm/BYpy5woKYmOn3b6bkSI2p8n2ip/q6sNoqatDQ3J1xohJutN0gyaElNwg==;EndpointSuffix=core.windows.net"
                    });
            var subject = new DefaultBlobContainerFactory(mockOpts.Object);
            Assert.IsTrue(subject.GetContainer().Result != null);
        }

        [TestMethod]
        public void DefaultKeyVaultClientFactoryTest()
        {
            var subject = new DefaultKeyVaultClientFactory();
            var client = subject.GetClient(new ServicePrincipalCredentials { ClientId = "test", TenantId = "test", Secret = "test" });
            Assert.IsNotNull(client);

            var client2 = subject.GetClient(null);
            Assert.IsNotNull(client2);
        }
    }
}
