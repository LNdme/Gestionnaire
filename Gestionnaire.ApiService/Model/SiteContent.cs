namespace Gestionnaire.ApiService.Model;

public sealed class SitePage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Intro { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<SitePageSection> Sections { get; set; } = [];
}

public sealed class SitePageSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SitePageId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Content { get; set; } = "{}";
    public int SortOrder { get; set; }
    public SitePage SitePage { get; set; } = null!;
}
