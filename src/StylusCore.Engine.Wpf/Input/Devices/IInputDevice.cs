using System;

namespace StylusCore.Engine.Wpf.Input.Devices
{
    /// <summary>
    /// Common interface for all input devices.
    /// Provides unified event handling for keyboard, mouse, and tablet.
    /// </summary>
    public interface IInputDevice
    {
        /// <summary>
        /// Unique identifier for the device
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Human-readable device name
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// Whether the device is currently connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Initialize the device
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Shutdown the device
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Event fired when a button is pressed
        /// </summary>
        event EventHandler<InputEventArgs> ButtonPressed;

        /// <summary>
        /// Event fired when a button is released
        /// </summary>
        event EventHandler<InputEventArgs> ButtonReleased;
    }

    /// <summary>
    /// Event args for input device events
    /// </summary>
    public class InputEventArgs : EventArgs
    {
        /// <summary>
        /// Device identifier (e.g., "Tablet_Button1", "Key_A", "Mouse_Left")
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Raw value for analog inputs (pressure, dial rotation)
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Whether modifier keys are pressed
        /// </summary>
        public bool IsCtrlPressed { get; set; }
        public bool IsShiftPressed { get; set; }
        public bool IsAltPressed { get; set; }

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTime Timestamp { get; set; }

        public InputEventArgs()
        {
            Timestamp = DateTime.Now;
            Value = 1.0f;
        }

        public InputEventArgs(string inputId) : this()
        {
            InputId = inputId;
        }
    }

    /// <summary>
    /// Event args for pointer/pen movement
    /// </summary>
    public class PointerEventArgs : InputEventArgs
    {
        public double X { get; set; }
        public double Y { get; set; }
        public float Pressure { get; set; }
        public float TiltX { get; set; }
        public float TiltY { get; set; }
        public bool IsEraser { get; set; }

        public PointerEventArgs() : base()
        {
            Pressure = 1.0f;
        }
    }

    /// <summary>
    /// Event args for dial/wheel rotation
    /// </summary>
    public class DialEventArgs : InputEventArgs
    {
        /// <summary>
        /// Rotation delta (-1 for left/down, +1 for right/up)
        /// </summary>
        public int Delta { get; set; }

        /// <summary>
        /// Current dial mode
        /// </summary>
        public string CurrentMode { get; set; }

        public DialEventArgs() : base()
        {
            CurrentMode = "Zoom";
        }
    }
}
