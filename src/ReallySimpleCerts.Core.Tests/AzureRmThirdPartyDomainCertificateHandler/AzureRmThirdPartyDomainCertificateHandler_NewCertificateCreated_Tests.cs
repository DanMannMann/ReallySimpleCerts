using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core.Tests.AzureRmThirdPartyDomainCertificateHandlerTests
{
    [TestClass]
    public sealed class AzureRmThirdPartyDomainCertificateHandler_NewCertificateCreated_Tests : AzureRmThirdPartyDomainCertificateHandlerTestBase
    {
        [TestInitialize]
        public void Prep()
        {

        }

        [TestMethod]
        public async Task ThrowsWhenWebAppNameNotMatched()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";

            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.DnsRecordType = CustomHostNameDnsRecordType.A;

            GetMocks(
               certOptsValue,
               optsValue,
               out var mockLogger,
               out var mockOpts,
               out var mockCertOpts,
               out var mockWebApp,
               out var mocks,
               "not-a-match",
               certOptsValue.DefaultAzureServicePrincipal.ClientId,
               certOptsValue.DefaultAzureServicePrincipal.TenantId);
            WebAppHasHostNames(mockWebApp);

            var cert = Utilities.GenerateCertificate("test");
            var pfx = cert.Export(X509ContentType.Pfx, "test");

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await subject.NewCertificateCreated(cert, pfx, "test"));
        }

        [TestMethod]
        public async Task CertNotCreatedWhenCertAlreadyExists()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";

            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.DnsRecordType = CustomHostNameDnsRecordType.A;

            GetMocks(
               certOptsValue,
               optsValue,
               out var mockLogger,
               out var mockOpts,
               out var mockCertOpts,
               out var mockWebApp,
               out var mocks,
               optsValue.WebAppName,
               certOptsValue.DefaultAzureServicePrincipal.ClientId,
               certOptsValue.DefaultAzureServicePrincipal.TenantId);

            mockWebApp.Setup(m => m.HostNameSslStates.ContainsKey(It.Is<string>(s => s == certOptsValue.CertificateInfo.CommonName)))
                      .Returns(true)
                      .Verifiable();

            var cert = Utilities.GenerateCertificate(certOptsValue.CertificateInfo.CommonName);
            var pfx = cert.Export(X509ContentType.Pfx, "test");

            var mockSllState = new HostNameSslState(thumbprint: cert.Thumbprint);

            mockWebApp.Setup(m => m.HostNameSslStates[It.Is<string>(s => s == certOptsValue.CertificateInfo.CommonName)])
                      .Returns(mockSllState)
                      .Verifiable();

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await subject.NewCertificateCreated(cert, pfx, "test");

            mockWebApp.Verify();
            mockWebApp.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task CertCreatedWhenDifferentCertExists()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";

            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.DnsRecordType = CustomHostNameDnsRecordType.A;

            GetMocks(
               certOptsValue,
               optsValue,
               out var mockLogger,
               out var mockOpts,
               out var mockCertOpts,
               out var mockWebApp,
               out var mocks,
               optsValue.WebAppName,
               certOptsValue.DefaultAzureServicePrincipal.ClientId,
               certOptsValue.DefaultAzureServicePrincipal.TenantId);
            WebAppHasHostNames(mockWebApp, certOptsValue.CertificateInfo.CommonName);

            mockWebApp.Setup(m => m.HostNameSslStates.ContainsKey(It.Is<string>(s => s == certOptsValue.CertificateInfo.CommonName)))
                      .Returns(true)
                      .Verifiable();

            var cert = Utilities.GenerateCertificate(certOptsValue.CertificateInfo.CommonName);
            var pfx = cert.Export(X509ContentType.Pfx, "test");

            var mockSllState = new HostNameSslState(thumbprint: "not-a-match");

            mockWebApp.Setup(m => m.HostNameSslStates[It.Is<string>(s => s == certOptsValue.CertificateInfo.CommonName)])
                      .Returns(mockSllState)
                      .Verifiable();

            mockWebApp.Setup(m => m.Update()
                                    .DefineSslBinding()
                                    .ForHostname(It.Is<string>(s => s == certOptsValue.CertificateInfo.CommonName))
                                    .WithPfxCertificateToUpload(It.IsAny<string>(), It.Is<string>(s => s == "test"))
                                    .WithSniBasedSsl()
                                    .Attach()
                                .ApplyAsync(It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                      .Returns(Task.FromResult(mockWebApp.Object))
                      .Verifiable();

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await subject.NewCertificateCreated(cert, pfx, "test");

            mockWebApp.Verify();
            mockWebApp.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task CertCreatedWhenNoCertExists()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";

            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.DnsRecordType = CustomHostNameDnsRecordType.A;

            GetMocks(
               certOptsValue,
               optsValue,
               out var mockLogger,
               out var mockOpts,
               out var mockCertOpts,
               out var mockWebApp,
               out var mocks,
               optsValue.WebAppName,
               certOptsValue.DefaultAzureServicePrincipal.ClientId,
               certOptsValue.DefaultAzureServicePrincipal.TenantId);
            WebAppHasHostNames(mockWebApp, certOptsValue.CertificateInfo.CommonName);

            mockWebApp.Setup(m => m.HostNameSslStates.ContainsKey(It.Is<string>(s => s == certOptsValue.CertificateInfo.CommonName)))
                      .Returns(false)
                      .Verifiable();

            mockWebApp.Setup(m => m.Update()
                                    .DefineSslBinding()
                                    .ForHostname(It.Is<string>(s => s == certOptsValue.CertificateInfo.CommonName))
                                    .WithPfxCertificateToUpload(It.IsAny<string>(), It.Is<string>(s => s == "test"))
                                    .WithSniBasedSsl()
                                    .Attach()
                                .ApplyAsync(It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                      .Returns(Task.FromResult(mockWebApp.Object))
                      .Verifiable();

            var cert = Utilities.GenerateCertificate(certOptsValue.CertificateInfo.CommonName);
            var pfx = cert.Export(X509ContentType.Pfx, "test");

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await subject.NewCertificateCreated(cert, pfx, "test");

            mockWebApp.Verify();
            mockWebApp.VerifyNoOtherCalls();
        }
    }
}