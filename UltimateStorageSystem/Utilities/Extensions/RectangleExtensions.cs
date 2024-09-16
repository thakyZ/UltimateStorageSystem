namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class RectangleExtensions
    {
        public static Rectangle MoveHorizontal(this Rectangle rectangle, int movement)
        {
            return new Rectangle(rectangle.X + movement, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static Rectangle MoveVertical(this Rectangle rectangle, int movement)
        {
            return new Rectangle(rectangle.X, rectangle.Y + movement, rectangle.Width, rectangle.Height);
        }

        public static Rectangle ResizeHorizontal(this Rectangle rectangle, int movement)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width + movement, rectangle.Height);
        }

        public static Rectangle ResizeVertical(this Rectangle rectangle, int movement)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height + movement);
        }
    }
}
