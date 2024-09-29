using System.Runtime.CompilerServices;

namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class ObjectExtensions
    {
        public static bool TrueIfNullAndPrint([NotNullWhen(false)] this object? obj, [CallerMemberName] string? name = null, int? index = null)
        {
            if (obj is null)
            {
                if (index is null)
                    Logger.Error($"Variable {name} is null and will not parse.");
                else
                    Logger.Error($"Variable {name} in iterator at index {index} is null and will not parse.");
                return true;
            }

            return false;
        }
    }
}
