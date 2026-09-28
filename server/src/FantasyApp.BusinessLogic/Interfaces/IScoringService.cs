using System.Threading.Tasks;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IScoringService
    {
        Task RecalculateGameweekScoresAsync(long gameweekId);
    }
}
