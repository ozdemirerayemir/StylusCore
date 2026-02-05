using System;
using System.Windows.Input;

namespace StylusCore.Engine.Wpf.Input.Devices
{
    /// <summary>
    /// Handles keyboard input for the application.
    /// </summary>
    public class KeyboardDevice : IInputDevice
    {
        public string DeviceId => "Keyboard";
        public string DeviceName => "Keyboard";
        public bool IsConnected => true; // Keyboard is always available

        public event EventHandler<InputEventArgs> ButtonPressed;
        public event EventHandler<InputEventArgs> ButtonReleased;

        private bool _isInitialized;

        public bool Initialize()
        {
            if (_isInitialized) return true;

            // Hook into WPF keyboard events
            _isInitialized = true;
            return true;
        }

        public void Shutdown()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Handle key down from WPF
        /// </summary>
        public void OnKeyDown(KeyEventArgs e)
        {
            var inputId = GetInputId(e.Key);
            var args = new InputEventArgs(inputId)
            {
                IsCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                IsShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                IsAltPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
            };
            ButtonPressed?.Invoke(this, args);
        }

        /// <summary>
        /// Handle key up from WPF
        /// </summary>
        public void OnKeyUp(KeyEventArgs e)
        {
            var inputId = GetInputId(e.Key);
            var args = new InputEventArgs(inputId);
            ButtonReleased?.Invoke(this, args);
        }

        /// <summary>
        /// Convert WPF Key to input ID string
        /// </summary>
        private string GetInputId(Key key)
        {
            return $"Key_{key}";
        }
    }
}
