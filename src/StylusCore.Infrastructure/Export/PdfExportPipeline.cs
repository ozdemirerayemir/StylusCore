using System;
using System.IO;
using System.Threading.Tasks;
using StylusCore.Core.Models;
using StylusCore.Core.Ports;

namespace StylusCore.Infrastructure.Export
{
    /// <summary>
    /// PDF/Image export pipeline.
    /// Implements IExportPipeline from Core/Ports.
    /// </summary>
    public class PdfExportPipeline : IExportPipeline
    {
        public async Task<byte[]> ExportToPdfAsync(Notebook notebook)
        {
            // TODO: Implement PDF export using PdfSharp or iTextSharp
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExportPageToPdfAsync(Page page)
        {
            // TODO: Implement single page PDF export
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExportPageToImageAsync(Page page)
        {
            // TODO: Implement PNG export
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        public async Task SaveToFileAsync(byte[] data, string filePath)
        {
            await File.WriteAllBytesAsync(filePath, data);
        }
    }
}
