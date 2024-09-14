namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class ListExtensions
    {
        public static void RemoveWhere<TSource>(this List<TSource> list, Predicate<TSource> match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (match.Invoke(item))
                {
                    list.RemoveAt(i);
                }

                i--;
            }
        }
    }
}
