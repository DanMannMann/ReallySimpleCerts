using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace ReallySimpleCerts.Core.Tests.Persistence.BlobStore
{

    [TestClass]
    public class BlobStorePersistenceTests : BlobStorePersistenceTestBase
    {
        [TestMethod]
        public void CanConstruct()
        {
            var mockOpts = new Mock<IOptions<BlobStorePersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new BlobStorePersistenceOptions
                    {

                    });

            Setup(mockOpts, out var mockContainer, out var mockContainerFactory);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ThrowsIfFactoryIsNull()
        {
            var mockOpts = new Mock<IOptions<BlobStorePersistenceOptions>>();
            var optsValue = new BlobStorePersistenceOptions
            {

            };
            mockOpts.Setup(m => m.Value)
                    .Returns(optsValue);

            Assert.ThrowsException<ArgumentNullException>(() => new BlobStorePersistence(null, mockOpts.Object));
        }

        [TestMethod]
        public void ThrowsIfOptionsIsNull()
        {
            var mockOpts = new Mock<IOptions<BlobStorePersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns((BlobStorePersistenceOptions)null);

            Assert.ThrowsException<ArgumentNullException>(() => Setup(null, out var mockContainer, out var mockContainerFactory));
            Assert.ThrowsException<ArgumentNullException>(() => Setup(mockOpts, out var mockContainer, out var mockContainerFactory));
        }
    }
}
