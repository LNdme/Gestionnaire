using Gestionnaire.ApiService.Data;
using Gestionnaire.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace Gestionnaire.ApiService.Services;

public sealed class SiteDatabaseInitializer(Contextbd dbContext, IConfiguration configuration, ILogger<SiteDatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        foreach (var (id, fileName, contentType) in SiteSeedContent.SeedMedia)
        {
            await SeedMediaAsync(id, fileName, contentType, cancellationToken);
        }

        // La réinitialisation remplace les pages, la navigation et les paramètres modifiés par l'administration :
        // elle n'est faite que sur demande explicite, jamais par défaut.
        if (configuration.GetValue<bool>("SiteContent:ResetContentOnStartup"))
        {
            await ResetContentAsync(cancellationToken);
        }

        if (!await dbContext.SiteSettings.AnyAsync(settings => settings.Id == 1, cancellationToken))
        {
            dbContext.SiteSettings.Add(SiteSeedContent.CreateSettings());
        }

        if (!await dbContext.SitePages.AnyAsync(cancellationToken))
        {
            dbContext.SitePages.AddRange(SiteSeedContent.CreatePages());
        }

        if (!await dbContext.SiteNavigationItems.AnyAsync(cancellationToken))
        {
            dbContext.SiteNavigationItems.AddRange(SiteSeedContent.CreateNavigation());
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ResetContentAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("Réinitialisation du contenu du site demandée : pages, navigation et paramètres sont remplacés par le contenu initial.");
        await dbContext.SitePages.ExecuteDeleteAsync(cancellationToken);
        await dbContext.SiteNavigationItems.ExecuteDeleteAsync(cancellationToken);
        await dbContext.SiteSettings.ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Crée ou met à jour un média initial (image ou vidéo de démo). Ses identifiants sont réservés au contenu
    /// initial (un envoi par l'administration crée toujours un nouvel identifiant) : remplacer le fichier de
    /// SeedMedia suffit donc à mettre à jour le média dans une base existante.
    /// </summary>
    private async Task SeedMediaAsync(Guid id, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SeedMedia", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Le média initial '{fileName}' est introuvable.", path);
        }

        var data = await File.ReadAllBytesAsync(path, cancellationToken);
        var asset = await dbContext.MediaAssets.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (asset is null)
        {
            dbContext.MediaAssets.Add(new MediaAsset { Id = id, FileName = fileName, ContentType = contentType, Data = data });
        }
        else if (!asset.Data.AsSpan().SequenceEqual(data) || asset.ContentType != contentType)
        {
            asset.Data = data;
            asset.ContentType = contentType;
            asset.FileName = fileName;
        }
    }
}
