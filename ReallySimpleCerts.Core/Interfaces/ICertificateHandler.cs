using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public interface ICertificateHandler
    {
        Task NewCertificateCreated(X509Certificate2 cert, byte[] pfx, string pfxpwd);
        Task CertificateRestored(X509Certificate2 cert, byte[] pfx, string pfxpwd);
    }
}
