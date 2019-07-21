using Microsoft.Azure.Storage.Blob;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public interface IBlobContainerFactory
    {
        Task<CloudBlobContainer> GetContainer();
    }
}
