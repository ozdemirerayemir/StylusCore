using System.Windows;
using StylusCore.Core.Models;

namespace StylusCore.Drawing.Tools
{
    /// <summary>
    /// Common interface for all drawing tools.
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Tool name for display
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Tool icon identifier
        /// </summary>
        string IconId { get; }

        /// <summary>
        /// Whether tool is currently active
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Current stroke color
        /// </summary>
        string Color { get; set; }

        /// <summary>
        /// Current stroke width
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Current opacity (0.0 to 1.0)
        /// </summary>
        double Opacity { get; set; }

        /// <summary>
        /// Called when tool is selected
        /// </summary>
        void Activate();

        /// <summary>
        /// Called when tool is deselected
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Called when pointer/pen touches down
        /// </summary>
        void OnPointerDown(Point position, float pressure);

        /// <summary>
        /// Called when pointer/pen moves
        /// </summary>
        void OnPointerMove(Point position, float pressure);

        /// <summary>
        /// Called when pointer/pen lifts up
        /// </summary>
        void OnPointerUp(Point position);

        /// <summary>
        /// Get the current stroke being drawn (if any)
        /// </summary>
        Stroke GetCurrentStroke();

        /// <summary>
        /// Cancel the current stroke
        /// </summary>
        void CancelStroke();
    }

    /// <summary>
    /// Base implementation for drawing tools
    /// </summary>
    public abstract class BaseTool : ITool
    {
        public abstract string Name { get; }
        public abstract string IconId { get; }
        public bool IsActive { get; set; }
        public string Color { get; set; } = "#000000";
        public double Width { get; set; } = 2.0;
        public double Opacity { get; set; } = 1.0;

        protected Stroke _currentStroke;
        protected bool _isDrawing;

        public virtual void Activate()
        {
            IsActive = true;
        }

        public virtual void Deactivate()
        {
            IsActive = false;
            CancelStroke();
        }

        public abstract void OnPointerDown(Point position, float pressure);
        public abstract void OnPointerMove(Point position, float pressure);
        public abstract void OnPointerUp(Point position);

        public Stroke GetCurrentStroke()
        {
            return _currentStroke;
        }

        public void CancelStroke()
        {
            _currentStroke = null;
            _isDrawing = false;
        }
    }
}
