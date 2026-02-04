using System;
using System.Linq;
using System.Windows;

namespace StylusCore.App.Services
{
    public class LanguageService
    {
        public enum AppLanguage
        {
            English,
            Turkish
        }

        private AppLanguage _currentLanguage;
        public AppLanguage CurrentLanguage => _currentLanguage;

        public event EventHandler<AppLanguage> LanguageChanged;

        public void SetLanguage(AppLanguage language)
        {
            // Determine dictionary URI
            string langUri = language == AppLanguage.English 
                ? "Resources/Languages/LanguageResources.en-US.xaml" 
                : "Resources/Languages/LanguageResources.tr-TR.xaml";

            // Load new dictionary
            var newDict = new ResourceDictionary { Source = new Uri(langUri, UriKind.Relative) };

            // Find and remove old language dictionary
            var oldDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.ToString().Contains("LanguageResources"));

            if (oldDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            // Add new dictionary
            Application.Current.Resources.MergedDictionaries.Add(newDict);

            _currentLanguage = language;
            LanguageChanged?.Invoke(this, _currentLanguage);
        }

        public void Initialize()
        {
            // Default to English or load from settings
            SetLanguage(AppLanguage.English);
        }
        
        public void ToggleLanguage()
        {
             SetLanguage(_currentLanguage == AppLanguage.English ? AppLanguage.Turkish : AppLanguage.English);
        }
    }
}
