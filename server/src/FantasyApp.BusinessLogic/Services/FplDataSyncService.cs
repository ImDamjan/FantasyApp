using System;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Common.Interfaces;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class FplDataSyncService : IFplDataSyncService
    {
        private readonly IFplApiClient _fplApiClient;
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameweekRepository _gameweekRepository;
        private readonly IFixtureRepository _fixtureRepository;
        private readonly IPlayerGameweekStatRepository _playerGameweekStatRepository;

        public FplDataSyncService(
            IFplApiClient fplApiClient,
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IGameweekRepository gameweekRepository,
            IFixtureRepository fixtureRepository,
            IPlayerGameweekStatRepository playerGameweekStatRepository)
        {
            _fplApiClient = fplApiClient;
            _teamRepository = teamRepository;
            _playerRepository = playerRepository;
            _gameweekRepository = gameweekRepository;
            _fixtureRepository = fixtureRepository;
            _playerGameweekStatRepository = playerGameweekStatRepository;
        }

        public async Task SyncStaticDataAsync()
        {
            var bootstrap = await _fplApiClient.GetBootstrapStaticAsync();

            foreach (var fplTeam in bootstrap.Teams)
            {
                var team = await _teamRepository.GetByFplIdAsync(fplTeam.Id);
                if (team == null)
                {
                    team = new Team { FplId = fplTeam.Id };
                    await _teamRepository.AddAsync(team);
                }

                team.Name = fplTeam.Name;
                team.ShortName = fplTeam.ShortName;
                team.StrengthOverallHome = fplTeam.StrengthOverallHome;
                team.StrengthOverallAway = fplTeam.StrengthOverallAway;
            }

            await _teamRepository.SaveChangesAsync();

            foreach (var fplEvent in bootstrap.Events)
            {
                var gameweek = await _gameweekRepository.GetByFplIdAsync(fplEvent.Id);
                if (gameweek == null)
                {
                    gameweek = new Gameweek { FplId = fplEvent.Id };
                    await _gameweekRepository.AddAsync(gameweek);
                }

                gameweek.Name = fplEvent.Name;
                gameweek.DeadlineTime = fplEvent.DeadlineTime;
                gameweek.IsCurrent = fplEvent.IsCurrent;
                gameweek.IsNext = fplEvent.IsNext;
                gameweek.IsFinished = fplEvent.Finished;
            }

            await _gameweekRepository.SaveChangesAsync();

            foreach (var fplElement in bootstrap.Elements)
            {
                var team = await _teamRepository.GetByFplIdAsync(fplElement.Team);
                if (team == null)
                {
                    continue;
                }

                var player = await _playerRepository.GetByFplIdAsync(fplElement.Id);
                if (player == null)
                {
                    player = new Player { FplId = fplElement.Id };
                    await _playerRepository.AddAsync(player);
                }

                player.FirstName = fplElement.FirstName;
                player.SecondName = fplElement.SecondName;
                player.WebName = fplElement.WebName;
                player.Position = (PlayerPosition)fplElement.ElementType;
                player.TeamId = team.Id;
                player.PriceTenths = fplElement.NowCost;
                player.TotalPoints = fplElement.TotalPoints;
                player.Form = decimal.TryParse(fplElement.Form, out var form) ? form : 0;
                player.Status = fplElement.Status;
                player.ChanceOfPlayingThisRound = fplElement.ChanceOfPlayingThisRound;
                player.LastSyncedAt = DateTime.UtcNow;
            }

            await _playerRepository.SaveChangesAsync();

            var fixtures = await _fplApiClient.GetFixturesAsync();

            foreach (var fplFixture in fixtures)
            {
                var homeTeam = await _teamRepository.GetByFplIdAsync(fplFixture.TeamH);
                var awayTeam = await _teamRepository.GetByFplIdAsync(fplFixture.TeamA);
                if (homeTeam == null || awayTeam == null)
                {
                    continue;
                }

                Gameweek? gameweek = null;
                if (fplFixture.Event.HasValue)
                {
                    gameweek = await _gameweekRepository.GetByFplIdAsync(fplFixture.Event.Value);
                }

                var fixture = await _fixtureRepository.GetByFplIdAsync(fplFixture.Id);
                if (fixture == null)
                {
                    fixture = new Fixture { FplId = fplFixture.Id };
                    await _fixtureRepository.AddAsync(fixture);
                }

                fixture.GameweekId = gameweek?.Id;
                fixture.HomeTeamId = homeTeam.Id;
                fixture.AwayTeamId = awayTeam.Id;
                fixture.HomeScore = fplFixture.TeamHScore;
                fixture.AwayScore = fplFixture.TeamAScore;
                fixture.KickoffTime = fplFixture.KickoffTime;
                fixture.HomeDifficulty = fplFixture.TeamHDifficulty;
                fixture.AwayDifficulty = fplFixture.TeamADifficulty;
                fixture.IsFinished = fplFixture.Finished;
            }

            await _fixtureRepository.SaveChangesAsync();
        }

        public async Task SyncLiveGameweekAsync(int gameweekFplId)
        {
            var gameweek = await _gameweekRepository.GetByFplIdAsync(gameweekFplId);
            if (gameweek == null)
            {
                return;
            }

            var live = await _fplApiClient.GetGameweekLiveAsync(gameweekFplId);

            foreach (var element in live.Elements)
            {
                var player = await _playerRepository.GetByFplIdAsync(element.Id);
                if (player == null)
                {
                    continue;
                }

                var stat = await _playerGameweekStatRepository.GetAsync(player.Id, gameweek.Id);
                if (stat == null)
                {
                    stat = new PlayerGameweekStat { PlayerId = player.Id, GameweekId = gameweek.Id };
                    await _playerGameweekStatRepository.AddAsync(stat);
                }

                stat.TotalPoints = element.Stats.TotalPoints;
                stat.Minutes = element.Stats.Minutes;
                stat.GoalsScored = element.Stats.GoalsScored;
                stat.Assists = element.Stats.Assists;
                stat.CleanSheets = element.Stats.CleanSheets;
                stat.GoalsConceded = element.Stats.GoalsConceded;
                stat.Saves = element.Stats.Saves;
                stat.Bonus = element.Stats.Bonus;
                stat.YellowCards = element.Stats.YellowCards;
                stat.RedCards = element.Stats.RedCards;
            }

            await _playerGameweekStatRepository.SaveChangesAsync();
        }
    }
}
