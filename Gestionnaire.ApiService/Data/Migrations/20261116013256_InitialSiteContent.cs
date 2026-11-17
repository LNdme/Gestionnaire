using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestionnaire.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSiteContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "demo_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Establishment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    StudentCount = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demo_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "site_navigation_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_navigation_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "site_pages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Intro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MetaDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "site_settings",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    SiteName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BrandMarkText = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LoginLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LoginUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DemoActionLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DemoActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FooterDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FooterTagline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CopyrightNotice = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_settings", x => x.Id);
                    table.CheckConstraint("CK_site_settings_singleton", "\"Id\" = 1");
                });

            migrationBuilder.CreateTable(
                name: "site_page_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SitePageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Heading = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Content = table.Column<string>(type: "jsonb", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_page_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_site_page_sections_site_pages_SitePageId",
                        column: x => x.SitePageId,
                        principalTable: "site_pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_demo_requests_CreatedAt",
                table: "demo_requests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_site_page_sections_SitePageId",
                table: "site_page_sections",
                column: "SitePageId");

            migrationBuilder.CreateIndex(
                name: "IX_site_pages_Slug",
                table: "site_pages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "demo_requests");

            migrationBuilder.DropTable(
                name: "media_assets");

            migrationBuilder.DropTable(
                name: "site_navigation_items");

            migrationBuilder.DropTable(
                name: "site_page_sections");

            migrationBuilder.DropTable(
                name: "site_settings");

            migrationBuilder.DropTable(
                name: "site_pages");
        }
    }
}
