using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Platform.Application.Contracts;
using Platform.Application.Services;
using Platform.Core.Appraisals;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;

var builder = WebApplication.CreateBuilder(args);

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
    options.UseNpgsql(
        Environment.GetEnvironmentVariable("PLATFORM_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=platform;Username=postgres;Password=pass"));
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IStudentIdentityService, StudentIdentityService>();
builder.Services.AddScoped<IStudentCourseService, StudentCourseService>();
builder.Services.AddScoped<IStudentAchievementService, StudentAchievementService>();
builder.Services.AddSingleton<IAppraisalPayloadParser, AppraisalPayloadParser>();
builder.Services.AddSingleton<IAppraisalFactsExtractor, AppraisalFactsExtractor>();
builder.Services.AddSingleton<IAppraisalPayloadProvider, FixedAppraisalPayloadProvider>();
builder.Services.AddScoped(serviceProvider => new AchievementProcessingCycle(
    Environment.GetEnvironmentVariable("PLATFORM_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=platform;Username=postgres;Password=pass",
    serviceProvider.GetRequiredService<IAppraisalPayloadProvider>(),
    serviceProvider.GetRequiredService<IAppraisalFactsExtractor>()));

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
