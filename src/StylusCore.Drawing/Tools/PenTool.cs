using System.Windows;
using StylusCore.Core.Models;

namespace StylusCore.Drawing.Tools
{
    /// <summary>
    /// Pen tool for freehand drawing with pressure sensitivity.
    /// </summary>
    public class PenTool : BaseTool
    {
        public override string Name => "Pen";
        public override string IconId => "pen_icon";

        /// <summary>
        /// Whether to use pressure for width variation
        /// </summary>
        public bool UsePressure { get; set; } = true;

        /// <summary>
        /// Minimum width multiplier for pressure
        /// </summary>
        public double MinPressureWidth { get; set; } = 0.2;

        public override void OnPointerDown(Point position, float pressure)
        {
            _isDrawing = true;
            _currentStroke = new Stroke
            {
                Color = Color,
                Width = Width,
                Opacity = Opacity,
                IsHighlighter = false
            };

            AddPoint(position, pressure);
        }

        public override void OnPointerMove(Point position, float pressure)
        {
            if (!_isDrawing || _currentStroke == null) return;

            AddPoint(position, pressure);
        }

        public override void OnPointerUp(Point position)
        {
            if (!_isDrawing) return;

            // Stroke is complete - will be added to page by the canvas
            _isDrawing = false;
        }

        private void AddPoint(Point position, float pressure)
        {
            var point = new StrokePoint(position.X, position.Y, pressure);
            _currentStroke.Points.Add(point);
        }
    }
}
