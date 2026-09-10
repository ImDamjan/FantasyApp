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
        private static readonly TimeSpan LiveGameweekWindow = TimeSpan.FromHours(48);

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
                try
                {
                    if (DateTime.UtcNow - _lastStaticSyncAt >= StaticSyncInterval)
                    {
                        await RunStaticSyncAsync();
                        _lastStaticSyncAt = DateTime.UtcNow;
                    }

                    await RunLiveSyncIfGameweekActiveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FPL sync loop failed.");
                }

                await Task.Delay(LiveSyncInterval, stoppingToken);
            }
        }

        private async Task RunStaticSyncAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IFplDataSyncService>();
            await syncService.SyncStaticDataAsync();
            _logger.LogInformation("FPL static data sync completed.");
        }

        private async Task RunLiveSyncIfGameweekActiveAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var gameweekRepository = scope.ServiceProvider.GetRequiredService<IGameweekRepository>();
            var currentGameweek = await gameweekRepository.GetCurrentAsync();

            if (currentGameweek == null || currentGameweek.IsFinished)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var windowEnd = currentGameweek.DeadlineTime.Add(LiveGameweekWindow);
            if (now < currentGameweek.DeadlineTime || now > windowEnd)
            {
                return;
            }

            var syncService = scope.ServiceProvider.GetRequiredService<IFplDataSyncService>();
            await syncService.SyncLiveGameweekAsync(currentGameweek.FplId);

            var scoringService = scope.ServiceProvider.GetRequiredService<IScoringService>();
            await scoringService.RecalculateGameweekScoresAsync(currentGameweek.Id);

            _logger.LogInformation("FPL live gameweek sync completed for gameweek {GameweekFplId}.", currentGameweek.FplId);
        }
    }
}
