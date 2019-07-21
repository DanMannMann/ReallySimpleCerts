using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ReallySimpleCerts.Core.Tests.Persistence.BlobStore
{
    [TestClass]
    public class BlobStorePersistenceAuthzTests : BlobStorePersistenceTestBase
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

            var token = "testtokenjkdfbnkjdfbd9";
            var blobName = $"{optsValue.BlobPathPrefix}/authz/{token}";
            var exists = false;
            SetupGet(mockOpts, blobName, exists, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);

            var result = await subject.GetAuthz(token);

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

            var token = "testtokenjkdfbnkjdfbd9";
            var blobName = $"{optsValue.BlobPathPrefix}/authz/{token}";
            var exists = true;
            SetupGet(mockOpts, blobName, exists, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);
            mockBlock.Setup(m => m.DownloadTextAsync())
                     .Returns(Task.FromResult(JsonConvert.SerializeObject((authz: "testauthz", location: new Uri("https://test.test/test")))))
                     .Verifiable();

            var result = await subject.GetAuthz(token);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockBlock.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
            mockBlock.VerifyNoOtherCalls();
            Assert.AreEqual("https://test.test/test", result.location.ToString());
            Assert.AreEqual("testauthz", result.authz);
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

            var token = "testtokenjkdfbnkjdfbd9";
            var value = (authz: "testauthz", location: new Uri("https://test.test/test"));
            var blobName = $"{optsValue.BlobPathPrefix}/authz/{token}";
            SetupStore(mockOpts, blobName, out var mockBlock, out var subject, out var mockContainer, out var mockContainerFactory);

            mockBlock.Setup(m => m.UploadTextAsync(It.Is<string>(s => s == JsonConvert.SerializeObject(value))))
                     .Returns(Task.CompletedTask)
                     .Verifiable();

            await subject.StoreAuthz(token, value);

            mockContainer.Verify();
            mockContainerFactory.Verify();
            mockBlock.Verify();
            mockContainer.VerifyNoOtherCalls();
            mockContainerFactory.VerifyNoOtherCalls();
            mockBlock.VerifyNoOtherCalls();
        }
    }
}
