using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StylusCore.Core.Enums;

namespace StylusCore.Core.Models
{
    /// <summary>
    /// Represents a notebook that contains pages.
    /// Example: "C# Project", "Biology 101"
    /// </summary>
    public class Notebook
    {
        /// <summary>
        /// Unique identifier for the notebook
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to parent library
        /// </summary>
        public Guid LibraryId { get; set; }

        /// <summary>
        /// Display name of the notebook
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Cover image or color identifier
        /// </summary>
        public string CoverId { get; set; }

        /// <summary>
        /// Default page format for new pages
        /// </summary>
        public PageFormat DefaultPageFormat { get; set; }

        /// <summary>
        /// Default page template for new pages
        /// </summary>
        public PageTemplate DefaultPageTemplate { get; set; }

        /// <summary>
        /// Default page orientation for new pages
        /// </summary>
        public PageOrientation DefaultPageOrientation { get; set; }

        /// <summary>
        /// Cover color for the notebook card
        /// </summary>
        public CoverColor CoverColor { get; set; }

        /// <summary>
        /// Date when the notebook was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the notebook was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Collection of pages in this notebook (ObservableCollection for UI binding)
        /// </summary>
        public ObservableCollection<Page> Pages { get; set; }

        /// <summary>
        /// Collection of section markers in this notebook (ObservableCollection for UI binding)
        /// </summary>
        public ObservableCollection<Section> Sections { get; set; }

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; }

        public Notebook()
        {
            Id = Guid.NewGuid();
            LibraryId = Guid.Empty;
            Name = string.Empty;
            Description = string.Empty;
            CoverId = string.Empty;
            DefaultPageFormat = PageFormat.A4;
            DefaultPageTemplate = PageTemplate.Blank;
            DefaultPageOrientation = PageOrientation.Portrait;
            CoverColor = CoverColor.Purple;
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
            Pages = new ObservableCollection<Page>();
            Sections = new ObservableCollection<Section>();
            SortOrder = 0;
        }
    }
}
