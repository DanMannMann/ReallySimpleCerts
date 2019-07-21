using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;

namespace ReallySimpleCerts.Core.Tests.Persistence.BlobStore
{
    [TestClass]
    public class BlobStorePersistencePfxTests : BlobStorePersistenceTestBase
    {
        [TestMethod]
        public async Task GetReturnsDefaultIfBlobDoesNotExist()
        {
            var mockOpts = new Mock<IOptions<BlobStorePersistenceOptions>>();
            var optsValue = new BlobStorePersistenceOptions
            {

            };
            mockOpts.Setup(m => m.Value)
                    .Returns(optsValue);

            var url = "testsub.testdomain.com";
            var blobName = $"{optsValue.BlobPathPrefix}/pfx/{url}";
            var exists = false;
            SetupGet(mockOpts, blobName, exists, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);

            var result = await subject.GetPfx(url);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockBlock.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
            mockBlock.VerifyNoOtherCalls();
            Assert.IsTrue(result == default);
        }

        [TestMethod]
        public async Task GetReturnsCorrectValue()
        {
            var mockOpts = new Mock<IOptions<BlobStorePersistenceOptions>>();
            var optsValue = new BlobStorePersistenceOptions
            {

            };
            mockOpts.Setup(m => m.Value)
                    .Returns(optsValue);

            var url = "testsub.testdomain.com";
            var blobName = $"{optsValue.BlobPathPrefix}/pfx/{url}";
            var exists = true;
            SetupGet(mockOpts, blobName, exists, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);
            mockBlock.Setup(m => m.DownloadToStreamAsync(It.IsAny<MemoryStream>()))
                     .Returns(Task.FromResult(10))
                     .Verifiable();

            var result = await subject.GetPfx(url);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockBlock.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
            mockBlock.VerifyNoOtherCalls();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StoreCallsDeleteIfExistsThenPersistsCorrectValue()
        {
            var mockOpts = new Mock<IOptions<BlobStorePersistenceOptions>>();
            var optsValue = new BlobStorePersistenceOptions
            {

            };
            mockOpts.Setup(m => m.Value)
                    .Returns(optsValue);

            var url = "testsub.testdomain.com";
            var blobName = $"{optsValue.BlobPathPrefix}/pfx/{url}";
            var result = new byte[] { 3, 4, 5, 6, 7 };

            SetupStore(mockOpts, blobName, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);

            mockBlock.Setup(m => m.UploadFromByteArrayAsync(It.IsAny<byte[]>(), It.Is<int>(s => s == 0), It.Is<int>(s => s == 5)))
                     .Returns(Task.CompletedTask)
                     .Verifiable();

            await subject.StorePfx(url, result);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockBlock.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
            mockBlock.VerifyNoOtherCalls();
        }
    }
}
