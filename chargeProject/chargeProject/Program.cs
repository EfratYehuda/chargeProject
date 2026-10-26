using DAL.Data;
using Microsoft.EntityFrameworkCore;
using BLL.Mapper;
using BLL.Services;
using DAL.Repositories.IRepositories;
using DAL.Repositories.ClassRepositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DAL.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddScoped<InterfaceUser, UserRepository>();
builder.Services.AddScoped<InterfaceDriver, DriversRepository>();
builder.Services.AddScoped<InterfaceOwner, OwnerRepository>();
builder.Services.AddScoped<InterfaceUsersVehicles, UsersVehiclesRepository>();
builder.Services.AddScoped<InterfaceChargingStations, ChargingStationsRepository>();
builder.Services.AddScoped<InterfaceChargeBooking, ChargeBookingRepository>();
builder.Services.AddScoped<InterfaceReviewsToCharge, ReviewsToChargeRepository>();
builder.Services.AddScoped<InterfaceStationStatusLogs, StationStatusLogsRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDriversService, DriversService>();
builder.Services.AddScoped<IOwnersService, OwnersService>();
builder.Services.AddScoped<IUsersVehiclesService, UsersVehiclesService>();
builder.Services.AddScoped<IChargingStationsService, ChargingStationsService>();
builder.Services.AddScoped<IChargeBookingService, ChargeBookingService>();
builder.Services.AddScoped<IReviewsToChargeService, ReviewsToChargeService>();
builder.Services.AddScoped<IStationStatusLogsService, StationStatusLogsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
