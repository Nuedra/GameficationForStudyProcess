using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Platform.Application.Authentication;
using Platform.Application.Contracts;
using Platform.Application.Middleware;
using Platform.Application.Services;
using Platform.Application.Swagger;
using Platform.Core.AchievementGraphs;
using Platform.Core.Appraisals;
using Platform.Core.Models;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;

var builder = WebApplication.CreateBuilder(args);

string GetConnectionString() => PlatformDatabaseConnection.Require(
    builder.Configuration.GetConnectionString(PlatformDatabaseConnection.ConnectionStringName));

var enableHttpsRedirection = !builder.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("HttpsRedirection:Enabled");
var httpsPort = builder.Configuration.GetValue<int?>("HttpsRedirection:HttpsPort");

if (enableHttpsRedirection && httpsPort.HasValue)
{
    builder.Services.AddHttpsRedirection(options => options.HttpsPort = httpsPort.Value);
}

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gamification Platform API",
        Version = "v1",
        Description = "API для Blazor-клиента платформы учебных ачивок."
    });
    options.AddSecurityDefinition("authenticationCookie", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "Platform.Auth",
        Description = "Cookie создаётся запросом POST /api/auth/login."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.OperationFilter<AchievementGraphXmlExampleFilter>();
    options.OperationFilter<AntiforgeryHeaderOperationFilter>();
});
builder.Services
    .AddControllers(options =>
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Platform.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.EventsType = typeof(PlatformCookieAuthenticationEvents);
    });
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "Platform.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<PlatformCookieAuthenticationEvents>();
builder.Services.AddSingleton<InMemoryAuthenticationTicketStore>();
builder.Services
    .AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<InMemoryAuthenticationTicketStore>((options, ticketStore) =>
        options.SessionStore = ticketStore);
builder.Services
    .AddOptions<GuidAuthenticationOptions>()
    .BindConfiguration(GuidAuthenticationOptions.SectionName)
    .Validate(
        options => options.PrivilegedUsers.All(user =>
            user.Id != Guid.Empty &&
            !string.IsNullOrWhiteSpace(user.DisplayName) &&
            user.Role is UserRole.Teacher or UserRole.Administrator),
        "Привилегированные GUID-пользователи должны иметь ID, имя и роль teacher или administrator.")
    .Validate(
        options => options.PrivilegedUsers
            .Select(user => user.Id)
            .Distinct()
            .Count() == options.PrivilegedUsers.Count,
        "GUID привилегированных пользователей не должны повторяться.")
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseNpgsql(GetConnectionString()));
builder.Services.AddScoped<IUserIdentityService, UserIdentityService>();
builder.Services.AddScoped<IStudentCourseService, StudentCourseService>();
builder.Services.AddScoped<IStudentAchievementGraphService, StudentAchievementGraphService>();
builder.Services.AddSingleton<IAchievementGraphTemplateProvider, FileAchievementGraphTemplateProvider>();
builder.Services.AddSingleton<IAchievementGraphXmlSerializer, AchievementGraphXmlSerializer>();
builder.Services.AddSingleton<IAppraisalPayloadParser, AppraisalPayloadParser>();
builder.Services.AddSingleton<IAppraisalFactsExtractor, AppraisalFactsExtractor>();
builder.Services.AddSingleton<IAppraisalPayloadProvider, FixedAppraisalPayloadProvider>();
builder.Services.AddScoped(serviceProvider => new AchievementProcessingCycle(
    GetConnectionString(),
    serviceProvider.GetRequiredService<IAppraisalPayloadProvider>(),
    serviceProvider.GetRequiredService<IAppraisalFactsExtractor>()));

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (enableHttpsRedirection)
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/ready", async (
    PlatformDbContext dbContext,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (await dbContext.Database.CanConnectAsync(cancellationToken))
            return Results.Ok(new { status = "ready" });
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "Database readiness check failed.");
    }

    return Results.Problem(
        title: "База данных недоступна",
        detail: "Проверьте, что база данных запущена и настройки подключения корректны.",
        statusCode: StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public partial class Program;
