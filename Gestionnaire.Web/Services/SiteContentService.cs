using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gestionnaire.Web.DTO;

namespace Gestionnaire.Web.Services;

/// <summary>
/// Client typé des routes publiques de Gestionnaire.ApiService.
/// Les routes d'administration (X-API-Key) ne sont volontairement pas exposées au site public.
/// </summary>
public sealed class SiteContentService(HttpClient httpClient)
{
    /// <summary><c>GET /api/pages</c> : pages publiées, triées par ordre d'affichage.</summary>
    public async Task<IReadOnlyList<SitePageDto>> GetPagesAsync(CancellationToken cancellationToken = default)
    {
        var pages = await httpClient.GetFromJsonAsync<List<SitePageDto>>("/api/pages", cancellationToken)
            ?? throw new JsonException("L'API a renvoyé une liste de pages vide.");
        return pages.OrderBy(page => page.SortOrder).ToList();
    }

    /// <summary><c>GET /api/pages/{slug}</c> : une page publiée, ou <c>null</c> si elle n'existe pas.</summary>
    public async Task<SitePageDto?> GetPageAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"/api/pages/{Uri.EscapeDataString(slug)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SitePageDto>(cancellationToken)
            ?? throw new JsonException("L'API a renvoyé une page vide.");
    }

    /// <summary><c>GET /api/navigation</c> : liens visibles de l'en-tête et du pied de page.</summary>
    public async Task<SiteNavigationItemDto[]> GetNavigationAsync(CancellationToken cancellationToken = default)
    {
        var items = await httpClient.GetFromJsonAsync<SiteNavigationItemDto[]>("/api/navigation", cancellationToken)
            ?? throw new JsonException("L'API a renvoyé une navigation vide.");
        return items.Where(item => item.IsVisible).OrderBy(item => item.SortOrder).ToArray();
    }

    /// <summary><c>GET /api/site-settings</c> : nom, marque, liens d'accès et textes du pied de page.</summary>
    public async Task<SiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<SiteSettingsDto>("/api/site-settings", cancellationToken)
            ?? throw new JsonException("L'API a renvoyé des paramètres vides.");
    }

    /// <summary>
    /// <c>GET /api/media/{id}</c> : image ou vidéo gérée par l'API, relayée par le site sous la même URL.
    /// L'en-tête <c>Range</c> est transmis pour que la vidéo puisse être lue par morceaux. La réponse est
    /// renvoyée sans être lue : l'appelant la diffuse puis la libère.
    /// </summary>
    public async Task<HttpResponseMessage> OpenMediaAsync(Guid id, string? range, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/media/{id}");
        if (!string.IsNullOrWhiteSpace(range))
        {
            request.Headers.TryAddWithoutValidation("Range", range);
        }

        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    /// <summary><c>POST /api/demo-requests</c> : enregistre une demande de démonstration.</summary>
    public async Task<DemoRequestResult> SubmitDemoRequestAsync(
        DemoRequestInput request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/demo-requests", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return DemoRequestResult.Sent;
        }

        return response.StatusCode == HttpStatusCode.BadRequest
            ? DemoRequestResult.Invalid
            : DemoRequestResult.Failed;
    }
}
