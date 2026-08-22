# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Premier League Fantasy App — a lookalike of the official Fantasy Premier League app. The codebase is split into `server/` (.NET 8 backend) and `client/` (Angular, not yet created). Currently only the backend auth foundation exists.

## Commands

All commands run from `server/` unless noted.

```bash
# Build everything
dotnet build

# Run the API (from server/src/FantasyApp.Api)
dotnet run --urls "http://localhost:5080"
# Swagger UI at http://localhost:5080/swagger

# EF Core migrations (dotnet-ef must be installed: dotnet tool install --global dotnet-ef)
# Run from server/src/FantasyApp.Api
dotnet ef migrations add <Name> --project ../FantasyApp.Repository --startup-project . --output-dir Migrations
dotnet ef database update --project ../FantasyApp.Repository --startup-project .
dotnet ef database drop --project ../FantasyApp.Repository --startup-project . --force

# User secrets (from server/src/FantasyApp.Api) — never put real values in appsettings.json
dotnet user-secrets list
dotnet user-secrets set "Jwt:Key" "<value>"
```

No test project exists yet.

### Local infrastructure

- **SQL Server**: not natively supported on Fedora, so it runs in Docker: `docker run -e 'ACCEPT_EULA=Y' -e "MSSQL_SA_PASSWORD=<pwd>" -p 1433:1433 --name fantasyapp-sqlserver --restart unless-stopped -d mcr.microsoft.com/mssql/server:2022-latest`. Inspect data with `docker exec -it fantasyapp-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '<pwd>' -C -d FantasyAppDb`.
- **Secrets** (JWT key, DB connection string, SMTP credentials) live only in `dotnet user-secrets` for `FantasyApp.Api`, never committed. `appsettings.json` holds only placeholder/non-secret config (issuer, audience, token lifetimes, SMTP host/port).
- **Email**: forgot-password sends real emails via MailKit through Gmail SMTP (App Password based, not OAuth).

## Architecture

Solution: `server/FantasyApp.sln`, 5 projects under `server/src/`, layered with these dependency rules:

```
FantasyApp.Entity   (no deps — models + DTOs)
FantasyApp.Common   → Entity                          (JWT/token generation, email sending, settings POCOs)
FantasyApp.Repository → Entity                        (EF Core DbContext, migrations, data-access repositories)
FantasyApp.BusinessLogic → Entity, Repository, Common  (services orchestrating business logic, e.g. AuthService)
FantasyApp.Api → all of the above                     (controllers, DI wiring, Program.cs)
```

Naming maps to standard layers: **Entity** = Domain models/DTOs, **Common** = cross-cutting services (also where a future third-party API client for live player data will live), **Repository** = data access, **BusinessLogic** = Application/service layer, **Api** = presentation.

No repository/unit-of-work abstraction over EF Core beyond `IRefreshTokenRepository` — `AuthService` uses `UserManager<ApplicationUser>` / EF Core directly rather than generic repositories.

### Auth design

- **ASP.NET Core Identity** (`ApplicationUser : IdentityUser<Guid>`) owns user storage, password hashing, and password-reset token generation/validation — not custom code. Identity tables are renamed via `ApplicationDbContext.OnModelCreating` from the `AspNet*` defaults to plain names: `Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `UserTokens`, `RoleClaims`.
- **JWT access tokens** (`FantasyApp.Common.Services.TokenService`) are short-lived (15 min default), claims: `sub`, `email`, name, `jti`. No role claims — the app has a single role (User), so `[Authorize]` alone gates access.
- **Refresh tokens** are custom (not part of Identity), stored in the `RefreshTokens` table, rotated on every use (`AuthService.RefreshTokenAsync`: old token revoked + `ReplacedByToken` set, new token issued), giving a sliding 7-day expiration.
- Password reset tokens are **not stored anywhere** — Identity encrypts a payload (user id + purpose + `SecurityStamp` + timestamp) via the Data Protection API; validity is checked by re-deriving and comparing against the user's current `SecurityStamp`, which changes whenever the password changes (invalidating old outstanding tokens).
- `AuthController` (`server/src/FantasyApp.Api/Controllers/AuthController.cs`) exposes `POST /api/auth/{register,login,refresh-token,revoke-token,forgot-password,reset-password}` and `GET /api/auth/me` (protected, for testing JWT auth).

### Client token storage plan (for when `client/` is built)

Refresh token is intended to be stored in `localStorage` on the Angular client and sent explicitly in the request body to `/api/auth/refresh-token` — not via httpOnly cookie. CORS is already configured in `Program.cs` for `http://localhost:4200`.
