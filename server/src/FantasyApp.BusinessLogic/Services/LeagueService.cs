using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Entity.Dtos.Leagues;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;

namespace FantasyApp.BusinessLogic.Services
{
    public class LeagueService : ILeagueService
    {
        private const string JoinCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int JoinCodeLength = 6;

        private readonly ILeagueRepository _leagueRepository;
        private readonly IUserGameweekScoreRepository _scoreRepository;
        private readonly IGameweekRepository _gameweekRepository;

        public LeagueService(
            ILeagueRepository leagueRepository,
            IUserGameweekScoreRepository scoreRepository,
            IGameweekRepository gameweekRepository)
        {
            _leagueRepository = leagueRepository;
            _scoreRepository = scoreRepository;
            _gameweekRepository = gameweekRepository;
        }

        public async Task<ServiceResult<LeagueDto>> CreateLeagueAsync(long userId, CreateLeagueRequestDto request)
        {
            var joinCode = await GenerateUniqueJoinCodeAsync();

            var league = new League
            {
                Name = request.Name,
                JoinCode = joinCode,
                IsOfficial = false,
                OwnerUserId = userId,
                MaxMembers = 50,
                CreatedAt = DateTime.UtcNow
            };
            await _leagueRepository.AddAsync(league);
            await _leagueRepository.SaveChangesAsync();

            await _leagueRepository.AddMembershipAsync(new LeagueMembership
            {
                LeagueId = league.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
            await _leagueRepository.SaveChangesAsync();

            return ServiceResult<LeagueDto>.Success(new LeagueDto
            {
                Id = league.Id,
                Name = league.Name,
                JoinCode = league.JoinCode,
                IsOfficial = league.IsOfficial,
                MemberCount = 1
            });
        }

        public async Task<ServiceResult<LeagueDto>> JoinLeagueAsync(long userId, JoinLeagueRequestDto request)
        {
            var league = await _leagueRepository.GetByJoinCodeAsync(request.JoinCode.Trim().ToUpperInvariant());
            if (league == null)
            {
                return ServiceResult<LeagueDto>.Failure("Invalid join code.");
            }

            if (await _leagueRepository.IsMemberAsync(league.Id, userId))
            {
                return ServiceResult<LeagueDto>.Failure("You are already in this league.");
            }

            var memberCount = await _leagueRepository.GetMemberCountAsync(league.Id);
            if (memberCount >= league.MaxMembers)
            {
                return ServiceResult<LeagueDto>.Failure("This league is full.");
            }

            await _leagueRepository.AddMembershipAsync(new LeagueMembership
            {
                LeagueId = league.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
            await _leagueRepository.SaveChangesAsync();

            return ServiceResult<LeagueDto>.Success(new LeagueDto
            {
                Id = league.Id,
                Name = league.Name,
                JoinCode = league.JoinCode,
                IsOfficial = league.IsOfficial,
                MemberCount = memberCount + 1
            });
        }

        public async Task<List<LeagueDto>> GetMyLeaguesAsync(long userId)
        {
            var leagues = await _leagueRepository.GetUserLeaguesAsync(userId);
            var result = new List<LeagueDto>();

            foreach (var league in leagues)
            {
                result.Add(new LeagueDto
                {
                    Id = league.Id,
                    Name = league.Name,
                    JoinCode = league.JoinCode,
                    IsOfficial = league.IsOfficial,
                    MemberCount = await _leagueRepository.GetMemberCountAsync(league.Id)
                });
            }

            return result;
        }

        public async Task<ServiceResult<LeagueStandingsDto>> GetStandingsAsync(long userId, long leagueId)
        {
            var league = await _leagueRepository.GetByIdAsync(leagueId);
            if (league == null)
            {
                return ServiceResult<LeagueStandingsDto>.Failure("League not found.");
            }

            if (!await _leagueRepository.IsMemberAsync(leagueId, userId))
            {
                return ServiceResult<LeagueStandingsDto>.Failure("You are not a member of this league.");
            }

            var memberships = await _leagueRepository.GetMembershipsWithUsersAsync(leagueId);
            var userIds = memberships.Select(m => m.UserId).ToList();

            var totalPoints = await _scoreRepository.GetTotalPointsByUserIdsAsync(userIds);

            var currentGameweek = await _gameweekRepository.GetCurrentAsync();
            var gameweekPoints = currentGameweek != null
                ? await _scoreRepository.GetPointsForGameweekByUserIdsAsync(userIds, currentGameweek.Id)
                : new Dictionary<long, int>();

            var entries = memberships
                .Where(m => m.User != null)
                .Select(m => new LeagueStandingEntryDto
                {
                    UserId = m.UserId,
                    Username = m.User!.UserName ?? string.Empty,
                    TotalPoints = totalPoints.GetValueOrDefault(m.UserId),
                    GameweekPoints = gameweekPoints.GetValueOrDefault(m.UserId)
                })
                .OrderByDescending(e => e.TotalPoints)
                .ToList();

            for (var i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }

            return ServiceResult<LeagueStandingsDto>.Success(new LeagueStandingsDto
            {
                LeagueName = league.Name,
                Entries = entries
            });
        }

        private async Task<string> GenerateUniqueJoinCodeAsync()
        {
            while (true)
            {
                var code = GenerateJoinCode();
                if (await _leagueRepository.GetByJoinCodeAsync(code) == null)
                {
                    return code;
                }
            }
        }

        private static string GenerateJoinCode()
        {
            var bytes = RandomNumberGenerator.GetBytes(JoinCodeLength);
            var chars = new char[JoinCodeLength];
            for (var i = 0; i < JoinCodeLength; i++)
            {
                chars[i] = JoinCodeAlphabet[bytes[i] % JoinCodeAlphabet.Length];
            }
            return new string(chars);
        }
    }
}
