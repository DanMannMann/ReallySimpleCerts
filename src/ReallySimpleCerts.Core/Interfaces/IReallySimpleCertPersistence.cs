using System;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public interface IReallySimpleCertPersistence
    {
        Task StorePemKey(string email, string pemKey);
        Task<string> GetPemKey(string email);

        Task StoreAuthz(string token, (string authz, Uri location) value);
        Task<(string authz, Uri location)> GetAuthz(string token);

        Task StorePfx(string nakedUrl, byte[] pfx);
        Task<byte[]> GetPfx(string nakedUrl);

        Task StorePfxPassword(string nakedUrl, string password);
        Task<string> GetPfxPassword(string nakedUrl);
    } 
}
