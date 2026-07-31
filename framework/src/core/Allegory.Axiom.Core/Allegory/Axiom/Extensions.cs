using System.Threading;

namespace Allegory.Axiom;

public static class Extensions
{
    extension(CancellationToken preferred)
    {
        public CancellationToken FallbackTo(CancellationToken fallback)
        {
            return preferred == CancellationToken.None ? fallback : preferred;
        }
    }
}