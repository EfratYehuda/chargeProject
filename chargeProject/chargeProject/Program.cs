using BLL.Agent;
using BLL.Mapper;
using BLL.Services;
using chargeProject.Middleware;
using DAL.Classes;
using DAL.Data;
using DAL.Repositories.ClassRepositories;
using DAL.Repositories.IRepositories;
using DTO;
using Google.GenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyApp.BLL.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------- AI Agent (Official Google.GenAI SDK) ---------
var apiKey = builder.Configuration["GEMINI:APIKEY"]
    ?? throw new InvalidOperationException("GEMINI:APIKEY is missing");
var geminiModel = builder.Configuration["GEMINI:MODEL"] ?? "gemini-3.5-flash";

var geminiClient = new Client(apiKey: apiKey);
IChatClient innerClient = geminiClient.AsIChatClient(geminiModel);

builder.Services.AddChatClient(innerClient)
    .UseFunctionInvocation(configure: client => client.MaximumIterationsPerRequest = 4);

builder.Services.AddScoped<IAgentService, AgentService>();

// --- כלי ה-AI ---
builder.Services.AddScoped<StationTools>();

builder.Services.AddHttpClient<LocationTools>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "ChargeProjectApp/1.0");
    client.Timeout = TimeSpan.FromSeconds(10);
});
// ----------------------------------------

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000", "https://evcharging-app.runasp.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// תפיסת שגיאות גלובלית
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger / OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the token only (without the word Bearer)."
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<DAL.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Configuration 'Jwt:Key' is missing or shorter than 32 characters.");
if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Configuration 'Jwt:Issuer' and 'Jwt:Audience' are required.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("Driver", policy => policy.RequireClaim("driverId"));
    options.AddPolicy("Owner", policy => policy.RequireClaim("ownerId"));
});

// Repositories
builder.Services.AddScoped<InterfaceUser, UserRepository>();
builder.Services.AddScoped<InterfaceDriver, DriversRepository>();
builder.Services.AddScoped<InterfaceOwner, OwnerRepository>();
builder.Services.AddScoped<InterfaceUsersVehicles, UsersVehiclesRepository>();
builder.Services.AddScoped<InterfaceChargingStations, ChargingStationsRepository>();
builder.Services.AddScoped<InterfaceChargeBooking, ChargeBookingRepository>();
builder.Services.AddScoped<InterfaceReviewsToCharge, ReviewsToChargeRepository>();
builder.Services.AddScoped<InterfaceStationStatusLogs, StationStatusLogsRepository>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDriversService, DriversService>();
builder.Services.AddScoped<IOwnersService, OwnersService>();
builder.Services.AddScoped<IUsersVehiclesService, UsersVehiclesService>();
builder.Services.AddScoped<IChargingStationsService, ChargingStationsService>();
builder.Services.AddScoped<IChargeBookingService, ChargeBookingService>();
// שירות רקע לשחרור עמדות שפג תוקף השריון שלהן
builder.Services.AddHostedService<chargeProject.BackgroundServices.ExpireHoldsService>();
builder.Services.AddScoped<IReviewsToChargeService, ReviewsToChargeService>();
builder.Services.AddScoped<IStationStatusLogsService, StationStatusLogsService>();
builder.Services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
builder.Services.AddScoped<BLL.Algorithm.StationMatchingAlgorithm>();

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate(); // מוודא שהמיגרציות כולל מניעת שריון כפול חלות על המסד
        var adminEmails = (app.Configuration.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .ToList();

        foreach (var email in adminEmails)
        {
            var u = db.Users.FirstOrDefault(x => x.Email == email);
            if (u == null)
                logger.LogWarning("Admin email {Email} has not registered yet.", email);
            else if (u.Access != UserRoles.Admin)
            {
                u.Access = UserRoles.Admin;
                logger.LogInformation("Promoted {Email} to Admin.", email);
            }
        }
        if (db.ChangeTracker.HasChanges()) db.SaveChanges();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Could not apply AdminEmails or Migrations at startup.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowReactDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => "ChargeFlow API is running!").AllowAnonymous();
app.MapControllers();
app.Run();