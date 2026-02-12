using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StylusCore.Core.Enums;
using StylusCore.Core.Models;
using StylusCore.Core.Services;
using StylusCore.Engine.Wpf.Input.Bindings;
using StylusCore.Engine.Wpf.Input.Devices;

namespace StylusCore.App.Shell.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application.
    /// Manages global state, input mode, and view navigation.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Services

        private readonly IStorageService _storageService;
        private readonly IAutoSaveService _autoSaveService;
        private readonly BindingManager _bindingManager;
        private readonly TabletDevice _tabletDevice;
        private readonly KeyboardDevice _keyboardDevice;
        private readonly MouseDevice _mouseDevice;

        #endregion

        #region Properties

        private InputMode _currentInputMode;
        /// <summary>
        /// Current input mode (Keyboard+Mouse or Graphics Tablet)
        /// </summary>
        public InputMode CurrentInputMode
        {
            get => _currentInputMode;
            set
            {
                if (_currentInputMode != value)
                {
                    _currentInputMode = value;
                    OnPropertyChanged();
                    InputModeChanged?.Invoke(this, value);
                }
            }
        }

        private ToolMode _currentToolMode;
        /// <summary>
        /// Current drawing tool
        /// </summary>
        public ToolMode CurrentToolMode
        {
            get => _currentToolMode;
            set
            {
                if (_currentToolMode != value)
                {
                    _currentToolMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _currentTheme;
        /// <summary>
        /// Current theme (Light/Dark)
        /// </summary>
        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        private Library _currentLibrary;
        /// <summary>
        /// Currently selected library
        /// </summary>
        public Library CurrentLibrary
        {
            get => _currentLibrary;
            set
            {
                _currentLibrary = value;
                OnPropertyChanged();
            }
        }

        private Notebook _currentNotebook;
        /// <summary>
        /// Currently open notebook
        /// </summary>
        public Notebook CurrentNotebook
        {
            get => _currentNotebook;
            set
            {
                _currentNotebook = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditorActive));
                OnPropertyChanged(nameof(IsRibbonVisible));
                OnPropertyChanged(nameof(IsBookTextVisible));
                NotebookChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// True if a notebook is currently open (Editor Mode)
        /// </summary>
        public bool IsEditorActive => CurrentNotebook != null;

        private bool _isSidebarExpanded;
        /// <summary>
        /// Whether sidebar is expanded
        /// </summary>
        public bool IsSidebarExpanded
        {
            get => _isSidebarExpanded;
            set
            {
                _isSidebarExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBookTextVisible));
            }
        }

        public bool IsBookTextVisible => IsEditorActive && IsSidebarExpanded;

        private RibbonMode _ribbonMode;
        /// <summary>
        /// Current ribbon display mode
        /// </summary>
        public RibbonMode RibbonMode
        {
            get => _ribbonMode;
            set
            {
                _ribbonMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRibbonVisible));
                OnPropertyChanged(nameof(IsRibbonFull));
                OnPropertyChanged(nameof(IsRibbonTabsOnly));
            }
        }

        public bool IsRibbonVisible => IsEditorActive && RibbonMode != RibbonMode.Hidden;
        public bool IsRibbonFull => RibbonMode == RibbonMode.Full;
        public bool IsRibbonTabsOnly => RibbonMode == RibbonMode.TabsOnly;

        #endregion

        #region Events

        public event EventHandler<InputMode> InputModeChanged;
        public event EventHandler<string> ThemeChanged;
        public event EventHandler<Notebook> NotebookChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public MainViewModel()
        {
            // Initialize services
            _storageService = new StorageService();
            _autoSaveService = new AutoSaveService(_storageService);
            _bindingManager = new BindingManager();

            // Initialize devices
            _tabletDevice = new TabletDevice();
            _keyboardDevice = new KeyboardDevice();
            _mouseDevice = new MouseDevice();

            // Set defaults
            CurrentInputMode = InputMode.KeyboardMouse;
            CurrentToolMode = ToolMode.Pen;
            CurrentTheme = "Light";
            IsSidebarExpanded = true;
            RibbonMode = RibbonMode.Full;

            // Load bindings
            _bindingManager.LoadBindings();

            // Connect binding manager to action handler
            _bindingManager.ActionTriggered += OnActionTriggered;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the application
        /// </summary>
        public void Initialize()
        {
            // Initialize devices
            _keyboardDevice.Initialize();
            _mouseDevice.Initialize();
            _tabletDevice.Initialize();

            // Wire up device events to binding manager
            _keyboardDevice.ButtonPressed += (s, e) => _bindingManager.HandleInput(e);
            _mouseDevice.ButtonPressed += (s, e) => _bindingManager.HandleInput(e);
            _tabletDevice.ButtonPressed += (s, e) => _bindingManager.HandleInput(e);
        }

        /// <summary>
        /// Handle action from binding manager
        /// </summary>
        private void OnActionTriggered(object sender, ActionEventArgs e)
        {
            switch (e.Action)
            {
                case ActionTypes.SetInputMode:
                    // Toggle or set specific mode
                    CurrentInputMode = CurrentInputMode == InputMode.KeyboardMouse
                        ? InputMode.GraphicsTablet
                        : InputMode.KeyboardMouse;
                    break;

                case ActionTypes.SetToolMode:
                    if (e.Parameters.TryGetValue("mode", out var mode))
                    {
                        if (Enum.TryParse<ToolMode>(mode, out var toolMode))
                        {
                            CurrentToolMode = toolMode;
                        }
                    }
                    break;

                case ActionTypes.ToggleEraser:
                    CurrentToolMode = CurrentToolMode == ToolMode.Eraser
                        ? ToolMode.Pen
                        : ToolMode.Eraser;
                    break;
            }
        }

        /// <summary>
        /// Toggle sidebar visibility
        /// </summary>
        public void ToggleSidebar()
        {
            IsSidebarExpanded = !IsSidebarExpanded;
        }

        /// <summary>
        /// Toggle theme
        /// </summary>
        public void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == "Light" ? "Dark" : "Light";
        }

        /// <summary>
        /// Shutdown the application
        /// </summary>


        /// <summary>
        /// Shutdown the application
        /// </summary>
        public void Shutdown()
        {
            _autoSaveService.Stop();
            _tabletDevice.Shutdown();
            _keyboardDevice.Shutdown();
            _mouseDevice.Shutdown();
            _bindingManager.SaveBindings();
        }

        /// <summary>
        /// Global Window Commands
        /// </summary>
        public RelayCommand MinimizeCommand => new RelayCommand(o =>
            System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized);

        public RelayCommand MaximizeCommand => new RelayCommand(o =>
        {
            var win = System.Windows.Application.Current.MainWindow;
            win.WindowState = win.WindowState == System.Windows.WindowState.Maximized
                ? System.Windows.WindowState.Normal
                : System.Windows.WindowState.Maximized;
        });

        public RelayCommand CloseCommand => new RelayCommand(o =>
            System.Windows.Application.Current.Shutdown());


        public class RelayCommand : System.Windows.Input.ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged
            {
                add { System.Windows.Input.CommandManager.RequerySuggested += value; }
                remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
            }
        }

        /// <summary>
        /// Breadcrumb Navigation Commands
        /// </summary>
        public RelayCommand NavigateToLibraryRootCommand => new RelayCommand(o =>
        {
            CurrentNotebook = null;
            CurrentLibrary = null;
            // MainWindow will sniff this change via property change or we can fire an event
            // But better: let's expose an event relevant for Frame navigation
            NavigationRequested?.Invoke(this, "LibraryView");
        });

        public RelayCommand NavigateToNotebookListCommand => new RelayCommand(o =>
        {
            CurrentNotebook = null;
            // Ensure library is still set, just clear notebook to go back to list
            NavigationRequested?.Invoke(this, "NotebookList");
        });

        /// <summary>
        /// Command to go back to Book List (Level 2)
        /// Requested by user prompt. Same logic as NavigateToNotebookListCommand.
        /// </summary>
        public RelayCommand MapsToBookListCommand => new RelayCommand(o =>
        {
            CurrentNotebook = null;
            // CRITICAL: Keep SelectedLibrary as is.
            NavigationRequested?.Invoke(this, "NotebookList");
        });

        /// <summary>
        /// Cycle through Ribbon modes: Full -> TabsOnly -> Hidden -> Full
        /// </summary>
        public RelayCommand CycleRibbonModeCommand => new RelayCommand(o =>
        {
            switch (RibbonMode)
            {
                case RibbonMode.Full:
                    RibbonMode = RibbonMode.TabsOnly;
                    break;
                case RibbonMode.TabsOnly:
                    RibbonMode = RibbonMode.Hidden;
                    break;
                case RibbonMode.Hidden:
                    RibbonMode = RibbonMode.Full;
                    break;
                default:
                    RibbonMode = RibbonMode.Full;
                    break;
            }
        });

        public event EventHandler<string> NavigationRequested;


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
