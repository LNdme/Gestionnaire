using Gestionnaire.ApiService.Data;
using Gestionnaire.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace Gestionnaire.ApiService.Services;

public sealed class SiteSettingsService(Contextbd dbContext)
{
    public async Task<SiteSettings?> GetAsync(CancellationToken cancellationToken) =>
        await dbContext.SiteSettings.AsNoTracking().SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);

    public async Task<(SiteSettings? Settings, string? Error)> UpdateAsync(
        SiteSettingsWriteRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SiteName) || request.SiteName.Length > 120 ||
            string.IsNullOrWhiteSpace(request.BrandMarkText) || request.BrandMarkText.Length > 4 ||
            request.LogoUrl is null || request.LogoUrl.Length > 500 ||
            (request.LogoUrl.Length > 0 && !IsSafeUrl(request.LogoUrl)) ||
            string.IsNullOrWhiteSpace(request.LoginLabel) || request.LoginLabel.Length > 120 ||
            request.LoginUrl is null || request.LoginUrl.Length > 500 || !IsAbsoluteHttpUrl(request.LoginUrl) ||
            string.IsNullOrWhiteSpace(request.DemoActionLabel) || request.DemoActionLabel.Length > 120 ||
            string.IsNullOrWhiteSpace(request.DemoActionUrl) || request.DemoActionUrl.Length > 500 || !IsSafeUrl(request.DemoActionUrl) ||
            request.FooterDescription is null || request.FooterDescription.Length > 2000 ||
            request.FooterTagline is null || request.FooterTagline.Length > 500 ||
            request.CopyrightNotice is null || request.CopyrightNotice.Length > 300 ||
            !IsOptionalSafeUrl(request.DemoVideoUrl, 500) || !IsOptionalSafeUrl(request.DemoVideoPosterUrl, 500) ||
            request.DemoVideoLabel?.Length > 120 || request.DemoVideoTitle?.Length > 200 ||
            !PhoneNumber.IsValidOrEmpty(request.ContactPhone) || !PhoneNumber.IsValidOrEmpty(request.ContactWhatsApp) ||
            request.ContactHours?.Length > 200)
        {
            return (null, "Le nom du site, les libellés et les URL doivent être valides et les textes respecter leurs limites.");
        }

        var settings = await dbContext.SiteSettings.SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (settings is null)
        {
            throw new InvalidOperationException("Les paramètres du site ne sont pas initialisés.");
        }

        settings.SiteName = request.SiteName.Trim();
        settings.BrandMarkText = request.BrandMarkText.Trim();
        settings.LogoUrl = request.LogoUrl.Trim();
        settings.LoginLabel = request.LoginLabel.Trim();
        settings.LoginUrl = request.LoginUrl.Trim();
        settings.DemoActionLabel = request.DemoActionLabel.Trim();
        settings.DemoActionUrl = request.DemoActionUrl.Trim();
        settings.FooterDescription = request.FooterDescription.Trim();
        settings.FooterTagline = request.FooterTagline.Trim();
        settings.CopyrightNotice = request.CopyrightNotice.Trim();
        settings.DemoVideoUrl = request.DemoVideoUrl?.Trim() ?? settings.DemoVideoUrl;
        settings.DemoVideoPosterUrl = request.DemoVideoPosterUrl?.Trim() ?? settings.DemoVideoPosterUrl;
        settings.DemoVideoLabel = request.DemoVideoLabel?.Trim() ?? settings.DemoVideoLabel;
        settings.DemoVideoTitle = request.DemoVideoTitle?.Trim() ?? settings.DemoVideoTitle;
        settings.ContactPhone = request.ContactPhone is null ? settings.ContactPhone : PhoneNumber.Normalize(request.ContactPhone);
        settings.ContactWhatsApp = request.ContactWhatsApp is null ? settings.ContactWhatsApp : PhoneNumber.Normalize(request.ContactWhatsApp);
        settings.ContactHours = request.ContactHours?.Trim() ?? settings.ContactHours;
        if (settings.DemoVideoUrl.Length > 0 && settings.DemoVideoLabel.Length == 0)
        {
            settings.DemoVideoLabel = "Regarder la démo";
        }
        settings.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return (settings, null);
    }

    public static SiteSettingsResponse ToResponse(SiteSettings settings) => new(
        settings.SiteName, settings.BrandMarkText, settings.LogoUrl, settings.LoginLabel, settings.LoginUrl,
        settings.DemoActionLabel, settings.DemoActionUrl, settings.FooterDescription, settings.FooterTagline,
        settings.CopyrightNotice, settings.UpdatedAt,
        settings.DemoVideoUrl, settings.DemoVideoPosterUrl, settings.DemoVideoLabel, settings.DemoVideoTitle,
        settings.ContactPhone, settings.ContactWhatsApp, settings.ContactHours);

    private static bool IsAbsoluteHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static bool IsOptionalSafeUrl(string? value, int maxLength) =>
        value is null || (value.Length <= maxLength && (value.Trim().Length == 0 || IsSafeUrl(value.Trim())));

    private static bool IsSafeUrl(string value) =>
        (value.StartsWith('/', StringComparison.Ordinal) && !value.StartsWith("//", StringComparison.Ordinal)) ||
        IsAbsoluteHttpUrl(value);
}
