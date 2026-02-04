namespace StylusCore.Core.Enums
{
    /// <summary>
    /// Defines the active drawing/editing tool.
    /// </summary>
    public enum ToolMode
    {
        /// <summary>
        /// Selection tool - select and move objects
        /// </summary>
        Selection,

        /// <summary>
        /// Pen tool - freehand drawing with pressure
        /// </summary>
        Pen,

        /// <summary>
        /// Highlighter tool - semi-transparent strokes
        /// </summary>
        Highlighter,

        /// <summary>
        /// Eraser tool - remove strokes
        /// </summary>
        Eraser,

        /// <summary>
        /// Shape tool - draw geometric shapes
        /// </summary>
        Shape,

        /// <summary>
        /// Text tool - insert and edit text
        /// </summary>
        Text,

        /// <summary>
        /// Lasso tool - select by drawing around
        /// </summary>
        Lasso,

        /// <summary>
        /// Ruler tool - draw straight lines at any angle
        /// </summary>
        Ruler
    }
}
