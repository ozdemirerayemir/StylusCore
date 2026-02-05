using System;
using System.Windows;
using StylusCore.Core.Models;
using StylusCore.Engine.Wpf.Shapes;

namespace StylusCore.Engine.Wpf.Tools
{
    /// <summary>
    /// Shape tool for drawing geometric shapes.
    /// Supports Shift key for constraining angles/proportions.
    /// </summary>
    public class ShapeTool : BaseTool
    {
        public override string Name => "Shape";
        public override string IconId => "shape_icon";

        /// <summary>
        /// Currently selected shape type
        /// </summary>
        public string ShapeType { get; set; } = "Rectangle";

        /// <summary>
        /// Whether to constrain shape (Shift key)
        /// </summary>
        public bool IsConstrained { get; set; }

        /// <summary>
        /// Fill color (null for no fill)
        /// </summary>
        public string FillColor { get; set; }

        /// <summary>
        /// Whether to add arrow at start (for lines)
        /// </summary>
        public bool HasArrowStart { get; set; }

        /// <summary>
        /// Whether to add arrow at end (for lines)
        /// </summary>
        public bool HasArrowEnd { get; set; }

        private Point _startPoint;
        private ShapeBlock _currentShape;

        /// <summary>
        /// Event fired when shape drawing is complete
        /// </summary>
        public event EventHandler<ShapeCompletedEventArgs> ShapeCompleted;

        public override void OnPointerDown(Point position, float pressure)
        {
            _isDrawing = true;
            _startPoint = position;

            _currentShape = new ShapeBlock
            {
                ShapeType = ShapeType,
                X = position.X,
                Y = position.Y,
                StrokeColor = Color,
                StrokeWidth = Width,
                FillColor = FillColor ?? "transparent",
                HasArrowStart = HasArrowStart,
                HasArrowEnd = HasArrowEnd
            };
        }

        public override void OnPointerMove(Point position, float pressure)
        {
            if (!_isDrawing || _currentShape == null) return;

            UpdateShape(position);
        }

        public override void OnPointerUp(Point position)
        {
            if (!_isDrawing || _currentShape == null) return;

            UpdateShape(position);
            _isDrawing = false;

            // Complete the shape
            ShapeCompleted?.Invoke(this, new ShapeCompletedEventArgs { Shape = _currentShape });
            _currentShape = null;
        }

        private void UpdateShape(Point endPoint)
        {
            var adjustedEnd = IsConstrained ? ConstrainPoint(endPoint) : endPoint;

            if (ShapeType == "Line" || ShapeType == "Arrow")
            {
                // Lines use X,Y for start and X2,Y2 for end
                _currentShape.X = _startPoint.X;
                _currentShape.Y = _startPoint.Y;
                _currentShape.X2 = adjustedEnd.X;
                _currentShape.Y2 = adjustedEnd.Y;
            }
            else
            {
                // Rectangles, ellipses use X,Y for top-left and Width,Height
                _currentShape.X = Math.Min(_startPoint.X, adjustedEnd.X);
                _currentShape.Y = Math.Min(_startPoint.Y, adjustedEnd.Y);
                _currentShape.Width = Math.Abs(adjustedEnd.X - _startPoint.X);
                _currentShape.Height = Math.Abs(adjustedEnd.Y - _startPoint.Y);

                // If constrained, make square/circle
                if (IsConstrained)
                {
                    var size = Math.Max(_currentShape.Width, _currentShape.Height);
                    _currentShape.Width = size;
                    _currentShape.Height = size;
                }
            }
        }

        /// <summary>
        /// Constrain point to 0°, 45°, or 90° angles
        /// </summary>
        private Point ConstrainPoint(Point endPoint)
        {
            var dx = endPoint.X - _startPoint.X;
            var dy = endPoint.Y - _startPoint.Y;
            var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            // Snap to nearest 45° increment
            var snappedAngle = Math.Round(angle / 45) * 45;
            var radians = snappedAngle * Math.PI / 180;

            return new Point(
                _startPoint.X + distance * Math.Cos(radians),
                _startPoint.Y + distance * Math.Sin(radians)
            );
        }

        /// <summary>
        /// Get current shape being drawn (for preview)
        /// </summary>
        public ShapeBlock GetCurrentShape()
        {
            return _currentShape;
        }
    }

    /// <summary>
    /// Event args for shape completion
    /// </summary>
    public class ShapeCompletedEventArgs : EventArgs
    {
        public ShapeBlock Shape { get; set; }
    }
}
