using System;
using System.Threading.Tasks;
using StylusCore.Core.Models;
using StylusCore.Core.Services;

namespace StylusCore.Core.Ports
{
    /// <summary>
    /// Port for automatic save scheduling.
    /// Implementations: BackgroundAutosaveService (Infrastructure)
    /// </summary>
    public interface IAutosaveScheduler
    {
        event EventHandler<AutoSaveEventArgs> SaveCompleted;

        void Start(Notebook notebook);
        void Stop();
        Task SaveNowAsync();

        int IntervalSeconds { get; set; }
        bool IsRunning { get; }
    }
}
