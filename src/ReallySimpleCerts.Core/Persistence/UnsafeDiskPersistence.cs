using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public class UnsafeDiskPersistence : IReallySimpleCertPersistence
    {
        private readonly UnsafeDiskPersistenceOptions options;

        public UnsafeDiskPersistence(IOptions<UnsafeDiskPersistenceOptions> options)
        {
            this.options = options.Value;
            CheckDirectoryExists(this.options.FolderPath);
        }

        public Task<(string authz, Uri location)> GetAuthz(string token)
        {
            if (File.Exists(Path.Combine(options.FolderPath, "authz", token)))
            {
                return Task.FromResult(
                    JsonConvert.DeserializeObject<(string authz, Uri location)>(File.ReadAllText(Path.Combine(options.FolderPath, "authz", token))));
            }
            return Task.FromResult<(string authz, Uri location)>(default);
        }

        public Task<string> GetPemKey(string email)
        {
            if (File.Exists(Path.Combine(options.FolderPath, "pem", email)))
            {
                return Task.FromResult(File.ReadAllText(Path.Combine(options.FolderPath, "pem", email)));
            }
            return Task.FromResult<string>(null);
        }

        public Task<byte[]> GetPfx(string nakedUrl)
        {
            if (File.Exists(Path.Combine(options.FolderPath, "pfx", nakedUrl)))
            {
                return Task.FromResult(File.ReadAllBytes(Path.Combine(options.FolderPath, "pfx", nakedUrl)));
            }
            return Task.FromResult<byte[]>(null);
        }

        public Task<string> GetPfxPassword(string nakedUrl)
        {
            if (File.Exists(Path.Combine(options.FolderPath, "pfxpwd", nakedUrl)))
            {
                return Task.FromResult(File.ReadAllText(Path.Combine(options.FolderPath, "pfxpwd", nakedUrl)));
            }
            return Task.FromResult<string>(null);
        }

        public Task StoreAuthz(string token, (string authz, Uri location) value)
        {
            CheckDirectoryExists(Path.Combine(options.FolderPath, "authz"));
            File.WriteAllText(Path.Combine(options.FolderPath, "authz", token), JsonConvert.SerializeObject(value));
            return Task.CompletedTask;
        }

        public Task StorePemKey(string email, string pemKey)
        {
            CheckDirectoryExists(Path.Combine(options.FolderPath, "pem"));
            File.WriteAllText(Path.Combine(options.FolderPath, "pem", email), pemKey);
            return Task.CompletedTask;
        }

        public Task StorePfx(string nakedUrl, byte[] pfx)
        {
            CheckDirectoryExists(Path.Combine(options.FolderPath, "pfx"));
            File.WriteAllBytes(Path.Combine(options.FolderPath, "pfx", nakedUrl), pfx);
            return Task.CompletedTask;
        }

        public Task StorePfxPassword(string nakedUrl, string password)
        {
            CheckDirectoryExists(Path.Combine(options.FolderPath, "pfxpwd"));
            File.WriteAllText(Path.Combine(options.FolderPath, "pfxpwd", nakedUrl), password);
            return Task.CompletedTask;
        }

        private void CheckDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
