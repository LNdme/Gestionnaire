using Gestionnaire.Web;
using Gestionnaire.Web.Components;
using Gestionnaire.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");
builder.Services.AddOutputCache();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<SiteContentService>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Les URL inconnues (et les pages absentes de l'API) affichent la page 404 du site avec le bon code HTTP.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

// Relais des médias de l'API (images et vidéo de démo), diffusés sans être chargés en mémoire.
app.MapGet("/api/media/{id:guid}", async Task (
    Guid id,
    HttpContext context,
    SiteContentService siteContentService,
    CancellationToken cancellationToken) =>
{
    using var upstream = await siteContentService.OpenMediaAsync(id, context.Request.Headers.Range, cancellationToken);
    var status = (int)upstream.StatusCode;
    if (status is not (StatusCodes.Status200OK or StatusCodes.Status206PartialContent or StatusCodes.Status416RangeNotSatisfiable))
    {
        context.Response.StatusCode = upstream.StatusCode == System.Net.HttpStatusCode.NotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status502BadGateway;
        return;
    }

    context.Response.StatusCode = status;
    var headers = upstream.Content.Headers;
    if (headers.ContentType is not null) context.Response.ContentType = headers.ContentType.ToString();
    if (headers.ContentLength is long length) context.Response.ContentLength = length;
    if (headers.ContentRange is not null) context.Response.Headers.ContentRange = headers.ContentRange.ToString();
    context.Response.Headers.AcceptRanges = "bytes";
    context.Response.Headers.CacheControl = "public, max-age=3600";
    await upstream.Content.CopyToAsync(context.Response.Body, cancellationToken);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
