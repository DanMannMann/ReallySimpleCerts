using Microsoft.Azure.Management.Fluent;

namespace ReallySimpleCerts.Core
{
    public interface IAzureConfigurationFactory
    {
        Azure.IConfigurable Configuration { get; }
    }
}
