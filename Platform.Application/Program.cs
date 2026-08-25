using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Platform.Application.Contracts;
using Platform.Application.Middleware;
using Platform.Application.Services;
using Platform.Application.Swagger;
using Platform.Core.AchievementGraphs;
using Platform.Core.Appraisals;
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
    options.AddSecurityDefinition("studentCookie", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "Platform.Student",
        Description = "Cookie создаётся запросом POST /api/auth/student/login."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.OperationFilter<AchievementGraphXmlExampleFilter>();
});
builder.Services
    .AddControllers()
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
        options.Cookie.Name = "Platform.Student";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return context.Response.WriteAsJsonAsync(ApiErrors.AuthenticationRequired);
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.Response.WriteAsJsonAsync(ApiErrors.CourseAccessDenied);
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseNpgsql(GetConnectionString()));
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IStudentIdentityService, StudentIdentityService>();
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
