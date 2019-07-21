using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Azure.Storage.Blob;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace ReallySimpleCerts.Core.Tests.Persistence.BlobStore
{
    public class BlobStorePersistenceTestBase
    {
        protected BlobStorePersistence Setup(Mock<IOptions<BlobStorePersistenceOptions>> mockOpts, out Mock<CloudBlobContainer> mockContainer, out Mock<IBlobContainerFactory> mockContainerFactory)
        {
            mockContainer = new Mock<CloudBlobContainer>(MockBehavior.Strict, new object[] { new Uri("https://test/blob") });
            mockContainer.Setup(m => m.CreateIfNotExistsAsync())
                         .Returns(Task.FromResult(true))
                         .Verifiable();

            mockContainerFactory = new Mock<IBlobContainerFactory>();
            mockContainerFactory.Setup(m => m.GetContainer())
                                .Returns(Task.FromResult(mockContainer.Object))
                                .Verifiable();

            var subject = new BlobStorePersistence(mockContainerFactory.Object, mockOpts?.Object);
            Assert.IsNotNull(subject);
            return subject;
        }

        protected void SetupGet(Mock<IOptions<BlobStorePersistenceOptions>> mockOpts, string blobName, bool exists, out Mock<CloudBlockBlob> mockBlock, out BlobStorePersistence subject, out Mock<CloudBlobContainer> mockContainer, out Mock<IBlobContainerFactory> mockContainerFactory)
        {
            mockBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, new object[] { new Uri("https://test/blob/blobby") });
            mockBlock.Setup(m => m.ExistsAsync())
                     .Returns(Task.FromResult(exists))
                     .Verifiable();

            Expression<Func<string, bool>> blobNameMatcher = s => blobName.EndsWith(s, StringComparison.InvariantCultureIgnoreCase);

            subject = Setup(mockOpts, out mockContainer, out mockContainerFactory);
            mockContainer.Setup(m => m.GetBlockBlobReference(It.Is(blobNameMatcher)))
                         .Returns(mockBlock.Object);
        }

        protected void SetupStore(Mock<IOptions<BlobStorePersistenceOptions>> mockOpts, string blobName, out Mock<CloudBlockBlob> mockBlock, out BlobStorePersistence subject, out Mock<CloudBlobContainer> mockContainer, out Mock<IBlobContainerFactory> mockContainerFactory)
        {
            mockBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, new object[] { new Uri("https://test/blob/blobby") });
            mockBlock.Setup(m => m.DeleteIfExistsAsync())
                     .Returns(Task.FromResult(true))
                     .Verifiable();

            Expression<Func<string, bool>> blobNameMatcher = s => blobName.EndsWith(s, StringComparison.InvariantCultureIgnoreCase);

            subject = Setup(mockOpts, out mockContainer, out mockContainerFactory);
            mockContainer.Setup(m => m.GetBlockBlobReference(It.Is(blobNameMatcher)))
                         .Returns(mockBlock.Object);
        }
    }
}
