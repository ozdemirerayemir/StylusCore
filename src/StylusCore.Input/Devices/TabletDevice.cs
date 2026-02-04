using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace StylusCore.Input.Devices
{
    /// <summary>
    /// Handles graphics tablet input using Wintab API.
    /// Supports VEIKK, Wacom, Huion, XP-Pen and other tablets.
    /// </summary>
    public class TabletDevice : IInputDevice
    {
        public string DeviceId => "Tablet";
        public string DeviceName { get; private set; }
        public bool IsConnected { get; private set; }

        public event EventHandler<InputEventArgs> ButtonPressed;
        public event EventHandler<InputEventArgs> ButtonReleased;
        public event EventHandler<PointerEventArgs> PenMoved;
        public event EventHandler<DialEventArgs> DialRotated;

        // Tablet capabilities
        public int ButtonCount { get; private set; }
        public int DialCount { get; private set; }
        public int PenButtonCount { get; private set; }
        public int PressureLevels { get; private set; }
        public bool SupportsTilt { get; private set; }

        // Dial state (for mode cycling)
        private Dictionary<int, string[]> _dialModes;
        private Dictionary<int, int> _currentDialModeIndex;

        private bool _isInitialized;

        public TabletDevice()
        {
            DeviceName = "Graphics Tablet";
            ButtonCount = 6; // Default for VEIKK VK1060 Pro
            DialCount = 2;
            PenButtonCount = 2;
            PressureLevels = 8192;
            SupportsTilt = true;

            // Initialize dial modes
            _dialModes = new Dictionary<int, string[]>
            {
                { 1, new[] { "Zoom", "ScrollH", "ScrollV" } },
                { 2, new[] { "BrushSize", "Opacity" } }
            };

            _currentDialModeIndex = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 }
            };
        }

        public bool Initialize()
        {
            if (_isInitialized) return true;

            try
            {
                // TODO: Initialize Wintab API
                // WintabOpen();

                IsConnected = true; // For now, assume connected
                _isInitialized = true;
                return true;
            }
            catch (Exception)
            {
                IsConnected = false;
                return false;
            }
        }

        public void Shutdown()
        {
            // TODO: Close Wintab context
            _isInitialized = false;
            IsConnected = false;
        }

        /// <summary>
        /// Handle tablet button press
        /// </summary>
        public void OnButtonDown(int buttonIndex)
        {
            var args = new InputEventArgs($"Tablet_Button{buttonIndex}");
            ButtonPressed?.Invoke(this, args);
        }

        /// <summary>
        /// Handle tablet button release
        /// </summary>
        public void OnButtonUp(int buttonIndex)
        {
            var args = new InputEventArgs($"Tablet_Button{buttonIndex}");
            ButtonReleased?.Invoke(this, args);
        }

        /// <summary>
        /// Handle pen button press
        /// </summary>
        public void OnPenButtonDown(int buttonIndex)
        {
            var args = new InputEventArgs($"Pen_Button{buttonIndex}");
            ButtonPressed?.Invoke(this, args);
        }

        /// <summary>
        /// Handle pen button release
        /// </summary>
        public void OnPenButtonUp(int buttonIndex)
        {
            var args = new InputEventArgs($"Pen_Button{buttonIndex}");
            ButtonReleased?.Invoke(this, args);
        }

        /// <summary>
        /// Handle dial click (cycle mode)
        /// </summary>
        public void OnDialClick(int dialIndex)
        {
            if (_dialModes.ContainsKey(dialIndex))
            {
                var modes = _dialModes[dialIndex];
                _currentDialModeIndex[dialIndex] = (_currentDialModeIndex[dialIndex] + 1) % modes.Length;
                var newMode = modes[_currentDialModeIndex[dialIndex]];

                var args = new InputEventArgs($"Tablet_Dial{dialIndex}_Click");
                ButtonPressed?.Invoke(this, args);

                // Also fire a dial event with the new mode
                var dialArgs = new DialEventArgs
                {
                    InputId = $"Tablet_Dial{dialIndex}",
                    CurrentMode = newMode,
                    Delta = 0
                };
                DialRotated?.Invoke(this, dialArgs);
            }
        }

        /// <summary>
        /// Handle dial rotation
        /// </summary>
        public void OnDialRotate(int dialIndex, int delta)
        {
            var currentMode = GetCurrentDialMode(dialIndex);
            var args = new DialEventArgs
            {
                InputId = $"Tablet_Dial{dialIndex}",
                Delta = delta,
                CurrentMode = currentMode
            };
            DialRotated?.Invoke(this, args);
        }

        /// <summary>
        /// Handle pen movement with pressure
        /// </summary>
        public void OnPenMove(double x, double y, float pressure, float tiltX = 0, float tiltY = 0, bool isEraser = false)
        {
            var args = new PointerEventArgs
            {
                InputId = isEraser ? "Pen_Eraser" : "Pen_Tip",
                X = x,
                Y = y,
                Pressure = pressure,
                TiltX = tiltX,
                TiltY = tiltY,
                IsEraser = isEraser
            };
            PenMoved?.Invoke(this, args);
        }

        /// <summary>
        /// Get current mode for a dial
        /// </summary>
        public string GetCurrentDialMode(int dialIndex)
        {
            if (_dialModes.ContainsKey(dialIndex) && _currentDialModeIndex.ContainsKey(dialIndex))
            {
                return _dialModes[dialIndex][_currentDialModeIndex[dialIndex]];
            }
            return "Unknown";
        }

        /// <summary>
        /// Set dial modes
        /// </summary>
        public void SetDialModes(int dialIndex, string[] modes)
        {
            _dialModes[dialIndex] = modes;
            _currentDialModeIndex[dialIndex] = 0;
        }

        #region Wintab API Declarations (TODO: Implement)

        // TODO: Add Wintab32.dll P/Invoke declarations
        // [DllImport("Wintab32.dll")]
        // private static extern IntPtr WTOpen(IntPtr hWnd, IntPtr lpLogCtx, bool fEnable);

        #endregion
    }
}
