namespace Gestionnaire.ApiService.Services;

/// <summary>
/// Numéros de téléphone saisis librement (« +237 6 96 64 92 33 », « 696-64-92-33 »…) :
/// ils sont stockés sans espaces ni séparateurs, avec un « + » initial facultatif.
/// </summary>
public static class PhoneNumber
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        var digits = new string(trimmed.Where(char.IsAsciiDigit).ToArray());
        return trimmed.StartsWith('+') ? "+" + digits : digits;
    }

    /// <summary>Vide, ou 8 à 15 chiffres (norme E.164) avec seulement espaces, points, tirets et parenthèses autour.</summary>
    public static bool IsValidOrEmpty(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 30 || trimmed.Skip(trimmed.StartsWith('+') ? 1 : 0)
                .Any(c => !char.IsAsciiDigit(c) && c is not (' ' or '.' or '-' or '(' or ')')))
        {
            return false;
        }

        var digitCount = trimmed.Count(char.IsAsciiDigit);
        return digitCount is >= 8 and <= 15;
    }
}
