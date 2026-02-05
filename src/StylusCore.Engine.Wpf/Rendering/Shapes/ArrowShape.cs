using System;
using System.Windows;

namespace StylusCore.Engine.Wpf.Shapes
{
    /// <summary>
    /// Arrow shape with optional heads at start and/or end.
    /// </summary>
    public class ArrowShape : LineShape
    {
        public new string ShapeType => "Arrow";

        /// <summary>
        /// Whether to show arrow head at start
        /// </summary>
        public bool HasArrowStart { get; set; }

        /// <summary>
        /// Whether to show arrow head at end
        /// </summary>
        public bool HasArrowEnd { get; set; } = true;

        /// <summary>
        /// Size of the arrow head
        /// </summary>
        public double ArrowSize { get; set; } = 15.0;

        /// <summary>
        /// Arrow head angle in degrees
        /// </summary>
        public double ArrowAngle { get; set; } = 30.0;

        public ArrowShape() { }

        public ArrowShape(Point start, Point end, bool hasArrowStart = false, bool hasArrowEnd = true)
        {
            StartPoint = start;
            EndPoint = end;
            HasArrowStart = hasArrowStart;
            HasArrowEnd = hasArrowEnd;
        }

        /// <summary>
        /// Calculate arrow head points for rendering
        /// </summary>
        public (Point left, Point right) GetArrowHeadPoints(bool isStart)
        {
            var basePoint = isStart ? StartPoint : EndPoint;
            var direction = isStart ? EndPoint : StartPoint;

            // Calculate angle
            var angle = Math.Atan2(basePoint.Y - direction.Y, basePoint.X - direction.X);
            var angleRad = ArrowAngle * Math.PI / 180;

            var leftAngle = angle + angleRad;
            var rightAngle = angle - angleRad;

            var left = new Point(
                basePoint.X - ArrowSize * Math.Cos(leftAngle),
                basePoint.Y - ArrowSize * Math.Sin(leftAngle)
            );

            var right = new Point(
                basePoint.X - ArrowSize * Math.Cos(rightAngle),
                basePoint.Y - ArrowSize * Math.Sin(rightAngle)
            );

            return (left, right);
        }
    }
}
