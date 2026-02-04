using System;
using System.Windows;
using System.Windows.Media;
using StylusCore.Core.Enums;
using StylusCore.Core.Models;

namespace StylusCore.Drawing.Rendering
{
    /// <summary>
    /// Handles rendering of pages with strokes, shapes, and content blocks.
    /// </summary>
    public class PageRenderer
    {
        /// <summary>
        /// Current zoom level (1.0 = 100%)
        /// </summary>
        public double ZoomLevel { get; set; } = 1.0;

        /// <summary>
        /// Pan offset X
        /// </summary>
        public double OffsetX { get; set; } = 0;

        /// <summary>
        /// Pan offset Y
        /// </summary>
        public double OffsetY { get; set; } = 0;

        /// <summary>
        /// Minimum zoom level
        /// </summary>
        public double MinZoom { get; set; } = 0.1;

        /// <summary>
        /// Maximum zoom level
        /// </summary>
        public double MaxZoom { get; set; } = 5.0;

        /// <summary>
        /// Zoom step for each wheel tick
        /// </summary>
        public double ZoomStep { get; set; } = 0.1;

        /// <summary>
        /// Apply page template background pattern
        /// </summary>
        public DrawingBrush RenderPageTemplate(PageTemplate template, double width, double height)
        {
            return template switch
            {
                PageTemplate.Lined => CreateLinedPattern(width),
                PageTemplate.Grid => CreateGridPattern(),
                PageTemplate.Dotted => CreateDottedPattern(),
                _ => null // Blank
            };
        }

        /// <summary>
        /// Create lined paper pattern
        /// </summary>
        private DrawingBrush CreateLinedPattern(double width)
        {
            const double lineSpacing = 30;
            var lineColor = Color.FromRgb(200, 220, 255);

            var drawingGroup = new DrawingGroup();
            using (var context = drawingGroup.Open())
            {
                var pen = new Pen(new SolidColorBrush(lineColor), 1);
                context.DrawLine(pen, new Point(0, lineSpacing), new Point(width, lineSpacing));
            }

            return new DrawingBrush(drawingGroup)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, width, lineSpacing),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        /// <summary>
        /// Create grid paper pattern
        /// </summary>
        private DrawingBrush CreateGridPattern()
        {
            const double gridSize = 30;
            var lineColor = Color.FromRgb(200, 200, 220);

            var drawingGroup = new DrawingGroup();
            using (var context = drawingGroup.Open())
            {
                var pen = new Pen(new SolidColorBrush(lineColor), 0.5);
                context.DrawLine(pen, new Point(0, 0), new Point(gridSize, 0));
                context.DrawLine(pen, new Point(0, 0), new Point(0, gridSize));
            }

            return new DrawingBrush(drawingGroup)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, gridSize, gridSize),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        /// <summary>
        /// Create dotted paper pattern
        /// </summary>
        private DrawingBrush CreateDottedPattern()
        {
            const double dotSize = 3;
            const double dotSpacing = 25;
            var dotColor = Color.FromRgb(180, 180, 200);

            var drawingGroup = new DrawingGroup();
            using (var context = drawingGroup.Open())
            {
                var brush = new SolidColorBrush(dotColor);
                context.DrawEllipse(brush, null, new Point(dotSpacing / 2, dotSpacing / 2), dotSize / 2, dotSize / 2);
            }

            return new DrawingBrush(drawingGroup)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, dotSpacing, dotSpacing),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        /// <summary>
        /// Zoom in by one step
        /// </summary>
        public void ZoomIn()
        {
            ZoomLevel = Math.Min(MaxZoom, ZoomLevel + ZoomStep);
        }

        /// <summary>
        /// Zoom out by one step
        /// </summary>
        public void ZoomOut()
        {
            ZoomLevel = Math.Max(MinZoom, ZoomLevel - ZoomStep);
        }

        /// <summary>
        /// Set zoom level centered on a point
        /// </summary>
        public void ZoomAtPoint(double newZoom, Point center)
        {
            var oldZoom = ZoomLevel;
            ZoomLevel = Math.Max(MinZoom, Math.Min(MaxZoom, newZoom));

            // Adjust offset to keep center point fixed
            var zoomRatio = ZoomLevel / oldZoom;
            OffsetX = center.X - (center.X - OffsetX) * zoomRatio;
            OffsetY = center.Y - (center.Y - OffsetY) * zoomRatio;
        }

        /// <summary>
        /// Pan the view by delta
        /// </summary>
        public void Pan(double deltaX, double deltaY)
        {
            OffsetX += deltaX;
            OffsetY += deltaY;
        }

        /// <summary>
        /// Convert screen coordinates to page coordinates
        /// </summary>
        public Point ScreenToPage(Point screenPoint)
        {
            return new Point(
                (screenPoint.X - OffsetX) / ZoomLevel,
                (screenPoint.Y - OffsetY) / ZoomLevel
            );
        }

        /// <summary>
        /// Convert page coordinates to screen coordinates
        /// </summary>
        public Point PageToScreen(Point pagePoint)
        {
            return new Point(
                pagePoint.X * ZoomLevel + OffsetX,
                pagePoint.Y * ZoomLevel + OffsetY
            );
        }

        /// <summary>
        /// Get the transform for rendering
        /// </summary>
        public Transform GetRenderTransform()
        {
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(ZoomLevel, ZoomLevel));
            transformGroup.Children.Add(new TranslateTransform(OffsetX, OffsetY));
            return transformGroup;
        }
    }
}
