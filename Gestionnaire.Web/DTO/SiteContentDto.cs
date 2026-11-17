using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Gestionnaire.Web.DTO;

public sealed record SitePageDto(
    Guid Id,
    string Slug,
    string Title,
    string Intro,
    string MetaDescription,
    bool IsPublished,
    int SortOrder,
    DateTime UpdatedAt,
    IReadOnlyList<SitePageSectionDto> Sections);

public sealed record SitePageSectionDto(Guid Id, string Type, string Heading, string Body, JsonElement Content, int SortOrder);

public sealed record SiteNavigationItemDto(Guid Id, string Label, string Url, int SortOrder, bool IsVisible);

public sealed record SiteSettingsDto(
    string SiteName,
    string BrandMarkText,
    string LogoUrl,
    string LoginLabel,
    string LoginUrl,
    string DemoActionLabel,
    string DemoActionUrl,
    string FooterDescription,
    string FooterTagline,
    string CopyrightNotice,
    DateTime UpdatedAt,
    string DemoVideoUrl = "",
    string DemoVideoPosterUrl = "",
    string DemoVideoLabel = "",
    string DemoVideoTitle = "",
    string ContactPhone = "",
    string ContactWhatsApp = "",
    string ContactHours = "");


/// <summary>Résultat de l'envoi d'une demande de démonstration à l'API.</summary>
public enum DemoRequestResult
{
    Sent,
    Invalid,
    Failed
}

public sealed class DemoRequestInput : IValidatableObject
{
    [Required(ErrorMessage = "Indiquez votre nom complet.")]
    [StringLength(160, ErrorMessage = "Le nom ne doit pas dépasser 160 caractères.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indiquez le nom de l'établissement.")]
    [StringLength(200, ErrorMessage = "Le nom de l'établissement ne doit pas dépasser 200 caractères.")]
    public string Establishment { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Ce numéro de téléphone n'est pas valide.")]
    [StringLength(30, ErrorMessage = "Le numéro de téléphone est trop long.")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Cette adresse e-mail n'est pas valide.")]
    [StringLength(254, ErrorMessage = "L'adresse e-mail est trop longue.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choisissez le nombre approximatif d'élèves.")]
    [StringLength(40)]
    public string StudentCount { get; set; } = string.Empty;

    [StringLength(5000, ErrorMessage = "Le message ne doit pas dépasser 5 000 caractères.")]
    public string Message { get; set; } = string.Empty;

    // Un téléphone OU une adresse e-mail suffit : beaucoup préfèrent être rappelés.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Phone) && string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult(
                "Indiquez un numéro de téléphone ou une adresse e-mail pour que nous puissions vous répondre.",
                [nameof(Phone)]);
        }
    }
}
