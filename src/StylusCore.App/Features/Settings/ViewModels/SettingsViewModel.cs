using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using StylusCore.Core.Enums;
using StylusCore.Engine.Wpf.Input.Bindings;

namespace StylusCore.App.Features.Settings.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings view.
    /// Manages application settings, themes, language, and key bindings.
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly string _settingsFilePath;
        private readonly BindingManager _bindingManager;
        private readonly RadialMenuBinding _radialMenuBinding;

        #region Settings Properties

        private string _theme;
        /// <summary>
        /// Current theme (Light/Dark)
        /// </summary>
        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged();
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        private string _language;
        /// <summary>
        /// Current language code (en, tr)
        /// </summary>
        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged();
                    LanguageChanged?.Invoke(this, value);
                }
            }
        }

        private int _autoSaveInterval;
        /// <summary>
        /// Auto-save interval in seconds
        /// </summary>
        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set
            {
                _autoSaveInterval = Math.Max(10, Math.Min(600, value));
                OnPropertyChanged();
            }
        }

        private RibbonMode _ribbonMode;
        /// <summary>
        /// Ribbon display mode
        /// </summary>
        public RibbonMode RibbonMode
        {
            get => _ribbonMode;
            set
            {
                _ribbonMode = value;
                OnPropertyChanged();
            }
        }

        private string _radialMenuSize;
        /// <summary>
        /// Radial menu size (small/medium/large)
        /// </summary>
        public string RadialMenuSize
        {
            get => _radialMenuSize;
            set
            {
                _radialMenuSize = value;
                OnPropertyChanged();
            }
        }

        private RadialMenuActivation _radialMenuActivation;
        /// <summary>
        /// Radial menu activation mode
        /// </summary>
        public RadialMenuActivation RadialMenuActivation
        {
            get => _radialMenuActivation;
            set
            {
                _radialMenuActivation = value;
                OnPropertyChanged();
            }
        }

        private bool _showSaveIndicator;
        /// <summary>
        /// Whether to show save indicator
        /// </summary>
        public bool ShowSaveIndicator
        {
            get => _showSaveIndicator;
            set
            {
                _showSaveIndicator = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// All key bindings
        /// </summary>
        public IReadOnlyList<KeyBinding> KeyBindings => _bindingManager.GetAllBindings();

        /// <summary>
        /// All radial menus
        /// </summary>
        public IEnumerable<RadialMenuConfig> RadialMenus => _radialMenuBinding.GetAllMenus();

        /// <summary>
        /// Available languages
        /// </summary>
        public List<LanguageOption> AvailableLanguages { get; }

        /// <summary>
        /// Available themes
        /// </summary>
        public List<string> AvailableThemes { get; }

        #endregion

        #region Events

        public event EventHandler<string> ThemeChanged;
        public event EventHandler<string> LanguageChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public SettingsViewModel()
        {
            _settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StylusCore",
                "data",
                "settings.json"
            );

            _bindingManager = new BindingManager();
            _radialMenuBinding = new RadialMenuBinding();

            // Available options
            AvailableLanguages = new List<LanguageOption>
            {
                new LanguageOption { Code = "en", Name = "English" },
                new LanguageOption { Code = "tr", Name = "Türkçe" }
            };

            AvailableThemes = new List<string> { "Light", "Dark" };

            // Set defaults
            Theme = "Light";
            Language = "en";
            AutoSaveInterval = 60;
            RibbonMode = RibbonMode.Full;
            RadialMenuSize = "medium";
            RadialMenuActivation = RadialMenuActivation.Click;
            ShowSaveIndicator = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load settings from file
        /// </summary>
        public void LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    if (settings != null)
                    {
                        Theme = settings.Theme ?? "Light";
                        Language = settings.Language ?? "en";
                        AutoSaveInterval = settings.AutoSaveInterval > 0 ? settings.AutoSaveInterval : 60;
                        ShowSaveIndicator = settings.ShowSaveIndicator;
                    }
                }
                catch (Exception)
                {
                    // Use defaults if loading fails
                }
            }

            _bindingManager.LoadBindings();
        }

        /// <summary>
        /// Save settings to file
        /// </summary>
        public void SaveSettings()
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var settings = new SettingsData
            {
                Theme = Theme,
                Language = Language,
                AutoSaveInterval = AutoSaveInterval,
                ShowSaveIndicator = ShowSaveIndicator
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);

            _bindingManager.SaveBindings();
        }

        /// <summary>
        /// Update a key binding
        /// </summary>
        public void UpdateKeyBinding(KeyBinding binding)
        {
            var conflicts = _bindingManager.CheckConflicts(binding);
            if (conflicts.Count > 0)
            {
                // Notify about conflicts
            }
            _bindingManager.UpdateBinding(binding);
            OnPropertyChanged(nameof(KeyBindings));
        }

        /// <summary>
        /// Add a new key binding
        /// </summary>
        public void AddKeyBinding(KeyBinding binding)
        {
            _bindingManager.AddBinding(binding);
            OnPropertyChanged(nameof(KeyBindings));
        }

        /// <summary>
        /// Remove a key binding
        /// </summary>
        public void RemoveKeyBinding(Guid bindingId)
        {
            _bindingManager.RemoveBinding(bindingId);
            OnPropertyChanged(nameof(KeyBindings));
        }

        /// <summary>
        /// Reset all bindings to defaults
        /// </summary>
        public void ResetBindingsToDefault()
        {
            _bindingManager.LoadDefaultBindings();
            OnPropertyChanged(nameof(KeyBindings));
        }

        /// <summary>
        /// Update a radial menu
        /// </summary>
        public void UpdateRadialMenu(RadialMenuConfig menu)
        {
            _radialMenuBinding.SetMenu(menu);
            OnPropertyChanged(nameof(RadialMenus));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region Helper Classes

    public class LanguageOption
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class SettingsData
    {
        public string Theme { get; set; }
        public string Language { get; set; }
        public int AutoSaveInterval { get; set; }
        public bool ShowSaveIndicator { get; set; }
    }

    #endregion
}
