using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ReallySimpleCerts.Core.Tests.Persistence.BlobStore
{
    [TestClass]
    public class BlobStorePersistencePfxPasswordTests : BlobStorePersistenceTestBase
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
            var blobName = $"{optsValue.BlobPathPrefix}/pfxpwd/{url}";
            var exists = false;
            SetupGet(mockOpts, blobName, exists, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);

            var result = await subject.GetPfxPassword(url);

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
            var blobName = $"{optsValue.BlobPathPrefix}/pfxpwd/{url}";
            var resultPassword = "passw0rRD!";
            var exists = true;
            SetupGet(mockOpts, blobName, exists, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);
            mockBlock.Setup(m => m.DownloadTextAsync())
                     .Returns(Task.FromResult(resultPassword))
                     .Verifiable();

            var result = await subject.GetPfxPassword(url);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockBlock.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
            mockBlock.VerifyNoOtherCalls();
            Assert.AreEqual(resultPassword, result);
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

            var resultPem = "testpemfgfh87089dffd";
            var email = "test@test.test";
            var blobName = $"{optsValue.BlobPathPrefix}/pem/{email}";

            SetupStore(mockOpts, blobName, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);

            mockBlock.Setup(m => m.UploadTextAsync(It.Is<string>(s => s == resultPem)))
                     .Returns(Task.CompletedTask)
                     .Verifiable();

            await subject.StorePemKey(email, resultPem);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockBlock.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
            mockBlock.VerifyNoOtherCalls();
        }
    }
}
