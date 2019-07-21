using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.Azure.KeyVault;
using System.Threading;
using Microsoft.Azure.KeyVault.Models;
using System.Collections.Generic;
using Microsoft.Rest.Azure;

namespace ReallySimpleCerts.Core.Tests.Persistence.KeyVault
{
    [TestClass]
    public class KeyVaultPersistencePemTests : KeyVaultPersistenceTestBase
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

            var email = "test@test.test";
            mockClient.Setup(m => m.GetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"),
                                                                   It.Is<string>(s => s == "pem-testtesttest"),
                                                                   It.IsAny<string>(),
                                                                   It.IsAny<Dictionary<string, List<string>>>(),
                                                                   It.IsAny<CancellationToken>()))
                      .Callback(() => throw new KeyVaultErrorException() { Response = new Microsoft.Rest.HttpResponseMessageWrapper(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound), null) })
                      .Returns(Task.FromResult((AzureOperationResponse<SecretBundle>)null))
                      .Verifiable();

            var result = await subject.GetPemKey(email);

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

            var email = "test@test.test";
            var resultPem = "pas5w0rD!!£";
            mockClient.Setup(m => m.GetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"),
                                                                   It.Is<string>(s => s == "pem-testtesttest"),
                                                                   It.IsAny<string>(),
                                                                   It.IsAny<Dictionary<string, List<string>>>(),
                                                                   It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(new AzureOperationResponse<SecretBundle>() { Body = new SecretBundle(resultPem) }))
                      .Verifiable();

            var result = await subject.GetPemKey(email);

            mockClientFactory.Verify();
            mockClient.Verify();
            mockClientFactory.VerifyNoOtherCalls();
            mockClient.VerifyNoOtherCalls();
            Assert.IsTrue(result == resultPem);
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

            var email = "test@test.test";
            var resultPem = "pas5w0rD!!£";
            mockClient.Setup(m => m.SetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"),
                                                   It.Is<string>(s => s == "pem-testtesttest"),
                                                   It.Is<string>(s => s == resultPem),
                                                   It.IsAny<IDictionary<string, string>>(),
                                                   It.IsAny<string>(),
                                                   It.IsAny<SecretAttributes>(),
                                                   It.IsAny<Dictionary<string, List<string>>>(),
                                                   It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(new AzureOperationResponse<SecretBundle>() { Body = new SecretBundle(resultPem) }))
                      .Verifiable();

            await subject.StorePemKey(email, resultPem);

            mockClientFactory.Verify();
            mockClient.Verify();
            mockClientFactory.VerifyNoOtherCalls();
            mockClient.VerifyNoOtherCalls();
        }
    }
}
