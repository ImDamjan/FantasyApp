using System;
using System.Threading;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FantasyApp.Api.BackgroundServices
{
    public class FplSyncService : BackgroundService
    {
        private static readonly TimeSpan StaticSyncInterval = TimeSpan.FromMinutes(60);
        private static readonly TimeSpan LiveSyncInterval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FplSyncService> _logger;

        private DateTime _lastStaticSyncAt = DateTime.MinValue;

        public FplSyncService(IServiceScopeFactory scopeFactory, ILogger<FplSyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (DateTime.UtcNow - _lastStaticSyncAt >= StaticSyncInterval &&
                    await RunSafelyAsync(RunStaticSyncAsync, "static data sync"))
                {
                    _lastStaticSyncAt = DateTime.UtcNow;
                }

                await RunSafelyAsync(RunSnapshotsAsync, "squad snapshot");
                await RunSafelyAsync(RunLiveSyncAsync, "live gameweek sync");

                await Task.Delay(LiveSyncInterval, stoppingToken);
            }
        }

        private async Task<bool> RunSafelyAsync(Func<Task> step, string stepName)
        {
            try
            {
                await step();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FPL {StepName} failed.", stepName);
                return false;
            }
        }

        private async Task RunStaticSyncAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IFplDataSyncService>();
            await syncService.SyncStaticDataAsync();
            _logger.LogInformation("FPL static data sync completed.");
        }

        private async Task RunSnapshotsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var snapshotService = scope.ServiceProvider.GetRequiredService<IGameweekSnapshotService>();
            await snapshotService.EnsureSnapshotsAsync();
        }

        private async Task RunLiveSyncAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var gameweekRepository = scope.ServiceProvider.GetRequiredService<IGameweekRepository>();
            var syncService = scope.ServiceProvider.GetRequiredService<IFplDataSyncService>();
            var scoringService = scope.ServiceProvider.GetRequiredService<IScoringService>();

            foreach (var gameweek in await gameweekRepository.GetStartedWithoutFinalScoresAsync())
            {
                await syncService.SyncLiveGameweekAsync(gameweek.FplId);
                await scoringService.RecalculateGameweekScoresAsync(gameweek.Id);

                _logger.LogInformation("FPL live sync completed for gameweek {GameweekFplId}.", gameweek.FplId);
            }
        }
    }
}
