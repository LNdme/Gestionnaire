using Gestionnaire.ApiService;
using Gestionnaire.ApiService.Data;
using Gestionnaire.ApiService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("sitecontent")
    ?? throw new InvalidOperationException("La connexion PostgreSQL 'sitecontent' n'est pas configurée.");
builder.Services.AddDbContext<Contextbd>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<SiteDatabaseInitializer>();
builder.Services.AddScoped<SitePageService>();
builder.Services.AddScoped<SiteNavigationService>();
builder.Services.AddScoped<SiteSettingsService>();
builder.Services.AddScoped<DemoRequestService>();
builder.Services.AddScoped<MediaAssetService>();
builder.Services.AddScoped<AdminApiKeyFilter>();

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<SiteDatabaseInitializer>().InitializeAsync();
}

app.MapGet("/", () => "Gestionnaire API is running.");
app.MapSiteApiEndpoints();
app.MapDefaultEndpoints();
app.Run();
