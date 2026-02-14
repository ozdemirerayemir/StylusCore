using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation;
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

        private double MaxPanelWidth => (double)FindResource("Editor.SectionsPanelMaxWidth");
        private double MinPanelWidth => (double)FindResource("Editor.SectionsPanelMinWidth");
        private double DefaultPanelWidth => (double)FindResource("Editor.SectionsPanelDefaultWidth");
        private double CollapsedWidth => (double)FindResource("Editor.SectionsPanelCollapsedWidth");

        // _lastPanelWidth should default to a valid expanded width (e.g. 250)
        private double _lastPanelWidth = 250;
        private bool _isPanelCollapsed = false;
        private Guid? _activeSectionId = null;

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
            DrawingCanvas.Renderer = _viewModel.Renderer;
            DrawingCanvas.PointerDown += (p, pressure) => _viewModel.OnPointerDown(p, pressure);
            DrawingCanvas.PointerMove += (p, pressure) => _viewModel.OnPointerMove(p, pressure);
            DrawingCanvas.PointerUp += (p) => _viewModel.OnPointerUp(p);

            _viewModel.PageChanged += (s, page) => DrawingCanvas.RenderPage(page);
            _viewModel.StartAutoSave();

            // Initialize default state
            _lastPanelWidth = (double)FindResource("Editor.SectionsPanelDefaultWidth");
            ExpandPanel(); // Ensure we start expanded with correct constraints

            RefreshSectionsUI();

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
            // If window resized, ensure panel respects constraints if expanded
            if (!_isPanelCollapsed)
            {
                var currentWidth = SectionsPanelColumn.ActualWidth;
                if (currentWidth > MaxPanelWidth)
                {
                    SetPanelWidth(MaxPanelWidth);
                }
                // We don't forcibly clamp min here as GridSplitter logic handles user interaction, 
                // and window shrinking shouldn't necessarily break resizing unless critical.
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

            var headerRow = new Border
            {
                Padding = (Thickness)FindResource("Margin_Standard"), // Use resource
                Tag = section,
                Background = Brushes.Transparent // Default
            };
            headerRow.SetResourceReference(Border.BorderBrushProperty, "PrimaryBorderBrush");
            headerRow.MouseRightButtonUp += SectionRow_RightClick;
            headerRow.MouseLeftButtonDown += SectionRow_Click;

            if (section.Id == _activeSectionId)
            {
                headerRow.SetResourceReference(Border.BackgroundProperty, "TertiaryBackgroundBrush");
            }

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });

            // Drag Handle (Icon)
            var dragHandlePath = new System.Windows.Shapes.Path
            {
                Data = (Geometry)FindResource("Icon.DragHandle"),
                Style = (Style)FindResource("IconBase"),
                Width = 12,
                Height = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.SizeAll,
                Tag = section
            };
            // Wrap in a transparent border or grid to increase hit target if needed, but Path is fine for now
            var dragContainer = new Grid { Background = Brushes.Transparent, Cursor = Cursors.SizeAll, Tag = section };
            dragContainer.Children.Add(dragHandlePath);
            dragContainer.MouseMove += SectionDragHandle_MouseMove;
            dragContainer.PreviewMouseLeftButtonDown += SectionDragHandle_PreviewMouseDown;

            Grid.SetColumn(dragContainer, 0);
            headerGrid.Children.Add(dragContainer);

            // Drop targets
            headerRow.AllowDrop = true;
            headerRow.Drop += SectionHeader_Drop;
            headerRow.DragOver += SectionHeader_DragOver;

            // Title + Color
            var titleGrid = new Grid();
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleGrid.Margin = new Thickness(4, 0, 8, 0);

            var colorDot = new System.Windows.Shapes.Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = GetSectionColorBrush(section.Color),
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

            // Page Count
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

            // Expand Button
            var expandBtn = new ToggleButton
            {
                Width = 24,
                Height = 24,
                Tag = section
            };
            expandBtn.SetResourceReference(FrameworkElement.StyleProperty, "ChevronToggleStyle");
            expandBtn.SetValue(AutomationProperties.NameProperty, (string)FindResource("Str_ToggleSection"));

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

            // Pages
            var pages = _viewModel.Notebook.Pages
                .Where(p => p.SectionId == section.Id)
                .OrderBy(p => p.PageNumber)
                .ToList();

            if (pages.Any())
            {
                var pagesPanel = CreatePagesGrid(pages);
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

        private Brush GetSectionColorBrush(string hexOrName)
        {
            // Strict mapping to shared Brush resources
            var colorMap = new Dictionary<string, string>
            {
                { "#7C3AED", "Brush.Section.Purple" },
                { "#2563EB", "Brush.Section.Blue" },
                { "#059669", "Brush.Section.Green" },
                { "#D97706", "Brush.Section.Yellow" },
                { "#DC2626", "Brush.Section.Red" },
                { "#DB2777", "Brush.Section.Pink" }
            };

            if (!string.IsNullOrEmpty(hexOrName) && colorMap.TryGetValue(hexOrName.ToUpper(), out var resourceKey))
            {
                return (Brush)FindResource(resourceKey);
            }

            return (Brush)FindResource("Brush.Section.Purple");
        }

        private UIElement CreateUnsectionedPagesRow(List<ModelPage> pages)
        {
            var container = new StackPanel();
            var headerRow = new Border
            {
                Padding = new Thickness(8, 10, 8, 10),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            headerRow.SetResourceReference(Border.BorderBrushProperty, "PrimaryBorderBrush");

            var titleText = new System.Windows.Controls.TextBlock
            {
                Text = $"{(string)FindResource("Str_Editor_UnsectionedPages")} ({pages.Count})",
                FontSize = 13,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(28, 0, 0, 0)
            };
            titleText.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "SecondaryTextBrush");
            headerRow.Child = titleText;
            container.Children.Add(headerRow);
            container.Children.Add(CreatePagesGrid(pages));
            return container;
        }

        private UIElement CreatePagesGrid(List<ModelPage> pages)
        {
            var wrapPanel = new WrapPanel
            {
                Margin = new Thickness(8, 8, 8, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var thumbWidth = (double)FindResource("Editor.PageThumbnailWidth");
            var thumbHeight = (double)FindResource("Editor.PageThumbnailHeight");

            foreach (var page in pages)
            {
                var pageThumbnail = new Border
                {
                    Width = thumbWidth,
                    Height = thumbHeight,
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
                var pageNumber = new System.Windows.Controls.TextBlock
                {
                    Text = page.PageNumber.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 6),
                    FontSize = 11
                };
                pageNumber.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "SecondaryTextBrush");
                grid.Children.Add(pageNumber);

                var colorBar = new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Width = 3,
                    CornerRadius = new CornerRadius(4, 0, 0, 4)
                };
                colorBar.SetResourceReference(Border.BackgroundProperty, "PrimaryAccentBrush");
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

            if (!pages.Any()) return (string)FindResource("Str_Editor_EmptySection");

            var first = pages.First().PageNumber;
            var last = pages.Last().PageNumber;

            if (first == last)
            {
                return string.Format((string)FindResource("Str_Editor_PageSingle_Format"), first);
            }
            return string.Format((string)FindResource("Str_Editor_PageRange_Format"), first, last);
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
            // Store current width ONLY if we are currently expanded (and valid)
            if (!_isPanelCollapsed && SectionsPanelColumn.ActualWidth >= MinPanelWidth)
            {
                _lastPanelWidth = SectionsPanelColumn.ActualWidth;
            }

            _isPanelCollapsed = true;

            // Lock panel visuals
            SectionsPanelColumn.Width = new GridLength(CollapsedWidth);
            SectionsPanelColumn.MinWidth = CollapsedWidth;
            SectionsPanelColumn.MaxWidth = CollapsedWidth;

            // UI State
            CollapsedOverlay.Visibility = Visibility.Visible;
            SectionsTitle.Visibility = Visibility.Collapsed;
            CollapseBtn.Visibility = Visibility.Collapsed;
            if (BottomButtonsBorder != null) BottomButtonsBorder.Visibility = Visibility.Collapsed;

            // Disable resizing
            var splitter = FindGridSplitter();
            if (splitter != null) splitter.IsEnabled = false;
        }

        private void ExpandPanel()
        {
            _isPanelCollapsed = false;

            // Restore logic: Clamp _lastPanelWidth to [Min, Max]
            var baseWidth = (_lastPanelWidth > 0 && !double.IsNaN(_lastPanelWidth)) ? _lastPanelWidth : DefaultPanelWidth;
            var targetWidth = Math.Max(MinPanelWidth, Math.Min(baseWidth, MaxPanelWidth));

            // Set constraints FIRST
            SectionsPanelColumn.MinWidth = MinPanelWidth;
            SectionsPanelColumn.MaxWidth = MaxPanelWidth;
            SectionsPanelColumn.Width = new GridLength(targetWidth);

            // UI State
            CollapsedOverlay.Visibility = Visibility.Collapsed;
            SectionsTitle.Visibility = Visibility.Visible;
            CollapseBtn.Visibility = Visibility.Visible;
            if (BottomButtonsBorder != null) BottomButtonsBorder.Visibility = Visibility.Visible;

            // Enable resizing
            var splitter = FindGridSplitter();
            if (splitter != null) splitter.IsEnabled = true;
        }

        private GridSplitter FindGridSplitter()
        {
            // Helper to find GridSplitter in visual tree or by traversing siblings if not named
            // It is defined in XAML without a name, but we know it's in Column 1
            foreach (var child in ((Grid)SectionsPanelColumn.Parent).Children)
            {
                if (child is GridSplitter gs) return gs;
            }
            return null;
        }

        private void SetPanelWidth(double width)
        {
            // STRICT clamping
            var clamped = Math.Max(MinPanelWidth, Math.Min(width, MaxPanelWidth));
            SectionsPanelColumn.Width = new GridLength(clamped);
        }

        private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // Only update stored width if expanded
            if (!_isPanelCollapsed)
            {
                var cur = SectionsPanelColumn.ActualWidth;
                // Just in case it went out of bounds (though MaxWidth/MinWidth should prevent it)
                if (cur >= MinPanelWidth && cur <= MaxPanelWidth)
                {
                    _lastPanelWidth = cur;
                }
            }
        }

        #endregion

        #region Section Events

        private void SectionRow_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Section section)
            {
                var contextMenu = new ContextMenu();

                var renameItem = new MenuItem { Header = (string)FindResource("Str_Editor_RenameSection") };
                renameItem.Click += (s, args) => RenameSection(section);
                // renameItem.SetValue(AutomationProperties.NameProperty, ...); 
                contextMenu.Items.Add(renameItem);

                var colorItem = new MenuItem { Header = (string)FindResource("Str_ChangeColor") };
                colorItem.Click += (s, args) => ChangeSectionColor(section);
                contextMenu.Items.Add(colorItem);

                contextMenu.Items.Add(new Separator());

                var deleteItem = new MenuItem { Header = (string)FindResource("Str_Editor_DeleteSection") };
                deleteItem.Click += (s, args) => DeleteSection(section);
                contextMenu.Items.Add(deleteItem);

                contextMenu.IsOpen = true;
            }
        }

        private void RenameSection(Section section)
        {
            var dialog = new InputDialog((string)FindResource("Str_Editor_RenameSection"), (string)FindResource("Str_Editor_NewSectionName"));
            dialog.SetInputText(section.Title);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                section.Title = dialog.InputText;
                RefreshSectionsUI();
            }
        }

        private void ChangeSectionColor(Section section)
        {
            // Cycle through known colors using Resource keys if possible, or just strict Hex from the design
            var colors = new[] { "#7C3AED", "#2563EB", "#059669", "#D97706", "#DC2626", "#DB2777" };
            var currentIndex = Array.IndexOf(colors, section.Color);
            section.Color = colors[(currentIndex + 1) % colors.Length];
            RefreshSectionsUI();
        }

        private void DeleteSection(Section section)
        {
            var confirmFormat = (string)FindResource("Str_Editor_DeleteSectionConfirm");
            var result = MessageBox.Show(
                string.Format(confirmFormat, section.Title),
                (string)FindResource("Str_Editor_DeleteSection"),
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
            var dialog = new InputDialog((string)FindResource("Str_Editor_NewSection"), (string)FindResource("Str_Editor_SectionName"));
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                _viewModel.AddNewSection(dialog.InputText);
                RefreshSectionsUI();
            }
        }

        private void SectionRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Section section)
            {
                _activeSectionId = section.Id;
                RefreshSectionsUI();
            }
        }

        #endregion

        #region Drag & Drop

        private void SectionDragHandle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _dragObject = (sender as FrameworkElement)?.Tag;
            _isDragging = false;
        }

        private void SectionDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            // Standard drag logic...
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
                    var sections = _viewModel.Notebook.Sections.OrderBy(s => s.SortOrder).ToList();
                    var targetIndex = sections.IndexOf(sections.First(s => s.Id == targetSection.Id));
                    _viewModel.ReorderSection(sourceSection, targetIndex);
                    RefreshSectionsUI();
                }
                else if (data is ModelPage sourcePage && sourcePage.SectionId != targetSection.Id)
                {
                    _viewModel.MovePageToSection(sourcePage, targetSection.Id);
                    RefreshSectionsUI();
                }
            }
        }

        #endregion

        #region Page Events

        private void PageThumbnail_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is ModelPage page)
            {
                var index = _viewModel.Notebook.Pages.IndexOf(page);
                if (index >= 0) _viewModel.CurrentPageIndex = index;
            }
        }

        private void AddPage_Click(object sender, RoutedEventArgs e)
        {
            var targetSectionId = _activeSectionId;
            if (targetSectionId == null && _viewModel.Notebook.Sections.Count > 0)
            {
                targetSectionId = _viewModel.Notebook.Sections.First().Id;
            }
            _viewModel.AddNewPage(targetSectionId);
            RefreshSectionsUI();
        }

        #endregion
    }
}
