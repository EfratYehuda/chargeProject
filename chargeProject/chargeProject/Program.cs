using BLL.Mapper;
using BLL.Services;
using DAL.Classes;
using DAL.Data;
using DAL.Repositories.ClassRepositories;
using DAL.Repositories.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// ---------AI agent
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

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
});
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
builder.Services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


//// בדיקה ישירה של ה-AgentService בעליית השרת
//using (var scope = app.Services.CreateScope())
//{
//    var agent = scope.ServiceProvider.GetRequiredService<BLL.Services.IAgentService>();
//    Console.WriteLine(">>> שואל את האייג'נט האמיתי...");

//    var reply = await agent.AskAsync(new List<DTO.AIDTO.ChatMessageDto>
//    {
//        new DTO.AIDTO.ChatMessageDto { Role = "user", Text = "שלום, תציג את עצמך במשפט אחד קצר" }
//    }, CancellationToken.None);

//    Console.WriteLine("*****************************************************");
//    Console.WriteLine($">>> תשובה ישירה מהאייג'נט:\n{reply}");
//    Console.WriteLine("*****************************************************");
//}
app.Run();
