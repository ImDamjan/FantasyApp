using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Transfers;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class TransferService : ITransferService
    {
        private readonly IFantasyTeamRepository _fantasyTeamRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameweekRepository _gameweekRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly IUserGameweekScoreRepository _scoreRepository;
        private readonly IGameweekSnapshotService _snapshotService;

        public TransferService(
            IFantasyTeamRepository fantasyTeamRepository,
            IPlayerRepository playerRepository,
            IGameweekRepository gameweekRepository,
            ITransferRepository transferRepository,
            IUserGameweekScoreRepository scoreRepository,
            IGameweekSnapshotService snapshotService)
        {
            _fantasyTeamRepository = fantasyTeamRepository;
            _playerRepository = playerRepository;
            _gameweekRepository = gameweekRepository;
            _transferRepository = transferRepository;
            _scoreRepository = scoreRepository;
            _snapshotService = snapshotService;
        }

        public async Task<ServiceResult<TransferResultDto>> SubmitTransfersAsync(long userId, SubmitTransfersRequestDto request)
        {
            await _snapshotService.EnsureSnapshotsAsync();

            var fantasyTeam = await _fantasyTeamRepository.GetByUserIdWithSquadAsync(userId);
            if (fantasyTeam == null || !fantasyTeam.HasPickedInitialSquad)
            {
                return ServiceResult<TransferResultDto>.Failure("Pick your initial squad before making transfers.");
            }

            if (request.Transfers.Count == 0)
            {
                return ServiceResult<TransferResultDto>.Failure("No transfers specified.");
            }

            var outIds = request.Transfers.Select(t => t.PlayerOutId).ToList();
            var inIds = request.Transfers.Select(t => t.PlayerInId).ToList();

            if (outIds.Distinct().Count() != outIds.Count || inIds.Distinct().Count() != inIds.Count)
            {
                return ServiceResult<TransferResultDto>.Failure("Duplicate players in transfer request.");
            }

            var squadPlayerByOutId = fantasyTeam.SquadPlayers.ToDictionary(sp => sp.PlayerId);
            if (!outIds.All(id => squadPlayerByOutId.ContainsKey(id)))
            {
                return ServiceResult<TransferResultDto>.Failure("One or more players are not in your squad.");
            }

            var currentSquadIds = fantasyTeam.SquadPlayers.Select(sp => sp.PlayerId).ToHashSet();
            if (inIds.Any(id => currentSquadIds.Contains(id)))
            {
                return ServiceResult<TransferResultDto>.Failure("You already own one of the selected players.");
            }

            var playersIn = await _playerRepository.GetByIdsAsync(inIds);
            if (playersIn.Count != inIds.Count)
            {
                return ServiceResult<TransferResultDto>.Failure("One or more incoming players were not found.");
            }

            var playersInById = playersIn.ToDictionary(p => p.Id);

            foreach (var transfer in request.Transfers)
            {
                var outgoing = squadPlayerByOutId[transfer.PlayerOutId].Player;
                var incoming = playersInById[transfer.PlayerInId];
                if (outgoing == null || outgoing.Position != incoming.Position)
                {
                    return ServiceResult<TransferResultDto>.Failure("Replacements must play the same position as the player they replace.");
                }
            }

            var clubCounts = fantasyTeam.SquadPlayers
                .Where(sp => sp.Player != null)
                .GroupBy(sp => sp.Player!.TeamId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var transfer in request.Transfers)
            {
                var outgoingClub = squadPlayerByOutId[transfer.PlayerOutId].Player!.TeamId;
                var incomingClub = playersInById[transfer.PlayerInId].TeamId;
                clubCounts[outgoingClub] = clubCounts.GetValueOrDefault(outgoingClub) - 1;
                clubCounts[incomingClub] = clubCounts.GetValueOrDefault(incomingClub) + 1;
            }

            if (clubCounts.Values.Any(count => count > 3))
            {
                return ServiceResult<TransferResultDto>.Failure("You cannot have more than 3 players from the same club.");
            }

            var totalCostDelta = request.Transfers.Sum(t =>
                playersInById[t.PlayerInId].PriceTenths - squadPlayerByOutId[t.PlayerOutId].Player!.PriceTenths);

            if (totalCostDelta > fantasyTeam.BudgetRemainingTenths)
            {
                return ServiceResult<TransferResultDto>.Failure("Not enough budget for these transfers.");
            }

            var targetGameweek = await _gameweekRepository.GetNextAsync();
            if (targetGameweek == null)
            {
                return ServiceResult<TransferResultDto>.Failure("There are no more gameweeks this season.");
            }

            var newTransfers = new List<Transfer>();
            foreach (var item in request.Transfers)
            {
                var squadPlayer = squadPlayerByOutId[item.PlayerOutId];
                var incoming = playersInById[item.PlayerInId];

                var transfer = new Transfer
                {
                    FantasyTeamId = fantasyTeam.Id,
                    GameweekId = targetGameweek.Id,
                    PlayerOutId = item.PlayerOutId,
                    PlayerInId = item.PlayerInId,
                    CreatedAt = DateTime.UtcNow
                };
                newTransfers.Add(transfer);
                await _transferRepository.AddAsync(transfer);

                squadPlayer.PlayerId = incoming.Id;
                squadPlayer.Player = incoming;
                squadPlayer.PurchasePriceTenths = incoming.PriceTenths;
                squadPlayer.IsCaptain = false;
                squadPlayer.IsViceCaptain = false;
            }

            SquadService.ReassignMissingArmbands(fantasyTeam.SquadPlayers);

            fantasyTeam.BudgetRemainingTenths -= totalCostDelta;

            await _transferRepository.SaveChangesAsync();

            var windowTransfers = await _transferRepository.GetByFantasyTeamAndGameweekDescAsync(fantasyTeam.Id, targetGameweek.Id);
            var allowance = TransferAllowance.Calculate(fantasyTeam, targetGameweek.Id, windowTransfers);

            foreach (var transfer in newTransfers)
            {
                transfer.WasFreeTransfer = allowance.PaidTransfers == 0;
            }

            var score = await _scoreRepository.GetAsync(fantasyTeam.UserId, targetGameweek.Id);
            if (score == null)
            {
                score = new UserGameweekScore { UserId = fantasyTeam.UserId, GameweekId = targetGameweek.Id };
                await _scoreRepository.AddAsync(score);
            }
            score.TransferCost = allowance.PointsCost;
            score.NetPoints = score.RawPoints;
            await _scoreRepository.SaveChangesAsync();

            return ServiceResult<TransferResultDto>.Success(new TransferResultDto
            {
                Squad = SquadService.ToSquadDto(fantasyTeam, allowance),
                TransfersMade = request.Transfers.Count,
                FreeTransfersUsed = allowance.FreeTransfersUsed,
                PaidTransfers = allowance.PaidTransfers,
                PointsCost = allowance.PointsCost
            });
        }

        public async Task<ServiceResult<List<TransferHistoryItemDto>>> GetHistoryAsync(long userId)
        {
            var fantasyTeam = await _fantasyTeamRepository.GetByUserIdAsync(userId);
            if (fantasyTeam == null)
            {
                return ServiceResult<List<TransferHistoryItemDto>>.Success(new List<TransferHistoryItemDto>());
            }

            var transfers = await _transferRepository.GetByFantasyTeamIdAsync(fantasyTeam.Id);

            var history = transfers.Select(t => new TransferHistoryItemDto
            {
                GameweekName = t.Gameweek?.Name ?? string.Empty,
                PlayerOutName = t.PlayerOut?.WebName ?? string.Empty,
                PlayerInName = t.PlayerIn?.WebName ?? string.Empty,
                WasFreeTransfer = t.WasFreeTransfer,
                CreatedAt = t.CreatedAt
            }).ToList();

            return ServiceResult<List<TransferHistoryItemDto>>.Success(history);
        }
    }
}
