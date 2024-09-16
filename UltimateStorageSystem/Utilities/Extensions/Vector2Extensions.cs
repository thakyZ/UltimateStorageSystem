namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class Vector2Extensions
    {
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }
    }
}
