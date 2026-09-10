using System;
using FantasyApp.Entity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FantasyApp.Repository.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Gameweek> Gameweeks => Set<Gameweek>();
        public DbSet<Fixture> Fixtures => Set<Fixture>();
        public DbSet<PlayerGameweekStat> PlayerGameweekStats => Set<PlayerGameweekStat>();
        public DbSet<FantasyTeam> FantasyTeams => Set<FantasyTeam>();
        public DbSet<SquadPlayer> SquadPlayers => Set<SquadPlayer>();
        public DbSet<League> Leagues => Set<League>();
        public DbSet<LeagueMembership> LeagueMemberships => Set<LeagueMembership>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<UserGameweekScore> UserGameweekScores => Set<UserGameweekScore>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole<long>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<long>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<long>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<long>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<long>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("RoleClaims");

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Token).IsRequired();
                entity.HasIndex(rt => rt.Token).IsUnique();

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Team>(entity =>
            {
                entity.HasIndex(t => t.FplId).IsUnique();
            });

            builder.Entity<Player>(entity =>
            {
                entity.HasIndex(p => p.FplId).IsUnique();
                entity.Property(p => p.Form).HasPrecision(5, 1);

                entity.HasOne(p => p.Team)
                    .WithMany(t => t.Players)
                    .HasForeignKey(p => p.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Gameweek>(entity =>
            {
                entity.HasIndex(g => g.FplId).IsUnique();
            });

            builder.Entity<Fixture>(entity =>
            {
                entity.HasIndex(f => f.FplId).IsUnique();

                entity.HasOne(f => f.Gameweek)
                    .WithMany()
                    .HasForeignKey(f => f.GameweekId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.HomeTeam)
                    .WithMany()
                    .HasForeignKey(f => f.HomeTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.AwayTeam)
                    .WithMany()
                    .HasForeignKey(f => f.AwayTeamId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PlayerGameweekStat>(entity =>
            {
                entity.HasIndex(s => new { s.PlayerId, s.GameweekId }).IsUnique();

                entity.HasOne(s => s.Player)
                    .WithMany()
                    .HasForeignKey(s => s.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Gameweek)
                    .WithMany()
                    .HasForeignKey(s => s.GameweekId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<FantasyTeam>(entity =>
            {
                entity.HasIndex(ft => ft.UserId).IsUnique();

                entity.HasOne(ft => ft.User)
                    .WithMany()
                    .HasForeignKey(ft => ft.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Gameweek>()
                    .WithMany()
                    .HasForeignKey(ft => ft.ActiveChipGameweekId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Gameweek>()
                    .WithMany()
                    .HasForeignKey(ft => ft.LastFreeTransferGameweekId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SquadPlayer>(entity =>
            {
                entity.HasIndex(sp => new { sp.FantasyTeamId, sp.PlayerId }).IsUnique();

                entity.HasOne(sp => sp.FantasyTeam)
                    .WithMany(ft => ft.SquadPlayers)
                    .HasForeignKey(sp => sp.FantasyTeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sp => sp.Player)
                    .WithMany()
                    .HasForeignKey(sp => sp.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<League>(entity =>
            {
                entity.HasIndex(l => l.JoinCode).IsUnique();

                entity.HasOne(l => l.OwnerUser)
                    .WithMany()
                    .HasForeignKey(l => l.OwnerUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<LeagueMembership>(entity =>
            {
                entity.HasIndex(lm => new { lm.LeagueId, lm.UserId }).IsUnique();

                entity.HasOne(lm => lm.League)
                    .WithMany(l => l.Memberships)
                    .HasForeignKey(lm => lm.LeagueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lm => lm.User)
                    .WithMany()
                    .HasForeignKey(lm => lm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Transfer>(entity =>
            {
                entity.HasOne(t => t.FantasyTeam)
                    .WithMany()
                    .HasForeignKey(t => t.FantasyTeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Gameweek)
                    .WithMany()
                    .HasForeignKey(t => t.GameweekId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.PlayerOut)
                    .WithMany()
                    .HasForeignKey(t => t.PlayerOutId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.PlayerIn)
                    .WithMany()
                    .HasForeignKey(t => t.PlayerInId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserGameweekScore>(entity =>
            {
                entity.HasIndex(s => new { s.UserId, s.GameweekId }).IsUnique();

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Gameweek)
                    .WithMany()
                    .HasForeignKey(s => s.GameweekId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
