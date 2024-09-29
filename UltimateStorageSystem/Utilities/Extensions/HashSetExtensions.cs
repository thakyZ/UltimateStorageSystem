namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class HashSetExtensions
    {
        public static int Merge<TSource>(this HashSet<TSource> self, HashSet<TSource> other)
        {
            int count = 0;
            foreach (TSource item in other.Where(item => !self.Contains(item)))
            {
                self.Add(item);
                count++;
            }
            return count;
        }
    }
}
