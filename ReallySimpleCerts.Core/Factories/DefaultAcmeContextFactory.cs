using Certes;
using System;

namespace ReallySimpleCerts.Core
{
    public class DefaultAcmeContextFactory : IAcmeContextFactory
    {
        public IAcmeContext GetContext(Uri uri, IKey key = null)
        {
            return new AcmeContext(uri, key);
        }
    }
}
