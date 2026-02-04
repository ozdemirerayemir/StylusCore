using System;
using System.Windows;
using StylusCore.App.ViewModels;
using StylusCore.App.Core.ViewModels;

namespace StylusCore.App
{
    /// <summary>
    /// Application entry point and global state management.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Main ViewModel shared across the application
        /// </summary>
        public static MainViewModel MainViewModel { get; private set; }

        /// <summary>
        /// Global Theme Service
        /// </summary>
        public static Services.ThemeService ThemeService { get; private set; }

        /// <summary>
        /// Global Language Service
        /// </summary>
        public static Services.LanguageService LanguageService { get; private set; }

        /// <summary>
        /// Global Settings ViewModel
        /// </summary>
        public static SettingsViewModel SettingsViewModel { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Catch non-UI thread exceptions fatal errors
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show($"FATAL ERROR (Non-UI):\n{ex?.Message}\n\n{ex?.StackTrace}", "Fatal Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            base.OnStartup(e);

            // Add global exception handler for debugging
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            try 
            {
                // Initialize Services (Resources first)
                ThemeService = new Services.ThemeService();
                LanguageService = new Services.LanguageService();
                
                // Initialize ViewModels
                MainViewModel = new MainViewModel();
                SettingsViewModel = new SettingsViewModel();

                // Load settings
                SettingsViewModel.LoadSettings();

                // Apply default theme and language
                ThemeService.Initialize();
                LanguageService.Initialize();

                // Subscribe to settings changes
                SettingsViewModel.ThemeChanged += (s, theme) => 
                {
                   ThemeService.SetTheme(theme == "Dark" ? Services.ThemeService.AppTheme.Dark : Services.ThemeService.AppTheme.Light);
                };

                // Initialize devices and bindings
                MainViewModel.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup Error:\n{ex.Message}\n\n{ex.StackTrace}", "Startup Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private bool _isShowingError = false;

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; // Prevent crash

            if (_isShowingError) return;

            _isShowingError = true;

            try
            {
                string errorMessage = $"An unexpected error occurred:\n{e.Exception.Message}\n\nSource: {e.Exception.Source}";
                MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isShowingError = false;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Save settings
            SettingsViewModel.SaveSettings();

            // Shutdown devices
            MainViewModel.Shutdown();

            base.OnExit(e);
        }


    }
}
