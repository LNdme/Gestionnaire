using Microsoft.Extensions.Logging;

namespace Gestionnaire.Tests;

/// <summary>
/// Vidéo de démonstration et contact direct : la vidéo se lit par morceaux à travers le site
/// et les liens d'appel et WhatsApp sont présents sur l'accueil.
/// </summary>
public class SiteContactAndVideoTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
    private const string DemoVideoPath = "/api/media/46a0d31a-af12-40d6-b9ba-e05ed4087511";

    [Test]
    public async Task VideoDemoEtContactTelephoneSontServis()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Gestionnaire_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var web = app.CreateHttpClient("webfrontend");

        // Paramètres publics : vidéo et téléphone renseignés.
        var api = app.CreateHttpClient("apiservice");
        var settings = await api.GetStringAsync("/api/site-settings", cancellationToken);
        Assert.That(settings, Does.Contain(DemoVideoPath));
        Assert.That(settings, Does.Contain("+237696649233"));

        // Lecture partielle de la vidéo à travers le site (nécessaire pour avancer dans la vidéo).
        using var request = new HttpRequestMessage(HttpMethod.Get, DemoVideoPath);
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 1023);
        using var video = await web.SendAsync(request, cancellationToken);
        Assert.That(video.StatusCode, Is.EqualTo(HttpStatusCode.PartialContent));
        Assert.That(video.Content.Headers.ContentType?.MediaType, Is.EqualTo("video/mp4"));
        Assert.That(video.Content.Headers.ContentLength, Is.EqualTo(1024));

        // Accueil : bouton vidéo, appel direct et WhatsApp.
        var home = await web.GetStringAsync("/", cancellationToken);
        Assert.That(home, Does.Contain("href=\"tel:&#x2B;237696649233\""));
        Assert.That(home, Does.Contain("https://wa.me/237696649233"));
        Assert.That(home, Does.Contain(DemoVideoPath));
    }
}
