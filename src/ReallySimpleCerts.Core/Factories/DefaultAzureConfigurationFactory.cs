using Microsoft.Azure.Management.Fluent;

namespace ReallySimpleCerts.Core
{
    public class DefaultAzureConfigurationFactory : IAzureConfigurationFactory
    {
        public Azure.IConfigurable Configuration { get => Azure.Configure(); }
    }
}
