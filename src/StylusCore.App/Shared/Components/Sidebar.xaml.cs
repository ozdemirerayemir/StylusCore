using System;
using System.Windows;
using System.Windows.Controls;

namespace StylusCore.App.Shared.Components
{
    /// <summary>
    /// Fixed Rail Sidebar Control.
    /// </summary>
    public partial class Sidebar : UserControl
    {
        public event EventHandler<string> NavigationRequested;
        public event EventHandler ToggleRequested; 

        public Sidebar()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Sidebar), new PropertyMetadata(false));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            ToggleRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(this, "Library");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(this, "Settings");
        }
    }
}
