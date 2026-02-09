using System.Windows;
using System.Windows.Controls;
using StylusCore.App.Features.Settings.ViewModels;

namespace StylusCore.App.Features.Settings.Views
{
    /// <summary>
    /// Settings view for application configuration.
    /// </summary>
    public partial class SettingsView : Page
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView()
        {
            InitializeComponent();

            _viewModel = App.SettingsViewModel;
            DataContext = _viewModel;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveSettings();
            MessageBox.Show("Settings saved!", "StylusCore", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetBindings_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Reset all key bindings to defaults?",
                "Reset Bindings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.ResetBindingsToDefault();
            }
        }
    }
}
