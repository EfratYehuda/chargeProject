using BLL.Mapper;
using BLL.Services;
using BLL.Agent; // נוסף כדי לזהות את הכלים
using chargeProject.Middleware;
using DAL.Classes;
using DAL.Data;
using DAL.Repositories.ClassRepositories;
using DAL.Repositories.IRepositories;
using DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenAI;
using System.ClientModel;
using MyApp.BLL.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------- AI Agent ---------
var apiKey = builder.Configuration["GEMINI:APIKEY"]
    ?? throw new InvalidOperationException("Gemini:ApiKey is missing");

// הכתובת של Gemini שמדברת בפורמט של OpenAI
var options = new OpenAIClientOptions
{
    Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")
};

IChatClient innerClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient("gemini-3.8-flash")
    .AsIChatClient();

builder.Services.AddChatClient(innerClient)
    .UseFunctionInvocation();   // מריץ את הכלים שהמודל מבקש

builder.Services.AddScoped<IAgentService, AgentService>();

// --- רישום כלי ה-AI (חלק ב' של הפרויקט) ---
// 1. כלי מסד הנתונים
builder.Services.AddScoped<StationTools>();

// 2. כלי ה-API החיצוני למיקומים עם HttpClient
builder.Services.AddHttpClient<LocationTools>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    // OpenStreetMap מחייב User-Agent תקין
    client.DefaultRequestHeaders.Add("User-Agent", "ChargeProjectApp/1.0");
    client.Timeout = TimeSpan.FromSeconds(10);
});
// ----------------------------------------

// Add services to the container.
builder.Services.AddControllers();

// טיפול מרכזי בשגיאות
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// סוואגר עם אפשרות להדבקת טוקן
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
        logger.LogError(ex, "Could not apply AdminEmails at startup. Is the database created and migrated?");
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();