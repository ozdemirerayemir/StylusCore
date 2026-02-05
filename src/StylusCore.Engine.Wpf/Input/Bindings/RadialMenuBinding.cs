using System;
using System.Collections.Generic;
using StylusCore.Core.Enums;

namespace StylusCore.Engine.Wpf.Input.Bindings
{
    /// <summary>
    /// Represents a radial menu with its items and configuration.
    /// </summary>
    public class RadialMenuConfig
    {
        /// <summary>
        /// Unique identifier for the menu
        /// </summary>
        public string MenuId { get; set; }

        /// <summary>
        /// Display name of the menu
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Items in the menu
        /// </summary>
        public List<RadialMenuItem> Items { get; set; }

        /// <summary>
        /// Menu radius in pixels
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Inner radius for center area
        /// </summary>
        public double InnerRadius { get; set; }

        /// <summary>
        /// Activation mode (Click or Hold)
        /// </summary>
        public RadialMenuActivation ActivationMode { get; set; }

        /// <summary>
        /// Whether to constrain cursor within menu
        /// </summary>
        public bool ConstrainCursor { get; set; }

        public RadialMenuConfig()
        {
            MenuId = Guid.NewGuid().ToString();
            Name = "New Menu";
            Items = new List<RadialMenuItem>();
            Radius = 120;
            InnerRadius = 40;
            ActivationMode = RadialMenuActivation.Click;
            ConstrainCursor = true; // Default: cursor locked in menu circle
        }
    }

    /// <summary>
    /// Represents a single item in a radial menu.
    /// </summary>
    public class RadialMenuItem
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Icon identifier or path
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Action to perform when selected
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Parameters for the action
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Position angle in degrees (0 = right, 90 = top)
        /// </summary>
        public double AnglePosition { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Foreground/text color
        /// </summary>
        public string ForegroundColor { get; set; }

        /// <summary>
        /// Sub-menu ID (if this item opens another menu)
        /// </summary>
        public string SubMenuId { get; set; }

        /// <summary>
        /// Whether this item is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        public RadialMenuItem()
        {
            Id = Guid.NewGuid();
            Label = string.Empty;
            Icon = string.Empty;
            Action = string.Empty;
            Parameters = new Dictionary<string, string>();
            AnglePosition = 0;
            BackgroundColor = "#3C3C3C";
            ForegroundColor = "#FFFFFF";
            SubMenuId = null;
            IsEnabled = true;
        }
    }

    /// <summary>
    /// Manages radial menu bindings and configurations.
    /// </summary>
    public class RadialMenuBinding
    {
        private Dictionary<string, RadialMenuConfig> _menus;
        private Dictionary<string, List<string>> _inputBindings; // InputId -> MenuIds

        public RadialMenuBinding()
        {
            _menus = new Dictionary<string, RadialMenuConfig>();
            _inputBindings = new Dictionary<string, List<string>>();

            // Initialize default menus
            CreateDefaultMenus();
        }

        /// <summary>
        /// Create default radial menus
        /// </summary>
        private void CreateDefaultMenus()
        {
            // Main mode menu
            var mainMenu = new RadialMenuConfig
            {
                MenuId = "main_mode",
                Name = "Mode Selection",
                Items = new List<RadialMenuItem>
                {
                    new RadialMenuItem { Label = "Keyboard", Action = ActionTypes.SetInputMode, AnglePosition = 0 },
                    new RadialMenuItem { Label = "Tablet", Action = ActionTypes.SetInputMode, AnglePosition = 180 }
                }
            };
            _menus[mainMenu.MenuId] = mainMenu;

            // Pen tools menu
            var penMenu = new RadialMenuConfig
            {
                MenuId = "pen_tools",
                Name = "Pen Tools",
                Items = new List<RadialMenuItem>
                {
                    new RadialMenuItem { Label = "Pen", Action = ActionTypes.SetToolMode, AnglePosition = 0 },
                    new RadialMenuItem { Label = "Highlighter", Action = ActionTypes.SetToolMode, AnglePosition = 90 },
                    new RadialMenuItem { Label = "Eraser", Action = ActionTypes.SetToolMode, AnglePosition = 180 },
                    new RadialMenuItem { Label = "Selection", Action = ActionTypes.SetToolMode, AnglePosition = 270 }
                }
            };
            _menus[penMenu.MenuId] = penMenu;

            // Shapes menu
            var shapesMenu = new RadialMenuConfig
            {
                MenuId = "shapes",
                Name = "Shapes",
                Items = new List<RadialMenuItem>
                {
                    new RadialMenuItem { Label = "Line", Action = ActionTypes.SetToolMode, AnglePosition = 0 },
                    new RadialMenuItem { Label = "Arrow", Action = ActionTypes.SetToolMode, AnglePosition = 60 },
                    new RadialMenuItem { Label = "Rectangle", Action = ActionTypes.SetToolMode, AnglePosition = 120 },
                    new RadialMenuItem { Label = "Circle", Action = ActionTypes.SetToolMode, AnglePosition = 180 },
                    new RadialMenuItem { Label = "Triangle", Action = ActionTypes.SetToolMode, AnglePosition = 240 },
                    new RadialMenuItem { Label = "Ruler", Action = ActionTypes.SetToolMode, AnglePosition = 300 }
                }
            };
            _menus[shapesMenu.MenuId] = shapesMenu;
        }

        /// <summary>
        /// Get a menu by ID
        /// </summary>
        public RadialMenuConfig GetMenu(string menuId)
        {
            return _menus.TryGetValue(menuId, out var menu) ? menu : null;
        }

        /// <summary>
        /// Get all menus
        /// </summary>
        public IEnumerable<RadialMenuConfig> GetAllMenus()
        {
            return _menus.Values;
        }

        /// <summary>
        /// Add or update a menu
        /// </summary>
        public void SetMenu(RadialMenuConfig menu)
        {
            _menus[menu.MenuId] = menu;
        }

        /// <summary>
        /// Remove a menu
        /// </summary>
        public void RemoveMenu(string menuId)
        {
            _menus.Remove(menuId);
        }

        /// <summary>
        /// Bind an input to a menu
        /// </summary>
        public void BindInputToMenu(string inputId, string menuId)
        {
            if (!_inputBindings.ContainsKey(inputId))
            {
                _inputBindings[inputId] = new List<string>();
            }
            if (!_inputBindings[inputId].Contains(menuId))
            {
                _inputBindings[inputId].Add(menuId);
            }
        }

        /// <summary>
        /// Get menus bound to an input
        /// </summary>
        public IEnumerable<string> GetMenusForInput(string inputId)
        {
            return _inputBindings.TryGetValue(inputId, out var menus) ? menus : Array.Empty<string>();
        }
    }
}
