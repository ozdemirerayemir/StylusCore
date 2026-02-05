using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using StylusCore.Engine.Wpf.Rendering;
using ModelPage = StylusCore.Core.Models.Page;
using ModelStroke = StylusCore.Core.Models.Stroke;
using ModelShapeBlock = StylusCore.Core.Models.ShapeBlock;

namespace StylusCore.Engine.Wpf.Controls
{
    /// <summary>
    /// Canvas host control for drawing with pen, mouse, and stylus input.
    /// Supports zoom, pan, and pressure-sensitive drawing.
    /// </summary>
    public partial class CanvasHostControl : UserControl
    {
        private object _viewModel;
        private PageRenderer _renderer;
        private List<Point> _currentPoints;
        private bool _isDrawing;
        private bool _isPanning;
        private Point _lastPanPoint;

        // Events for App layer to subscribe
        public event Action<Point, float> PointerDown;
        public event Action<Point, float> PointerMove;
        public event Action<Point> PointerUp;
        public event Action<ModelPage> PageChangeRequested;

        public CanvasHostControl()
        {
            InitializeComponent();
            _currentPoints = new List<Point>();
            _renderer = new PageRenderer();
        }

        /// <summary>
        /// Page renderer instance
        /// </summary>
        public PageRenderer Renderer
        {
            get => _renderer;
            set => _renderer = value ?? new PageRenderer();
        }

        /// <summary>
        /// Render a page
        /// </summary>
        public void RenderPage(ModelPage page)
        {
            if (page == null) return;

            // Clear existing strokes
            StrokesLayer.Children.Clear();
            ShapesLayer.Children.Clear();

            // Set page dimensions
            PageBackground.Width = page.Width;
            PageBackground.Height = page.Height;

            // Apply template
            var templateBrush = _renderer.RenderPageTemplate(page.Template, page.Width, page.Height);
            if (templateBrush != null)
            {
                PageBackground.Background = templateBrush;
            }

            // Render strokes
            foreach (var stroke in page.Strokes)
            {
                RenderStroke(stroke);
            }

            // Render shapes
            foreach (var shape in page.ShapeBlocks)
            {
                RenderShape(shape);
            }

            // Apply transform
            DrawingSurface.RenderTransform = _renderer.GetRenderTransform();
        }

        /// <summary>
        /// Render a single stroke
        /// </summary>
        private void RenderStroke(ModelStroke stroke)
        {
            if (stroke.Points.Count < 2) return;

            var path = new Path
            {
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stroke.Color)),
                StrokeThickness = stroke.Width,
                Opacity = stroke.Opacity,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };

            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(stroke.Points[0].X, stroke.Points[0].Y)
            };

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                figure.Segments.Add(new LineSegment(
                    new Point(stroke.Points[i].X, stroke.Points[i].Y), true));
            }

            geometry.Figures.Add(figure);
            path.Data = geometry;

            StrokesLayer.Children.Add(path);
        }

        /// <summary>
        /// Render a shape
        /// </summary>
        private void RenderShape(ModelShapeBlock shape)
        {
            Shape wpfShape = null;

            switch (shape.ShapeType)
            {
                case "Rectangle":
                    wpfShape = new Rectangle
                    {
                        Width = shape.Width,
                        Height = shape.Height
                    };
                    Canvas.SetLeft(wpfShape, shape.X);
                    Canvas.SetTop(wpfShape, shape.Y);
                    break;

                case "Ellipse":
                    wpfShape = new Ellipse
                    {
                        Width = shape.Width,
                        Height = shape.Height
                    };
                    Canvas.SetLeft(wpfShape, shape.X);
                    Canvas.SetTop(wpfShape, shape.Y);
                    break;

                case "Line":
                    wpfShape = new Line
                    {
                        X1 = shape.X,
                        Y1 = shape.Y,
                        X2 = shape.X2,
                        Y2 = shape.Y2
                    };
                    break;
            }

            if (wpfShape != null)
            {
                wpfShape.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(shape.StrokeColor));
                wpfShape.StrokeThickness = shape.StrokeWidth;

                if (!string.IsNullOrEmpty(shape.FillColor) && shape.FillColor != "transparent")
                {
                    wpfShape.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(shape.FillColor));
                }

                ShapesLayer.Children.Add(wpfShape);
            }
        }

        #region Mouse Input

        private void DrawingSurface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _isPanning = true;
                _lastPanPoint = e.GetPosition(this);
                DrawingSurface.CaptureMouse();
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                StartDrawing(e.GetPosition(DrawingSurface), 1.0f);
            }
        }

        private void DrawingSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                var currentPoint = e.GetPosition(this);
                var delta = currentPoint - _lastPanPoint;
                _renderer.Pan(delta.X, delta.Y);
                _lastPanPoint = currentPoint;
                DrawingSurface.RenderTransform = _renderer.GetRenderTransform();
                return;
            }

            if (_isDrawing)
            {
                ContinueDrawing(e.GetPosition(DrawingSurface), 1.0f);
            }
        }

        private void DrawingSurface_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                DrawingSurface.ReleaseMouseCapture();
                return;
            }

            if (_isDrawing)
            {
                EndDrawing(e.GetPosition(DrawingSurface));
            }
        }

        private void DrawingSurface_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var center = e.GetPosition(DrawingSurface);
            var newZoom = _renderer.ZoomLevel + (e.Delta > 0 ? 0.1 : -0.1);
            _renderer.ZoomAtPoint(newZoom, center);
            DrawingSurface.RenderTransform = _renderer.GetRenderTransform();
            UpdateZoomLabel();
        }

        #endregion

        #region Stylus Input

        private void DrawingSurface_StylusDown(object sender, StylusDownEventArgs e)
        {
            var point = e.GetPosition(DrawingSurface);
            var pressure = GetPressure(e.StylusDevice);
            StartDrawing(point, pressure);
        }

        private void DrawingSurface_StylusMove(object sender, StylusEventArgs e)
        {
            if (_isDrawing)
            {
                var point = e.GetPosition(DrawingSurface);
                var pressure = GetPressure(e.StylusDevice);
                ContinueDrawing(point, pressure);
            }
        }

        private void DrawingSurface_StylusUp(object sender, StylusEventArgs e)
        {
            if (_isDrawing)
            {
                EndDrawing(e.GetPosition(DrawingSurface));
            }
        }

        private float GetPressure(StylusDevice stylus)
        {
            return 1.0f; // TODO: Extract actual pressure from stylus data
        }

        #endregion

        #region Drawing Operations

        private void StartDrawing(Point position, float pressure)
        {
            _isDrawing = true;
            _currentPoints.Clear();
            _currentPoints.Add(position);

            PointerDown?.Invoke(position, pressure);
            DrawingSurface.CaptureMouse();
        }

        private void ContinueDrawing(Point position, float pressure)
        {
            _currentPoints.Add(position);
            PointerMove?.Invoke(position, pressure);

            UpdatePreviewPath();
        }

        private void EndDrawing(Point position)
        {
            _isDrawing = false;
            PointerUp?.Invoke(position);
            DrawingSurface.ReleaseMouseCapture();

            CurrentStrokePath.Data = null;
            _currentPoints.Clear();
        }

        private void UpdatePreviewPath()
        {
            if (_currentPoints.Count < 2) return;

            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = _currentPoints[0] };

            for (int i = 1; i < _currentPoints.Count; i++)
            {
                figure.Segments.Add(new LineSegment(_currentPoints[i], true));
            }

            geometry.Figures.Add(figure);
            CurrentStrokePath.Data = geometry;
        }

        #endregion

        #region Zoom Controls

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _renderer.ZoomIn();
            DrawingSurface.RenderTransform = _renderer.GetRenderTransform();
            UpdateZoomLabel();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _renderer.ZoomOut();
            DrawingSurface.RenderTransform = _renderer.GetRenderTransform();
            UpdateZoomLabel();
        }

        private void ZoomFit_Click(object sender, RoutedEventArgs e)
        {
            _renderer.ZoomLevel = 1.0;
            _renderer.OffsetX = 0;
            _renderer.OffsetY = 0;
            DrawingSurface.RenderTransform = _renderer.GetRenderTransform();
            UpdateZoomLabel();
        }

        private void UpdateZoomLabel()
        {
            ZoomLabel.Text = $"{_renderer.ZoomLevel:P0}";
        }

        #endregion
    }
}
