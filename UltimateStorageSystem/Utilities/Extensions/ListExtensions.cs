namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class ListExtensions
    {
        public static void RemoveWhere<TSource>(this List<TSource> self, Predicate<TSource> match)
        {
            for (int i = self.Count - 1; i >= 0; i--)
            {
                if (match(self[i]))
                {
                    self.RemoveAt(i);
                }
            }
        }

        public static IEnumerable<(int Index, TSource Item)> Iterate<TSource>(this List<TSource> self)
        {
            return self.Select((x, i) => (i, x));
        }
    }
}
