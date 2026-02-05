using System;
using System.Threading;
using System.Threading.Tasks;
using StylusCore.Core.Models;

namespace StylusCore.Core.Services
{
    /// <summary>
    /// Service for automatic periodic saving of notebooks.
    /// </summary>
    public interface IAutoSaveService
    {
        /// <summary>
        /// Event fired when auto-save completes
        /// </summary>
        event EventHandler<AutoSaveEventArgs> SaveCompleted;

        /// <summary>
        /// Start auto-save timer
        /// </summary>
        void Start(Notebook notebook);

        /// <summary>
        /// Stop auto-save timer
        /// </summary>
        void Stop();

        /// <summary>
        /// Trigger immediate save
        /// </summary>
        Task SaveNowAsync();

        /// <summary>
        /// Get or set the auto-save interval in seconds
        /// </summary>
        int IntervalSeconds { get; set; }

        /// <summary>
        /// Whether auto-save is currently running
        /// </summary>
        bool IsRunning { get; }
    }

    /// <summary>
    /// Event args for auto-save completion
    /// </summary>
    public class AutoSaveEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public DateTime SaveTime { get; set; }
        public string Message { get; set; }

        public AutoSaveEventArgs(bool success, string message = "")
        {
            Success = success;
            SaveTime = DateTime.Now;
            Message = message;
        }
    }

    /// <summary>
    /// Implementation of auto-save service
    /// </summary>
    public class AutoSaveService : IAutoSaveService, Ports.IAutosaveScheduler, IDisposable
    {
        private readonly IStorageService _storageService;
        private Timer _timer;
        private Notebook _currentNotebook;
        private bool _isRunning;

        public event EventHandler<AutoSaveEventArgs> SaveCompleted;

        public int IntervalSeconds { get; set; } = 60; // Default: 1 minute
        public bool IsRunning => _isRunning;

        public AutoSaveService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public void Start(Notebook notebook)
        {
            _currentNotebook = notebook;
            _timer?.Dispose();
            _timer = new Timer(
                async _ => await AutoSaveCallback(),
                null,
                TimeSpan.FromSeconds(IntervalSeconds),
                TimeSpan.FromSeconds(IntervalSeconds)
            );
            _isRunning = true;
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            _isRunning = false;
        }

        public async Task SaveNowAsync()
        {
            await AutoSaveCallback();
        }

        private async Task AutoSaveCallback()
        {
            if (_currentNotebook == null) return;

            try
            {
                await _storageService.SaveNotebookAsync(_currentNotebook);
                _currentNotebook.ModifiedAt = DateTime.Now;
                SaveCompleted?.Invoke(this, new AutoSaveEventArgs(true, "Saved"));
            }
            catch (Exception ex)
            {
                SaveCompleted?.Invoke(this, new AutoSaveEventArgs(false, ex.Message));
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
