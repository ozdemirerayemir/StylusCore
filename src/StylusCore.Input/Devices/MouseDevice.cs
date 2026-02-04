using System;
using System.Windows.Input;

namespace StylusCore.Input.Devices
{
    /// <summary>
    /// Handles mouse input for the application.
    /// </summary>
    public class MouseDevice : IInputDevice
    {
        public string DeviceId => "Mouse";
        public string DeviceName => "Mouse";
        public bool IsConnected => true; // Mouse is always available

        public event EventHandler<InputEventArgs> ButtonPressed;
        public event EventHandler<InputEventArgs> ButtonReleased;
        public event EventHandler<PointerEventArgs> PointerMoved;
        public event EventHandler<DialEventArgs> WheelScrolled;

        private bool _isInitialized;

        public bool Initialize()
        {
            if (_isInitialized) return true;
            _isInitialized = true;
            return true;
        }

        public void Shutdown()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Handle mouse button down
        /// </summary>
        public void OnMouseDown(MouseButtonEventArgs e)
        {
            var inputId = GetInputId(e.ChangedButton);
            var args = new InputEventArgs(inputId)
            {
                IsCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                IsShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                IsAltPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
            };
            ButtonPressed?.Invoke(this, args);
        }

        /// <summary>
        /// Handle mouse button up
        /// </summary>
        public void OnMouseUp(MouseButtonEventArgs e)
        {
            var inputId = GetInputId(e.ChangedButton);
            var args = new InputEventArgs(inputId);
            ButtonReleased?.Invoke(this, args);
        }

        /// <summary>
        /// Handle mouse move
        /// </summary>
        public void OnMouseMove(System.Windows.Point position)
        {
            var args = new PointerEventArgs
            {
                InputId = "Mouse_Move",
                X = position.X,
                Y = position.Y,
                Pressure = 1.0f
            };
            PointerMoved?.Invoke(this, args);
        }

        /// <summary>
        /// Handle mouse wheel
        /// </summary>
        public void OnMouseWheel(MouseWheelEventArgs e)
        {
            var args = new DialEventArgs
            {
                InputId = e.Delta > 0 ? "Mouse_Wheel_Up" : "Mouse_Wheel_Down",
                Delta = e.Delta / 120, // Normalize to units
                IsCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
            };
            WheelScrolled?.Invoke(this, args);
        }

        /// <summary>
        /// Convert mouse button to input ID string
        /// </summary>
        private string GetInputId(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => "Mouse_Left",
                MouseButton.Right => "Mouse_Right",
                MouseButton.Middle => "Mouse_Middle",
                MouseButton.XButton1 => "Mouse_4",
                MouseButton.XButton2 => "Mouse_5",
                _ => $"Mouse_{button}"
            };
        }
    }
}
