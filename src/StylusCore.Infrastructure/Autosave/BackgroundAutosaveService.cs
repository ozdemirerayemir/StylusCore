using System;
using System.Threading;
using System.Threading.Tasks;
using StylusCore.Core.Models;
using StylusCore.Core.Ports;
using StylusCore.Core.Services;

namespace StylusCore.Infrastructure.Autosave
{
    /// <summary>
    /// Background autosave service using immutable snapshots.
    /// Implements IAutosaveScheduler from Core/Ports.
    /// </summary>
    public class BackgroundAutosaveService : IAutosaveScheduler, IDisposable
    {
        private readonly IDocumentStore _documentStore;
        private Timer _timer;
        private Notebook _currentNotebook;
        private bool _isRunning;

        public event EventHandler<AutoSaveEventArgs> SaveCompleted;

        public int IntervalSeconds { get; set; } = 60;
        public bool IsRunning => _isRunning;

        public BackgroundAutosaveService(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
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
                await _documentStore.SaveNotebookAsync(_currentNotebook);
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
