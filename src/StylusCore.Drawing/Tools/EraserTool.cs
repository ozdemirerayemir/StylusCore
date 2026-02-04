using System;
using System.Collections.Generic;
using System.Windows;
using StylusCore.Core.Enums;
using StylusCore.Core.Models;

namespace StylusCore.Drawing.Tools
{
    /// <summary>
    /// Eraser tool with multiple modes: Normal, Stroke, and Lasso.
    /// </summary>
    public class EraserTool : BaseTool
    {
        public override string Name => "Eraser";
        public override string IconId => "eraser_icon";

        /// <summary>
        /// Current eraser mode
        /// </summary>
        public EraserMode Mode { get; set; } = EraserMode.Normal;

        /// <summary>
        /// Eraser size (radius)
        /// </summary>
        public double EraserSize { get; set; } = 20.0;

        /// <summary>
        /// Collection of points for lasso selection
        /// </summary>
        private List<Point> _lassoPoints;

        /// <summary>
        /// Event fired when strokes should be erased
        /// </summary>
        public event EventHandler<EraseEventArgs> EraseRequested;

        public EraserTool()
        {
            _lassoPoints = new List<Point>();
        }

        public override void OnPointerDown(Point position, float pressure)
        {
            _isDrawing = true;

            switch (Mode)
            {
                case EraserMode.Normal:
                    // Erase at point
                    RequestErase(position);
                    break;

                case EraserMode.Stroke:
                    // Will erase entire stroke on touch
                    RequestEraseStroke(position);
                    break;

                case EraserMode.Lasso:
                    // Start lasso selection
                    _lassoPoints.Clear();
                    _lassoPoints.Add(position);
                    break;
            }
        }

        public override void OnPointerMove(Point position, float pressure)
        {
            if (!_isDrawing) return;

            switch (Mode)
            {
                case EraserMode.Normal:
                    RequestErase(position);
                    break;

                case EraserMode.Stroke:
                    RequestEraseStroke(position);
                    break;

                case EraserMode.Lasso:
                    _lassoPoints.Add(position);
                    break;
            }
        }

        public override void OnPointerUp(Point position)
        {
            if (!_isDrawing) return;
            _isDrawing = false;

            if (Mode == EraserMode.Lasso && _lassoPoints.Count > 2)
            {
                // Close the lasso and erase everything inside
                RequestEraseLasso(_lassoPoints);
                _lassoPoints.Clear();
            }
        }

        private void RequestErase(Point center)
        {
            EraseRequested?.Invoke(this, new EraseEventArgs
            {
                Mode = EraserMode.Normal,
                Center = center,
                Radius = EraserSize
            });
        }

        private void RequestEraseStroke(Point point)
        {
            EraseRequested?.Invoke(this, new EraseEventArgs
            {
                Mode = EraserMode.Stroke,
                Center = point,
                Radius = 5 // Small touch radius for stroke detection
            });
        }

        private void RequestEraseLasso(List<Point> points)
        {
            EraseRequested?.Invoke(this, new EraseEventArgs
            {
                Mode = EraserMode.Lasso,
                LassoPoints = new List<Point>(points)
            });
        }
    }

    /// <summary>
    /// Event args for erase operations
    /// </summary>
    public class EraseEventArgs : EventArgs
    {
        public EraserMode Mode { get; set; }
        public Point Center { get; set; }
        public double Radius { get; set; }
        public List<Point> LassoPoints { get; set; }

        public EraseEventArgs()
        {
            LassoPoints = new List<Point>();
        }
    }
}
