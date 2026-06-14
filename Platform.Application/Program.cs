using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Services;
using Platform.Core.Appraisals;
using Platform.Core.Processing;
using Platform.DataAccess.Postgress;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
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
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
