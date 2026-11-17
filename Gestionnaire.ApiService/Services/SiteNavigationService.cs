using Gestionnaire.ApiService.Data;
using Gestionnaire.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace Gestionnaire.ApiService.Services;

public sealed class SiteNavigationService(Contextbd dbContext)
{
    public Task<List<SiteNavigationItem>> GetVisibleAsync(CancellationToken cancellationToken) =>
        dbContext.SiteNavigationItems.AsNoTracking().Where(item => item.IsVisible)
            .OrderBy(item => item.SortOrder).ToListAsync(cancellationToken);

    public Task<List<SiteNavigationItem>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.SiteNavigationItems.AsNoTracking().OrderBy(item => item.SortOrder).ToListAsync(cancellationToken);

    public async Task<(SiteNavigationItem? Item, string? Error)> CreateAsync(
        SiteNavigationWriteRequest request,
        CancellationToken cancellationToken)
    {
        var error = Validate(request);
        if (error is not null)
        {
            return (null, error);
        }

        var item = new SiteNavigationItem
        {
            Label = request.Label.Trim(),
            Url = request.Url.Trim(),
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible
        };
        dbContext.SiteNavigationItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (item, null);
    }

    public async Task<(SiteNavigationItem? Item, string? Error)> UpdateAsync(
        Guid id,
        SiteNavigationWriteRequest request,
        CancellationToken cancellationToken)
    {
        var error = Validate(request);
        if (error is not null)
        {
            return (null, error);
        }

        var item = await dbContext.SiteNavigationItems.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (item is null)
        {
            return (null, null);
        }

        item.Label = request.Label.Trim();
        item.Url = request.Url.Trim();
        item.SortOrder = request.SortOrder;
        item.IsVisible = request.IsVisible;
        await dbContext.SaveChangesAsync(cancellationToken);
        return (item, null);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await dbContext.SiteNavigationItems.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        dbContext.SiteNavigationItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static SiteNavigationResponse ToResponse(SiteNavigationItem item) =>
        new(item.Id, item.Label, item.Url, item.SortOrder, item.IsVisible);

    private static string? Validate(SiteNavigationWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Label) || request.Label.Length > 120 ||
            string.IsNullOrWhiteSpace(request.Url) || request.Url.Length > 500)
        {
            return "Le libellé et l'URL sont obligatoires et doivent respecter leurs limites de longueur.";
        }

        return IsSafeUrl(request.Url) ? null : "L'URL doit être un chemin interne ou une adresse HTTP(S).";
    }

    private static bool IsSafeUrl(string value) =>
        (value.StartsWith('/', StringComparison.Ordinal) && !value.StartsWith("//", StringComparison.Ordinal)) ||
        (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
         (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp));
}
