using System.Threading.Tasks;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IGameweekSnapshotService
    {
        Task EnsureSnapshotsAsync();
    }
}
