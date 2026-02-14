using System;
using System.Windows;
using System.Windows.Controls;

namespace StylusCore.App.Shared.Components
{
    public partial class Sidebar : UserControl
    {
        // Events
        public event EventHandler<string> NavigationRequested;
        public event EventHandler ToggleRequested;

        // Width values read from resource tokens (GeneralResources.xaml)
        private double CollapsedWidth => (double)FindResource("Shell.SidebarCollapsedWidth");
        private double ExpandedWidth => (double)FindResource("Shell.SidebarExpandedWidth");

        // State
        private bool _isExpanded = false;

        public Sidebar()
        {
            InitializeComponent();
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    ApplyState();
                }
            }
        }

        private void ApplyState()
        {
            // Width güncelle
            this.Width = _isExpanded ? ExpandedWidth : CollapsedWidth;

            // Text visibility güncelle
            LibraryText.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
            SettingsText.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            ToggleRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Library_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(this, "Library");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(this, "Settings");
        }
    }
}
