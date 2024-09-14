using System.Reflection;

namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class FieldInfoExtensions
    {
        public static bool TryGetValue<TOut>(this FieldInfo fieldInfo, object? instance, [NotNullWhen(true)] out TOut? value, [MaybeNullWhen(false)] out Exception? exception)
        {
            value     = default;
            exception = null;

            try
            {
                value = (TOut?)fieldInfo.GetValue(instance);

                return value is not null;
            }
            catch (Exception _exception)
            {
                exception = _exception;
            }

            return false;
        }
    }
}
