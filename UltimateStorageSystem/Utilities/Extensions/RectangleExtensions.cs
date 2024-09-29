namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class RectangleExtensions
    {
        public static Rectangle MoveHorizontal(this Rectangle rectangle, int movement)
        {
            var loc = rectangle.Location;
            rectangle.Location = new Point(loc.X + movement, loc.Y);
            return rectangle;
        }

        public static Rectangle MoveVertical(this Rectangle rectangle, int movement)
        {
            var loc = rectangle.Location;
            rectangle.Location = new Point(loc.X, loc.Y + movement);
            return rectangle;
        }

        public static Rectangle ResizeHorizontal(this Rectangle rectangle, int movement)
        {
            var siz = rectangle.Size;
            rectangle.Size = new Point(siz.X + movement, siz.Y);
            return rectangle;
        }

        public static Rectangle ResizeVertical(this Rectangle rectangle, int movement)
        {
            var siz = rectangle.Size;
            rectangle.Size = new Point(siz.X, siz.Y + movement);
            return rectangle;
        }
    }
}
