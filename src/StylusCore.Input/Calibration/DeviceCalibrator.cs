using System;
using System.Collections.Generic;
using StylusCore.Input.Devices;

namespace StylusCore.Input.Calibration
{
    /// <summary>
    /// Handles device detection and calibration wizard.
    /// </summary>
    public class DeviceCalibrator
    {
        /// <summary>
        /// Event fired when calibration step changes
        /// </summary>
        public event EventHandler<CalibrationStepEventArgs> StepChanged;

        /// <summary>
        /// Event fired when calibration completes
        /// </summary>
        public event EventHandler<CalibrationResultEventArgs> CalibrationCompleted;

        private CalibrationState _state;
        private TabletDevice _tabletDevice;
        private Dictionary<string, string> _detectedInputs;
        private int _currentStep;
        private int _totalSteps;

        public DeviceCalibrator()
        {
            _state = CalibrationState.NotStarted;
            _detectedInputs = new Dictionary<string, string>();
            _currentStep = 0;
            _totalSteps = 10; // Button 1-6, Dial 1-2, Pen 1-2
        }

        /// <summary>
        /// Start the calibration wizard
        /// </summary>
        public void StartCalibration(TabletDevice device)
        {
            _tabletDevice = device;
            _state = CalibrationState.DetectingButtons;
            _currentStep = 0;
            _detectedInputs.Clear();

            // Subscribe to device events
            _tabletDevice.ButtonPressed += OnButtonPressed;

            // Start first step
            NextStep();
        }

        /// <summary>
        /// Skip current step
        /// </summary>
        public void SkipStep()
        {
            NextStep();
        }

        /// <summary>
        /// Cancel calibration
        /// </summary>
        public void Cancel()
        {
            Cleanup();
            _state = CalibrationState.Cancelled;
        }

        private void NextStep()
        {
            _currentStep++;

            if (_currentStep > _totalSteps)
            {
                Complete();
                return;
            }

            var stepInfo = GetStepInfo(_currentStep);
            StepChanged?.Invoke(this, new CalibrationStepEventArgs
            {
                CurrentStep = _currentStep,
                TotalSteps = _totalSteps,
                Instruction = stepInfo.instruction,
                InputName = stepInfo.inputName
            });
        }

        private (string instruction, string inputName) GetStepInfo(int step)
        {
            return step switch
            {
                1 => ("Press Button 1 on your tablet", "Tablet_Button1"),
                2 => ("Press Button 2 on your tablet", "Tablet_Button2"),
                3 => ("Press Button 3 on your tablet", "Tablet_Button3"),
                4 => ("Press Button 4 on your tablet", "Tablet_Button4"),
                5 => ("Press Button 5 on your tablet", "Tablet_Button5"),
                6 => ("Press Button 6 on your tablet", "Tablet_Button6"),
                7 => ("Rotate Dial 1 on your tablet", "Tablet_Dial1"),
                8 => ("Rotate Dial 2 on your tablet", "Tablet_Dial2"),
                9 => ("Press the first button on your pen", "Pen_Button1"),
                10 => ("Press the second button on your pen", "Pen_Button2"),
                _ => ("Unknown step", "Unknown")
            };
        }

        private void OnButtonPressed(object sender, InputEventArgs e)
        {
            var stepInfo = GetStepInfo(_currentStep);
            _detectedInputs[stepInfo.inputName] = e.InputId;
            NextStep();
        }

        private void Complete()
        {
            Cleanup();
            _state = CalibrationState.Completed;

            CalibrationCompleted?.Invoke(this, new CalibrationResultEventArgs
            {
                Success = true,
                DetectedInputs = new Dictionary<string, string>(_detectedInputs),
                DeviceName = _tabletDevice?.DeviceName ?? "Unknown",
                ButtonCount = 6,
                DialCount = 2,
                PenButtonCount = 2
            });
        }

        private void Cleanup()
        {
            if (_tabletDevice != null)
            {
                _tabletDevice.ButtonPressed -= OnButtonPressed;
            }
        }

        /// <summary>
        /// Auto-detect connected devices
        /// </summary>
        public static List<DetectedDevice> DetectDevices()
        {
            var devices = new List<DetectedDevice>();

            // TODO: Use Windows HID API to enumerate devices
            // For now, return mock data
            devices.Add(new DetectedDevice
            {
                DeviceId = "Tablet_1",
                DeviceName = "VEIKK VK1060 Pro",
                DeviceType = DeviceType.GraphicsTablet,
                IsConnected = true
            });

            return devices;
        }
    }

    public enum CalibrationState
    {
        NotStarted,
        DetectingButtons,
        DetectingDials,
        DetectingPen,
        Completed,
        Cancelled
    }

    public enum DeviceType
    {
        Keyboard,
        Mouse,
        GraphicsTablet,
        Unknown
    }

    public class DetectedDevice
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceType DeviceType { get; set; }
        public bool IsConnected { get; set; }
    }

    public class CalibrationStepEventArgs : EventArgs
    {
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string Instruction { get; set; }
        public string InputName { get; set; }
    }

    public class CalibrationResultEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public Dictionary<string, string> DetectedInputs { get; set; }
        public string DeviceName { get; set; }
        public int ButtonCount { get; set; }
        public int DialCount { get; set; }
        public int PenButtonCount { get; set; }
    }
}
