using Gestionnaire.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gestionnaire.ApiService;

public static class SiteApiEndpoints
{
    public static void MapSiteApiEndpoints(this WebApplication app)
    {
        app.MapGet("/api/pages", async (SitePageService service, CancellationToken token) =>
            Results.Ok((await service.GetPublishedAsync(token)).Select(SitePageService.ToResponse)));

        app.MapGet("/api/pages/{slug}", async Task<IResult> (string slug, SitePageService service, CancellationToken token) =>
        {
            var page = await service.GetPublishedBySlugAsync(slug, token);
            return page is null ? Results.NotFound() : Results.Ok(SitePageService.ToResponse(page));
        });

        app.MapGet("/api/navigation", async (SiteNavigationService service, CancellationToken token) =>
            Results.Ok((await service.GetVisibleAsync(token)).Select(SiteNavigationService.ToResponse)));

        app.MapGet("/api/site-settings", async Task<IResult> (SiteSettingsService service, CancellationToken token) =>
        {
            var settings = await service.GetAsync(token);
            return settings is null ? Results.Problem("Les paramètres du site ne sont pas initialisés.") :
                Results.Ok(SiteSettingsService.ToResponse(settings));
        });

        app.MapGet("/api/media/{id:guid}", async Task<IResult> (Guid id, MediaAssetService service, CancellationToken token) =>
        {
            var asset = await service.GetAsync(id, token);
            // Affichage dans la page (pas de nom de téléchargement) et requêtes partielles pour la lecture vidéo.
            return asset is null ? Results.NotFound() :
                Results.File(asset.Data, asset.ContentType, enableRangeProcessing: true);
        });

        app.MapPost("/api/demo-requests", async Task<IResult> (
            DemoRequestCreateRequest request,
            DemoRequestService service,
            CancellationToken token) =>
        {
            var (demoRequest, error) = await service.CreateAsync(request, token);
            return error is not null
                ? Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = [error] })
                : Results.Created("/api/demo-requests", DemoRequestService.ToResponse(demoRequest!));
        }).WithMetadata(new RequestSizeLimitAttribute(16_384));

        var admin = app.MapGroup("/api/admin")
            .AddEndpointFilter<AdminApiKeyFilter>()
            .WithMetadata(new RequestSizeLimitAttribute(65_536));

        admin.MapGet("/pages", async (SitePageService service, CancellationToken token) =>
            Results.Ok((await service.GetAllAsync(token)).Select(SitePageService.ToResponse)));

        admin.MapPost("/pages", async Task<IResult> (
            SitePageWriteRequest request,
            SitePageService service,
            CancellationToken token) =>
        {
            var (page, error, conflict) = await service.SaveAsync(null, request, token);
            if (error is not null)
            {
                return conflict ? Results.Conflict(new { error }) : Results.ValidationProblem(new Dictionary<string, string[]> { ["page"] = [error] });
            }

            return Results.Created($"/api/pages/{page!.Slug}", SitePageService.ToResponse(page));
        });

        admin.MapPut("/pages/{id:guid}", async Task<IResult> (
            Guid id,
            SitePageWriteRequest request,
            SitePageService service,
            CancellationToken token) =>
        {
            var (page, error, conflict) = await service.SaveAsync(id, request, token);
            if (error is not null)
            {
                return conflict ? Results.Conflict(new { error }) : Results.ValidationProblem(new Dictionary<string, string[]> { ["page"] = [error] });
            }

            return page is null ? Results.NotFound() : Results.Ok(SitePageService.ToResponse(page));
        });

        admin.MapDelete("/pages/{id:guid}", async Task<IResult> (Guid id, SitePageService service, CancellationToken token) =>
            await service.DeleteAsync(id, token) ? Results.NoContent() : Results.NotFound());

        admin.MapGet("/navigation", async (SiteNavigationService service, CancellationToken token) =>
            Results.Ok((await service.GetAllAsync(token)).Select(SiteNavigationService.ToResponse)));

        admin.MapPost("/navigation", async Task<IResult> (
            SiteNavigationWriteRequest request,
            SiteNavigationService service,
            CancellationToken token) =>
        {
            var (item, error) = await service.CreateAsync(request, token);
            return error is not null
                ? Results.ValidationProblem(new Dictionary<string, string[]> { ["navigation"] = [error] })
                : Results.Created($"/api/admin/navigation/{item!.Id}", SiteNavigationService.ToResponse(item));
        });

        admin.MapPut("/navigation/{id:guid}", async Task<IResult> (
            Guid id,
            SiteNavigationWriteRequest request,
            SiteNavigationService service,
            CancellationToken token) =>
        {
            var (item, error) = await service.UpdateAsync(id, request, token);
            if (error is not null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["navigation"] = [error] });
            }

            return item is null ? Results.NotFound() : Results.Ok(SiteNavigationService.ToResponse(item));
        });

        admin.MapDelete("/navigation/{id:guid}", async Task<IResult> (Guid id, SiteNavigationService service, CancellationToken token) =>
            await service.DeleteAsync(id, token) ? Results.NoContent() : Results.NotFound());

        admin.MapGet("/site-settings", async Task<IResult> (SiteSettingsService service, CancellationToken token) =>
        {
            var settings = await service.GetAsync(token);
            return settings is null ? Results.Problem("Les paramètres du site ne sont pas initialisés.") :
                Results.Ok(SiteSettingsService.ToResponse(settings));
        });

        admin.MapPut("/site-settings", async Task<IResult> (
            SiteSettingsWriteRequest request,
            SiteSettingsService service,
            CancellationToken token) =>
        {
            var (settings, error) = await service.UpdateAsync(request, token);
            return error is not null
                ? Results.ValidationProblem(new Dictionary<string, string[]> { ["settings"] = [error] })
                : Results.Ok(SiteSettingsService.ToResponse(settings!));
        });

        admin.MapPost("/media", async Task<IResult> (
            IFormFile file,
            MediaAssetService service,
            CancellationToken token) =>
        {
            var maximumSize = MediaAssetService.MaximumSizeFor(file.ContentType);
            if (file.Length > maximumSize)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["file"] = [$"Le fichier ne doit pas dépasser {maximumSize / (1024 * 1024)} Mo."]
                });
            }

            await using var stream = file.OpenReadStream();
            var (asset, error) = await service.CreateAsync(file.FileName, file.ContentType, stream, token);
            return error is not null
                ? Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = [error] })
                : Results.Created($"/api/media/{asset!.Id}", new { asset.Id, asset.FileName, asset.ContentType, url = $"/api/media/{asset.Id}" });
        }).DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MediaAssetService.MaximumFileSize + 65_536));

        admin.MapDelete("/media/{id:guid}", async Task<IResult> (Guid id, MediaAssetService service, CancellationToken token) =>
            await service.DeleteAsync(id, token) ? Results.NoContent() : Results.NotFound());

        admin.MapGet("/demo-requests", async (DemoRequestService service, CancellationToken token) =>
            Results.Ok((await service.GetAllAsync(token)).Select(DemoRequestService.ToResponse)));

        admin.MapPut("/demo-requests/{id:guid}/status", async Task<IResult> (
            Guid id,
            DemoRequestStatusRequest request,
            DemoRequestService service,
            CancellationToken token) =>
        {
            var (demoRequest, invalidStatus) = await service.SetStatusAsync(id, request.Status, token);
            return invalidStatus ? Results.ValidationProblem(new Dictionary<string, string[]> { ["status"] = ["Statut invalide."] }) :
                demoRequest is null ? Results.NotFound() : Results.Ok(DemoRequestService.ToResponse(demoRequest));
        });

        admin.MapDelete("/demo-requests/{id:guid}", async Task<IResult> (Guid id, DemoRequestService service, CancellationToken token) =>
            await service.DeleteAsync(id, token) ? Results.NoContent() : Results.NotFound());
    }
}
