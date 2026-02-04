using System;
using System.Collections.Generic;
using System.Windows;

namespace StylusCore.Core.Models
{
    /// <summary>
    /// Represents a single pen stroke on a page.
    /// Contains all points with pressure data.
    /// </summary>
    public class Stroke
    {
        /// <summary>
        /// Unique identifier for the stroke
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to parent page
        /// </summary>
        public Guid PageId { get; set; }

        /// <summary>
        /// Collection of points that make up the stroke
        /// </summary>
        public List<StrokePoint> Points { get; set; }

        /// <summary>
        /// Stroke color in hex format
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Base stroke width (before pressure adjustment)
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Opacity of the stroke (0.0 to 1.0)
        /// </summary>
        public double Opacity { get; set; }

        /// <summary>
        /// Whether this is a highlighter stroke
        /// </summary>
        public bool IsHighlighter { get; set; }

        /// <summary>
        /// Z-index for layering
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Date when the stroke was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public Stroke()
        {
            Id = Guid.NewGuid();
            PageId = Guid.Empty;
            Points = new List<StrokePoint>();
            Color = "#000000";
            Width = 2.0;
            Opacity = 1.0;
            IsHighlighter = false;
            ZIndex = 0;
            CreatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents a single point in a stroke with pressure and tilt data.
    /// </summary>
    public class StrokePoint
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Pressure value (0.0 to 1.0)
        /// </summary>
        public float Pressure { get; set; }

        /// <summary>
        /// Pen tilt X (-1.0 to 1.0)
        /// </summary>
        public float TiltX { get; set; }

        /// <summary>
        /// Pen tilt Y (-1.0 to 1.0)
        /// </summary>
        public float TiltY { get; set; }

        /// <summary>
        /// Timestamp of this point (for replay/animation)
        /// </summary>
        public long Timestamp { get; set; }

        public StrokePoint()
        {
            Pressure = 1.0f;
            TiltX = 0;
            TiltY = 0;
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public StrokePoint(double x, double y, float pressure = 1.0f)
        {
            X = x;
            Y = y;
            Pressure = pressure;
            TiltX = 0;
            TiltY = 0;
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}
