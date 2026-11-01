using BLL.Mapper;
using BLL.Services;
using DAL.Classes;
using DAL.Data;
using DAL.Repositories.ClassRepositories;
using DAL.Repositories.IRepositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyApp.BLL.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
});

builder.Services.AddDbContext<DAL.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "THIS_IS_A_VERY_LONG_SECRET_KEY_FOR_JWT_AUTHENTICATION_1234567890";
}

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
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "chargeProject",

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "chargeProjectUsers",

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
});

builder.Services.AddScoped<InterfaceUser, UserRepository>();
builder.Services.AddScoped<InterfaceDriver, DriversRepository>();
builder.Services.AddScoped<InterfaceOwner, OwnerRepository>();
builder.Services.AddScoped<InterfaceUsersVehicles, UsersVehiclesRepository>();
builder.Services.AddScoped<InterfaceChargingStations, ChargingStationsRepository>();
builder.Services.AddScoped<InterfaceChargeBooking, ChargeBookingRepository>();
builder.Services.AddScoped<InterfaceReviewsToCharge, ReviewsToChargeRepository>();
builder.Services.AddScoped<InterfaceStationStatusLogs, StationStatusLogsRepository>();

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

var app = builder.Build();

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