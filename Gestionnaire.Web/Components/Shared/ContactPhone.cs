namespace Gestionnaire.Web.Components.Shared;

/// <summary>
/// Liens d'appel à partir du numéro des paramètres du site (stocké au format international, ex. +237696649233).
/// </summary>
public static class ContactPhone
{
    private static string Digits(string? phone) => new((phone ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    public static bool IsSet(string? phone) => Digits(phone).Length >= 8;

    /// <summary>Lien <c>tel:</c> qui lance l'appel sur un téléphone.</summary>
    public static string CallHref(string? phone)
    {
        var digits = Digits(phone);
        return (phone ?? string.Empty).TrimStart().StartsWith('+') ? $"tel:+{digits}" : $"tel:{digits}";
    }

    /// <summary>Conversation WhatsApp (wa.me attend l'indicatif sans « + »).</summary>
    public static string WhatsAppHref(string? phone) => $"https://wa.me/{Digits(phone)}";

    /// <summary>
    /// Numéro lisible à l'oral : « +237 6 96 64 92 33 » pour le Cameroun (indicatif + 9 chiffres),
    /// sinon l'indicatif puis des groupes de deux chiffres.
    /// </summary>
    public static string Display(string? phone)
    {
        var digits = Digits(phone);
        if (digits.Length == 0)
        {
            return string.Empty;
        }

        var international = (phone ?? string.Empty).TrimStart().StartsWith('+');
        string prefix = string.Empty, rest = digits;
        if (international && digits.StartsWith("237") && digits.Length == 12)
        {
            prefix = "+237 ";
            rest = digits[3..];
        }
        else if (international && digits.Length > 9)
        {
            var countryLength = digits.Length - 9;
            prefix = $"+{digits[..countryLength]} ";
            rest = digits[countryLength..];
        }

        // 9 chiffres : « 6 96 64 92 33 » ; sinon des paires.
        var groups = new List<string>();
        var start = rest.Length % 2;
        if (start == 1) groups.Add(rest[..1]);
        for (var i = start; i < rest.Length; i += 2) groups.Add(rest.Substring(i, 2));
        return prefix + string.Join(' ', groups);
    }
}
