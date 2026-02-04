using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using StylusCore.Core.Models;
using LibraryModel = StylusCore.Core.Models.Library;
using StylusCore.Core.Services;

namespace StylusCore.App.Features.Library.ViewModels
{
    /// <summary>
    /// ViewModel for the Library view.
    /// Manages libraries, notebooks, and navigation.
    /// </summary>
    public class LibraryViewModel : INotifyPropertyChanged
    {
        private readonly IStorageService _storageService;

        #region Properties

        private ObservableCollection<LibraryModel> _libraries;
        /// <summary>
        /// All libraries
        /// </summary>
        public ObservableCollection<LibraryModel> Libraries
        {
            get => _libraries;
            set
            {
                _libraries = value;
                OnPropertyChanged();
            }
        }

        private LibraryModel _selectedLibrary;
        /// <summary>
        /// Currently selected library
        /// </summary>
        public LibraryModel SelectedLibrary
        {
            get => _selectedLibrary;
            set
            {
                _selectedLibrary = value;
                OnPropertyChanged();
                SelectedLibraryChanged?.Invoke(this, value);
            }
        }

        private Notebook _selectedNotebook;
        /// <summary>
        /// Currently selected notebook
        /// </summary>
        public Notebook SelectedNotebook
        {
            get => _selectedNotebook;
            set
            {
                _selectedNotebook = value;
                OnPropertyChanged();
                SelectedNotebookChanged?.Invoke(this, value);
            }
        }

        private bool _isLoading;
        /// <summary>
        /// Whether data is loading
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Events

        public event EventHandler<LibraryModel> SelectedLibraryChanged;
        public event EventHandler<Notebook> SelectedNotebookChanged;
        public event EventHandler<Notebook> NotebookOpenRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public LibraryViewModel(IStorageService storageService)
        {
            _storageService = storageService;
            Libraries = new ObservableCollection<LibraryModel>();
        }
        
        // Constructor that allows injecting validation service (TODO: Use Dependency Injection properly later)
        private readonly IValidationService _validationService;
        public LibraryViewModel(IStorageService storageService, IValidationService validationService) : this(storageService)
        {
            _validationService = validationService;
        }

        public LibraryViewModel() : this(new StorageService(), new ValidationService())
        {
        }

        // ...

        /// <summary>
        /// Load all libraries from storage
        /// </summary>
        public async Task LoadLibrariesAsync()
        {
            IsLoading = true;
            try
            {
                var libraries = await _storageService.GetAllLibrariesAsync();
                Libraries.Clear();
                foreach (var lib in libraries)
                {
                    Libraries.Add(lib);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Create a new library
        /// </summary>
        public async Task<LibraryModel> CreateLibraryAsync(string name)
        {
            // Validate
            if (_validationService != null)
            {
                var result = _validationService.ValidateLibraryName(name, _libraries);
                if (!result.IsValid)
                {
                    // For now, throw exception or handle UI side. 
                    // To keep it simple for the first iteration, we'll throw, and the View can catch/display.
                    throw new ArgumentException(result.ErrorMessage);
                }
            }

            var library = new LibraryModel
            {
                Name = name
            };
            await _storageService.SaveLibraryAsync(library);
            Libraries.Add(library);
            return library;
        }

        /// <summary>
        /// Create a new notebook in the selected library
        /// </summary>
        public async Task<Notebook> CreateNotebookAsync(string name)
        {
            if (SelectedLibrary == null) return null;

            // Validate
            if (_validationService != null)
            {
                // We need all notebooks to check for uniqueness. 
                // Currently only Loaded notebooks are checking against. Ideally should check against DB.
                // Since StorageService is InMemory now, we can check against SelectedLibrary.Notebooks or fetch all.
                // For simplified scope: Check against SelectedLibrary.Notebooks
                var result = _validationService.ValidateNotebookName(name, SelectedLibrary.Id, SelectedLibrary.Notebooks);
                if (!result.IsValid)
                {
                     throw new ArgumentException(result.ErrorMessage);
                }
            }

            var notebook = new Notebook
            {
                Name = name,
                LibraryId = SelectedLibrary.Id
            };
            await _storageService.SaveNotebookAsync(notebook);
            SelectedLibrary.Notebooks.Add(notebook);
            OnPropertyChanged(nameof(SelectedLibrary));
            return notebook;
        }

        /// <summary>
        /// Delete a library
        /// </summary>
        public async Task DeleteLibraryAsync(LibraryModel library)
        {
            await _storageService.DeleteLibraryAsync(library.Id);
            Libraries.Remove(library);
            if (SelectedLibrary == library)
            {
                SelectedLibrary = null;
            }
        }

        /// <summary>
        /// Delete a notebook
        /// </summary>
        public async Task DeleteNotebookAsync(Notebook notebook)
        {
            await _storageService.DeleteNotebookAsync(notebook.Id);
            SelectedLibrary?.Notebooks.Remove(notebook);
            if (SelectedNotebook == notebook)
            {
                SelectedNotebook = null;
            }
        }

        /// <summary>
        /// Open a notebook for editing
        /// </summary>
        public void OpenNotebook(Notebook notebook)
        {
            NotebookOpenRequested?.Invoke(this, notebook);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
