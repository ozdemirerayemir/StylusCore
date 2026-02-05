using System;
using System.Windows;

namespace StylusCore.Engine.Wpf.Shapes
{
    /// <summary>
    /// Line shape implementation.
    /// </summary>
    public class LineShape : IShape
    {
        public string ShapeType => "Line";
        public string StrokeColor { get; set; } = "#000000";
        public double StrokeWidth { get; set; } = 2.0;
        public string FillColor { get; set; } = "transparent";

        /// <summary>
        /// Start point of the line
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// End point of the line
        /// </summary>
        public Point EndPoint { get; set; }

        public LineShape() { }

        public LineShape(Point start, Point end)
        {
            StartPoint = start;
            EndPoint = end;
        }

        public bool ContainsPoint(Point point)
        {
            // Check if point is within tolerance of the line
            const double tolerance = 5.0;
            return DistanceToLine(point) <= tolerance + StrokeWidth / 2;
        }

        public Rect GetBounds()
        {
            return new Rect(StartPoint, EndPoint);
        }

        public void Move(double deltaX, double deltaY)
        {
            StartPoint = new Point(StartPoint.X + deltaX, StartPoint.Y + deltaY);
            EndPoint = new Point(EndPoint.X + deltaX, EndPoint.Y + deltaY);
        }

        public void Resize(Rect newBounds)
        {
            StartPoint = new Point(newBounds.Left, newBounds.Top);
            EndPoint = new Point(newBounds.Right, newBounds.Bottom);
        }

        /// <summary>
        /// Calculate distance from point to line segment
        /// </summary>
        private double DistanceToLine(Point point)
        {
            var dx = EndPoint.X - StartPoint.X;
            var dy = EndPoint.Y - StartPoint.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);

            if (length == 0) return (point - StartPoint).Length;

            var t = Math.Max(0, Math.Min(1, ((point.X - StartPoint.X) * dx + (point.Y - StartPoint.Y) * dy) / (length * length)));
            var projection = new Point(StartPoint.X + t * dx, StartPoint.Y + t * dy);

            return (point - projection).Length;
        }
    }
}
