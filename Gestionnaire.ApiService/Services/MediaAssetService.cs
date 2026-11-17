using Gestionnaire.ApiService.Data;
using System.Text.Json.Nodes;
using Gestionnaire.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace Gestionnaire.ApiService.Services;

public sealed class MediaAssetService(Contextbd dbContext)
{
    public static readonly long MaximumImageSize = 5 * 1024 * 1024;
    public static readonly long MaximumVideoSize = 60 * 1024 * 1024;

    /// <summary>Plus grande taille acceptée tous formats confondus (limite de la requête d'envoi).</summary>
    public static long MaximumFileSize => MaximumVideoSize;

    private static readonly HashSet<string> ImageContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private static readonly HashSet<string> VideoContentTypes = ["video/mp4", "video/webm"];

    public static long MaximumSizeFor(string contentType) =>
        VideoContentTypes.Contains(contentType) ? MaximumVideoSize : MaximumImageSize;

    public Task<MediaAsset?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.MediaAssets.AsNoTracking().SingleOrDefaultAsync(asset => asset.Id == id, cancellationToken);

    public async Task<(MediaAsset? Asset, string? Error)> CreateAsync(
        string fileName,
        string contentType,
        Stream stream,
        CancellationToken cancellationToken)
    {
        if (!ImageContentTypes.Contains(contentType) && !VideoContentTypes.Contains(contentType))
        {
            return (null, "Seules les images JPEG, PNG, WebP et les vidéos MP4 ou WebM sont acceptées.");
        }

        var maximumSize = MaximumSizeFor(contentType);

        await using var buffer = new MemoryStream();
        var chunk = new byte[81_920];
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + bytesRead > maximumSize)
            {
                return (null, $"Le fichier ne doit pas dépasser {maximumSize / (1024 * 1024)} Mo.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
        }

        if (buffer.Length == 0)
        {
            return (null, "Le fichier ne peut pas être vide.");
        }

        var data = buffer.ToArray();
        if (!MatchesContentType(data, contentType))
        {
            return (null, "Le contenu du fichier ne correspond pas au format annoncé.");
        }

        var asset = new MediaAsset
        {
            FileName = Path.GetFileName(fileName),
            ContentType = contentType,
            Data = data
        };
        dbContext.MediaAssets.Add(asset);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (asset, null);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var asset = await dbContext.MediaAssets.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (asset is null)
        {
            return false;
        }

        var publicUrl = $"/api/media/{id}";
        var sections = await dbContext.SitePageSections
            .Where(section => section.Content.Contains(id.ToString()))
            .ToListAsync(cancellationToken);
        var affectedPageIds = new HashSet<Guid>();
        foreach (var section in sections)
        {
            var content = JsonNode.Parse(section.Content);
            if (RemoveMediaReferences(content, publicUrl))
            {
                section.Content = content!.ToJsonString();
                affectedPageIds.Add(section.SitePageId);
            }
        }

        if (affectedPageIds.Count > 0)
        {
            var pages = await dbContext.SitePages
                .Where(page => affectedPageIds.Contains(page.Id))
                .ToListAsync(cancellationToken);
            foreach (var page in pages)
            {
                page.UpdatedAt = DateTime.UtcNow;
            }
        }

        var settings = await dbContext.SiteSettings.SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (settings is not null &&
            (settings.LogoUrl == publicUrl || settings.DemoVideoUrl == publicUrl || settings.DemoVideoPosterUrl == publicUrl))
        {
            if (settings.LogoUrl == publicUrl) settings.LogoUrl = string.Empty;
            if (settings.DemoVideoUrl == publicUrl) settings.DemoVideoUrl = string.Empty;
            if (settings.DemoVideoPosterUrl == publicUrl) settings.DemoVideoPosterUrl = string.Empty;
            settings.UpdatedAt = DateTime.UtcNow;
        }

        dbContext.MediaAssets.Remove(asset);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool RemoveMediaReferences(JsonNode? node, string publicUrl)
    {
        var removedReference = false;
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (property.Value is JsonValue value && value.TryGetValue<string>(out var text) &&
                    property.Key.EndsWith("Url", StringComparison.OrdinalIgnoreCase) && text == publicUrl)
                {
                    jsonObject.Remove(property.Key);
                    removedReference = true;
                }
                else
                {
                    removedReference |= RemoveMediaReferences(property.Value, publicUrl);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                removedReference |= RemoveMediaReferences(item, publicUrl);
            }
        }

        return removedReference;
    }

    private static bool MatchesContentType(byte[] data, string contentType) => contentType switch
    {
        "image/jpeg" => data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
        "image/png" => data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
                        data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A,
        "image/webp" => data.Length >= 12 && data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F' &&
                         data[8] == 'W' && data[9] == 'E' && data[10] == 'B' && data[11] == 'P',
        // MP4 : boîte « ftyp » à l'octet 4 ; WebM : en-tête EBML.
        "video/mp4" => data.Length >= 12 && data[4] == 'f' && data[5] == 't' && data[6] == 'y' && data[7] == 'p',
        "video/webm" => data.Length >= 4 && data[0] == 0x1A && data[1] == 0x45 && data[2] == 0xDF && data[3] == 0xA3,
        _ => false
    };
}
