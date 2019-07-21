using Certes;
using Certes.Acme;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.Azure.Authentication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{

    public class ReallySimpleCertProvider : IHostedService
    {
        private readonly ReallySimpleCertOptions options;
        private readonly ILogger<ReallySimpleCertProvider> logger;
        private readonly IReallySimpleCertPersistence persistence;
        private readonly IEnumerable<ICertificateHandler> certHandlers;
        private readonly IEnumerable<IHostNameHandler> hostnameHandlers;
        private readonly IAcmeContextFactory acmeContextFactory;
        private bool stopped;
        private IOrderContext order;
        private Dictionary<string,bool> handledTokens = new Dictionary<string, bool>();

        public ReallySimpleCertProvider(IOptions<ReallySimpleCertOptions> options, 
                                        ILogger<ReallySimpleCertProvider> logger, 
                                        IReallySimpleCertPersistence persistence, 
                                        IEnumerable<ICertificateHandler> certHandlers, 
                                        IEnumerable<IHostNameHandler> hostnameHandlers,
                                        IAcmeContextFactory acmeContextFactory)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            this.certHandlers = certHandlers ?? throw new ArgumentNullException(nameof(certHandlers));
            this.hostnameHandlers = hostnameHandlers ?? throw new ArgumentNullException(nameof(hostnameHandlers));
            this.acmeContextFactory = acmeContextFactory ?? throw new ArgumentNullException(nameof(acmeContextFactory));
            if (Instance != null)
                throw new InvalidOperationException($"{nameof(ReallySimpleCertProvider)} should be registered as a singleton");
            if (!options.Value.LetsEncryptTermsOfServiceAgreed)
                throw new InvalidOperationException("You must agree to the terms of service by setting LetsEncryptTermsOfServiceAgreed to true");

            Instance = this;
        }

        public static ReallySimpleCertProvider Instance { get; private set; }

        public X509Certificate2 Certificate { get; private set; }

        public async Task Continue(string token)
        {
            lock (token)
            {
                if (handledTokens.ContainsKey(token))
                {
                    return;
                }
                handledTokens.Add(token, false);
            }
            try
            {
                var orderResource = (await order.Resource());
                while (orderResource.Status != Certes.Acme.Resource.OrderStatus.Ready)
                {
                    await Task.Delay(500);
                    orderResource = (await order.Resource());
                }

                var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);
                var cert = await order.Generate(options.CertificateInfo, privateKey);
                var certPfx = cert.ToPfx(privateKey);
                var pwd = Guid.NewGuid().ToString();
                var pfx = certPfx.Build(options.CertificateInfo.CommonName, pwd);
                await persistence.StorePfx(options.CertificateInfo.CommonName, pfx);
                await persistence.StorePfxPassword(options.CertificateInfo.CommonName, pwd);
                Certificate = new X509Certificate2(pfx, pwd);
                handledTokens[token] = true;
                await RunCertificateHandlers(pwd, pfx, x => x.NewCertificateCreated);
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception in ConfigureCertificate: {ex}");
                handledTokens[token] = false;
            }
        }

        public async Task ConfigureCertificate()
        {
            IAcmeContext acmeContext;
            var pem = await persistence.GetPemKey(options.Email);

            if (string.IsNullOrWhiteSpace(pem))
            {
                acmeContext = acmeContextFactory.GetContext(options.IssuerRootUri);
                await acmeContext.NewAccount(options.Email, options.LetsEncryptTermsOfServiceAgreed);
                await persistence.StorePemKey(options.Email, acmeContext.AccountKey.ToPem());
            }
            else
            {
                acmeContext = acmeContextFactory.GetContext(options.IssuerRootUri, KeyFactory.FromPem(pem));
            }

            var pfx = await persistence.GetPfx(options.CertificateInfo.CommonName);
            if (pfx == null)
            {
                await OrderNewCert(acmeContext);
            }
            else
            {
                var pwd = await persistence.GetPfxPassword(options.CertificateInfo.CommonName);
                var certificate = new X509Certificate2(pfx, pwd);

                if (certificate.NotBefore > DateTime.Now || DateTime.Now > certificate.NotAfter - options.RefreshCertEarly)
                {
                    await OrderNewCert(acmeContext);
                }
                else
                {
                    Certificate = certificate;
                    await RunHostnameHandlers();
                    await RunCertificateHandlers(pwd, pfx, x => x.CertificateRestored);
                }
            }
        }

        private async Task RunHostnameHandlers()
        {
            foreach (var handler in hostnameHandlers)
            {
                try
                {
                    await handler.EnsureHostNameBinding();
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception in a certificate handler: {ex}");
                }
            }
        }

        private async Task RunCertificateHandlers(string pwd, byte[] pfx, Func<ICertificateHandler, Func<X509Certificate2, byte[], string, Task>> method)
        {
            foreach (var handler in certHandlers)
            {
                try
                {
                    await method(handler)(Certificate, pfx, pwd);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception in a certificate handler: {ex}");
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            stopped = false;
            await ConfigureCertificate();
            _ = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested && !stopped)
                {
                    try
                    {
                        await Task.Delay(options.CheckDelay, cancellationToken);

                        if (!cancellationToken.IsCancellationRequested && !stopped)
                            await ConfigureCertificate();
                    }
                    catch (TaskCanceledException ex)
                    {
                        logger.LogInformation("Provider timer task cancelled. Ex: {ex}", ex);
                        stopped = true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Unexpected error in provider timer loop. Ex: {ex}", ex);
                    }
                }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            stopped = true;
            return Task.CompletedTask;
        }

        private async Task OrderNewCert(IAcmeContext acmeContext)
        {
            await RunHostnameHandlers();

            order = await acmeContext.NewOrder(new[] { options.CertificateInfo.CommonName });
            var auth = (await order.Authorizations())?.First();
            var httpChallenge = await auth.Http();

            await persistence.StoreAuthz(httpChallenge.Token, (httpChallenge.KeyAuthz, httpChallenge.Location));
            await httpChallenge.Validate();
        }
    }
}
