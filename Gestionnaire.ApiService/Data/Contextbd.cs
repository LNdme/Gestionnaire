using Gestionnaire.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace Gestionnaire.ApiService.Data;

public sealed class Contextbd(DbContextOptions<Contextbd> options) : DbContext(options)
{
    public DbSet<SitePage> SitePages => Set<SitePage>();
    public DbSet<SitePageSection> SitePageSections => Set<SitePageSection>();
    public DbSet<SiteNavigationItem> SiteNavigationItems => Set<SiteNavigationItem>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<DemoRequest> DemoRequests => Set<DemoRequest>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SitePage>(entity =>
        {
            entity.ToTable("site_pages");
            entity.HasKey(page => page.Id);
            entity.HasIndex(page => page.Slug).IsUnique();
            entity.Property(page => page.Slug).HasMaxLength(100).IsRequired();
            entity.Property(page => page.Title).HasMaxLength(200).IsRequired();
            entity.Property(page => page.Intro).HasMaxLength(2000).IsRequired();
            entity.Property(page => page.MetaDescription).HasMaxLength(320).IsRequired();
            entity.HasMany(page => page.Sections).WithOne(section => section.SitePage)
                .HasForeignKey(section => section.SitePageId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SitePageSection>(entity =>
        {
            entity.ToTable("site_page_sections");
            entity.HasKey(section => section.Id);
            entity.Property(section => section.Type).HasMaxLength(50).IsRequired();
            entity.Property(section => section.Heading).HasMaxLength(200).IsRequired();
            entity.Property(section => section.Body).HasMaxLength(10_000).IsRequired();
            entity.Property(section => section.Content).HasColumnType("jsonb").IsRequired();
        });

        modelBuilder.Entity<SiteNavigationItem>(entity =>
        {
            entity.ToTable("site_navigation_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Label).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Url).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.ToTable("site_settings", table => table.HasCheckConstraint("CK_site_settings_singleton", "\"Id\" = 1"));
            entity.HasKey(settings => settings.Id);
            entity.Property(settings => settings.Id).ValueGeneratedNever();
            entity.Property(settings => settings.SiteName).HasMaxLength(120).IsRequired();
            entity.Property(settings => settings.BrandMarkText).HasMaxLength(4).IsRequired();
            entity.Property(settings => settings.LogoUrl).HasMaxLength(500).IsRequired();
            entity.Property(settings => settings.LoginLabel).HasMaxLength(120).IsRequired();
            entity.Property(settings => settings.LoginUrl).HasMaxLength(500).IsRequired();
            entity.Property(settings => settings.DemoActionLabel).HasMaxLength(120).IsRequired();
            entity.Property(settings => settings.DemoActionUrl).HasMaxLength(500).IsRequired();
            entity.Property(settings => settings.DemoVideoUrl).HasMaxLength(500).IsRequired();
            entity.Property(settings => settings.DemoVideoPosterUrl).HasMaxLength(500).IsRequired();
            entity.Property(settings => settings.DemoVideoLabel).HasMaxLength(120).IsRequired();
            entity.Property(settings => settings.DemoVideoTitle).HasMaxLength(200).IsRequired();
            entity.Property(settings => settings.ContactPhone).HasMaxLength(30).IsRequired();
            entity.Property(settings => settings.ContactWhatsApp).HasMaxLength(30).IsRequired();
            entity.Property(settings => settings.ContactHours).HasMaxLength(200).IsRequired();
            entity.Property(settings => settings.FooterDescription).HasMaxLength(2000).IsRequired();
            entity.Property(settings => settings.FooterTagline).HasMaxLength(500).IsRequired();
            entity.Property(settings => settings.CopyrightNotice).HasMaxLength(300).IsRequired();
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.ToTable("media_assets");
            entity.HasKey(asset => asset.Id);
            entity.Property(asset => asset.FileName).HasMaxLength(255).IsRequired();
            entity.Property(asset => asset.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(asset => asset.Data).IsRequired();
            entity.Property(asset => asset.UploadedAt).IsRequired();
        });

        modelBuilder.Entity<DemoRequest>(entity =>
        {
            entity.ToTable("demo_requests");
            entity.HasKey(request => request.Id);
            entity.HasIndex(request => request.CreatedAt);
            entity.Property(request => request.FullName).HasMaxLength(160).IsRequired();
            entity.Property(request => request.Establishment).HasMaxLength(200).IsRequired();
            entity.Property(request => request.Email).HasMaxLength(254).IsRequired();
            entity.Property(request => request.Phone).HasMaxLength(30).IsRequired();
            entity.Property(request => request.StudentCount).HasMaxLength(40).IsRequired();
            entity.Property(request => request.Message).HasMaxLength(5000).IsRequired();
            entity.Property(request => request.Status).HasMaxLength(20).IsRequired();
        });
    }
}
