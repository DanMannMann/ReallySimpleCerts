using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{

    public class BlobStorePersistence : IReallySimpleCertPersistence
    {
        private readonly CloudBlobContainer container;
        private readonly BlobStorePersistenceOptions options;

        public BlobStorePersistence(IBlobContainerFactory containerFactory, IOptions<BlobStorePersistenceOptions> options)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            container = containerFactory?.GetContainer().Result ?? throw new ArgumentNullException(nameof(containerFactory));
            container.CreateIfNotExistsAsync().Wait();
        }

        public async Task<(string authz, Uri location)> GetAuthz(string token)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"authz/{token}" : $"{options.BlobPathPrefix}/authz/{token}");
            if (await blob.ExistsAsync())
            {
                return JsonConvert.DeserializeObject<(string authz, Uri location)>(await blob.DownloadTextAsync());
            }
            return default;
        }

        public async Task<string> GetPemKey(string email)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"pem/{email}" : $"{options.BlobPathPrefix}/pem/{email}");
            if (await blob.ExistsAsync())
            {
                return await blob.DownloadTextAsync();
            }
            return default;
        }

        public async Task<byte[]> GetPfx(string nakedUrl)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"pfx/{nakedUrl}" : $"{options.BlobPathPrefix}/pfx/{nakedUrl}");
            if (await blob.ExistsAsync())
            {
                using (var ms = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(ms);
                    return ms.ToArray();
                }
            }
            return default;
        }

        public async Task<string> GetPfxPassword(string nakedUrl)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"pfxpwd/{nakedUrl}" : $"{options.BlobPathPrefix}/pfxpwd/{nakedUrl}");
            if (await blob.ExistsAsync())
            {
                return await blob.DownloadTextAsync();
            }
            return default;
        }

        public async Task StoreAuthz(string token, (string authz, Uri location) value)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"authz/{token}" : $"{options.BlobPathPrefix}/authz/{token}");
            await blob.DeleteIfExistsAsync();
            await blob.UploadTextAsync(JsonConvert.SerializeObject(value));
        }

        public async Task StorePemKey(string email, string pemKey)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"pem/{email}" : $"{options.BlobPathPrefix}/pem/{email}");
            await blob.DeleteIfExistsAsync();
            await blob.UploadTextAsync(pemKey);
        }

        public async Task StorePfx(string nakedUrl, byte[] pfx)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"pfx/{nakedUrl}" : $"{options.BlobPathPrefix}/pfx/{nakedUrl}");
            await blob.DeleteIfExistsAsync();
            await blob.UploadFromByteArrayAsync(pfx, 0, pfx.Length);
        }

        public async Task StorePfxPassword(string nakedUrl, string password)
        {
            var blob = container.GetBlockBlobReference(string.IsNullOrWhiteSpace(options.BlobPathPrefix) ? $"pfxpwd/{nakedUrl}" : $"{options.BlobPathPrefix}/pfxpwd/{nakedUrl}");
            await blob.DeleteIfExistsAsync();
            await blob.UploadTextAsync(password);
        }
    }
}
