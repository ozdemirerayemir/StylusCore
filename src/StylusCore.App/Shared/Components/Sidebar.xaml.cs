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

        // Sabit değerler
        private const double CollapsedWidth = 48;
        private const double ExpandedWidth = 198;

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

            // BookText visibility only when BookButton is visible
            if (BookButton.Visibility == Visibility.Visible)
            {
                BookText.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
            }
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

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(this, "Book");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(this, "Settings");
        }

        /// <summary>
        /// Show or hide the Book navigation item based on active surface
        /// </summary>
        public void SetBookButtonVisible(bool isVisible)
        {
            BookButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            if (_isExpanded)
            {
                BookText.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
