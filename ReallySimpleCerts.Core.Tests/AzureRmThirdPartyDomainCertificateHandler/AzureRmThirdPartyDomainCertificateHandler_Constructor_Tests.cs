using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ReallySimpleCerts.Core.Tests.AzureRmThirdPartyDomainCertificateHandlerTests
{
    [TestClass]
    public sealed class AzureRmThirdPartyDomainCertificateHandler_Constructor_Tests : AzureRmThirdPartyDomainCertificateHandlerTestBase
    {
        [TestInitialize]
        public void Prep()
        {

        }

        [TestMethod]
        public void CanConstruct()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            var optsValue = new AzureRmOptions();

            GetMocks(certOptsValue, optsValue, out var mockLogger, out var mockOpts, out var mockCertOpts, out var mockWebApp, out var mocks, "test", "test", "test");
            Assert.IsTrue(new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object) is AzureRmThirdPartyDomainCertificateHandler);
        }

        [TestMethod]
        public void ThrowsWhenServicePrincipalCredentialsIsNull()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.DefaultAzureServicePrincipal = null;
            var optsValue = new AzureRmOptions();
            optsValue.ServicePrincipalCredentials = null;

            GetMocks(certOptsValue, optsValue, out var mockLogger, out var mockOpts, out var mockCertOpts, out var mockWebApp, out var mocks, "test", "test", "test");
            Assert.ThrowsException<ArgumentNullException>(() => new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenCertificateInfoIsNull()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.CertificateInfo = null;
            var optsValue = new AzureRmOptions();

            GetMocks(certOptsValue, optsValue, out var mockLogger, out var mockOpts, out var mockCertOpts, out var mockWebApp, out var mocks, "test", "test", "test");
            Assert.ThrowsException<ArgumentNullException>(() => new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenOptsIsNull()
        {
            var certOptsValue = new ReallySimpleCertOptions();
            certOptsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            certOptsValue.CertificateInfo = new Certes.CsrInfo();
            AzureRmOptions optsValue = null;

            GetMocks(certOptsValue, optsValue, out var mockLogger, out var mockOpts, out var mockCertOpts, out var mockWebApp, out var mocks, "test", "test", "test");
            Assert.ThrowsException<ArgumentNullException>(() => new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenCertOptsIsNull()
        {
            ReallySimpleCertOptions certOptsValue = null;
            AzureRmOptions optsValue = new AzureRmOptions();

            GetMocks(certOptsValue, optsValue, out var mockLogger, out var mockOpts, out var mockCertOpts, out var mockWebApp, out var mocks, "test", "test", "test");
            Assert.ThrowsException<ArgumentNullException>(() => new AzureRmThirdPartyDomainCertificateHandler(mockOpts.Object, mockCertOpts.Object, mockLogger.Object, mocks.MockAzureFactory.Object));
        }
    }
}
