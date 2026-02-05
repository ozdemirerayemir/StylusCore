namespace StylusCore.Core.Actions
{
    /// <summary>
    /// Centralized action IDs for the application.
    /// These are stable string identifiers used for keybindings and command dispatch.
    /// 
    /// IMPORTANT: This file contains ONLY constants.
    /// Handler registration and dispatch logic belongs in Engine.Wpf, NOT here.
    /// </summary>
    public static class ActionIds
    {
        #region Tool Selection

        /// <summary>Select pen tool</summary>
        public const string ToolPen = "tool.pen";

        /// <summary>Select eraser tool</summary>
        public const string ToolEraser = "tool.eraser";

        /// <summary>Select highlighter tool</summary>
        public const string ToolHighlighter = "tool.highlighter";

        /// <summary>Select lasso selection tool</summary>
        public const string ToolLasso = "tool.lasso";

        /// <summary>Select shape tool</summary>
        public const string ToolShape = "tool.shape";

        /// <summary>Select text tool</summary>
        public const string ToolText = "tool.text";

        #endregion

        #region Navigation

        /// <summary>Navigate to next page</summary>
        public const string NavPageNext = "nav.page.next";

        /// <summary>Navigate to previous page</summary>
        public const string NavPagePrev = "nav.page.prev";

        /// <summary>Navigate to first page</summary>
        public const string NavPageFirst = "nav.page.first";

        /// <summary>Navigate to last page</summary>
        public const string NavPageLast = "nav.page.last";

        /// <summary>Go to specific page by number</summary>
        public const string NavPageGoto = "nav.page.goto";

        #endregion

        #region Edit Operations

        /// <summary>Undo last action</summary>
        public const string EditUndo = "edit.undo";

        /// <summary>Redo last undone action</summary>
        public const string EditRedo = "edit.redo";

        /// <summary>Cut selection</summary>
        public const string EditCut = "edit.cut";

        /// <summary>Copy selection</summary>
        public const string EditCopy = "edit.copy";

        /// <summary>Paste from clipboard</summary>
        public const string EditPaste = "edit.paste";

        /// <summary>Delete selection</summary>
        public const string EditDelete = "edit.delete";

        /// <summary>Select all on current page</summary>
        public const string EditSelectAll = "edit.select.all";

        #endregion

        #region View Operations

        /// <summary>Zoom in</summary>
        public const string ViewZoomIn = "view.zoom.in";

        /// <summary>Zoom out</summary>
        public const string ViewZoomOut = "view.zoom.out";

        /// <summary>Fit page to view</summary>
        public const string ViewZoomFit = "view.zoom.fit";

        /// <summary>Reset zoom to 100%</summary>
        public const string ViewZoomReset = "view.zoom.reset";

        /// <summary>Toggle fullscreen mode</summary>
        public const string ViewFullscreen = "view.fullscreen";

        #endregion

        #region File Operations

        /// <summary>Save current notebook</summary>
        public const string FileSave = "file.save";

        /// <summary>Open notebook</summary>
        public const string FileOpen = "file.open";

        /// <summary>Create new notebook</summary>
        public const string FileNew = "file.new";

        /// <summary>Export notebook</summary>
        public const string FileExport = "file.export";

        #endregion

        #region Page Operations

        /// <summary>Add new page after current</summary>
        public const string PageNew = "page.new";

        /// <summary>Delete current page</summary>
        public const string PageDelete = "page.delete";

        /// <summary>Duplicate current page</summary>
        public const string PageDuplicate = "page.duplicate";

        #endregion
    }
}
