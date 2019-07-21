using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using System;
using System.Collections.Generic;
using Microsoft.Rest.Azure;

namespace ReallySimpleCerts.Core.Tests.Persistence.KeyVault
{
    [TestClass]
    public class KeyVaultPersistencePfxTests : KeyVaultPersistenceTestBase
    {
        [TestMethod]
        public async Task GetReturnsDefaultIfBlobDoesNotExist()
        {
            var mockOpts = new Mock<IOptions<KeyVaultPersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new KeyVaultPersistenceOptions
                    {
                        KeyVaultRootUrl = "https://test.test:443"
                    });

            var mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value)
                    .Returns(new ReallySimpleCertOptions
                    {

                    });

            var subject = Setup(mockOpts, mockCertOpts, out var mockClient, out var mockClientFactory, out var mockLogger);

            var url = "testsub.testdomain.com";
            mockClient.Setup(m => m.GetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"), 
                                                                   It.Is<string>(s => s == "pfx-testsubtestdomaincom"), 
                                                                   It.IsAny<string>(), 
                                                                   It.IsAny<Dictionary<string,List<string>>>(), 
                                                                   It.IsAny<CancellationToken>()))
                      .Callback(() => throw new KeyVaultErrorException() { Response = new Microsoft.Rest.HttpResponseMessageWrapper(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound), null) })
                      .Returns(Task.FromResult((AzureOperationResponse<SecretBundle>)null))
                      .Verifiable();

            var result = await subject.GetPfx(url);

            mockClientFactory.Verify();
            mockClient.Verify();
            mockClientFactory.VerifyNoOtherCalls();
            mockClient.VerifyNoOtherCalls();
            Assert.IsTrue(result == default);
        }

        [TestMethod]
        public async Task GetReturnsCorrectValue()
        {
            var mockOpts = new Mock<IOptions<KeyVaultPersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new KeyVaultPersistenceOptions
                    {
                        KeyVaultRootUrl = "https://test.test:443"
                    });

            var mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value)
                    .Returns(new ReallySimpleCertOptions
                    {

                    });

            var subject = Setup(mockOpts, mockCertOpts, out var mockClient, out var mockClientFactory, out var mockLogger);

            var url = "testsub.testdomain.com";
            var resultArray = new byte[] { 3, 4, 5, 6, 7 };
            var resultb64 = Convert.ToBase64String(resultArray);
            mockClient.Setup(m => m.GetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"),
                                                                   It.Is<string>(s => s == "pfx-testsubtestdomaincom"),
                                                                   It.IsAny<string>(),
                                                                   It.IsAny<Dictionary<string, List<string>>>(),
                                                                   It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(new AzureOperationResponse<SecretBundle>() { Body = new SecretBundle(resultb64) }))
                      .Verifiable();

            var result = await subject.GetPfx(url);

            mockClientFactory.Verify();
            mockClient.Verify();
            mockClientFactory.VerifyNoOtherCalls();
            mockClient.VerifyNoOtherCalls();
            Assert.IsTrue(result.Length == 5);
        }

        [TestMethod]
        public async Task StorePersistsCorrectValue()
        {
            var mockOpts = new Mock<IOptions<KeyVaultPersistenceOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(new KeyVaultPersistenceOptions
                    {
                        KeyVaultRootUrl = "https://test.test:443"
                    });

            var mockCertOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockCertOpts.Setup(m => m.Value)
                    .Returns(new ReallySimpleCertOptions
                    {

                    });

            var subject = Setup(mockOpts, mockCertOpts, out var mockClient, out var mockClientFactory, out var mockLogger);

            var url = "testsub.testdomain.com";
            var resultArray = new byte[] { 3, 4, 5, 6, 7 };
            var resultb64 = Convert.ToBase64String(resultArray);
            mockClient.Setup(m => m.SetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"), 
                                                   It.Is<string>(s => s == "pfx-testsubtestdomaincom"), 
                                                   It.Is<string>(s => s == resultb64), 
                                                   It.IsAny<IDictionary<string,string>>(), 
                                                   It.IsAny<string>(), 
                                                   It.IsAny<SecretAttributes>(),
                                                   It.IsAny<Dictionary<string, List<string>>>(),
                                                   It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(new AzureOperationResponse<SecretBundle>() { Body = new SecretBundle(resultb64) }))
                      .Verifiable();

            await subject.StorePfx(url, resultArray);

            mockClientFactory.Verify();
            mockClient.Verify();
            mockClientFactory.VerifyNoOtherCalls();
            mockClient.VerifyNoOtherCalls();
        }
    }
}
