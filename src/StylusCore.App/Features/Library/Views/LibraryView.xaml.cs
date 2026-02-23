using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StylusCore.App.Shell.ViewModels;
using StylusCore.App.Features.Library.ViewModels;
using LibraryModel = StylusCore.Core.Models.Library;
using StylusCore.Core.Enums;
using StylusCore.App.Dialogs;
using StylusCore.App.Features.Library.Dialogs;
using StylusCore.Core.Models;
using StylusCore.App.Views;

namespace StylusCore.App.Features.Library.Views
{
    /// <summary>
    /// Library view for browsing libraries and notebooks.
    /// Supports two states: Library list view and Notebook list view (inside a library).
    /// </summary>
    public partial class LibraryView : System.Windows.Controls.Page
    {
        private readonly LibraryViewModel _viewModel;
        private bool _isViewingNotebooks = false;

        public LibraryView()
        {
            InitializeComponent();

            _viewModel = new LibraryViewModel();
            DataContext = _viewModel;

            _viewModel.NotebookOpenRequested += ViewModel_NotebookOpenRequested;

            Loaded += LibraryView_Loaded;
        }

        private async void LibraryView_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadLibrariesAsync();
            UpdateViewState();
        }

        /// <summary>
        /// Update UI based on current view state (libraries or notebooks)
        /// </summary>
        public void UpdateViewState()
        {
            if (_isViewingNotebooks && _viewModel.SelectedLibrary != null)
            {
                // Viewing notebooks inside a library
                PageTitle.Text = _viewModel.SelectedLibrary.Name;
                // BackButton.Visibility = Visibility.Visible; (Removed)
                NewLibraryButton.Visibility = Visibility.Collapsed;
                NewNotebookButton.Visibility = Visibility.Visible;

                LibrariesScrollViewer.Visibility = Visibility.Collapsed;
                NotebooksScrollViewer.Visibility = Visibility.Visible;
                EmptyLibrariesState.Visibility = Visibility.Collapsed;

                // Show empty state if no notebooks
                bool hasNotebooks = _viewModel.SelectedLibrary.Notebooks.Count > 0;
                NotebooksScrollViewer.Visibility = hasNotebooks ? Visibility.Visible : Visibility.Collapsed;
                EmptyNotebooksState.Visibility = hasNotebooks ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                // Viewing libraries
                PageTitle.Text = TryFindResource("Str_Library") as string ?? "Library";
                // BackButton.Visibility = Visibility.Collapsed; (Removed)
                NewLibraryButton.Visibility = Visibility.Visible;
                NewNotebookButton.Visibility = Visibility.Collapsed;

                NotebooksScrollViewer.Visibility = Visibility.Collapsed;
                EmptyNotebooksState.Visibility = Visibility.Collapsed;

                // Show empty state if no libraries
                bool hasLibraries = _viewModel.Libraries.Count > 0;
                LibrariesScrollViewer.Visibility = hasLibraries ? Visibility.Visible : Visibility.Collapsed;
                EmptyLibrariesState.Visibility = hasLibraries ? Visibility.Collapsed : Visibility.Visible;
            }
        }



        private async void NewLibrary_Click(object sender, RoutedEventArgs e)
        {
            // Show input dialog for library name
            var dialogTitle = TryFindResource("Str_CreateNewLibrary") as string ?? "Create New Library";
            var dialogPrompt = TryFindResource("Str_LibraryName") as string ?? "Library Name";
            var name = ShowInputDialog(dialogTitle, dialogPrompt);
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _viewModel.CreateLibraryAsync(name);
                UpdateViewState();
            }
        }

        private void NewNotebook_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedLibrary == null) return;

            // Show the new notebook creation dialog
            var dialog = new CreateNotebookDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.CreatedNotebook != null)
            {
                // Add the notebook to the current library
                var notebook = dialog.CreatedNotebook;
                notebook.LibraryId = _viewModel.SelectedLibrary.Id;
                _viewModel.SelectedLibrary.Notebooks.Add(notebook);
                UpdateViewState();
            }
        }

        private void LibraryCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is LibraryModel library)
            {
                _viewModel.SelectedLibrary = library;
                _isViewingNotebooks = true;
                UpdateViewState();

                // Breadcrumb: Set Global Library
                if (App.MainViewModel != null) App.MainViewModel.CurrentLibrary = library;
            }
        }

        private void NotebookCard_Click(object sender, MouseButtonEventArgs e)
        {
            // Fix: Prevent opening notebook if we clicked the context menu button
            if (e.OriginalSource is DependencyObject obj)
            {
                // Walk up to check if we clicked a button
                var current = obj;
                while (current != null && current != sender)
                {
                    if (current is Button) return; // Ignored clicked button
                    current = System.Windows.Media.VisualTreeHelper.GetParent(current);
                }
            }

            if (sender is FrameworkElement element && element.DataContext is Notebook notebook)
            {
                _viewModel.OpenNotebook(notebook);
            }
        }

        private void OpenNotebook_ContextClick(object sender, RoutedEventArgs e)
        {
            var notebook = GetNotebookFromMenuItem(sender);
            if (notebook != null)
            {
                _viewModel.OpenNotebook(notebook);
            }
        }

        private LibraryModel GetLibraryFromMenuItem(object sender)
        {
            if (sender is FrameworkElement element)
            {
                // Retrieve the library from the Menu/Button's Tag
                if (element.Tag is LibraryModel tagLibrary) return tagLibrary;

                // Retrieve the library from the ContextMenu PlacementTarget
                if (element is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
                {
                    if (contextMenu.PlacementTarget is FrameworkElement target && target.Tag is LibraryModel targetLibrary) return targetLibrary;
                }
            }
            return null;
        }

        private void RenameLibrary_Click(object sender, RoutedEventArgs e)
        {
            var library = GetLibraryFromMenuItem(sender);
            if (library == null) return;

            var dialogTitle = TryFindResource("Str_Rename") as string ?? "Rename";
            var dialogPrompt = TryFindResource("Str_NewName") as string ?? "New Name";
            var newName = ShowInputDialog(dialogTitle, dialogPrompt);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                library.Name = newName;
                UpdateViewState();
            }
        }

        private async void DeleteLibrary_Click(object sender, RoutedEventArgs e)
        {
            var library = GetLibraryFromMenuItem(sender);
            if (library == null) return;

            var result = MessageBox.Show(
               $"Are you sure you want to delete '{library.Name}' and all its notebooks?",
               "Delete Library",
               MessageBoxButton.YesNo,
               MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _viewModel.DeleteLibraryAsync(library);
                UpdateViewState();
            }
        }

        #region Context Menu Handlers

        private void DuplicateNotebook_Click(object sender, RoutedEventArgs e)
        {
            var notebook = GetNotebookFromMenuItem(sender);
            if (notebook == null || _viewModel.SelectedLibrary == null) return;

            var duplicate = new Notebook
            {
                Name = notebook.Name + " (Copy)",
                CoverColor = notebook.CoverColor,
                DefaultPageFormat = notebook.DefaultPageFormat,
                DefaultPageOrientation = notebook.DefaultPageOrientation,
                DefaultPageTemplate = notebook.DefaultPageTemplate,
                LibraryId = notebook.LibraryId
            };

            _viewModel.SelectedLibrary.Notebooks.Add(duplicate);
            UpdateViewState();
        }

        private void RenameNotebook_Click(object sender, RoutedEventArgs e)
        {
            var notebook = GetNotebookFromMenuItem(sender);
            if (notebook == null) return;

            var newName = ShowInputDialog("Rename Notebook", "Enter new name:");
            if (!string.IsNullOrWhiteSpace(newName))
            {
                notebook.Name = newName;
                // Force UI refresh
                UpdateViewState();
            }
        }

        private void NotebookOptions_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.ContextMenu.IsOpen = true;
                // Prevent the click from bubbling to the Card Click event
                e.Handled = true;
            }
        }

        private void ChangeNotebookColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CoverColor selectedColor)
            {
                var notebook = GetNotebookFromMenuItem(sender);
                if (notebook != null)
                {
                    notebook.CoverColor = selectedColor;
                    UpdateViewState();

                    var parent = fe.Parent;
                    while (parent != null)
                    {
                        if (parent is ContextMenu cm)
                        {
                            cm.IsOpen = false;
                            break;
                        }
                        parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                    }
                }
            }
        }

        private void ChangeLibraryColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CoverColor selectedColor)
            {
                // We need to find which library's context menu was open. We can pass the library via the Button's Tag.
                var library = GetLibraryFromMenuItem(sender);
                if (library != null)
                {
                    library.CoverColor = selectedColor;
                    UpdateViewState();

                    // Close the context menu
                    var parent = fe.Parent;
                    while (parent != null)
                    {
                        if (parent is ContextMenu cm)
                        {
                            cm.IsOpen = false;
                            break;
                        }
                        parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                    }
                }
            }
        }

        private bool AreColorsSimilar(System.Windows.Media.Color a, System.Windows.Media.Color b)
        {
            // Simple distance check in RGB space
            int diff = Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
            return diff < 100; // Tolerance
        }

        private async void DeleteNotebook_Click(object sender, RoutedEventArgs e)
        {
            var notebook = GetNotebookFromMenuItem(sender);
            if (notebook == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{notebook.Name}'?",
                "Delete Notebook",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _viewModel.DeleteNotebookAsync(notebook);
                UpdateViewState();
            }
        }

        private Notebook GetNotebookFromMenuItem(object sender)
        {
            if (sender is FrameworkElement element)
            {
                // Retrieve the notebook from the Menu/Button's Tag
                if (element.Tag is Notebook tagNotebook) return tagNotebook;

                // Retrieve the notebook from the ContextMenu PlacementTarget
                if (element is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
                {
                    if (contextMenu.PlacementTarget is FrameworkElement target && target.Tag is Notebook targetNotebook) return targetNotebook;
                }
            }
            return null;
        }

        #endregion

        private void ViewModel_NotebookOpenRequested(object sender, Notebook notebook)
        {
            // Navigate to notebook editor
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToNotebook(notebook);
            }
        }

        private string ShowInputDialog(string title, string prompt)
        {
            var dialog = new InputDialog(title, prompt);
            if (Window.GetWindow(this) is Window owner)
            {
                dialog.Owner = owner;
            }

            if (dialog.ShowDialog() == true)
            {
                return dialog.InputText;
            }
            return null;
        }
    }
}
