using Platform.Application.Services;
using Platform.Core.Appraisals;
using Platform.Core.Processing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<IUserContextService, UserContextService>();
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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
