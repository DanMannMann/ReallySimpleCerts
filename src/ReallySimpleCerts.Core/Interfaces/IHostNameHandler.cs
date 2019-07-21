using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public interface IHostNameHandler
    {
        Task EnsureHostNameBinding();
    }
}
