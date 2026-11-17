using System.Text.Json;
using System.Text.RegularExpressions;
using Gestionnaire.ApiService.Data;
using Gestionnaire.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace Gestionnaire.ApiService.Services;

public sealed class SitePageService(Contextbd dbContext)
{
    public Task<List<SitePage>> GetPublishedAsync(CancellationToken cancellationToken) =>
        dbContext.SitePages.AsNoTracking()
            .Where(page => page.IsPublished)
            .Include(page => page.Sections.OrderBy(section => section.SortOrder))
            .OrderBy(page => page.SortOrder)
            .ToListAsync(cancellationToken);

    public Task<SitePage?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.SitePages.AsNoTracking()
            .Where(page => page.IsPublished && page.Slug == slug)
            .Include(page => page.Sections.OrderBy(section => section.SortOrder))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<List<SitePage>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.SitePages.AsNoTracking()
            .Include(page => page.Sections.OrderBy(section => section.SortOrder))
            .OrderBy(page => page.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<(SitePage? Page, string? Error, bool Conflict)> SaveAsync(
        Guid? id,
        SitePageWriteRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
        {
            return (null, validationError, false);
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await dbContext.SitePages.AnyAsync(page => page.Slug == slug && page.Id != id, cancellationToken))
        {
            return (null, "Une page avec ce slug existe déjà.", true);
        }

        SitePage? page;
        if (id is null)
        {
            page = new SitePage();
            dbContext.SitePages.Add(page);
        }
        else
        {
            page = await dbContext.SitePages.Include(item => item.Sections)
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (page is null)
            {
                return (null, null, false);
            }

            dbContext.SitePageSections.RemoveRange(page.Sections);
        }

        var previousSlug = page.Slug;
        page.Slug = slug;
        if (previousSlug.Length > 0 && previousSlug != slug)
        {
            var previousUrl = previousSlug == "home" ? "/" : $"/{previousSlug}";
            var updatedUrl = slug == "home" ? "/" : $"/{slug}";
            var navigationItems = await dbContext.SiteNavigationItems
                .Where(item => item.Url == previousUrl)
                .ToListAsync(cancellationToken);
            foreach (var navigationItem in navigationItems)
            {
                navigationItem.Url = updatedUrl;
            }
        }

        page.Title = request.Title.Trim();
        page.Intro = request.Intro.Trim();
        page.MetaDescription = request.MetaDescription.Trim();
        page.IsPublished = request.IsPublished;
        page.SortOrder = request.SortOrder;
        page.UpdatedAt = DateTime.UtcNow;
        page.Sections = request.Sections.Select(section => new SitePageSection
        {
            Type = section.Type.Trim().ToLowerInvariant(),
            Heading = section.Heading.Trim(),
            Body = section.Body.Trim(),
            Content = section.Content.GetRawText(),
            SortOrder = section.SortOrder
        }).ToList();

        await dbContext.SaveChangesAsync(cancellationToken);
        return (page, null, false);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var page = await dbContext.SitePages.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (page is null)
        {
            return false;
        }

        var pageUrl = page.Slug == "home" ? "/" : $"/{page.Slug}";
        var navigationItems = await dbContext.SiteNavigationItems
            .Where(item => item.Url == pageUrl)
            .ToListAsync(cancellationToken);
        dbContext.SiteNavigationItems.RemoveRange(navigationItems);
        dbContext.SitePages.Remove(page);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static SitePageResponse ToResponse(SitePage page) => new(
        page.Id,
        page.Slug,
        page.Title,
        page.Intro,
        page.MetaDescription,
        page.IsPublished,
        page.SortOrder,
        page.UpdatedAt,
        page.Sections.OrderBy(section => section.SortOrder).Select(section =>
        {
            using var document = JsonDocument.Parse(section.Content);
            return new SitePageSectionResponse(
                section.Id,
                section.Type,
                section.Heading,
                section.Body,
                document.RootElement.Clone(),
                section.SortOrder);
        }).ToList());

    private static string? Validate(SitePageWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Slug) ||
            !Regex.IsMatch(request.Slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.IgnoreCase))
        {
            return "Le slug doit contenir uniquement des lettres, chiffres et tirets.";
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200 ||
            request.Intro is null || request.Intro.Length > 2000 ||
            request.MetaDescription is null || request.MetaDescription.Length > 320)
        {
            return "Le titre est obligatoire (200 caractères maximum) et les textes dépassent leurs limites.";
        }

        if (request.Sections is null || request.Sections.Count > 50 || request.Sections.Any(section =>
                section is null || string.IsNullOrWhiteSpace(section.Type) || section.Type.Length > 50 ||
                section.Heading is null || section.Heading.Length > 200 ||
                section.Body is null || section.Body.Length > 10_000 ||
                section.Content.ValueKind != JsonValueKind.Object || HasUnsafeUrl(section.Content)))
        {
            return "Les sections doivent avoir un type, des textes valides et un contenu objet JSON (50 sections maximum).";
        }

        return null;
    }

    private static bool HasUnsafeUrl(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.EndsWith("Url", StringComparison.OrdinalIgnoreCase) &&
                    property.Value.ValueKind == JsonValueKind.String &&
                    !IsSafeUrl(property.Value.GetString() ?? string.Empty))
                {
                    return true;
                }

                if (HasUnsafeUrl(property.Value))
                {
                    return true;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray().Any(HasUnsafeUrl);
        }

        return false;
    }

    private static bool IsSafeUrl(string value)
    {
        if (value.StartsWith('/', StringComparison.Ordinal))
        {
            return !value.StartsWith("//", StringComparison.Ordinal);
        }

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
