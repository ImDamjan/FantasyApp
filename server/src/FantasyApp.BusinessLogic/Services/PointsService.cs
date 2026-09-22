using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Players;
using FantasyApp.Entity.Dtos.Points;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class PointsService : IPointsService
    {
        private readonly IUserGameweekScoreRepository _scoreRepository;
        private readonly IGameweekRepository _gameweekRepository;
        private readonly IFantasyTeamRepository _fantasyTeamRepository;
        private readonly IPlayerGameweekStatRepository _playerGameweekStatRepository;
        private readonly IFixtureRepository _fixtureRepository;

        public PointsService(
            IUserGameweekScoreRepository scoreRepository,
            IGameweekRepository gameweekRepository,
            IFantasyTeamRepository fantasyTeamRepository,
            IPlayerGameweekStatRepository playerGameweekStatRepository,
            IFixtureRepository fixtureRepository)
        {
            _scoreRepository = scoreRepository;
            _gameweekRepository = gameweekRepository;
            _fantasyTeamRepository = fantasyTeamRepository;
            _playerGameweekStatRepository = playerGameweekStatRepository;
            _fixtureRepository = fixtureRepository;
        }

        public async Task<PointsSummaryDto> GetSummaryAsync(long userId, string username)
        {
            var totalPoints = (await _scoreRepository.GetTotalPointsByUserIdsAsync(new[] { userId }))
                .GetValueOrDefault(userId);

            var currentGameweek = await _gameweekRepository.GetCurrentAsync();

            var currentPoints = 0;
            var averagePoints = 0;
            var highestPoints = 0;
            if (currentGameweek != null)
            {
                var allGameweekPoints = await _scoreRepository.GetAllPointsForGameweekAsync(currentGameweek.Id);
                currentPoints = allGameweekPoints.GetValueOrDefault(userId);
                if (allGameweekPoints.Count > 0)
                {
                    averagePoints = (int)allGameweekPoints.Values.Average();
                    highestPoints = allGameweekPoints.Values.Max();
                }
            }

            var allTotals = await _scoreRepository.GetAllTotalPointsAsync();
            var overallRank = allTotals
                .Values
                .Count(v => v > totalPoints) + 1;

            var totalPlayers = (await _fantasyTeamRepository.GetAllWithSquadAsync()).Count;
            var fantasyTeam = await _fantasyTeamRepository.GetByUserIdAsync(userId);

            return new PointsSummaryDto
            {
                TeamName = fantasyTeam?.Name ?? string.Empty,
                ManagerName = username,
                CurrentGameweekName = currentGameweek?.Name ?? string.Empty,
                CurrentGameweekPoints = currentPoints,
                TotalPoints = totalPoints,
                AverageGameweekPoints = averagePoints,
                HighestGameweekPoints = highestPoints,
                OverallRank = overallRank,
                TotalPlayers = totalPlayers,
            };
        }

        public async Task<List<PointsHistoryItemDto>> GetHistoryAsync(long userId)
        {
            var scores = await _scoreRepository.GetByUserIdAsync(userId);

            return scores.Select(s => new PointsHistoryItemDto
            {
                GameweekName = s.Gameweek?.Name ?? string.Empty,
                RawPoints = s.RawPoints,
                TransferCost = s.TransferCost,
                NetPoints = s.NetPoints,
                ChipUsed = s.ChipUsed?.ToString(),
                IsFinal = s.IsFinal
            }).ToList();
        }

        public async Task<List<SquadPlayerPointsDto>> GetSquadPointsAsync(long userId)
        {
            var currentGameweek = await _gameweekRepository.GetCurrentAsync();
            return await GetSquadPointsForGameweekAsync(userId, currentGameweek);
        }

        private async Task<List<SquadPlayerPointsDto>> GetSquadPointsForGameweekAsync(long userId, Gameweek? gameweek)
        {
            var fantasyTeam = await _fantasyTeamRepository.GetByUserIdWithSquadAsync(userId);
            if (fantasyTeam == null || !fantasyTeam.HasPickedInitialSquad)
            {
                return new List<SquadPlayerPointsDto>();
            }

            var statsByPlayerId = gameweek != null
                ? await _playerGameweekStatRepository.GetForGameweekAsync(gameweek.Id)
                : new Dictionary<long, PlayerGameweekStat>();

            var effectiveCaptainPlayerId = ResolveEffectiveCaptain(fantasyTeam, statsByPlayerId);

            var result = new List<SquadPlayerPointsDto>();
            foreach (var squadPlayer in fantasyTeam.SquadPlayers)
            {
                var player = squadPlayer.Player;
                if (player == null)
                {
                    continue;
                }

                statsByPlayerId.TryGetValue(player.Id, out var stat);
                var points = stat?.TotalPoints ?? 0;
                if (player.Id == effectiveCaptainPlayerId)
                {
                    points *= 2;
                }

                var nextFixtures = await _fixtureRepository.GetUpcomingForTeamAsync(player.TeamId, 3);

                result.Add(new SquadPlayerPointsDto
                {
                    PlayerId = player.Id,
                    WebName = player.WebName,
                    Position = player.Position.ToString(),
                    TeamShortName = player.Team?.ShortName ?? string.Empty,
                    PriceMillions = player.PriceTenths / 10m,
                    IsStarting = squadPlayer.IsStarting,
                    IsCaptain = squadPlayer.IsCaptain,
                    IsViceCaptain = squadPlayer.IsViceCaptain,
                    GameweekPoints = points,
                    Minutes = stat?.Minutes ?? 0,
                    GoalsScored = stat?.GoalsScored ?? 0,
                    Assists = stat?.Assists ?? 0,
                    CleanSheets = stat?.CleanSheets ?? 0,
                    GoalsConceded = stat?.GoalsConceded ?? 0,
                    Saves = stat?.Saves ?? 0,
                    Bonus = stat?.Bonus ?? 0,
                    YellowCards = stat?.YellowCards ?? 0,
                    RedCards = stat?.RedCards ?? 0,
                    Form = player.Form,
                    NextFixtures = nextFixtures.Select(f => new UpcomingFixtureDto
                    {
                        IsHome = f.HomeTeamId == player.TeamId,
                        OpponentShortName = f.HomeTeamId == player.TeamId
                            ? f.AwayTeam?.ShortName ?? string.Empty
                            : f.HomeTeam?.ShortName ?? string.Empty,
                        Difficulty = f.HomeTeamId == player.TeamId ? f.HomeDifficulty : f.AwayDifficulty,
                        KickoffTime = f.KickoffTime
                    }).ToList()
                });
            }

            return result;
        }

        private static long? ResolveEffectiveCaptain(
            FantasyTeam team,
            Dictionary<long, PlayerGameweekStat> statsByPlayerId)
        {
            var captain = team.SquadPlayers.FirstOrDefault(sp => sp.IsCaptain);
            if (captain != null && statsByPlayerId.TryGetValue(captain.PlayerId, out var captainStat) && captainStat.Minutes > 0)
            {
                return captain.PlayerId;
            }

            var vice = team.SquadPlayers.FirstOrDefault(sp => sp.IsViceCaptain);
            if (vice != null && statsByPlayerId.TryGetValue(vice.PlayerId, out var viceStat) && viceStat.Minutes > 0)
            {
                return vice.PlayerId;
            }

            return null;
        }
    }
}
