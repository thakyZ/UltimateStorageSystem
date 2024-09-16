using Netcode;

namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class NetListExtensions
    {
        internal static List<T> GetAll<T, TField>(this NetList<T, TField> netList) where TField : NetField<T, TField>, new()
        {
            return netList.GetRange(0, netList.Count);
        }
    }
}
