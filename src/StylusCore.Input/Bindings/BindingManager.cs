using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using StylusCore.Input.Devices;

namespace StylusCore.Input.Bindings
{
    /// <summary>
    /// Manages all key bindings and handles input routing.
    /// </summary>
    public class BindingManager
    {
        private List<KeyBinding> _bindings;
        private readonly string _bindingsFilePath;

        /// <summary>
        /// Event fired when an action should be executed
        /// </summary>
        public event EventHandler<ActionEventArgs> ActionTriggered;

        /// <summary>
        /// Event fired when a binding conflict is detected
        /// </summary>
        public event EventHandler<BindingConflictEventArgs> ConflictDetected;

        public BindingManager()
        {
            _bindings = new List<KeyBinding>();
            _bindingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StylusCore",
                "data",
                "keybindings.json"
            );
        }

        public BindingManager(string bindingsFilePath) : this()
        {
            _bindingsFilePath = bindingsFilePath;
        }

        /// <summary>
        /// Load bindings from file
        /// </summary>
        public void LoadBindings()
        {
            if (File.Exists(_bindingsFilePath))
            {
                var json = File.ReadAllText(_bindingsFilePath);
                _bindings = JsonSerializer.Deserialize<List<KeyBinding>>(json) ?? new List<KeyBinding>();
            }
            else
            {
                LoadDefaultBindings();
            }
        }

        /// <summary>
        /// Save bindings to file
        /// </summary>
        public void SaveBindings()
        {
            var directory = Path.GetDirectoryName(_bindingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_bindings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_bindingsFilePath, json);
        }

        /// <summary>
        /// Load default bindings
        /// </summary>
        public void LoadDefaultBindings()
        {
            _bindings = new List<KeyBinding>
            {
                // Tablet buttons
                new KeyBinding("Tablet_Button1", ActionTypes.OpenRadialMenu) { Parameters = { { "menuId", "main_mode" } } },
                new KeyBinding("Tablet_Button2", ActionTypes.OpenRadialMenu) { Parameters = { { "menuId", "pen_tools" } } },
                new KeyBinding("Tablet_Button3", ActionTypes.OpenRadialMenu) { Parameters = { { "menuId", "shapes" } } },
                new KeyBinding("Tablet_Button4", ActionTypes.OpenRadialMenu) { Parameters = { { "menuId", "colors" } } },

                // Pen buttons
                new KeyBinding("Pen_Button1", ActionTypes.Pan),
                new KeyBinding("Pen_Button2", ActionTypes.ToggleEraser),

                // Keyboard shortcuts
                new KeyBinding("Key_Z", ActionTypes.Undo) { RequiresCtrl = true },
                new KeyBinding("Key_Y", ActionTypes.Redo) { RequiresCtrl = true },
                new KeyBinding("Key_S", ActionTypes.Save) { RequiresCtrl = true },
            };
        }

        /// <summary>
        /// Add a new binding
        /// </summary>
        public void AddBinding(KeyBinding binding)
        {
            // Check for conflicts
            var conflicts = CheckConflicts(binding);
            if (conflicts.Any())
            {
                ConflictDetected?.Invoke(this, new BindingConflictEventArgs(binding, conflicts));
            }

            _bindings.Add(binding);
        }

        /// <summary>
        /// Remove a binding
        /// </summary>
        public void RemoveBinding(Guid bindingId)
        {
            _bindings.RemoveAll(b => b.Id == bindingId);
        }

        /// <summary>
        /// Update an existing binding
        /// </summary>
        public void UpdateBinding(KeyBinding binding)
        {
            var index = _bindings.FindIndex(b => b.Id == binding.Id);
            if (index >= 0)
            {
                _bindings[index] = binding;
            }
        }

        /// <summary>
        /// Get all bindings
        /// </summary>
        public IReadOnlyList<KeyBinding> GetAllBindings()
        {
            return _bindings.AsReadOnly();
        }

        /// <summary>
        /// Get bindings for a specific input
        /// </summary>
        public IEnumerable<KeyBinding> GetBindingsForInput(string inputId)
        {
            return _bindings.Where(b => b.InputId == inputId);
        }

        /// <summary>
        /// Check for conflicting bindings
        /// </summary>
        public List<KeyBinding> CheckConflicts(KeyBinding newBinding)
        {
            return _bindings
                .Where(b => b.InputId == newBinding.InputId &&
                            b.RequiresCtrl == newBinding.RequiresCtrl &&
                            b.RequiresShift == newBinding.RequiresShift &&
                            b.RequiresAlt == newBinding.RequiresAlt &&
                            b.Id != newBinding.Id)
                .ToList();
        }

        /// <summary>
        /// Handle input from any device
        /// </summary>
        public void HandleInput(InputEventArgs args)
        {
            var matchingBindings = _bindings
                .Where(b => b.Matches(args.InputId, args.IsCtrlPressed, args.IsShiftPressed, args.IsAltPressed))
                .OrderByDescending(b => b.Priority)
                .ToList();

            if (matchingBindings.Any())
            {
                var binding = matchingBindings.First();
                ActionTriggered?.Invoke(this, new ActionEventArgs(binding.Action, binding.Parameters));
            }
        }
    }

    /// <summary>
    /// Event args for action execution
    /// </summary>
    public class ActionEventArgs : EventArgs
    {
        public string Action { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public ActionEventArgs(string action, Dictionary<string, string> parameters)
        {
            Action = action;
            Parameters = parameters ?? new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Event args for binding conflicts
    /// </summary>
    public class BindingConflictEventArgs : EventArgs
    {
        public KeyBinding NewBinding { get; set; }
        public List<KeyBinding> ConflictingBindings { get; set; }

        public BindingConflictEventArgs(KeyBinding newBinding, List<KeyBinding> conflicts)
        {
            NewBinding = newBinding;
            ConflictingBindings = conflicts;
        }
    }
}
