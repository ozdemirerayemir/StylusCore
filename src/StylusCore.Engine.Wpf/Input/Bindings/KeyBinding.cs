using System;
using System.Collections.Generic;

namespace StylusCore.Engine.Wpf.Input.Bindings
{
    /// <summary>
    /// Represents a key/button binding to an action.
    /// </summary>
    public class KeyBinding
    {
        /// <summary>
        /// Unique identifier for this binding
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Input device identifier (e.g., "Tablet_Button1", "Key_A", "Mouse_Left")
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Action to perform when input is triggered
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Parameters for the action (e.g., menu ID for OpenRadialMenu)
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Whether Ctrl must be held
        /// </summary>
        public bool RequiresCtrl { get; set; }

        /// <summary>
        /// Whether Shift must be held
        /// </summary>
        public bool RequiresShift { get; set; }

        /// <summary>
        /// Whether Alt must be held
        /// </summary>
        public bool RequiresAlt { get; set; }

        /// <summary>
        /// Priority for conflict resolution (higher = preferred)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// User-defined name for this binding
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether this binding is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        public KeyBinding()
        {
            Id = Guid.NewGuid();
            InputId = string.Empty;
            Action = string.Empty;
            Parameters = new Dictionary<string, string>();
            Priority = 0;
            DisplayName = string.Empty;
            IsEnabled = true;
        }

        public KeyBinding(string inputId, string action) : this()
        {
            InputId = inputId;
            Action = action;
        }

        /// <summary>
        /// Check if this binding matches the given input
        /// </summary>
        public bool Matches(string inputId, bool ctrlPressed, bool shiftPressed, bool altPressed)
        {
            if (!IsEnabled) return false;
            if (InputId != inputId) return false;
            if (RequiresCtrl != ctrlPressed) return false;
            if (RequiresShift != shiftPressed) return false;
            if (RequiresAlt != altPressed) return false;
            return true;
        }

        /// <summary>
        /// Get human-readable binding description
        /// </summary>
        public string GetDescription()
        {
            var parts = new List<string>();
            if (RequiresCtrl) parts.Add("Ctrl");
            if (RequiresShift) parts.Add("Shift");
            if (RequiresAlt) parts.Add("Alt");
            parts.Add(InputId);
            return string.Join(" + ", parts);
        }
    }

    /// <summary>
    /// Known action types
    /// </summary>
    public static class ActionTypes
    {
        public const string OpenRadialMenu = "OpenRadialMenu";
        public const string SetToolMode = "SetToolMode";
        public const string SetInputMode = "SetInputMode";
        public const string Undo = "Undo";
        public const string Redo = "Redo";
        public const string Save = "Save";
        public const string Zoom = "Zoom";
        public const string Pan = "Pan";
        public const string ToggleEraser = "ToggleEraser";
        public const string BrushSize = "BrushSize";
        public const string Opacity = "Opacity";
        public const string ScrollHorizontal = "ScrollH";
        public const string ScrollVertical = "ScrollV";
    }
}
