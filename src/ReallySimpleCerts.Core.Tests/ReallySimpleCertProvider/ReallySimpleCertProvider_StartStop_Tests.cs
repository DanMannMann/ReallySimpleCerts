using Certes;
using Certes.Acme;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core.Tests.ReallySimpleCertProviderTests
{
    [TestClass]
    public class ReallySimpleCertProvider_StartStop_Tests : ReallySimpleCertProviderTestBase
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
        public async Task ChecksCertOnStart()
        {

            var optsValue = new ReallySimpleCertOptions()
            {
                LetsEncryptTermsOfServiceAgreed = true
            };
            optsValue.DefaultAzureServicePrincipal = new ServicePrincipalCredentials();
            optsValue.CertificateInfo = new Certes.CsrInfo();

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

            mockAcmeContext.Setup(m => m.AccountKey.ToPem())
                           .Returns("testpem")
                                  .Verifiable();

            mockAcmeContext.Setup(m => m.NewAccount(It.IsAny<IList<string>>(), It.IsAny<bool>()))
                           .Returns(Task.FromResult((IAccountContext)null))
                                  .Verifiable();

            var src = new CancellationTokenSource();
            try
            {
                await subject.StartAsync(src.Token);
                Assert.Fail("Expected ArgumentException to be thrown");
            }
            catch (ArgumentException ex) when (ex.Message == "Array may not be empty or null.\r\nParameter name: rawData")
            {
                // This exc is throw by the X509Certificate2 ctr.
                // The subject is trying to construct a cert from the mock pfx, so we know that it checks the cert on start.
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            await subject.StopAsync(src.Token);

            mockAcmeContext.Verify();
            mockAcmeContext.VerifyNoOtherCalls();
            mockAcmeContextFactory.Verify();
            mockAcmeContextFactory.VerifyNoOtherCalls();
        }
    }
}
