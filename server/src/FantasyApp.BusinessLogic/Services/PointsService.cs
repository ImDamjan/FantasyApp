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
        private readonly IGameweekPickRepository _pickRepository;

        public PointsService(
            IUserGameweekScoreRepository scoreRepository,
            IGameweekRepository gameweekRepository,
            IFantasyTeamRepository fantasyTeamRepository,
            IPlayerGameweekStatRepository playerGameweekStatRepository,
            IFixtureRepository fixtureRepository,
            IGameweekPickRepository pickRepository)
        {
            _scoreRepository = scoreRepository;
            _gameweekRepository = gameweekRepository;
            _fantasyTeamRepository = fantasyTeamRepository;
            _playerGameweekStatRepository = playerGameweekStatRepository;
            _fixtureRepository = fixtureRepository;
            _pickRepository = pickRepository;
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

        public async Task<SquadPointsDto> GetSquadPointsAsync(long userId)
        {
            var result = new SquadPointsDto();

            var fantasyTeam = await _fantasyTeamRepository.GetByUserIdWithSquadAsync(userId);
            if (fantasyTeam == null || !fantasyTeam.HasPickedInitialSquad)
            {
                return result;
            }

            var gameweek = await _gameweekRepository.GetCurrentAsync();
            var picks = gameweek != null
                ? await _pickRepository.GetForTeamAndGameweekAsync(fantasyTeam.Id, gameweek.Id)
                : new List<GameweekPick>();

            result.GameweekName = gameweek?.Name ?? string.Empty;
            result.IsScoring = picks.Count > 0;

            if (!result.IsScoring)
            {
                picks = fantasyTeam.SquadPlayers.Select(sp => new GameweekPick
                {
                    PlayerId = sp.PlayerId,
                    Player = sp.Player,
                    IsStarting = sp.IsStarting,
                    BenchOrder = sp.BenchOrder,
                    IsCaptain = sp.IsCaptain,
                    IsViceCaptain = sp.IsViceCaptain
                }).ToList();
            }

            var statsByPlayerId = result.IsScoring
                ? await _playerGameweekStatRepository.GetForGameweekAsync(gameweek!.Id)
                : new Dictionary<long, PlayerGameweekStat>();

            ChipType? chip = null;
            if (result.IsScoring)
            {
                chip = (await _scoreRepository.GetAsync(userId, gameweek!.Id))?.ChipUsed;
                result.ChipUsed = chip?.ToString();
            }

            var teamIdsWithMatchesLeft = result.IsScoring
                ? await _fixtureRepository.GetTeamIdsWithMatchesLeftAsync(gameweek!.Id)
                : new HashSet<long>();
            var effectiveCaptainPlayerId = ScoringRules.ResolveEffectiveCaptain(picks, statsByPlayerId, teamIdsWithMatchesLeft);

            foreach (var pick in picks.OrderBy(p => p.IsStarting ? 0 : 1).ThenBy(p => p.BenchOrder))
            {
                var player = pick.Player;
                if (player == null)
                {
                    continue;
                }

                statsByPlayerId.TryGetValue(player.Id, out var stat);
                var multiplier = ScoringRules.GetMultiplier(pick, effectiveCaptainPlayerId, chip);
                var nextFixtures = await _fixtureRepository.GetUpcomingForTeamAsync(player.TeamId, 3);

                result.Players.Add(new SquadPlayerPointsDto
                {
                    PlayerId = player.Id,
                    WebName = player.WebName,
                    Position = player.Position.ToString(),
                    TeamShortName = player.Team?.ShortName ?? string.Empty,
                    PriceMillions = player.PriceTenths / 10m,
                    IsStarting = pick.IsStarting,
                    BenchOrder = pick.BenchOrder,
                    IsCaptain = pick.IsCaptain,
                    IsViceCaptain = pick.IsViceCaptain,
                    GameweekPoints = (stat?.TotalPoints ?? 0) * (multiplier == 0 ? 1 : multiplier),
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
    }
}
