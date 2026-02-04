using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StylusCore.Core.Models
{
    /// <summary>
    /// Represents a section marker within a notebook.
    /// Sections are pointers to specific pages that serve as chapter starts.
    /// </summary>
    public class Section : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Unique identifier for the section
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to parent notebook
        /// </summary>
        public Guid NotebookId { get; set; }

        /// <summary>
        /// Reference to the page where this section starts
        /// </summary>
        public Guid PageId { get; set; }

        private string _title;
        /// <summary>
        /// Display name/title of the section
        /// </summary>
        public string Title 
        { 
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _color;
        /// <summary>
        /// Optional color for the section marker
        /// </summary>
        public string Color 
        { 
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Optional icon identifier
        /// </summary>
        public string IconId { get; set; }

        /// <summary>
        /// Sort order for navigation display
        /// </summary>
        public int SortOrder { get; set; }

        private bool _isExpanded;
        /// <summary>
        /// Whether this section's pages are currently visible in the UI
        /// </summary>
        public bool IsExpanded 
        { 
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Date when the section was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public Section()
        {
            Id = Guid.NewGuid();
            NotebookId = Guid.Empty;
            PageId = Guid.Empty;
            Title = string.Empty;
            Color = "#000000";
            IconId = string.Empty;
            SortOrder = 0;
            CreatedAt = DateTime.Now;
        }
    }
}
