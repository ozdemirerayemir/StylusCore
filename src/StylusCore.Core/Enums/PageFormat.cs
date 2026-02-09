namespace StylusCore.Core.Enums
{
    /// <summary>
    /// Defines the page format/size.
    /// </summary>
    public enum PageFormat
    {
        /// <summary>
        /// A4 size (210 x 297 mm)
        /// </summary>
        A4,

        /// <summary>
        /// A3 size (297 x 420 mm)
        /// </summary>
        A3,

        /// <summary>
        /// A5 size (148 x 210 mm)
        /// </summary>
        A5,

        /// <summary>
        /// Letter size (8.5 x 11 inches)
        /// </summary>
        Letter,

        /// <summary>
        /// Infinite canvas - no boundaries
        /// </summary>
        Infinite
    }

    /// <summary>
    /// Defines the page template/background style.
    /// </summary>
    public enum PageTemplate
    {
        /// <summary>
        /// Plain white/blank background
        /// </summary>
        Blank,

        /// <summary>
        /// Horizontal lined paper
        /// </summary>
        Lined,

        /// <summary>
        /// Grid/squared paper
        /// </summary>
        Grid,

        /// <summary>
        /// Dotted paper
        /// </summary>
        Dotted,

        /// <summary>
        /// Cornell note-taking format
        /// </summary>
        Cornell
    }

    /// <summary>
    /// Defines the ribbon/toolbar display mode.
    /// </summary>
    public enum RibbonMode
    {
        /// <summary>
        /// Full ribbon - all tabs and tools visible
        /// </summary>
        Full,

        /// <summary>
        /// Tabs only - click to expand
        /// </summary>
        TabsOnly,

        /// <summary>
        /// FullScreen mode - minimal UI, maximum canvas space
        /// </summary>
        FullScreen
    }

    /// <summary>
    /// Defines the eraser mode type.
    /// </summary>
    public enum EraserMode
    {
        /// <summary>
        /// Normal eraser - erase by area
        /// </summary>
        Normal,

        /// <summary>
        /// Stroke eraser - erase entire stroke on touch
        /// </summary>
        Stroke,

        /// <summary>
        /// Lasso eraser - erase everything inside drawn shape
        /// </summary>
        Lasso
    }

    /// <summary>
    /// Defines the radial menu activation mode.
    /// </summary>
    public enum RadialMenuActivation
    {
        /// <summary>
        /// Click to select item
        /// </summary>
        Click,

        /// <summary>
        /// Release to select hovered item
        /// </summary>
        Hold
    }

    /// <summary>
    /// Defines the page orientation.
    /// </summary>
    public enum PageOrientation
    {
        /// <summary>
        /// Portrait orientation (vertical)
        /// </summary>
        Portrait,

        /// <summary>
        /// Landscape orientation (horizontal)
        /// </summary>
        Landscape
    }

    /// <summary>
    /// Predefined cover colors for notebooks.
    /// </summary>
    public enum CoverColor
    {
        Purple,
        Blue,
        Green,
        Yellow,
        Red,
        Pink,
        Orange,
        Teal
    }
}
