using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Squad;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class SquadService : ISquadService
    {
        private const int SquadSize = 15;
        private const int StartingSize = 11;
        private const int RequiredGoalkeepers = 2;
        private const int RequiredDefenders = 5;
        private const int RequiredMidfielders = 5;
        private const int RequiredForwards = 3;
        private const int MaxPlayersPerClub = 3;

        private readonly IFantasyTeamRepository _fantasyTeamRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameweekRepository _gameweekRepository;

        public SquadService(
            IFantasyTeamRepository fantasyTeamRepository,
            IPlayerRepository playerRepository,
            IGameweekRepository gameweekRepository)
        {
            _fantasyTeamRepository = fantasyTeamRepository;
            _playerRepository = playerRepository;
            _gameweekRepository = gameweekRepository;
        }

        public async Task<ServiceResult<SquadDto>> GetSquadAsync(long userId)
        {
            var fantasyTeam = await GetOrCreateFantasyTeamAsync(userId);
            return ServiceResult<SquadDto>.Success(ToSquadDto(fantasyTeam));
        }

        public async Task<ServiceResult<SquadDto>> PickInitialSquadAsync(long userId, PickSquadRequestDto request)
        {
            var fantasyTeam = await GetOrCreateFantasyTeamAsync(userId);
            if (fantasyTeam.HasPickedInitialSquad)
            {
                return ServiceResult<SquadDto>.Failure("Squad already picked. Use transfers or lineup changes instead.");
            }

            if (request.PlayerIds.Count != SquadSize || request.PlayerIds.Distinct().Count() != SquadSize)
            {
                return ServiceResult<SquadDto>.Failure($"Squad must have exactly {SquadSize} distinct players.");
            }

            var players = await _playerRepository.GetByIdsAsync(request.PlayerIds);
            if (players.Count != SquadSize)
            {
                return ServiceResult<SquadDto>.Failure("One or more selected players were not found.");
            }

            var compositionError = ValidateSquadComposition(players);
            if (compositionError != null)
            {
                return ServiceResult<SquadDto>.Failure(compositionError);
            }

            var totalCostTenths = players.Sum(p => p.PriceTenths);
            if (totalCostTenths > fantasyTeam.BudgetRemainingTenths)
            {
                return ServiceResult<SquadDto>.Failure("Squad cost exceeds available budget.");
            }

            var lineupError = ValidateLineup(
                players, request.StartingPlayerIds, request.BenchOrder, request.CaptainPlayerId, request.ViceCaptainPlayerId);
            if (lineupError != null)
            {
                return ServiceResult<SquadDto>.Failure(lineupError);
            }

            var startingIds = request.StartingPlayerIds.ToHashSet();
            var benchOrderByPlayer = request.BenchOrder.ToDictionary(b => b.PlayerId, b => b.Order);

            foreach (var player in players)
            {
                fantasyTeam.SquadPlayers.Add(new SquadPlayer
                {
                    PlayerId = player.Id,
                    Player = player,
                    IsStarting = startingIds.Contains(player.Id),
                    BenchOrder = benchOrderByPlayer.TryGetValue(player.Id, out var order) ? order : null,
                    IsCaptain = player.Id == request.CaptainPlayerId,
                    IsViceCaptain = player.Id == request.ViceCaptainPlayerId,
                    PurchasePriceTenths = player.PriceTenths
                });
            }

            fantasyTeam.BudgetRemainingTenths -= totalCostTenths;
            fantasyTeam.HasPickedInitialSquad = true;

            await _fantasyTeamRepository.SaveChangesAsync();

            return ServiceResult<SquadDto>.Success(ToSquadDto(fantasyTeam));
        }

        public async Task<ServiceResult<SquadDto>> UpdateLineupAsync(long userId, UpdateLineupRequestDto request)
        {
            var fantasyTeam = await GetOrCreateFantasyTeamAsync(userId);
            if (!fantasyTeam.HasPickedInitialSquad)
            {
                return ServiceResult<SquadDto>.Failure("Pick your initial squad before editing the lineup.");
            }

            var squadPlayers = fantasyTeam.SquadPlayers.Where(sp => sp.Player != null).Select(sp => sp.Player!).ToList();

            var lineupError = ValidateLineup(
                squadPlayers, request.StartingPlayerIds, request.BenchOrder, request.CaptainPlayerId, request.ViceCaptainPlayerId);
            if (lineupError != null)
            {
                return ServiceResult<SquadDto>.Failure(lineupError);
            }

            var startingIds = request.StartingPlayerIds.ToHashSet();
            var benchOrderByPlayer = request.BenchOrder.ToDictionary(b => b.PlayerId, b => b.Order);

            foreach (var squadPlayer in fantasyTeam.SquadPlayers)
            {
                squadPlayer.IsStarting = startingIds.Contains(squadPlayer.PlayerId);
                squadPlayer.BenchOrder = benchOrderByPlayer.TryGetValue(squadPlayer.PlayerId, out var order) ? order : null;
                squadPlayer.IsCaptain = squadPlayer.PlayerId == request.CaptainPlayerId;
                squadPlayer.IsViceCaptain = squadPlayer.PlayerId == request.ViceCaptainPlayerId;
            }

            await _fantasyTeamRepository.SaveChangesAsync();

            return ServiceResult<SquadDto>.Success(ToSquadDto(fantasyTeam));
        }

        public async Task<ServiceResult<SquadDto>> SetCaptainAsync(long userId, SetCaptainRequestDto request)
        {
            var fantasyTeam = await GetOrCreateFantasyTeamAsync(userId);
            if (!fantasyTeam.HasPickedInitialSquad)
            {
                return ServiceResult<SquadDto>.Failure("Pick your initial squad first.");
            }

            var squadIds = fantasyTeam.SquadPlayers.Select(sp => sp.PlayerId).ToHashSet();
            if (request.CaptainPlayerId == request.ViceCaptainPlayerId)
            {
                return ServiceResult<SquadDto>.Failure("Captain and vice-captain must be different players.");
            }

            if (!squadIds.Contains(request.CaptainPlayerId) || !squadIds.Contains(request.ViceCaptainPlayerId))
            {
                return ServiceResult<SquadDto>.Failure("Captain and vice-captain must be part of your squad.");
            }

            foreach (var squadPlayer in fantasyTeam.SquadPlayers)
            {
                squadPlayer.IsCaptain = squadPlayer.PlayerId == request.CaptainPlayerId;
                squadPlayer.IsViceCaptain = squadPlayer.PlayerId == request.ViceCaptainPlayerId;
            }

            await _fantasyTeamRepository.SaveChangesAsync();

            return ServiceResult<SquadDto>.Success(ToSquadDto(fantasyTeam));
        }

        public async Task<ServiceResult<SquadDto>> ActivateChipAsync(long userId, ActivateChipRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Chip) || !Enum.TryParse<ChipType>(request.Chip, true, out var chip))
            {
                return ServiceResult<SquadDto>.Failure("Invalid chip.");
            }

            var fantasyTeam = await GetOrCreateFantasyTeamAsync(userId);
            if (!fantasyTeam.HasPickedInitialSquad)
            {
                return ServiceResult<SquadDto>.Failure("Pick your initial squad first.");
            }

            var alreadyUsed = chip switch
            {
                ChipType.TripleCaptain => fantasyTeam.TripleCaptainUsed,
                ChipType.BenchBoost => fantasyTeam.BenchBoostUsed,
                ChipType.WildCard => fantasyTeam.WildCardUsed,
                _ => true
            };
            if (alreadyUsed)
            {
                return ServiceResult<SquadDto>.Failure("You've already used this chip this season.");
            }

            if (fantasyTeam.ActiveChip != null)
            {
                return ServiceResult<SquadDto>.Failure("You already have an active chip for the upcoming gameweek.");
            }

            var targetGameweek = await _gameweekRepository.GetNextAsync() ?? await _gameweekRepository.GetCurrentAsync();
            if (targetGameweek == null)
            {
                return ServiceResult<SquadDto>.Failure("Season data is not available yet.");
            }

            fantasyTeam.ActiveChip = chip;
            fantasyTeam.ActiveChipGameweekId = targetGameweek.Id;
            switch (chip)
            {
                case ChipType.TripleCaptain:
                    fantasyTeam.TripleCaptainUsed = true;
                    break;
                case ChipType.BenchBoost:
                    fantasyTeam.BenchBoostUsed = true;
                    break;
                case ChipType.WildCard:
                    fantasyTeam.WildCardUsed = true;
                    break;
            }

            await _fantasyTeamRepository.SaveChangesAsync();

            return ServiceResult<SquadDto>.Success(ToSquadDto(fantasyTeam));
        }

        private async Task<FantasyTeam> GetOrCreateFantasyTeamAsync(long userId)
        {
            var fantasyTeam = await _fantasyTeamRepository.GetByUserIdWithSquadAsync(userId);
            if (fantasyTeam != null)
            {
                return fantasyTeam;
            }

            fantasyTeam = new FantasyTeam
            {
                UserId = userId,
                Name = "My Team",
                BudgetRemainingTenths = 1000,
                FreeTransfersAvailable = 1
            };
            await _fantasyTeamRepository.AddAsync(fantasyTeam);
            await _fantasyTeamRepository.SaveChangesAsync();

            return fantasyTeam;
        }

        private static string? ValidateSquadComposition(List<Player> players)
        {
            var goalkeepers = players.Count(p => p.Position == PlayerPosition.Goalkeeper);
            var defenders = players.Count(p => p.Position == PlayerPosition.Defender);
            var midfielders = players.Count(p => p.Position == PlayerPosition.Midfielder);
            var forwards = players.Count(p => p.Position == PlayerPosition.Forward);

            if (goalkeepers != RequiredGoalkeepers || defenders != RequiredDefenders ||
                midfielders != RequiredMidfielders || forwards != RequiredForwards)
            {
                return $"Squad must have {RequiredGoalkeepers} GK, {RequiredDefenders} DEF, " +
                       $"{RequiredMidfielders} MID, {RequiredForwards} FWD.";
            }

            var clubCountExceeded = players.GroupBy(p => p.TeamId).Any(g => g.Count() > MaxPlayersPerClub);
            if (clubCountExceeded)
            {
                return $"You cannot pick more than {MaxPlayersPerClub} players from the same club.";
            }

            return null;
        }

        private static string? ValidateLineup(
            List<Player> squadPlayers,
            List<long> startingPlayerIds,
            List<BenchSlotDto> benchOrder,
            long captainPlayerId,
            long viceCaptainPlayerId)
        {
            if (startingPlayerIds.Count != StartingSize || startingPlayerIds.Distinct().Count() != StartingSize)
            {
                return $"Starting XI must have exactly {StartingSize} distinct players.";
            }

            var squadIds = squadPlayers.Select(p => p.Id).ToHashSet();
            if (!startingPlayerIds.All(id => squadIds.Contains(id)))
            {
                return "Starting players must be part of your squad.";
            }

            var benchIds = squadIds.Except(startingPlayerIds).ToHashSet();
            if (benchIds.Count != SquadSize - StartingSize)
            {
                return "Bench must have exactly 4 players.";
            }

            var benchOrderIds = benchOrder.Select(b => b.PlayerId).ToHashSet();
            var benchOrderValues = benchOrder.Select(b => b.Order).ToList();
            if (!benchOrderIds.SetEquals(benchIds) || benchOrderValues.Distinct().Count() != 4 ||
                benchOrderValues.Any(o => o is < 0 or > 3))
            {
                return "Bench order must assign unique positions 0-3 to exactly the 4 bench players.";
            }

            var startingPlayers = squadPlayers.Where(p => startingPlayerIds.Contains(p.Id)).ToList();
            var goalkeepers = startingPlayers.Count(p => p.Position == PlayerPosition.Goalkeeper);
            var defenders = startingPlayers.Count(p => p.Position == PlayerPosition.Defender);
            var midfielders = startingPlayers.Count(p => p.Position == PlayerPosition.Midfielder);
            var forwards = startingPlayers.Count(p => p.Position == PlayerPosition.Forward);

            if (goalkeepers != 1)
            {
                return "Starting XI must include exactly 1 goalkeeper.";
            }

            if (defenders is < 3 or > 5)
            {
                return "Starting XI must include between 3 and 5 defenders.";
            }

            if (midfielders is < 3 or > 5)
            {
                return "Starting XI must include between 3 and 5 midfielders.";
            }

            if (forwards is < 1 or > 3)
            {
                return "Starting XI must include between 1 and 3 forwards.";
            }

            if (captainPlayerId == viceCaptainPlayerId)
            {
                return "Captain and vice-captain must be different players.";
            }

            if (!squadIds.Contains(captainPlayerId) || !squadIds.Contains(viceCaptainPlayerId))
            {
                return "Captain and vice-captain must be part of your squad.";
            }

            return null;
        }

        internal static SquadDto ToSquadDto(FantasyTeam fantasyTeam) => new()
        {
            Name = fantasyTeam.Name,
            BudgetRemainingMillions = fantasyTeam.BudgetRemainingTenths / 10m,
            FreeTransfersAvailable = fantasyTeam.FreeTransfersAvailable,
            HasPickedInitialSquad = fantasyTeam.HasPickedInitialSquad,
            TripleCaptainUsed = fantasyTeam.TripleCaptainUsed,
            BenchBoostUsed = fantasyTeam.BenchBoostUsed,
            WildCardUsed = fantasyTeam.WildCardUsed,
            ActiveChip = fantasyTeam.ActiveChip?.ToString(),
            Players = fantasyTeam.SquadPlayers.Where(sp => sp.Player != null).Select(sp => new SquadPlayerDto
            {
                PlayerId = sp.PlayerId,
                WebName = sp.Player!.WebName,
                Position = sp.Player.Position.ToString(),
                TeamId = sp.Player.TeamId,
                TeamShortName = sp.Player.Team?.ShortName ?? string.Empty,
                PriceMillions = sp.Player.PriceTenths / 10m,
                IsStarting = sp.IsStarting,
                BenchOrder = sp.BenchOrder,
                IsCaptain = sp.IsCaptain,
                IsViceCaptain = sp.IsViceCaptain
            }).ToList()
        };
    }
}
