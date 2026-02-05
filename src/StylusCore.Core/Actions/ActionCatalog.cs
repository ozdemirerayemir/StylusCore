namespace StylusCore.Core.Actions
{
    /// <summary>
    /// Optional metadata for actions.
    /// Used for UI hints (icons, tooltips) without coupling to WPF.
    /// </summary>
    public readonly record struct ActionMetadata(
        /// <summary>Action category for grouping in UI</summary>
        string Category,

        /// <summary>Default icon key (resolved by UI layer)</summary>
        string IconKey,

        /// <summary>Display name for tooltips/menus</summary>
        string DisplayName,

        /// <summary>Default keyboard gesture hint (e.g., "Ctrl+Z")</summary>
        string DefaultGestureHint
    );

    /// <summary>
    /// Static catalog of action metadata.
    /// UI layer can query this for display purposes.
    /// </summary>
    public static class ActionCatalog
    {
        private static readonly Dictionary<string, ActionMetadata> _metadata = new()
        {
            [ActionIds.ToolPen] = new("Tools", "Icon.Pen", "Pen", "P"),
            [ActionIds.ToolEraser] = new("Tools", "Icon.Eraser", "Eraser", "E"),
            [ActionIds.ToolHighlighter] = new("Tools", "Icon.Highlighter", "Highlighter", "H"),
            [ActionIds.ToolLasso] = new("Tools", "Icon.Lasso", "Lasso Selection", "L"),
            [ActionIds.ToolShape] = new("Tools", "Icon.Shape", "Shapes", "S"),
            [ActionIds.ToolText] = new("Tools", "Icon.Text", "Text", "T"),

            [ActionIds.NavPageNext] = new("Navigation", "Icon.Next", "Next Page", "Page Down"),
            [ActionIds.NavPagePrev] = new("Navigation", "Icon.Prev", "Previous Page", "Page Up"),
            [ActionIds.NavPageFirst] = new("Navigation", "Icon.First", "First Page", "Ctrl+Home"),
            [ActionIds.NavPageLast] = new("Navigation", "Icon.Last", "Last Page", "Ctrl+End"),

            [ActionIds.EditUndo] = new("Edit", "Icon.Undo", "Undo", "Ctrl+Z"),
            [ActionIds.EditRedo] = new("Edit", "Icon.Redo", "Redo", "Ctrl+Y"),
            [ActionIds.EditCut] = new("Edit", "Icon.Cut", "Cut", "Ctrl+X"),
            [ActionIds.EditCopy] = new("Edit", "Icon.Copy", "Copy", "Ctrl+C"),
            [ActionIds.EditPaste] = new("Edit", "Icon.Paste", "Paste", "Ctrl+V"),
            [ActionIds.EditDelete] = new("Edit", "Icon.Delete", "Delete", "Delete"),
            [ActionIds.EditSelectAll] = new("Edit", "Icon.SelectAll", "Select All", "Ctrl+A"),

            [ActionIds.ViewZoomIn] = new("View", "Icon.ZoomIn", "Zoom In", "Ctrl++"),
            [ActionIds.ViewZoomOut] = new("View", "Icon.ZoomOut", "Zoom Out", "Ctrl+-"),
            [ActionIds.ViewZoomFit] = new("View", "Icon.ZoomFit", "Fit to Page", "Ctrl+0"),
            [ActionIds.ViewZoomReset] = new("View", "Icon.ZoomReset", "Reset Zoom", "Ctrl+1"),

            [ActionIds.FileSave] = new("File", "Icon.Save", "Save", "Ctrl+S"),
            [ActionIds.FileOpen] = new("File", "Icon.Open", "Open", "Ctrl+O"),
            [ActionIds.FileNew] = new("File", "Icon.New", "New Notebook", "Ctrl+N"),
            [ActionIds.FileExport] = new("File", "Icon.Export", "Export", "Ctrl+Shift+E"),

            [ActionIds.PageNew] = new("Page", "Icon.PageNew", "New Page", "Ctrl+Shift+N"),
            [ActionIds.PageDelete] = new("Page", "Icon.PageDelete", "Delete Page", null),
            [ActionIds.PageDuplicate] = new("Page", "Icon.PageDuplicate", "Duplicate Page", "Ctrl+D"),
        };

        /// <summary>
        /// Get metadata for an action ID.
        /// Returns null if action ID is not registered.
        /// </summary>
        public static ActionMetadata? GetMetadata(string actionId)
        {
            return _metadata.TryGetValue(actionId, out var meta) ? meta : null;
        }

        /// <summary>
        /// Get all registered action IDs.
        /// </summary>
        public static IEnumerable<string> GetAllActionIds() => _metadata.Keys;
    }
}
