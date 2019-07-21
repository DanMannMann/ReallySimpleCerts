using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace ReallySimpleCerts.Core
{
    internal class KestrelOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        public void Configure(KestrelServerOptions options)
        {
            options.ConfigureHttpsDefaults(o =>
            {
                o.ServerCertificateSelector = (ctx, str) => ReallySimpleCertProvider.Instance.Certificate;
            });
        }
    }
}
