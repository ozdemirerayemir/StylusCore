using System.Threading.Tasks;
using StylusCore.Core.Models;

namespace StylusCore.Core.Ports
{
    /// <summary>
    /// Port for export operations.
    /// Implementations: PdfExportPipeline (Infrastructure)
    /// </summary>
    public interface IExportPipeline
    {
        Task<byte[]> ExportToPdfAsync(Notebook notebook);
        Task<byte[]> ExportPageToPdfAsync(Page page);
        Task<byte[]> ExportPageToImageAsync(Page page);
        Task SaveToFileAsync(byte[] data, string filePath);
    }
}
