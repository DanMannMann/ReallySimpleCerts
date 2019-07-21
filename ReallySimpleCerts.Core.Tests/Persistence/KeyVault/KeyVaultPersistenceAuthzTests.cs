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
    public class KeyVaultPersistenceAuthzTests : KeyVaultPersistenceTestBase
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

            var token = "testtokenjkdfbnkjdfbd9";
            mockClient.Setup(m => m.GetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"),
                                                                   It.Is<string>(s => s == "authz-testtokenjkdfbnkjdfbd9"),
                                                                   It.IsAny<string>(),
                                                                   It.IsAny<Dictionary<string, List<string>>>(),
                                                                   It.IsAny<CancellationToken>()))
                      .Callback(() => throw new KeyVaultErrorException() { Response = new Microsoft.Rest.HttpResponseMessageWrapper(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound), null) })
                      .Returns(Task.FromResult((AzureOperationResponse<SecretBundle>)null))
                      .Verifiable();

            var result = await subject.GetAuthz(token);

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

            var value = (authz: "testauthz", location: new Uri("https://test.test:443"));
            var token = "testtokenjkdfbnkjdfbd9";
            mockClient.Setup(m => m.GetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"),
                                                                   It.Is<string>(s => s == "authz-testtokenjkdfbnkjdfbd9"),
                                                                   It.IsAny<string>(),
                                                                   It.IsAny<Dictionary<string, List<string>>>(),
                                                                   It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(new AzureOperationResponse<SecretBundle>() { Body = new SecretBundle(JsonConvert.SerializeObject(value)) }))
                      .Verifiable();

            var result = await subject.GetAuthz(token);

            mockClientFactory.Verify();
            mockClient.Verify();
            mockClientFactory.VerifyNoOtherCalls();
            mockClient.VerifyNoOtherCalls();
            Assert.IsTrue(result.authz == value.authz);
            Assert.IsTrue(result.location == value.location);
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

            var value = (authz: "testauthz", location: new Uri("https://test.test:443"));
            var token = "testtokenjkdfbnkjdfbd9";
            mockClient.Setup(m => m.SetSecretWithHttpMessagesAsync(It.Is<string>(s => s == "https://test.test:443"),
                                                   It.Is<string>(s => s == "authz-testtokenjkdfbnkjdfbd9"),
                                                   It.Is<string>(s => s == JsonConvert.SerializeObject(value)),
                                                   It.IsAny<IDictionary<string, string>>(),
                                                   It.IsAny<string>(),
                                                   It.IsAny<SecretAttributes>(),
                                                   It.IsAny<Dictionary<string,List<string>>>(),
                                                   It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(new AzureOperationResponse<SecretBundle>() { Body = new SecretBundle(JsonConvert.SerializeObject(value)) }))
                      .Verifiable();

            await subject.StoreAuthz(token, value);

            mockClientFactory.Verify();
            mockClient.Verify();
            mockClientFactory.VerifyNoOtherCalls();
            mockClient.VerifyNoOtherCalls();
        }
    }
}
