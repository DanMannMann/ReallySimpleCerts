using Certes;
using System;

namespace ReallySimpleCerts.Core
{
    public interface IAcmeContextFactory
    {
        IAcmeContext GetContext(Uri uri, IKey key = default);
    }
}
