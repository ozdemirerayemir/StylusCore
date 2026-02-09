using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using StylusCore.Core.Enums;
using StylusCore.Core.Models;
using StylusCore.Core.Services;
using StylusCore.Engine.Wpf.Rendering;
using StylusCore.Engine.Wpf.Tools;
using StylusCore.App.Core.ViewModels;

namespace StylusCore.App.Features.Editor.ViewModels
{
    /// <summary>
    /// ViewModel for the Editor view.
    /// Manages pages, drawing, and undo/redo.
    /// </summary>
    public class EditorViewModel : INotifyPropertyChanged
    {
        #region Services

        private readonly IStorageService _storageService;
        private readonly IAutoSaveService _autoSaveService;
        private readonly PageRenderer _pageRenderer;

        #endregion

        #region Tools

        private readonly Dictionary<ToolMode, ITool> _tools;
        private ITool _currentTool;

        #endregion

        #region Undo/Redo

        private readonly Stack<UndoAction> _undoStack;
        private readonly Stack<UndoAction> _redoStack;

        #endregion

        #region Properties

        private Notebook _notebook;
        /// <summary>
        /// Current notebook
        /// </summary>
        public Notebook Notebook
        {
            get => _notebook;
            set
            {
                _notebook = value;
                OnPropertyChanged();
                if (value?.Pages.Count > 0)
                {
                    CurrentPage = value.Pages[0];
                }
            }
        }

        private Page _currentPage;
        /// <summary>
        /// Currently displayed page
        /// </summary>
        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                PageChanged?.Invoke(this, value);
            }
        }

        private int _currentPageIndex;
        /// <summary>
        /// Current page index (0-based)
        /// </summary>
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                if (value >= 0 && value < (Notebook?.Pages.Count ?? 0))
                {
                    _currentPageIndex = value;
                    CurrentPage = Notebook.Pages[value];
                    OnPropertyChanged();
                }
            }
        }

        private ToolMode _currentToolMode;
        /// <summary>
        /// Current tool mode
        /// </summary>
        public ToolMode CurrentToolMode
        {
            get => _currentToolMode;
            set
            {
                _currentToolMode = value;
                _currentTool?.Deactivate();
                if (_tools.TryGetValue(value, out var tool))
                {
                    _currentTool = tool;
                    _currentTool.Activate();
                }
                OnPropertyChanged();
            }
        }

        private string _currentColor;
        /// <summary>
        /// Current stroke color
        /// </summary>
        public string CurrentColor
        {
            get => _currentColor;
            set
            {
                _currentColor = value;
                if (_currentTool != null)
                {
                    _currentTool.Color = value;
                }
                OnPropertyChanged();
            }
        }

        private double _currentWidth;
        /// <summary>
        /// Current stroke width
        /// </summary>
        public double CurrentWidth
        {
            get => _currentWidth;
            set
            {
                _currentWidth = value;
                if (_currentTool != null)
                {
                    _currentTool.Width = value;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Page renderer for zoom/pan
        /// </summary>
        public PageRenderer Renderer => _pageRenderer;

        /// <summary>
        /// Whether undo is available
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Whether redo is available
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        private RibbonMode _ribbonMode = RibbonMode.Full;
        /// <summary>
        /// Current ribbon display mode (Full, TabsOnly, FullScreen)
        /// </summary>
        public RibbonMode RibbonMode
        {
            get => _ribbonMode;
            set
            {
                if (_ribbonMode != value)
                {
                    _ribbonMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsRibbonVisible));
                    OnPropertyChanged(nameof(IsRibbonFull));
                }
            }
        }

        /// <summary>
        /// Whether the ribbon toolbar should be visible (false in FullScreen mode)
        /// </summary>
        public bool IsRibbonVisible => RibbonMode != RibbonMode.FullScreen;

        /// <summary>
        /// Whether the ribbon should show full content (false in TabsOnly mode)
        /// </summary>
        public bool IsRibbonFull => RibbonMode == RibbonMode.Full;

        private ICommand _cycleRibbonModeCommand;
        /// <summary>
        /// Command to cycle ribbon mode: Full → TabsOnly → FullScreen → Full
        /// </summary>
        public ICommand CycleRibbonModeCommand => _cycleRibbonModeCommand ??=
            new MainViewModel.RelayCommand(o => CycleRibbonMode());

        private void CycleRibbonMode()
        {
            RibbonMode = RibbonMode switch
            {
                RibbonMode.Full => RibbonMode.TabsOnly,
                RibbonMode.TabsOnly => RibbonMode.FullScreen,
                RibbonMode.FullScreen => RibbonMode.Full,
                _ => RibbonMode.Full
            };
        }

        #endregion

        #region Events

        public event EventHandler<Page> PageChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public EditorViewModel()
        {
            _storageService = new StorageService();
            _autoSaveService = new AutoSaveService(_storageService);
            _pageRenderer = new PageRenderer();
            _undoStack = new Stack<UndoAction>();
            _redoStack = new Stack<UndoAction>();

            // Initialize tools
            _tools = new Dictionary<ToolMode, ITool>
            {
                { ToolMode.Pen, new PenTool() },
                { ToolMode.Highlighter, new HighlighterTool() },
                { ToolMode.Eraser, new EraserTool() },
                { ToolMode.Shape, new ShapeTool() }
            };

            // Set defaults
            CurrentToolMode = ToolMode.Pen;
            CurrentColor = "#000000";
            CurrentWidth = 2.0;

            // Wire up auto-save
            _autoSaveService.SaveCompleted += OnAutoSaveCompleted;
        }

        #endregion

        #region Page Navigation

        /// <summary>
        /// Go to next page
        /// </summary>
        public void NextPage()
        {
            CurrentPageIndex++;
        }

        /// <summary>
        /// Go to previous page
        /// </summary>
        public void PreviousPage()
        {
            CurrentPageIndex--;
        }

        /// <summary>
        /// Add a new page. If sectionId is provided, adds to that section. 
        /// Otherwise adds after current page.
        /// </summary>
        public Page AddNewPage(Guid? sectionId = null)
        {
            int insertIndex;

            if (sectionId.HasValue)
            {
                // Find the last page of this section
                var sectionPages = Notebook.Pages
                    .Where(p => p.SectionId == sectionId.Value)
                    .OrderBy(p => p.PageNumber)
                    .ToList();

                if (sectionPages.Any())
                {
                    var lastPage = sectionPages.Last();
                    insertIndex = Notebook.Pages.IndexOf(lastPage) + 1;
                }
                else
                {
                    // If section has no pages (shouldn't happen often but safely handle),
                    // insert at the end or logic depending on section sort order?
                    // For now, let's append to end if empty, or after current if ambiguous
                    insertIndex = Notebook.Pages.Count;
                }
            }
            else
            {
                // Default behavior: after current page
                insertIndex = Notebook.Pages.Count > 0 ? CurrentPageIndex + 1 : 0;
            }

            var page = new Page
            {
                NotebookId = Notebook.Id,
                SectionId = sectionId,
                Format = Notebook.DefaultPageFormat,
                Template = Notebook.DefaultPageTemplate
            };

            Notebook.Pages.Insert(insertIndex, page);

            // Renumber pages
            RenumberPages();

            CurrentPageIndex = insertIndex;
            return page;
        }

        private void RenumberPages()
        {
            // Reconstruct the page list based on section order
            var newPageList = new List<Page>();

            // 1. Get ordered sections
            var orderedSections = Notebook.Sections.OrderBy(s => s.SortOrder).ToList();

            // 2. Add pages for each section in order
            foreach (var section in orderedSections)
            {
                var sectionPages = Notebook.Pages
                    .Where(p => p.SectionId == section.Id)
                    .OrderBy(p => p.PageNumber)
                    .ToList();

                newPageList.AddRange(sectionPages);
            }

            // 3. Add unsectioned pages (always at the end for now, or user can drag them)
            var unsectioned = Notebook.Pages
                .Where(p => p.SectionId == null)
                .OrderBy(p => p.PageNumber)
                .ToList();

            newPageList.AddRange(unsectioned);

            // 4. Update Notebook.Pages with the new order
            Notebook.Pages.Clear();
            foreach (var p in newPageList)
            {
                Notebook.Pages.Add(p);
            }

            // 5. Assign sequential numbers
            for (int i = 0; i < Notebook.Pages.Count; i++)
            {
                Notebook.Pages[i].PageNumber = i + 1;
            }
        }

        /// <summary>
        /// Move a page to a specific section
        /// </summary>
        public void MovePageToSection(Page page, Guid? targetSectionId)
        {
            page.SectionId = targetSectionId;
            RenumberPages();
        }

        /// <summary>
        /// Reorder a section to a new index
        /// </summary>
        public void ReorderSection(Section section, int newIndex)
        {
            var sections = Notebook.Sections.OrderBy(s => s.SortOrder).ToList();
            sections.Remove(section);

            // Clamp index
            newIndex = Math.Max(0, Math.Min(newIndex, sections.Count));

            sections.Insert(newIndex, section);

            // Update sort orders
            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].SortOrder = i;
            }

            // Important: Reconstruct page list to match new section order
            RenumberPages();
        }

        /// <summary>
        /// Navigate to a section
        /// </summary>
        public void GoToSection(Section section)
        {
            var pages = Notebook.Pages.ToList();
            var pageIndex = pages.FindIndex(p => p.Id == section.PageId);
            if (pageIndex >= 0)
            {
                CurrentPageIndex = pageIndex;
            }
        }

        /// <summary>
        /// Add a new section at the current page
        /// </summary>
        public Section AddNewSection(string title)
        {
            // If no pages exist, add one first
            if (Notebook.Pages.Count == 0)
            {
                AddNewPage();
            }

            var section = new Section
            {
                NotebookId = Notebook.Id,
                PageId = CurrentPage.Id,
                Title = title,
                Color = GetNextSectionColor(),
                SortOrder = Notebook.Sections.Count
            };

            Notebook.Sections.Add(section);

            // Mark current page as belonging to this section
            CurrentPage.SectionId = section.Id;

            return section;
        }

        /// <summary>
        /// Get next section color in rotation
        /// </summary>
        private string GetNextSectionColor()
        {
            var colors = new[] { "#7C3AED", "#2563EB", "#059669", "#D97706", "#DC2626", "#DB2777", "#4F46E5", "#0891B2" };
            return colors[Notebook.Sections.Count % colors.Length];
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Handle pointer down from canvas
        /// </summary>
        public void OnPointerDown(Point position, float pressure)
        {
            var pagePos = _pageRenderer.ScreenToPage(position);
            _currentTool?.OnPointerDown(pagePos, pressure);
        }

        /// <summary>
        /// Handle pointer move from canvas
        /// </summary>
        public void OnPointerMove(Point position, float pressure)
        {
            var pagePos = _pageRenderer.ScreenToPage(position);
            _currentTool?.OnPointerMove(pagePos, pressure);
        }

        /// <summary>
        /// Handle pointer up from canvas
        /// </summary>
        public void OnPointerUp(Point position)
        {
            var pagePos = _pageRenderer.ScreenToPage(position);
            _currentTool?.OnPointerUp(pagePos);

            // Add completed stroke to page
            var stroke = _currentTool?.GetCurrentStroke();
            if (stroke != null && stroke.Points.Count > 1)
            {
                AddStroke(stroke);
            }
        }

        /// <summary>
        /// Add a stroke to the current page
        /// </summary>
        public void AddStroke(Stroke stroke)
        {
            stroke.PageId = CurrentPage.Id;
            CurrentPage.Strokes.Add(stroke);

            // Record for undo
            _undoStack.Push(new UndoAction(UndoActionType.AddStroke, stroke));
            _redoStack.Clear();
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }

        #endregion

        #region Undo/Redo

        /// <summary>
        /// Undo last action
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) return;

            var action = _undoStack.Pop();
            action.Undo(CurrentPage);
            _redoStack.Push(action);

            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }

        /// <summary>
        /// Redo last undone action
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) return;

            var action = _redoStack.Pop();
            action.Redo(CurrentPage);
            _undoStack.Push(action);

            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }

        #endregion

        #region Auto-Save

        /// <summary>
        /// Start auto-save
        /// </summary>
        public void StartAutoSave()
        {
            if (Notebook != null)
            {
                _autoSaveService.Start(Notebook);
            }
        }

        /// <summary>
        /// Stop auto-save
        /// </summary>
        public void StopAutoSave()
        {
            _autoSaveService.Stop();
        }

        private void OnAutoSaveCompleted(object sender, AutoSaveEventArgs e)
        {
            // Notify UI of save status
        }

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #region Undo/Redo Support

    public enum UndoActionType
    {
        AddStroke,
        RemoveStroke,
        AddShape,
        RemoveShape,
        AddTextBlock,
        RemoveTextBlock
    }

    public class UndoAction
    {
        public UndoActionType Type { get; set; }
        public object Data { get; set; }

        public UndoAction(UndoActionType type, object data)
        {
            Type = type;
            Data = data;
        }

        public void Undo(Page page)
        {
            switch (Type)
            {
                case UndoActionType.AddStroke:
                    page.Strokes.Remove((Stroke)Data);
                    break;
                case UndoActionType.RemoveStroke:
                    page.Strokes.Add((Stroke)Data);
                    break;
            }
        }

        public void Redo(Page page)
        {
            switch (Type)
            {
                case UndoActionType.AddStroke:
                    page.Strokes.Add((Stroke)Data);
                    break;
                case UndoActionType.RemoveStroke:
                    page.Strokes.Remove((Stroke)Data);
                    break;
            }
        }
    }

    #endregion
}
