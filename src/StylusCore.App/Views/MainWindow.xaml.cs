using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using StylusCore.App.Shell.ViewModels;
using StylusCore.Engine.Wpf.Input.Bindings;
using StylusCore.App.Shared.Components;
using StylusCore.App.Features.Library.Views;
using StylusCore.App.Features.Settings.Views;
using StylusCore.App.Features.Editor.Views;

namespace StylusCore.App.Views
{
    /// <summary>
    /// Main application window.
    /// Clean design like Nebo - no menu bar, just sidebar and content.
    /// Menu/toolbar will appear inside NotebookView when editing.
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = App.MainViewModel;
            DataContext = _viewModel;

            // Navigate to Library view on startup
            MainContentFrame.Navigate(new LibraryView());

            // Subscribe to events
            Closing += MainWindow_Closing;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            PreviewKeyUp += MainWindow_PreviewKeyUp;

            // Subscribe to ViewModel navigation requests (Breadcrumb)
            _viewModel.NavigationRequested += ViewModel_NavigationRequested;

            // Wire up Sidebar
            Sidebar.NavigationRequested += Sidebar_NavigationRequested;
            Sidebar.ToggleRequested += Sidebar_ToggleRequested;
        }

        private void Sidebar_ToggleRequested(object sender, EventArgs e)
        {
            // Sidebar genişliğine göre column'u güncelle
            SidebarColumn.Width = new System.Windows.GridLength(Sidebar.Width);
        }

        private void ViewModel_NavigationRequested(object sender, string destination)
        {
            if (destination == "LibraryView")
            {
                // Go to root library list
                if (!(MainContentFrame.Content is LibraryView))
                {
                    MainContentFrame.Navigate(new LibraryView());
                }
                else
                {
                    MainContentFrame.Navigate(new LibraryView());
                }
            }
            else if (destination == "NotebookList")
            {
                // Ensure we are on LibraryView
                if (!(MainContentFrame.Content is LibraryView))
                {
                    MainContentFrame.Navigate(new LibraryView());
                }
                else
                {
                    // Force refresh/update state
                    (MainContentFrame.Content as LibraryView)?.UpdateViewState();
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // Logic: If click is on the window background (not handled by controls), allow drag.
                try
                {
                    this.DragMove();
                }
                catch
                {
                    // DragMove can throw if called while not pressed, double safety.
                }
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // Fix for Maximize Bug
                this.MaxHeight = SystemParameters.WorkArea.Height;
            }
            else
            {
                this.MaxHeight = double.PositiveInfinity;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cleanup
            _viewModel.Shutdown();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Route keyboard input to device handler
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // Route keyboard input to device handler
        }

        /// <summary>
        /// Navigate to Notebook editor view
        /// </summary>
        public void NavigateToNotebook(StylusCore.Core.Models.Notebook notebook)
        {
            var editorView = new EditorView();
            editorView.LoadNotebook(notebook);
            MainContentFrame.Navigate(editorView);
            // Ribbon now lives inside EditorView - no visibility toggle needed
        }

        /// <summary>
        /// Navigate to Library view
        /// </summary>
        public void NavigateToLibrary()
        {
            // Clear ViewModel State
            if (_viewModel != null)
            {
                _viewModel.CurrentNotebook = null;
            }

            // Navigate (Ribbon automatically disappears since it's inside EditorView)
            MainContentFrame.Navigate(new LibraryView());
        }

        /// <summary>
        /// Navigate to Settings view
        /// </summary>
        public void NavigateToSettings()
        {
            MainContentFrame.Navigate(new SettingsView());
        }

        private void Sidebar_NavigationRequested(object sender, string destination)
        {
            switch (destination)
            {
                case "Library":
                    NavigateToLibrary();
                    break;
                case "Settings":
                    NavigateToSettings();
                    break;
            }
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
