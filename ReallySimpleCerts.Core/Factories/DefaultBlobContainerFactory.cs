using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public class DefaultBlobContainerFactory : IBlobContainerFactory
    {
        private readonly BlobStorePersistenceOptions options;
        private CloudBlobContainer container;

        public DefaultBlobContainerFactory(IOptions<BlobStorePersistenceOptions> options)
        {
            this.options = options.Value;
        }

        public Task<CloudBlobContainer> GetContainer()
        {
            if (container == null)
            {
                var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
                var cloudBlobClient = storageAccount.CreateCloudBlobClient();
                container = cloudBlobClient.GetContainerReference(options.ContainerName);
            }
            return Task.FromResult(container);
        }
    }
}
