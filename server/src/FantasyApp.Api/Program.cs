using System.Text;
using FantasyApp.Api.BackgroundServices;
using FantasyApp.Api.Json;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.BusinessLogic.Services;
using FantasyApp.Common.Interfaces;
using FantasyApp.Common.Services;
using FantasyApp.Common.Settings;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Data;
using FantasyApp.Repository.Interfaces;
using FantasyApp.Repository.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.Configure<FplSettings>(builder.Configuration.GetSection("Fpl"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var fplSettings = builder.Configuration.GetSection("Fpl").Get<FplSettings>() ?? new FplSettings();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<long>>(options =>
    {
        options.Password.RequiredLength = 1;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameweekRepository, GameweekRepository>();
builder.Services.AddScoped<IFixtureRepository, FixtureRepository>();
builder.Services.AddScoped<IPlayerGameweekStatRepository, PlayerGameweekStatRepository>();
builder.Services.AddScoped<ILeagueRepository, LeagueRepository>();
builder.Services.AddScoped<IFantasyTeamRepository, FantasyTeamRepository>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<IUserGameweekScoreRepository, UserGameweekScoreRepository>();
builder.Services.AddScoped<IGameweekPickRepository, GameweekPickRepository>();
builder.Services.AddScoped<IGameweekSnapshotService, GameweekSnapshotService>();
builder.Services.AddScoped<IFplDataSyncService, FplDataSyncService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<ISquadService, SquadService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<IScoringService, ScoringService>();
builder.Services.AddScoped<IPointsService, PointsService>();
builder.Services.AddScoped<IGameweekService, GameweekService>();

builder.Services.AddHttpClient<IFplApiClient, FplApiClient>(client =>
{
    client.BaseAddress = new Uri(fplSettings.BaseUrl);
});

builder.Services.AddHostedService<FplSyncService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FantasyApp API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Unesi JWT access token (bez 'Bearer ' prefiksa, Swagger ga dodaje automatski)."
    };
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var leagueRepository = scope.ServiceProvider.GetRequiredService<ILeagueRepository>();
    var officialLeague = await leagueRepository.GetOfficialLeagueAsync();
    if (officialLeague == null)
    {
        await leagueRepository.AddAsync(new League
        {
            Name = "Overall League",
            JoinCode = "OFFICIAL",
            IsOfficial = true,
            MaxMembers = int.MaxValue,
            CreatedAt = DateTime.UtcNow
        });
        await leagueRepository.SaveChangesAsync();
    }
}

app.Run();
