using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ReallySimpleCerts.Core.Tests.AzureRmThirdPartyDomainCertificateHandlerTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core.Tests.ReallySimpleCertProviderTests
{
    [TestClass]
    public class ReallySimpleCertProvider_CheckCertificate_Tests : ReallySimpleCertProviderTestBase
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
        public async Task OrdersCertWhenNoCertExists()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo()
            {
                CommonName = "testsub.testdomain.com"
            };

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var mockAcmeContextFactory, out var mockAcmeContext);
            var subject = new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       mockAcmeContextFactory.Object);

            mockAcmeContextFactory.Setup(m => m.GetContext(It.IsAny<Uri>(), It.IsAny<IKey>()))
                                  .Returns(mockAcmeContext.Object)
                                  .Verifiable();

            mockPersistence.Setup(m => m.GetPfx(It.IsAny<string>()))
                           .Returns(Task.FromResult((byte[])null));

            SetupForNewOrderWithNewAccount(optsValue, mockAcmeContext, out var mockChallenge, out var mockOrder);

            await subject.ConfigureCertificate();

            VerifyForNewAccount(mockAcmeContextFactory, mockAcmeContext, mockChallenge, mockOrder);
        }

        [TestMethod]
        public async Task OrdersCertWhenCertExpired()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new CsrInfo()
            {
                CommonName = "testsub.testdomain.com"
            };
            var cert = Utilities.GenerateCertificate(optsValue.CertificateInfo.CommonName, true);
            var pfx = cert.Export(X509ContentType.Pfx, "test");

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var mockAcmeContextFactory, out var mockAcmeContext);
            var subject = new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       mockAcmeContextFactory.Object);

            mockAcmeContextFactory.Setup(m => m.GetContext(It.IsAny<Uri>(), It.IsAny<IKey>()))
                                  .Returns(mockAcmeContext.Object)
                                  .Verifiable();

            mockPersistence.Setup(m => m.GetPfx(It.IsAny<string>()))
                           .Returns(Task.FromResult(pfx));

            mockPersistence.Setup(m => m.GetPfxPassword(It.IsAny<string>()))
                           .Returns(Task.FromResult("test"));

            SetupForNewOrderWithNewAccount(optsValue, mockAcmeContext, out var mockChallenge, out var mockOrder);

            await subject.ConfigureCertificate();

            VerifyForNewAccount(mockAcmeContextFactory, mockAcmeContext, mockChallenge, mockOrder);
        }

        [TestMethod]
        public async Task DoesNotOrderCertWhenCertValid()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new CsrInfo()
            {
                CommonName = "testsub.testdomain.com"
            };
            var cert = Utilities.GenerateCertificate(optsValue.CertificateInfo.CommonName);
            var pfx = cert.Export(X509ContentType.Pfx, "test");

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var mockAcmeContextFactory, out var mockAcmeContext);
            var subject = new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       mockAcmeContextFactory.Object);

            mockAcmeContextFactory.Setup(m => m.GetContext(It.IsAny<Uri>(), It.IsAny<IKey>()))
                                  .Returns(mockAcmeContext.Object)
                                  .Verifiable();

            mockPersistence.Setup(m => m.GetPfx(It.IsAny<string>()))
                           .Returns(Task.FromResult(pfx))
                           .Verifiable();

            mockPersistence.Setup(m => m.GetPfxPassword(It.IsAny<string>()))
                           .Returns(Task.FromResult("test"))
                           .Verifiable();

            mockAcmeContext.Setup(m => m.AccountKey.ToPem())
                           .Returns("testpem")
                           .Verifiable();

            mockAcmeContext.Setup(m => m.NewAccount(It.IsAny<IList<string>>(), It.IsAny<bool>()))
                           .Returns(Task.FromResult((IAccountContext)null))
                           .Verifiable();

            await subject.ConfigureCertificate();

            mockAcmeContext.Verify();
            mockAcmeContext.VerifyNoOtherCalls();
            mockAcmeContextFactory.Verify();
            mockAcmeContextFactory.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task UsesExistingAccountIfPemFound()
        {
            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new CsrInfo()
            {
                CommonName = "testsub.testdomain.com"
            };
            var cert = Utilities.GenerateCertificate(optsValue.CertificateInfo.CommonName);
            var pfx = cert.Export(X509ContentType.Pfx, "test");

            GetMocks(optsValue, out var mockOpts, out var mockLogger, out var mockPersistence, out var mockCertHandlers, out var mockNameHandlers, out var mockAcmeContextFactory, out var mockAcmeContext);
            var subject = new ReallySimpleCertProvider(mockOpts.Object,
                                                       mockLogger.Object,
                                                       mockPersistence.Object,
                                                       mockCertHandlers.Select(x => x.Object),
                                                       mockNameHandlers.Select(x => x.Object),
                                                       mockAcmeContextFactory.Object);

            mockAcmeContextFactory.Setup(m => m.GetContext(It.IsAny<Uri>(), It.Is<IKey>(s => s != default)))
                                  .Returns(mockAcmeContext.Object)
                                  .Verifiable();

            mockPersistence.Setup(m => m.GetPfx(It.IsAny<string>()))
                           .Returns(Task.FromResult(pfx))
                           .Verifiable();

            mockPersistence.Setup(m => m.GetPfxPassword(It.IsAny<string>()))
                           .Returns(Task.FromResult("test"))
                           .Verifiable();

            mockPersistence.Setup(m => m.GetPemKey(It.IsAny<string>()))
                           .Returns(Task.FromResult(@"-----BEGIN EC PRIVATE KEY-----
MDECAQEEIFkja2a8nlTn+B6EsP0TSyiOMNohdtk1B/rQsslvAEsKoAoGCCqGSM49
AwEH
-----END EC PRIVATE KEY-----
"))
                           .Verifiable();

            await subject.ConfigureCertificate();

            mockAcmeContext.Verify();
            mockAcmeContext.VerifyNoOtherCalls();
            mockAcmeContextFactory.Verify();
            mockAcmeContextFactory.VerifyNoOtherCalls();
        }

        private static void VerifyForNewAccount(Mock<IAcmeContextFactory> mockAcmeContextFactory, Mock<IAcmeContext> mockAcmeContext, Mock<IChallengeContext> mockChallenge, Mock<IOrderContext> mockOrder)
        {
            mockChallenge.Verify();
            mockChallenge.VerifyNoOtherCalls();
            mockOrder.Verify();
            mockOrder.VerifyNoOtherCalls();
            mockAcmeContext.Verify();
            mockAcmeContext.VerifyNoOtherCalls();
            mockAcmeContextFactory.Verify();
            mockAcmeContextFactory.VerifyNoOtherCalls();
        }

        private static void SetupForNewOrderWithNewAccount(ReallySimpleCertOptions optsValue, Mock<IAcmeContext> mockAcmeContext, out Mock<IChallengeContext> mockChallenge, out Mock<IOrderContext> mockOrder)
        {
            var mockAuth = new Mock<IAuthorizationContext>();
            IEnumerable<IAuthorizationContext> mockAuths = new List<IAuthorizationContext>
            {
                mockAuth.Object
            };

            mockChallenge = new Mock<IChallengeContext>();
            IEnumerable<IChallengeContext> mockChallenges = new List<IChallengeContext>
            {
                mockChallenge.Object
            };
            mockChallenge.Setup(m => m.Type)
                         .Returns(ChallengeTypes.Http01)
                         .Verifiable();
            mockChallenge.Setup(m => m.Token)
                         .Returns("testtoken")
                         .Verifiable();
            mockChallenge.Setup(m => m.KeyAuthz)
                         .Returns("testkeyauthz")
                         .Verifiable();
            mockChallenge.Setup(m => m.Location)
                         .Returns(new Uri("http://test.test"))
                         .Verifiable();
            mockChallenge.Setup(m => m.Validate())
                         .Returns(Task.FromResult<Challenge>(null))
                         .Verifiable();

            mockOrder = new Mock<IOrderContext>();
            mockOrder.Setup(m => m.Authorizations())
                     .Returns(Task.FromResult(mockAuths))
                     .Verifiable();

            mockAuth.Setup(m => m.Challenges())
                    .Returns(Task.FromResult(mockChallenges))
                    .Verifiable();

            mockAcmeContext.Setup(m => m.NewOrder(It.Is<IList<string>>(s => s.Any(s0 => s0 == optsValue.CertificateInfo.CommonName)), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                           .Returns(Task.FromResult(mockOrder.Object))
                           .Verifiable();

            mockAcmeContext.Setup(m => m.AccountKey.ToPem())
                           .Returns("testpem")
                           .Verifiable();

            mockAcmeContext.Setup(m => m.NewAccount(It.IsAny<IList<string>>(), It.IsAny<bool>()))
                           .Returns(Task.FromResult((IAccountContext)null))
                                  .Verifiable();
        }
        // 2. Calls name handlers appropriately
        // 3. Calls cert handlers appropriately
    }
}
