using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core.Tests.AzureRmThirdPartyDomainCertificateHandlerTests
{
    public static class Utilities
    {
        public static X509Certificate2 GenerateCertificate(string certName, bool outOfDate = false)
        {
            var keypairgen = new RsaKeyPairGenerator();
            var random = new SecureRandom(new CryptoApiRandomGenerator());
            keypairgen.Init(new KeyGenerationParameters(random, 1024));

            var keypair = keypairgen.GenerateKeyPair();

            var gen = new X509V3CertificateGenerator();

            var CN = new X509Name("CN=" + certName);
            var SN = BigInteger.ProbablePrime(120, new Random());

            gen.SetSerialNumber(SN);
            gen.SetSubjectDN(CN);
            gen.SetIssuerDN(CN);
            gen.SetNotAfter(outOfDate ? DateTime.Now.Subtract(TimeSpan.FromDays(2)) : DateTime.MaxValue);
            gen.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)));
            gen.SetPublicKey(keypair.Public);

            var newCert = gen.Generate(new  Asn1SignatureFactory("MD5WithRSA", keypair.Private, random));

            return new X509Certificate2(DotNetUtilities.ToX509Certificate(newCert));
        }
    }

    [TestClass]
    public sealed class AzureRmThirdPartyDomainCertificateHandler_EnsureHostNameBinding_Tests : AzureRmThirdPartyDomainCertificateHandlerTestBase
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

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await subject.EnsureHostNameBinding());
        }

        [TestMethod]
        public async Task HostnameBindingIsCreatedWhenNoBindingsExist()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";

            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.ResourceGroupName = "testresourcegroup";
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
            WebAppHasHostNames(mockWebApp);

            mockWebApp.Setup(m => m.Update()
                                   .DefineHostnameBinding()
                                   .WithThirdPartyDomain(It.Is<string>(s => s == "testdomain.com"))
                                   .WithSubDomain(It.Is<string>(s => s == "testsub"))
                                   .WithDnsRecordType(It.Is<CustomHostNameDnsRecordType>(s => s == CustomHostNameDnsRecordType.A))
                                   .Attach()
                                   .ApplyAsync(It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                      .Returns(Task.FromResult(mockWebApp.Object))
                      .Verifiable();

            var hostnameBindingMock = new Mock<IHostNameBinding>();
            hostnameBindingMock.Setup(m => m.HostNameType)
                               .Returns(HostNameType.Verified)
                               .Verifiable();

            mockWebApp.Setup(m => m.GetHostNameBindingsAsync(It.IsAny<CancellationToken>()))
                      .Returns(
                        Task.FromResult<IReadOnlyDictionary<string, IHostNameBinding>>(
                            new ReadOnlyDictionary<string, IHostNameBinding>(new Dictionary<string, IHostNameBinding> { { "testsub.testdomain.com", hostnameBindingMock.Object } })))
                      .Verifiable();

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await subject.EnsureHostNameBinding();

            mockWebApp.Verify();
            mockWebApp.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task HostnameBindingIsCreatedWhenNoMatchingBindingsExist()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";

            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.ResourceGroupName = "testresourcegroup";
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
            WebAppHasHostNames(mockWebApp, "other.host.com");

            mockWebApp.Setup(m => m.Update()
                                   .DefineHostnameBinding()
                                   .WithThirdPartyDomain(It.Is<string>(s => s == "testdomain.com"))
                                   .WithSubDomain(It.Is<string>(s => s == "testsub"))
                                   .WithDnsRecordType(It.Is<CustomHostNameDnsRecordType>(s => s == CustomHostNameDnsRecordType.A))
                                   .Attach()
                                   .ApplyAsync(It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                      .Returns(Task.FromResult(mockWebApp.Object))
                      .Verifiable();

            var hostnameBindingMock = new Mock<IHostNameBinding>();
            hostnameBindingMock.Setup(m => m.HostNameType)
                               .Returns(HostNameType.Verified)
                               .Verifiable();

            mockWebApp.Setup(m => m.GetHostNameBindingsAsync(It.IsAny<CancellationToken>()))
                      .Returns(
                        Task.FromResult<IReadOnlyDictionary<string, IHostNameBinding>>(
                            new ReadOnlyDictionary<string, IHostNameBinding>(new Dictionary<string, IHostNameBinding> { { "testsub.testdomain.com", hostnameBindingMock.Object } })))
                      .Verifiable();

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await subject.EnsureHostNameBinding();

            mockWebApp.Verify();
            mockWebApp.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task HostnameBindingIsNotCreatedWhenBindingAlreadyExists()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";
            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.ResourceGroupName = "testresourcegroup";
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

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await subject.EnsureHostNameBinding();

            mockWebApp.Verify();
            mockWebApp.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task HostnameCheckIsNotCaseSensitive()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            certOptsValue.CertificateInfo.CommonName = "testsub.testdomain.com";
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.DefaultAzureServicePrincipal.TenantId = "testtenant";
            certOptsValue.DefaultAzureServicePrincipal.ClientId = "testclient";
            var optsValue = new AzureRmOptions();
            optsValue.WebAppName = "testwebapp";
            optsValue.ResourceGroupName = "testresourcegroup";
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
            WebAppHasHostNames(mockWebApp, "tEsTsub.TEsTDOmAiN.coM");

            var subject = new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object);
            await subject.EnsureHostNameBinding();

            mockWebApp.Verify();
            mockWebApp.VerifyNoOtherCalls();
        }
    }
}