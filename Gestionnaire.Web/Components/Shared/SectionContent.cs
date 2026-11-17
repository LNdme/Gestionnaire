using System.Text.Json;

namespace Gestionnaire.Web.Components.Shared;

/// <summary>
/// Lecture tolérante de l'objet <c>content</c> libre d'une section renvoyée par l'API.
/// Une clé absente ou d'un autre type renvoie la valeur par défaut au lieu de lever une exception.
/// </summary>
public static class SectionContent
{
    public static string Text(JsonElement element, string propertyName, string fallback = "") =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;

    public static bool Has(JsonElement element, string propertyName) =>
        !string.IsNullOrWhiteSpace(Text(element, propertyName));

    public static IReadOnlyList<JsonElement> Items(JsonElement element, string propertyName = "items") =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).ToList()
            : [];

    public static IReadOnlyList<string> Strings(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList()
            : [];

    /// <summary>Numéro affiché « 01 », « 02 »… à partir d'un index commençant à zéro.</summary>
    public static string Number(int index) => (index + 1).ToString("00");
}
