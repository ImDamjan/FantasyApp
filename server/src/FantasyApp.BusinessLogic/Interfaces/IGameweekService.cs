using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Gameweeks;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IGameweekService
    {
        Task<List<GameweekDeadlineDto>> GetUpcomingDeadlinesAsync();
    }
}
