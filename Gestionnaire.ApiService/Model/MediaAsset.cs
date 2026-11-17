namespace Gestionnaire.ApiService.Model;

public sealed class MediaAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
