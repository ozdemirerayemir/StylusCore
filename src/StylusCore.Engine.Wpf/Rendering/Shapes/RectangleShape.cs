using System.Windows;

namespace StylusCore.Engine.Wpf.Shapes
{
    /// <summary>
    /// Rectangle shape implementation.
    /// </summary>
    public class RectangleShape : IShape
    {
        public string ShapeType => "Rectangle";
        public string StrokeColor { get; set; } = "#000000";
        public double StrokeWidth { get; set; } = 2.0;
        public string FillColor { get; set; } = "transparent";

        /// <summary>
        /// Rectangle bounds
        /// </summary>
        public Rect Bounds { get; set; }

        /// <summary>
        /// Corner radius for rounded rectangles
        /// </summary>
        public double CornerRadius { get; set; } = 0;

        public RectangleShape() { }

        public RectangleShape(Rect bounds)
        {
            Bounds = bounds;
        }

        public RectangleShape(Point topLeft, Size size)
        {
            Bounds = new Rect(topLeft, size);
        }

        public bool ContainsPoint(Point point)
        {
            // Check if point is on the border or inside
            var expanded = new Rect(
                Bounds.X - StrokeWidth / 2,
                Bounds.Y - StrokeWidth / 2,
                Bounds.Width + StrokeWidth,
                Bounds.Height + StrokeWidth
            );
            return expanded.Contains(point);
        }

        public Rect GetBounds()
        {
            return Bounds;
        }

        public void Move(double deltaX, double deltaY)
        {
            Bounds = new Rect(Bounds.X + deltaX, Bounds.Y + deltaY, Bounds.Width, Bounds.Height);
        }

        public void Resize(Rect newBounds)
        {
            Bounds = newBounds;
        }
    }
}
