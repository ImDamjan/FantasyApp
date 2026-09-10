using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Players;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IFixtureRepository _fixtureRepository;

        public PlayerService(IPlayerRepository playerRepository, IFixtureRepository fixtureRepository)
        {
            _playerRepository = playerRepository;
            _fixtureRepository = fixtureRepository;
        }

        public async Task<PlayerListResultDto> GetPlayersAsync(
            PlayerPosition? position,
            decimal? maxPriceMillions,
            string? search,
            long? teamId,
            int page,
            int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

            var maxPriceTenths = maxPriceMillions.HasValue ? (int)(maxPriceMillions.Value * 10) : (int?)null;

            var (players, totalCount) = await _playerRepository.GetFilteredAsync(
                position, maxPriceTenths, search, teamId, page, pageSize);

            return new PlayerListResultDto
            {
                Players = players.Select(ToListItemDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ServiceResult<PlayerDetailDto>> GetPlayerDetailAsync(long id)
        {
            var player = await _playerRepository.GetByIdWithTeamAsync(id);
            if (player == null || player.Team == null)
            {
                return ServiceResult<PlayerDetailDto>.Failure("Player not found.");
            }

            var upcomingFixtures = await _fixtureRepository.GetUpcomingForTeamAsync(player.TeamId, 5);

            var dto = new PlayerDetailDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                SecondName = player.SecondName,
                WebName = player.WebName,
                Position = player.Position.ToString(),
                TeamId = player.TeamId,
                TeamName = player.Team.Name,
                TeamShortName = player.Team.ShortName,
                PriceMillions = player.PriceTenths / 10m,
                TotalPoints = player.TotalPoints,
                Form = player.Form,
                AveragePoints = Math.Round(player.Form, 1),
                Status = player.Status,
                NextFixtures = upcomingFixtures.Select(f => new UpcomingFixtureDto
                {
                    IsHome = f.HomeTeamId == player.TeamId,
                    OpponentShortName = f.HomeTeamId == player.TeamId
                        ? f.AwayTeam?.ShortName ?? string.Empty
                        : f.HomeTeam?.ShortName ?? string.Empty,
                    Difficulty = f.HomeTeamId == player.TeamId ? f.HomeDifficulty : f.AwayDifficulty,
                    KickoffTime = f.KickoffTime
                }).ToList()
            };

            return ServiceResult<PlayerDetailDto>.Success(dto);
        }

        private static PlayerListItemDto ToListItemDto(Player player) => new()
        {
            Id = player.Id,
            WebName = player.WebName,
            Position = player.Position.ToString(),
            TeamId = player.TeamId,
            TeamName = player.Team?.Name ?? string.Empty,
            TeamShortName = player.Team?.ShortName ?? string.Empty,
            PriceMillions = player.PriceTenths / 10m,
            TotalPoints = player.TotalPoints,
            Form = player.Form,
            Status = player.Status
        };
    }
}
