using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public class InMemoryPersistence : IReallySimpleCertPersistence
    {
        private readonly IDictionary<string, (string authz, Uri location)> authzDict = new Dictionary<string, (string authz, Uri location)>();
        private readonly IDictionary<string, string> pemKeysDict = new Dictionary<string, string>();
        private readonly IDictionary<string, byte[]> pfxDict = new Dictionary<string, byte[]>();
        private readonly IDictionary<string, string> pfxPwdDict = new Dictionary<string, string>();

        public Task<(string authz, Uri location)> GetAuthz(string token)
        {
            return Task.FromResult(authzDict.ContainsKey(token) ? authzDict[token] : default);
        }

        public Task<string> GetPemKey(string email)
        {
            return Task.FromResult(pemKeysDict.ContainsKey(email) ? pemKeysDict[email] : null);
        }

        public Task<byte[]> GetPfx(string nakedUrl)
        {
            return Task.FromResult(pfxDict.ContainsKey(nakedUrl) ? pfxDict[nakedUrl] : null);
        }

        public Task<string> GetPfxPassword(string nakedUrl)
        {
            return Task.FromResult(pfxPwdDict.ContainsKey(nakedUrl) ? pfxPwdDict[nakedUrl] : null);
        }

        public Task StoreAuthz(string token, (string authz, Uri location) value)
        {
            authzDict.Add(token, value);
            return Task.CompletedTask;
        }

        public Task StorePemKey(string email, string pemKey)
        {
            pemKeysDict.Add(email, pemKey);
            return Task.CompletedTask;
        }

        public Task StorePfx(string nakedUrl, byte[] pfx)
        {
            pfxDict.Add(nakedUrl, pfx);
            return Task.CompletedTask;
        }

        public Task StorePfxPassword(string nakedUrl, string password)
        {
            pfxPwdDict.Add(nakedUrl, password);
            return Task.CompletedTask;
        }
    }
}
