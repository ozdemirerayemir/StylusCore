using System;
using System.Collections.Generic;
using StylusCore.Core.Enums;

namespace StylusCore.Core.Models
{
    /// <summary>
    /// Represents a single page within a notebook.
    /// Contains all content blocks (strokes, text, images, etc.)
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Unique identifier for the page
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to parent notebook
        /// </summary>
        public Guid NotebookId { get; set; }

        /// <summary>
        /// Reference to parent section (null = no section)
        /// </summary>
        public Guid? SectionId { get; set; }

        /// <summary>
        /// Page number within the notebook (1-indexed)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Page format/size
        /// </summary>
        public PageFormat Format { get; set; }

        /// <summary>
        /// Page background template
        /// </summary>
        public PageTemplate Template { get; set; }

        /// <summary>
        /// Page width in pixels (for rendering)
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Page height in pixels (for rendering)
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Collection of strokes (pen drawings) on this page
        /// </summary>
        public List<Stroke> Strokes { get; set; }

        /// <summary>
        /// Collection of text blocks on this page
        /// </summary>
        public List<TextBlock> TextBlocks { get; set; }

        /// <summary>
        /// Collection of image blocks on this page
        /// </summary>
        public List<ImageBlock> ImageBlocks { get; set; }

        /// <summary>
        /// Collection of shape objects on this page
        /// </summary>
        public List<ShapeBlock> ShapeBlocks { get; set; }

        /// <summary>
        /// Date when the page was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the page was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        public Page()
        {
            Id = Guid.NewGuid();
            NotebookId = Guid.Empty;
            PageNumber = 1;
            Format = PageFormat.A4;
            Template = PageTemplate.Blank;
            Width = 794;  // A4 at 96 DPI
            Height = 1123; // A4 at 96 DPI
            Strokes = new List<Stroke>();
            TextBlocks = new List<TextBlock>();
            ImageBlocks = new List<ImageBlock>();
            ShapeBlocks = new List<ShapeBlock>();
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents a text block on a page
    /// </summary>
    public class TextBlock
    {
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Content { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public string Color { get; set; }
        public int ZIndex { get; set; }

        public TextBlock()
        {
            Id = Guid.NewGuid();
            Content = string.Empty;
            FontFamily = "Segoe UI";
            FontSize = 14;
            Color = "#000000";
            ZIndex = 0;
        }
    }

    /// <summary>
    /// Represents an image block on a page
    /// </summary>
    public class ImageBlock
    {
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string ImagePath { get; set; }
        public byte[] ImageData { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }

        public ImageBlock()
        {
            Id = Guid.NewGuid();
            ImagePath = string.Empty;
            ImageData = Array.Empty<byte>();
            Rotation = 0;
            ZIndex = 0;
        }
    }

    /// <summary>
    /// Represents a shape block on a page
    /// </summary>
    public class ShapeBlock
    {
        public Guid Id { get; set; }
        public string ShapeType { get; set; } // Line, Arrow, Rectangle, Ellipse, Triangle
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double X2 { get; set; } // End point for lines/arrows
        public double Y2 { get; set; }
        public string StrokeColor { get; set; }
        public double StrokeWidth { get; set; }
        public string FillColor { get; set; }
        public bool HasArrowStart { get; set; }
        public bool HasArrowEnd { get; set; }
        public int ZIndex { get; set; }

        public ShapeBlock()
        {
            Id = Guid.NewGuid();
            ShapeType = "Rectangle";
            StrokeColor = "#000000";
            StrokeWidth = 2;
            FillColor = "transparent";
            ZIndex = 0;
        }
    }
}
