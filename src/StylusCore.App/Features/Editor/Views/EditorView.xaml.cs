using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using StylusCore.App.ViewModels;
using StylusCore.App.Features.Editor.ViewModels;
using StylusCore.Core.Models;
using ModelPage = StylusCore.Core.Models.Page;
using StylusCore.App.Dialogs;

namespace StylusCore.App.Features.Editor.Views
{
    /// <summary>
    /// EditorView - Editor view with sections panel
    /// </summary>
    public partial class EditorView : System.Windows.Controls.Page
    {
        private readonly EditorViewModel _viewModel;

        // Panel width constants (Fixed Pixels)
        private const double DEFAULT_PANEL_WIDTH = 250;
        private const double MAX_PANEL_WIDTH = 350; // Hard pixel limit
        private const double MIN_PANEL_WIDTH = 100;
        private const double COLLAPSED_WIDTH = 48;

        private bool _isPanelCollapsed = false;
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private double _lastPanelWidth = 250;
#pragma warning restore CS0414
        private Guid? _activeSectionId = null; // Track active section for adding pages

        public EditorView()
        {
            InitializeComponent();

            _viewModel = new EditorViewModel();
            DataContext = _viewModel;

            Loaded += EditorView_Loaded;
            Unloaded += EditorView_Unloaded;
            SizeChanged += EditorView_SizeChanged;
        }

        private void EditorView_Loaded(object sender, RoutedEventArgs e)
        {
            // Wire up CanvasHostControl events
            DrawingCanvas.Renderer = _viewModel.Renderer;
            DrawingCanvas.PointerDown += (p, pressure) => _viewModel.OnPointerDown(p, pressure);
            DrawingCanvas.PointerMove += (p, pressure) => _viewModel.OnPointerMove(p, pressure);
            DrawingCanvas.PointerUp += (p) => _viewModel.OnPointerUp(p);

            // Subscribe to ViewModel page changes
            _viewModel.PageChanged += (s, page) => DrawingCanvas.RenderPage(page);

            _viewModel.StartAutoSave();
            // Start at default fixed width
            SetPanelWidth(DEFAULT_PANEL_WIDTH);
            RefreshSectionsUI();

            // Render initial page
            if (_viewModel.CurrentPage != null)
            {
                DrawingCanvas.RenderPage(_viewModel.CurrentPage);
            }
        }

        // Drag & Drop State
        private Point _startPoint;
        private bool _isDragging;
        private object _dragObject;

        private void EditorView_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopAutoSave();
        }

        private void EditorView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // With fixed pixels, we only need to care if the WINDOW is smaller than the panel
            if (!_isPanelCollapsed)
            {
                // If user manually resized > MAX, snap back
                // if (SectionsPanelColumn.Width.Value > MAX_PANEL_WIDTH)
                // {
                //     SetPanelWidth(MAX_PANEL_WIDTH);
                // }
            }
        }

        public void LoadNotebook(Notebook notebook)
        {
            _viewModel.Notebook = notebook;

            if (notebook.Pages.Count == 0)
            {
                _viewModel.AddNewPage();
            }

            RefreshSectionsUI();
        }

        #region Sections UI Generation

        private void RefreshSectionsUI()
        {
            SectionsContainer.Children.Clear();

            if (_viewModel.Notebook == null) return;

            var sections = _viewModel.Notebook.Sections.OrderBy(s => s.SortOrder).ToList();

            foreach (var section in sections)
            {
                var sectionUI = CreateSectionRow(section);
                SectionsContainer.Children.Add(sectionUI);
            }

            // Show pages without sections at the end
            var unsectionedPages = _viewModel.Notebook.Pages
                .Where(p => p.SectionId == null)
                .OrderBy(p => p.PageNumber)
                .ToList();

            if (unsectionedPages.Any())
            {
                var unsectionedUI = CreateUnsectionedPagesRow(unsectionedPages);
                SectionsContainer.Children.Add(unsectionedUI);
            }
        }

        private UIElement CreateSectionRow(Section section)
        {
            var container = new StackPanel();

            // Section header row
            var headerRow = new Border
            {
                Padding = new Thickness(8, 10, 8, 10),
                Background = Brushes.Transparent,
                Tag = section
            };
            headerRow.SetResourceReference(Border.BorderBrushProperty, "PrimaryBorderBrush");
            headerRow.MouseRightButtonUp += SectionRow_RightClick;
            headerRow.MouseLeftButtonDown += SectionRow_Click;

            if (section.Id == _activeSectionId)
            {
                // Use a theme-aware brush for selection highlight (e.g. TertiaryBackgroundBrush or a specific accent)
                headerRow.SetResourceReference(Border.BackgroundProperty, "TertiaryBackgroundBrush");
            }
            else
            {
                headerRow.Background = Brushes.Transparent;
            }

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });

            // Drag handle
            var dragHandle = new System.Windows.Controls.TextBlock
            {
                Text = "⋮",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.SizeAll,
                Tag = section // Tag for drag source
            };
            dragHandle.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "SecondaryTextBrush");
            dragHandle.MouseMove += SectionDragHandle_MouseMove;
            dragHandle.PreviewMouseLeftButtonDown += SectionDragHandle_PreviewMouseDown;

            Grid.SetColumn(dragHandle, 0);
            headerGrid.Children.Add(dragHandle);

            // Enable Drop on section header
            headerRow.AllowDrop = true;
            headerRow.Drop += SectionHeader_Drop;
            headerRow.DragOver += SectionHeader_DragOver;

            // Section color dot + title
            // Use Grid instead of StackPanel to properly constrain width for TextWrapping
            var titleGrid = new Grid();
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleGrid.Margin = new Thickness(4, 0, 8, 0);

            var colorDot = new System.Windows.Shapes.Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(section.Color ?? "#7C3AED")),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(colorDot, 0);
            titleGrid.Children.Add(colorDot);

            var titleText = new System.Windows.Controls.TextBlock
            {
                Text = section.Title,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleText.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "PrimaryTextBrush");
            Grid.SetColumn(titleText, 1);
            titleGrid.Children.Add(titleText);

            Grid.SetColumn(titleGrid, 1);
            headerGrid.Children.Add(titleGrid);

            // Page range
            var pageRange = GetPageRangeText(section);
            var rangeText = new System.Windows.Controls.TextBlock
            {
                Text = pageRange,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            rangeText.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "SecondaryTextBrush");
            Grid.SetColumn(rangeText, 2);
            headerGrid.Children.Add(rangeText);

            // Expand/collapse button
            var expandBtn = new ToggleButton
            {
                Width = 24,
                Height = 24,
                Tag = section
            };
            expandBtn.SetResourceReference(FrameworkElement.StyleProperty, "ChevronToggleStyle");

            // Bind IsChecked to section.IsExpanded
            var expandBinding = new System.Windows.Data.Binding("IsExpanded")
            {
                Source = section,
                Mode = System.Windows.Data.BindingMode.TwoWay
            };
            expandBtn.SetBinding(ToggleButton.IsCheckedProperty, expandBinding);

            Grid.SetColumn(expandBtn, 3);
            headerGrid.Children.Add(expandBtn);

            headerRow.Child = headerGrid;
            container.Children.Add(headerRow);

            // Pages grid (Always create if pages exist, bind Visibility)
            var pages = _viewModel.Notebook.Pages
                .Where(p => p.SectionId == section.Id)
                .OrderBy(p => p.PageNumber)
                .ToList();

            if (pages.Any())
            {
                var pagesPanel = CreatePagesGrid(pages);

                // Bind Visibility to section.IsExpanded
                var visibilityBinding = new System.Windows.Data.Binding("IsExpanded")
                {
                    Source = section,
                    Converter = (System.Windows.Data.IValueConverter)FindResource("BoolToVisibilityConverter")
                };
                ((FrameworkElement)pagesPanel).SetBinding(UIElement.VisibilityProperty, visibilityBinding);

                container.Children.Add(pagesPanel);
            }

            return container;
        }

        private UIElement CreateUnsectionedPagesRow(List<ModelPage> pages)
        {
            var container = new StackPanel();

            var headerRow = new Border
            {
                Padding = new Thickness(8, 10, 8, 10),
                Background = Brushes.Transparent,
                BorderBrush = (Brush)FindResource("PrimaryBorderBrush"),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            var titleText = new System.Windows.Controls.TextBlock
            {
                Text = $"Unsectioned Pages ({pages.Count})",
                FontSize = 13,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(28, 0, 0, 0)
            };
            titleText.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "SecondaryTextBrush");
            headerRow.Child = titleText;
            container.Children.Add(headerRow);

            var pagesPanel = CreatePagesGrid(pages);
            container.Children.Add(pagesPanel);

            return container;
        }

        private UIElement CreatePagesGrid(List<ModelPage> pages)
        {
            var wrapPanel = new WrapPanel
            {
                Margin = new Thickness(8, 8, 8, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            foreach (var page in pages)
            {
                var pageThumbnail = new Border
                {
                    Width = 80,
                    Height = 110,
                    Margin = new Thickness(4),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Cursor = Cursors.Hand,
                    Tag = page
                };
                pageThumbnail.SetResourceReference(Border.BackgroundProperty, "CanvasBackgroundBrush");
                pageThumbnail.SetResourceReference(Border.BorderBrushProperty, "PrimaryBorderBrush");
                pageThumbnail.MouseLeftButtonDown += PageThumbnail_Click;
                pageThumbnail.PreviewMouseLeftButtonDown += PageThumbnail_PreviewMouseDown;
                pageThumbnail.MouseMove += PageThumbnail_MouseMove;

                var grid = new Grid();

                // Page number
                var pageNumber = new System.Windows.Controls.TextBlock
                {
                    Text = page.PageNumber.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 6),
                    FontSize = 11,
                    Foreground = (Brush)FindResource("SecondaryTextBrush")
                };
                grid.Children.Add(pageNumber);

                // Color bar
                var colorBar = new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Width = 3,
                    CornerRadius = new CornerRadius(4, 0, 0, 4),
                    Background = (Brush)FindResource("PrimaryAccentBrush")
                };
                grid.Children.Add(colorBar);

                pageThumbnail.Child = grid;
                wrapPanel.Children.Add(pageThumbnail);
            }

            return wrapPanel;
        }

        private string GetPageRangeText(Section section)
        {
            var pages = _viewModel.Notebook.Pages
                .Where(p => p.SectionId == section.Id)
                .OrderBy(p => p.PageNumber)
                .ToList();

            if (!pages.Any()) return "(empty)";

            var first = pages.First().PageNumber;
            var last = pages.Last().PageNumber;

            return first == last ? $"({first})" : $"({first}-{last})";
        }

        #endregion

        #region Panel Collapse/Expand

        private void CollapsePanel_Click(object sender, RoutedEventArgs e)
        {
            CollapsePanel();
        }

        private void ExpandPanel_Click(object sender, RoutedEventArgs e)
        {
            ExpandPanel();
        }

        private void CollapsePanel()
        {
            _lastPanelWidth = SectionsPanelColumn.ActualWidth;
            _isPanelCollapsed = true;

            SectionsPanelColumn.Width = new GridLength(COLLAPSED_WIDTH);
            SectionsPanelColumn.MinWidth = COLLAPSED_WIDTH;
            SectionsPanelColumn.MaxWidth = COLLAPSED_WIDTH; // Lock at collapsed width

            CollapsedOverlay.Visibility = Visibility.Visible;
            SectionsTitle.Visibility = Visibility.Collapsed;
            CollapseBtn.Visibility = Visibility.Collapsed;
            if (BottomButtonsBorder != null) BottomButtonsBorder.Visibility = Visibility.Collapsed;
        }

        private void ExpandPanel()
        {
            _isPanelCollapsed = false;

            // Restore to default (250) or last valid width if reasonable
            var targetWidth = Math.Max(DEFAULT_PANEL_WIDTH, Math.Min(_lastPanelWidth, MAX_PANEL_WIDTH));

            SectionsPanelColumn.MinWidth = MIN_PANEL_WIDTH;
            SectionsPanelColumn.MaxWidth = MAX_PANEL_WIDTH; // Fixed Max Pixel Limit
            SetPanelWidth(targetWidth);

            CollapsedOverlay.Visibility = Visibility.Collapsed;
            SectionsTitle.Visibility = Visibility.Visible;
            CollapseBtn.Visibility = Visibility.Visible;
            if (BottomButtonsBorder != null) BottomButtonsBorder.Visibility = Visibility.Visible;
        }

        private void SetPanelWidth(double width)
        {
            var clampedWidth = Math.Max(MIN_PANEL_WIDTH, Math.Min(width, MAX_PANEL_WIDTH));
            SectionsPanelColumn.Width = new GridLength(clampedWidth);
        }

        private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            CheckAndSnapPanel();
        }

        private void GridSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Give live visual feedback before releasing
            var currentWidth = SectionsPanelColumn.ActualWidth;

            if (currentWidth < MIN_PANEL_WIDTH)
            {
                // Show collapsed state visuals immediately
                CollapsedOverlay.Visibility = Visibility.Visible;
                SectionsTitle.Visibility = Visibility.Collapsed;
                CollapseBtn.Visibility = Visibility.Collapsed;

                if (BottomButtonsBorder != null) BottomButtonsBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Show expanded state visuals
                CollapsedOverlay.Visibility = Visibility.Collapsed;
                SectionsTitle.Visibility = Visibility.Visible;
                CollapseBtn.Visibility = Visibility.Visible;

                if (BottomButtonsBorder != null) BottomButtonsBorder.Visibility = Visibility.Visible;
            }
        }

        private void CheckAndSnapPanel()
        {
            var currentWidth = SectionsPanelColumn.ActualWidth;

            if (currentWidth < MIN_PANEL_WIDTH)
            {
                CollapsePanel();
            }
            else if (currentWidth > MAX_PANEL_WIDTH)
            {
                // Snap back to MAX if exceeded (handle elasticity)
                SetPanelWidth(MAX_PANEL_WIDTH);
                _lastPanelWidth = MAX_PANEL_WIDTH;
            }
            else
            {
                // Remember valid width
                _lastPanelWidth = currentWidth;
            }
        }

        #endregion

        #region Section Events

        private void SectionRow_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Section section)
            {
                var contextMenu = new ContextMenu();

                var renameItem = new MenuItem { Header = "Rename" };
                renameItem.Click += (s, args) => RenameSection(section);
                contextMenu.Items.Add(renameItem);

                var colorItem = new MenuItem { Header = "Change Color" };
                colorItem.Click += (s, args) => ChangeSectionColor(section);
                contextMenu.Items.Add(colorItem);

                contextMenu.Items.Add(new Separator());

                var deleteItem = new MenuItem { Header = "Delete Section" };
                deleteItem.Click += (s, args) => DeleteSection(section);
                contextMenu.Items.Add(deleteItem);

                contextMenu.IsOpen = true;
            }
        }

        private void RenameSection(Section section)
        {
            var dialog = new InputDialog("Rename Section", "New Name:");
            dialog.SetInputText(section.Title);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                section.Title = dialog.InputText;
                RefreshSectionsUI();
            }
        }

        private void ChangeSectionColor(Section section)
        {
            var colors = new[] { "#7C3AED", "#2563EB", "#059669", "#D97706", "#DC2626", "#DB2777" };
            var currentIndex = Array.IndexOf(colors, section.Color);
            section.Color = colors[(currentIndex + 1) % colors.Length];
            RefreshSectionsUI();
        }

        private void DeleteSection(Section section)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete section \"{section.Title}\"?\n\nPages will not be deleted, they will become unsectioned.",
                "Delete Section",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var page in _viewModel.Notebook.Pages.Where(p => p.SectionId == section.Id))
                {
                    page.SectionId = null;
                }

                _viewModel.Notebook.Sections.Remove(section);
                RefreshSectionsUI();
            }
        }

        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("New Section", "Section Name:");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                _viewModel.AddNewSection(dialog.InputText);
                RefreshSectionsUI();
            }
        }

        #endregion

        private void SectionRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Section section)
            {
                _activeSectionId = section.Id;
                RefreshSectionsUI();
            }
        }

        // --- Drag & Drop Handlers ---

        private void SectionDragHandle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _dragObject = (sender as FrameworkElement)?.Tag;
            _isDragging = false;
        }

        private void SectionDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (_dragObject is Section section)
                    {
                        StartDrag(section);
                    }
                }
            }
        }

        private void PageThumbnail_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _dragObject = (sender as FrameworkElement)?.Tag;
            _isDragging = false;
        }

        private void PageThumbnail_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (_dragObject is ModelPage page)
                    {
                        StartDrag(page);
                    }
                }
            }
        }

        private void StartDrag(object data)
        {
            _isDragging = true;
            DataObject dataObject = new DataObject("StylusData", data);
            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Move);
            _isDragging = false;
            _dragObject = null;
        }

        private void SectionHeader_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("StylusData"))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void SectionHeader_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("StylusData") && sender is FrameworkElement element && element.Tag is Section targetSection)
            {
                var data = e.Data.GetData("StylusData");

                if (data is Section sourceSection && sourceSection.Id != targetSection.Id)
                {
                    // Reorder Section
                    // Find index of target
                    var sections = _viewModel.Notebook.Sections.OrderBy(s => s.SortOrder).ToList();
                    var targetIndex = sections.IndexOf(sections.First(s => s.Id == targetSection.Id));

                    _viewModel.ReorderSection(sourceSection, targetIndex);
                    RefreshSectionsUI();
                }
                else if (data is ModelPage sourcePage && sourcePage.SectionId != targetSection.Id)
                {
                    // Move Page to Section
                    _viewModel.MovePageToSection(sourcePage, targetSection.Id);
                    RefreshSectionsUI();
                }
            }
        }

        #region Page Events

        private void PageThumbnail_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is ModelPage page)
            {
                var index = _viewModel.Notebook.Pages.IndexOf(page);
                if (index >= 0)
                {
                    _viewModel.CurrentPageIndex = index;
                }
            }
        }

        private void AddPage_Click(object sender, RoutedEventArgs e)
        {
            // Add page to active section or first section if none active
            var targetSectionId = _activeSectionId;

            if (targetSectionId == null && _viewModel.Notebook.Sections.Count > 0)
            {
                targetSectionId = _viewModel.Notebook.Sections.First().Id;
            }

            var newPage = _viewModel.AddNewPage(targetSectionId);

            RefreshSectionsUI();
        }

        #endregion
    }
}
