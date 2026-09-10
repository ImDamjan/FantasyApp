# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Premier League Fantasy App — a lookalike of the official Fantasy Premier League app. The codebase is split into `server/` (.NET 8 backend) and `client/` (Angular 22). Currently only auth (register/login/refresh/forgot-reset password) exists end-to-end; gameplay features are not built yet.

## Commands

### Backend (`server/`)

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

### Frontend (`client/`)

```bash
# npm/ng are installed under ~/.npm-global — add to PATH if not already:
# export PATH=~/.npm-global/bin:$PATH

npm install
ng serve --port 4200      # http://localhost:4200, expects the API at http://localhost:5080
ng build
ng test --watch=false     # vitest
```

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

- **ASP.NET Core Identity** (`ApplicationUser : IdentityUser<long>`) owns user storage, password hashing, and password-reset token generation/validation — not custom code. Identity tables are renamed via `ApplicationDbContext.OnModelCreating` from the `AspNet*` defaults to plain names: `Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `UserTokens`, `RoleClaims`.
- **JWT access tokens** (`FantasyApp.Common.Services.TokenService`) are short-lived (15 min default), claims: `sub`, `email`, name, `jti`. No role claims — the app has a single role (User), so `[Authorize]` alone gates access.
- **Refresh tokens** are custom (not part of Identity), stored in the `RefreshTokens` table, rotated on every use (`AuthService.RefreshTokenAsync`: old token revoked + `ReplacedByToken` set, new token issued), giving a sliding 7-day expiration.
- Password reset tokens are **not stored anywhere** — Identity encrypts a payload (user id + purpose + `SecurityStamp` + timestamp) via the Data Protection API; validity is checked by re-deriving and comparing against the user's current `SecurityStamp`, which changes whenever the password changes (invalidating old outstanding tokens).
- `AuthController` (`server/src/FantasyApp.Api/Controllers/AuthController.cs`) exposes `POST /api/auth/{register,login,refresh-token,revoke-token,forgot-password,reset-password}` and `GET /api/auth/me` (protected, for testing JWT auth).

### Frontend architecture (`client/src/app/`)

Feature-based structure:

```
core/         # singletons: services (AuthService, TokenStorageService), guards, interceptor, models, validators
shared/       # reusable pieces used by 2+ features: FormField component, auth-card SCSS partial
features/
  auth/       # AuthPage (login+register slider), ForgotPasswordPage, ResetPasswordPage
  home/       # placeholder landing page post-login
```

- **Tokens**: both access and refresh tokens are stored in `localStorage` (`TokenStorageService`) and sent explicitly — refresh token goes in the `/api/auth/refresh-token` request body, not a cookie. `AuthService` exposes `currentUser`/`isAuthenticated` as signals.
- **`authInterceptor`** attaches `Authorization: Bearer <accessToken>` to outgoing requests (skipping the auth endpoints themselves) and transparently refreshes on a 401, retrying the original request; concurrent 401s share a single in-flight refresh call.
- **Guards**: `authGuard` blocks `/` (and anything else) unless logged in, redirecting to `/auth`; `guestGuard` blocks `/auth/*` while logged in, redirecting to `/`. Both are functional `CanActivateFn`s wired in `app.routes.ts`.
- **Styling**: plain SCSS, no UI framework. Design tokens (FPL-inspired purple/green palette) are CSS custom properties in `src/styles.scss` (`--fa-*`). The auth pages use a hand-rolled sliding-panel login/register card (`auth-page.scss`); forgot/reset-password reuse a shared card layout from `shared/styles/_auth-card.scss`.
- The logo is a plain text wordmark ("FANTASY" + "LEAGUE"), not a reproduction of the real Premier League/FPL crest — intentional, to avoid trademark issues.
