using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;

namespace ReallySimpleCerts.Core.Tests.ReallySimpleCertProviderTests
{

    [TestClass]
    public class ReallySimpleCertProvider_Constructor_Tests : ReallySimpleCertProviderTestBase
    {
        [TestInitialize]
        public void Prep()
        {

        }

        [TestCleanup]
        public void TearDown()
        {
            var prop = typeof(ReallySimpleCertProvider).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            prop.SetValue(null, null);
        }

        [TestMethod]
        public void CanConstruct()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.IsTrue(new ReallySimpleCertProvider(mockOpts.Object, 
                                                       mockLogger.Object, 
                                                       mockPersistence.Object, 
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object) is ReallySimpleCertProvider);
        }

        [TestMethod]
        public void SetsInstanceValueToSelf()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            var subject = new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object);

            Assert.IsTrue(subject is ReallySimpleCertProvider);
            Assert.AreSame(ReallySimpleCertProvider.Instance, subject);
        }

        [TestMethod]
        public void ThrowsOnNonSingletonConstruction()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.IsTrue(new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object) is ReallySimpleCertProvider);

            Assert.ThrowsException<InvalidOperationException>(() =>
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenTermsOfServiceNotAccepted()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.ThrowsException<InvalidOperationException>(() => 
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenOptionsNull()
        {
            ReallySimpleCertOptions optsValue = null;

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.ThrowsException<ArgumentNullException>(() =>
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                          new ReallySimpleCertProvider(null,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenLoggerNull()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.ThrowsException<ArgumentNullException>(() =>
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       null,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenPersistenceNull()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.ThrowsException<ArgumentNullException>(() =>
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       null,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenCertHandlersNull()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.ThrowsException<ArgumentNullException>(() =>
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       null,
                                                       mockNameHandlers.Select(x => x.Object),
                                                       acmeContextFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenNameHandlersNull()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.ThrowsException<ArgumentNullException>(() =>
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       null,
                                                       acmeContextFactory.Object));
        }

        [TestMethod]
        public void ThrowsWhenContextFactoryNull()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var acmeContextFactory, out var mockAcmeContext);
            Assert.ThrowsException<ArgumentNullException>(() =>
                          new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       null));
        }
    }
}
