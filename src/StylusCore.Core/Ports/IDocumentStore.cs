using System;
using System.Threading.Tasks;
using StylusCore.Core.Models;

namespace StylusCore.Core.Ports
{
    /// <summary>
    /// Port for document persistence operations.
    /// Implementations: SqliteDocumentStore (Infrastructure)
    /// </summary>
    public interface IDocumentStore
    {
        Task SaveLibraryAsync(Library library);
        Task<Library> LoadLibraryAsync(Guid libraryId);
        Task<Library[]> GetAllLibrariesAsync();
        Task DeleteLibraryAsync(Guid libraryId);

        Task SaveNotebookAsync(Notebook notebook);
        Task<Notebook> LoadNotebookAsync(Guid notebookId);
        Task DeleteNotebookAsync(Guid notebookId);

        Task SavePageAsync(Page page);
        Task<Page> LoadPageAsync(Guid pageId);
    }
}
