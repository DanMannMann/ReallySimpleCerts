using Certes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;

namespace ReallySimpleCerts.Core.Tests.ReallySimpleCertProviderTests
{
    public abstract class ReallySimpleCertProviderTestBase
    {
        protected void GetMocks(ReallySimpleCertOptions optsValue,
                                out Mock<IOptions<ReallySimpleCertOptions>> mockOpts,
                                out Mock<ILogger<ReallySimpleCertProvider>> mockLogger,
                                out Mock<IReallySimpleCertPersistence> mockPersistence,
                                out IEnumerable<Mock<ICertificateHandler>> mockCertHandlers,
                                out IEnumerable<Mock<IHostNameHandler>> mockNameHandlers,
                                out Mock<IAcmeContextFactory> mockAcmeContextFactory,
                                out Mock<IAcmeContext> mockAcmeContext)
        {
            mockOpts = new Mock<IOptions<ReallySimpleCertOptions>>();
            mockOpts.Setup(m => m.Value)
                    .Returns(optsValue);

            mockLogger = new Mock<ILogger<ReallySimpleCertProvider>>();

            mockPersistence = new Mock<IReallySimpleCertPersistence>();

            mockCertHandlers = new List<Mock<ICertificateHandler>>();

            mockNameHandlers = new List<Mock<IHostNameHandler>>();

            mockAcmeContext = new Mock<IAcmeContext>();

            mockAcmeContextFactory = new Mock<IAcmeContextFactory>();
        }
    }
}
