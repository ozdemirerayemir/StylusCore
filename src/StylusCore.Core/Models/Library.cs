using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StylusCore.Core.Enums;

namespace StylusCore.Core.Models
{
    /// <summary>
    /// Represents a library (top-level container) that holds multiple notebooks.
    /// Example: "Software", "School", "Personal"
    /// </summary>
    public class Library : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private string _iconId;
        private ObservableCollection<Notebook> _notebooks;

        /// <summary>
        /// Unique identifier for the library
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display name of the library
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Optional description
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Library icon or color identifier (Legacy)
        /// </summary>
        public string IconId
        {
            get => _iconId;
            set
            {
                _iconId = value;
                OnPropertyChanged();
            }
        }

        private CoverColor _coverColor;
        /// <summary>
        /// Cover color for the library card
        /// </summary>
        public CoverColor CoverColor
        {
            get => _coverColor;
            set
            {
                _coverColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Date when the library was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the library was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Collection of notebooks in this library (ObservableCollection for UI binding)
        /// </summary>
        public ObservableCollection<Notebook> Notebooks
        {
            get => _notebooks;
            set
            {
                if (_notebooks != null)
                {
                    _notebooks.CollectionChanged -= Notebooks_CollectionChanged;
                }
                _notebooks = value;
                if (_notebooks != null)
                {
                    _notebooks.CollectionChanged += Notebooks_CollectionChanged;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(NotebookCount));
            }
        }

        /// <summary>
        /// Helper property for binding notebook count
        /// </summary>
        public int NotebookCount => Notebooks?.Count ?? 0;

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; }

        public Library()
        {
            Id = Guid.NewGuid();
            _name = string.Empty;
            _description = string.Empty;
            _iconId = string.Empty;
            _coverColor = CoverColor.Orange; // Default gradient
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
            Notebooks = new ObservableCollection<Notebook>();
            SortOrder = 0;
        }

        private void Notebooks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Notify UI that notebook count changed
            OnPropertyChanged(nameof(NotebookCount));
            OnPropertyChanged(nameof(Notebooks));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
