using System.Windows;

namespace StylusCore.Drawing.Shapes
{
    /// <summary>
    /// Common interface for all shape objects.
    /// </summary>
    public interface IShape
    {
        /// <summary>
        /// Shape type name
        /// </summary>
        string ShapeType { get; }

        /// <summary>
        /// Stroke color
        /// </summary>
        string StrokeColor { get; set; }

        /// <summary>
        /// Stroke width
        /// </summary>
        double StrokeWidth { get; set; }

        /// <summary>
        /// Fill color
        /// </summary>
        string FillColor { get; set; }

        /// <summary>
        /// Check if a point is inside the shape
        /// </summary>
        bool ContainsPoint(Point point);

        /// <summary>
        /// Get the bounding box of the shape
        /// </summary>
        Rect GetBounds();

        /// <summary>
        /// Move the shape by a delta
        /// </summary>
        void Move(double deltaX, double deltaY);

        /// <summary>
        /// Resize the shape to fit new bounds
        /// </summary>
        void Resize(Rect newBounds);
    }
}
