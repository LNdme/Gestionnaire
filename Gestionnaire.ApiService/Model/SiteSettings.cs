namespace Gestionnaire.ApiService.Model;

public sealed class SiteSettings
{
    public short Id { get; set; } = 1;
    public string SiteName { get; set; } = "Gestionnaire";
    public string BrandMarkText { get; set; } = "G";
    public string LogoUrl { get; set; } = string.Empty;
    public string LoginLabel { get; set; } = "Se connecter";
    public string LoginUrl { get; set; } = "https://smallapp.org";
    public string DemoActionLabel { get; set; } = "Démo";
    public string DemoActionUrl { get; set; } = "/demo";

    // Vidéo de démonstration affichée dans une fenêtre du site (bouton de l'en-tête et de l'accueil).
    public string DemoVideoUrl { get; set; } = string.Empty;
    public string DemoVideoPosterUrl { get; set; } = string.Empty;
    public string DemoVideoLabel { get; set; } = "Regarder la démo";
    public string DemoVideoTitle { get; set; } = string.Empty;

    // Contact direct, pour les personnes qui préfèrent appeler plutôt que remplir un formulaire.
    // Le numéro est au format international (+237…) ; les boutons sont masqués tant qu'il est vide.
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactWhatsApp { get; set; } = string.Empty;
    public string ContactHours { get; set; } = string.Empty;
    public string FooterDescription { get; set; } = string.Empty;
    public string FooterTagline { get; set; } = string.Empty;
    public string CopyrightNotice { get; set; } = "Tous droits réservés.";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
