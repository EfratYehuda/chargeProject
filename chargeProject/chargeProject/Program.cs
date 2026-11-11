using BLL.Mapper;
using BLL.Services;
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyApp.BLL.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//   טיפול מרכזי בשגיאות 
// כל שגיאה שנזרקת בקוד הופכת לסטטוס HTTP מתאים (404, 401, 403, 409...) במקום 500 כללי.
// הקובץ Middleware/GlobalExceptionHandler.cs חייב להיות קיים לפני שמריצים.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//  סוואגר עם אפשרות להדבקת טוקן (כפתור Authorize)
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


// הוסר המפתח הקבוע שהיה כתוב בקוד (סיכון אבטחה: מי שרואה את הקוד יכול לזייף טוקן).
// עכשיו המפתח, ה-Issuer וה-Audience באים רק מ-appsettings.json,
// והאפליקציה מסרבת לעלות אם חסר משהו - כך שרואים את הבעיה מיד ולא דרך שגיאות 401 מסתוריות.
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
            ValidateIssuer = true,                 // נשאר true: השרת בודק מי הנפיק את הטוקן
            ValidIssuer = jwtIssuer,               // שונה: בלי ערך ברירת מחדל, בדיוק אותו ערך שה-TokenService חותם איתו

            ValidateAudience = true,               // נשאר true: השרת בודק למי הטוקן מיועד
            ValidAudience = jwtAudience,           // שונה: בלי ערך ברירת מחדל

            ValidateLifetime = true,               // נשאר true: טוקן שפג תוקפו נדחה
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,       // נשאר true: בודק שהחתימה תואמת למפתח הסודי
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // כל נקודת קצה דורשת התחברות, חוץ ממה שמסומן [AllowAnonymous] (register ו-login)
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // חדש: מדיניות לפי פרופיל. נשתמש בהן בחלקים הבאים עם [Authorize(Policy = "Driver")]
    options.AddPolicy("Driver", policy => policy.RequireClaim("driverId"));
    options.AddPolicy("Owner", policy => policy.RequireClaim("ownerId"));
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

// חייב להיות מוקדם בצנרת, כדי לתפוס שגיאות מכל מה שבא אחריו
app.UseExceptionHandler();

//  הפיכת משתמשים ספציפיים לאדמין בעת עליית השרת
// קורא את הרשימה "AdminEmails" מ-appsettings.json ומעדכן Access = "Admin".
// אזהרה: האימייל לא מאומת. האדמין האמיתי חייב להירשם קודם, ורק אחר כך להוסיף את האימייל שלו לרשימה.

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
        // אם מסד הנתונים עדיין לא קיים, השרת לא קורס, רק כותב ללוג
        logger.LogError(ex, "Could not apply AdminEmails at startup. Is the database created and migrated?");
    }
}

if (app.Environment.IsDevelopment())
{
    // AllowAnonymous נחוץ, אחרת ה-FallbackPolicy חוסם את הסוואגר עצמו
    app.MapOpenApi().AllowAnonymous();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();   // קודם בודק מי אתה (קורא את הטוקן)
app.UseAuthorization();    // ואז בודק מה מותר לך. הסדר הזה חשוב

app.MapControllers();

app.Run();