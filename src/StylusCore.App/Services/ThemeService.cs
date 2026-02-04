using System;
using System.Linq;
using System.Windows;

namespace StylusCore.App.Services
{
    public class ThemeService
    {
        public enum AppTheme
        {
            Light,
            Dark
        }

        private AppTheme _currentTheme;
        public AppTheme CurrentTheme => _currentTheme;

        public event EventHandler<AppTheme> ThemeChanged;

        public void SetTheme(AppTheme theme)
        {
            if (_currentTheme == theme) return;

            // Determine dictionary URI using Pack URI syntax with Assembly Name (Safety)
            string themeUri = theme == AppTheme.Light 
                ? "pack://application:,,,/StylusCore;component/Shared/Themes/LightTheme.xaml" 
                : "pack://application:,,,/StylusCore;component/Shared/Themes/DarkTheme.xaml";

            try 
            {
                // Load new dictionary
                var newDict = new ResourceDictionary { Source = new Uri(themeUri) };

                // Find and remove old theme dictionary
                var oldDict = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && (d.Source.ToString().Contains("LightTheme.xaml") || d.Source.ToString().Contains("DarkTheme.xaml")));

                if (oldDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                }

                // Add new dictionary
                Application.Current.Resources.MergedDictionaries.Add(newDict);

                _currentTheme = theme;
                ThemeChanged?.Invoke(this, _currentTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing theme to {theme}: {ex.Message}");
                Console.WriteLine($"Error changing theme to {theme}: {ex.Message}");
            }
        }

        public void Initialize()
        {
            // Default to Light or load from settings
            SetTheme(AppTheme.Light);
        }
    }
}
