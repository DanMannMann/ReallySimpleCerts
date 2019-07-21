using Certes;
using Certes.Acme;
using System;

namespace ReallySimpleCerts.Core
{
    public class ReallySimpleCertOptions
    {
        public Uri IssuerRootUri { get; set; } = WellKnownServers.LetsEncryptV2;
        public string Email { get; set; }
        public CsrInfo CertificateInfo { get; set; }
        public bool LetsEncryptTermsOfServiceAgreed { get; set; }
        public TimeSpan RefreshCertEarly { get; set; } = TimeSpan.FromDays(14);
        public TimeSpan CheckDelay { get; set; } = TimeSpan.FromHours(6);
        public ServicePrincipalCredentials DefaultAzureServicePrincipal { get; set; }
    }
}
