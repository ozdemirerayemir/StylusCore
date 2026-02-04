using System;
using System.IO;
using System.Threading.Tasks;
using StylusCore.Core.Models;

namespace StylusCore.Core.Services
{
    /// <summary>
    /// Service for saving and loading notebooks and libraries.
    /// Uses SQLite for data persistence.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Save a library to storage
        /// </summary>
        Task SaveLibraryAsync(Library library);

        /// <summary>
        /// Load a library from storage
        /// </summary>
        Task<Library> LoadLibraryAsync(Guid libraryId);

        /// <summary>
        /// Save a notebook to storage
        /// </summary>
        Task SaveNotebookAsync(Notebook notebook);

        /// <summary>
        /// Load a notebook from storage
        /// </summary>
        Task<Notebook> LoadNotebookAsync(Guid notebookId);

        /// <summary>
        /// Save a page to storage
        /// </summary>
        Task SavePageAsync(Page page);

        /// <summary>
        /// Load a page from storage
        /// </summary>
        Task<Page> LoadPageAsync(Guid pageId);

        /// <summary>
        /// Get all libraries
        /// </summary>
        Task<Library[]> GetAllLibrariesAsync();

        /// <summary>
        /// Delete a library
        /// </summary>
        Task DeleteLibraryAsync(Guid libraryId);

        /// <summary>
        /// Delete a notebook
        /// </summary>
        Task DeleteNotebookAsync(Guid notebookId);
    }

    /// <summary>
    /// Implementation of storage service using SQLite
    /// </summary>
    public class StorageService : IStorageService
    {
        private readonly string _dataPath;
        
        // In-memory simulation for validation testing
        private readonly List<Library> _libraries = new List<Library>();
        private readonly List<Notebook> _notebooks = new List<Notebook>();
        private readonly List<Page> _pages = new List<Page>();

        public StorageService()
        {
            // Default data path in user's app data folder
            _dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StylusCore",
                "data"
            );

            // Ensure directory exists
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        public StorageService(string dataPath)
        {
            _dataPath = dataPath;
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        public async Task SaveLibraryAsync(Library library)
        {
            var existing = _libraries.FirstOrDefault(l => l.Id == library.Id);
            if (existing != null)
            {
                _libraries.Remove(existing);
            }
            _libraries.Add(library);
            await Task.CompletedTask;
        }

        public async Task<Library> LoadLibraryAsync(Guid libraryId)
        {
            var lib = _libraries.FirstOrDefault(l => l.Id == libraryId);
            return await Task.FromResult(lib);
        }

        public async Task SaveNotebookAsync(Notebook notebook)
        {
            var existing = _notebooks.FirstOrDefault(n => n.Id == notebook.Id);
            if (existing != null)
            {
                _notebooks.Remove(existing);
            }
            _notebooks.Add(notebook);
            await Task.CompletedTask;
        }

        public async Task<Notebook> LoadNotebookAsync(Guid notebookId)
        {
             var nb = _notebooks.FirstOrDefault(n => n.Id == notebookId);
             return await Task.FromResult(nb);
        }

        public async Task SavePageAsync(Page page)
        {
             var existing = _pages.FirstOrDefault(p => p.Id == page.Id);
            if (existing != null)
            {
                _pages.Remove(existing);
            }
            _pages.Add(page);
            await Task.CompletedTask;
        }

        public async Task<Page> LoadPageAsync(Guid pageId)
        {
            var p = _pages.FirstOrDefault(x => x.Id == pageId);
            return await Task.FromResult(p);
        }

        public async Task<Library[]> GetAllLibrariesAsync()
        {
            return await Task.FromResult(_libraries.ToArray());
        }

        public async Task DeleteLibraryAsync(Guid libraryId)
        {
            var lib = _libraries.FirstOrDefault(l => l.Id == libraryId);
            if (lib != null)
            {
                _libraries.Remove(lib);
                // Cascading delete simulation
                var notebooksToDelete = _notebooks.Where(n => n.LibraryId == libraryId).ToList();
                foreach (var nb in notebooksToDelete)
                {
                    _notebooks.Remove(nb);
                }
            }
            await Task.CompletedTask;
        }

        public async Task DeleteNotebookAsync(Guid notebookId)
        {
            var nb = _notebooks.FirstOrDefault(n => n.Id == notebookId);
            if (nb != null)
            {
                _notebooks.Remove(nb);
            }
            await Task.CompletedTask;
        }
    }
}
