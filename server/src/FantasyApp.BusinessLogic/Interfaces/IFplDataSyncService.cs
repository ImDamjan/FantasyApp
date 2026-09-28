using System.Threading.Tasks;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IFplDataSyncService
    {
        Task SyncStaticDataAsync();
        Task SyncLiveGameweekAsync(int gameweekFplId);
    }
}
