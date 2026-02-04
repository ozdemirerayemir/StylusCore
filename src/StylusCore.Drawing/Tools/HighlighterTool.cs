using System.Windows;
using StylusCore.Core.Models;

namespace StylusCore.Drawing.Tools
{
    /// <summary>
    /// Highlighter tool for semi-transparent strokes.
    /// </summary>
    public class HighlighterTool : BaseTool
    {
        public override string Name => "Highlighter";
        public override string IconId => "highlighter_icon";

        public HighlighterTool()
        {
            // Highlighter defaults
            Color = "#FFFF00"; // Yellow
            Width = 20.0;
            Opacity = 0.4;
        }

        public override void OnPointerDown(Point position, float pressure)
        {
            _isDrawing = true;
            _currentStroke = new Stroke
            {
                Color = Color,
                Width = Width,
                Opacity = Opacity,
                IsHighlighter = true
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
            _isDrawing = false;
        }

        private void AddPoint(Point position, float pressure)
        {
            // Highlighter typically ignores pressure for consistent width
            var point = new StrokePoint(position.X, position.Y, 1.0f);
            _currentStroke.Points.Add(point);
        }
    }
}
