using System.Runtime.CompilerServices;

namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class BoolExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Invert(this bool b, bool invert)
        {
            return invert ? !b : b;
        }
    }
}
