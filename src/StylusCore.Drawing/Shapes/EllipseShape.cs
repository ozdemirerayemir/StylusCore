using System;
using System.Windows;

namespace StylusCore.Drawing.Shapes
{
    /// <summary>
    /// Ellipse/Circle shape implementation.
    /// </summary>
    public class EllipseShape : IShape
    {
        public string ShapeType => "Ellipse";
        public string StrokeColor { get; set; } = "#000000";
        public double StrokeWidth { get; set; } = 2.0;
        public string FillColor { get; set; } = "transparent";

        /// <summary>
        /// Center point of the ellipse
        /// </summary>
        public Point Center { get; set; }

        /// <summary>
        /// Horizontal radius
        /// </summary>
        public double RadiusX { get; set; }

        /// <summary>
        /// Vertical radius
        /// </summary>
        public double RadiusY { get; set; }

        public EllipseShape() { }

        public EllipseShape(Point center, double radiusX, double radiusY)
        {
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        /// <summary>
        /// Create a circle (equal radii)
        /// </summary>
        public static EllipseShape Circle(Point center, double radius)
        {
            return new EllipseShape(center, radius, radius);
        }

        public bool ContainsPoint(Point point)
        {
            // Check if point is inside ellipse (including stroke width)
            var dx = (point.X - Center.X) / (RadiusX + StrokeWidth / 2);
            var dy = (point.Y - Center.Y) / (RadiusY + StrokeWidth / 2);
            return dx * dx + dy * dy <= 1;
        }

        public Rect GetBounds()
        {
            return new Rect(
                Center.X - RadiusX,
                Center.Y - RadiusY,
                RadiusX * 2,
                RadiusY * 2
            );
        }

        public void Move(double deltaX, double deltaY)
        {
            Center = new Point(Center.X + deltaX, Center.Y + deltaY);
        }

        public void Resize(Rect newBounds)
        {
            Center = new Point(newBounds.X + newBounds.Width / 2, newBounds.Y + newBounds.Height / 2);
            RadiusX = newBounds.Width / 2;
            RadiusY = newBounds.Height / 2;
        }
    }
}
