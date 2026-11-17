using System.Text.Json;

namespace Gestionnaire.ApiService.Services;

public sealed record SitePageWriteRequest(
    string Slug,
    string Title,
    string Intro,
    string MetaDescription,
    bool IsPublished,
    int SortOrder,
    IReadOnlyList<SitePageSectionWriteRequest> Sections);

public sealed record SitePageSectionWriteRequest(
    string Type,
    string Heading,
    string Body,
    JsonElement Content,
    int SortOrder);

public sealed record SitePageResponse(
    Guid Id,
    string Slug,
    string Title,
    string Intro,
    string MetaDescription,
    bool IsPublished,
    int SortOrder,
    DateTime UpdatedAt,
    IReadOnlyList<SitePageSectionResponse> Sections);

public sealed record SitePageSectionResponse(
    Guid Id,
    string Type,
    string Heading,
    string Body,
    JsonElement Content,
    int SortOrder);

public sealed record SiteNavigationWriteRequest(string Label, string Url, int SortOrder, bool IsVisible);
public sealed record SiteNavigationResponse(Guid Id, string Label, string Url, int SortOrder, bool IsVisible);
public sealed record SiteSettingsWriteRequest(string SiteName, string BrandMarkText, string LogoUrl, string LoginLabel, string LoginUrl, string DemoActionLabel, string DemoActionUrl, string FooterDescription, string FooterTagline, string CopyrightNotice,
    // Champs ajoutés ensuite : absents (null) = valeur actuelle conservée, pour ne rien effacer par mégarde.
    string? DemoVideoUrl = null, string? DemoVideoPosterUrl = null, string? DemoVideoLabel = null, string? DemoVideoTitle = null,
    string? ContactPhone = null, string? ContactWhatsApp = null, string? ContactHours = null);
public sealed record SiteSettingsResponse(string SiteName, string BrandMarkText, string LogoUrl, string LoginLabel, string LoginUrl, string DemoActionLabel, string DemoActionUrl, string FooterDescription, string FooterTagline, string CopyrightNotice, DateTime UpdatedAt,
    string DemoVideoUrl, string DemoVideoPosterUrl, string DemoVideoLabel, string DemoVideoTitle,
    string ContactPhone, string ContactWhatsApp, string ContactHours);
public sealed record DemoRequestCreateRequest(string FullName, string Establishment, string? Email, string StudentCount, string Message, string? Phone = null);
public sealed record DemoRequestStatusRequest(string Status);
public sealed record DemoRequestResponse(Guid Id, string FullName, string Establishment, string Email, string Phone, string StudentCount, string Message, string Status, DateTime CreatedAt);
