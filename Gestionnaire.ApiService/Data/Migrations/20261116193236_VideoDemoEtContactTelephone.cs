using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestionnaire.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class VideoDemoEtContactTelephone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactHours",
                table: "site_settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "site_settings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactWhatsApp",
                table: "site_settings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DemoVideoLabel",
                table: "site_settings",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DemoVideoPosterUrl",
                table: "site_settings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DemoVideoTitle",
                table: "site_settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DemoVideoUrl",
                table: "site_settings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "demo_requests",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            // Remplit la ligne de paramètres existante avec la vidéo de démo et le contact téléphonique
            // (mêmes valeurs que SiteSeedContent.CreateSettings) pour ne pas exiger de réinitialisation.
            migrationBuilder.Sql("""
                UPDATE site_settings SET
                    "DemoVideoUrl" = '/api/media/46a0d31a-af12-40d6-b9ba-e05ed4087511',
                    "DemoVideoPosterUrl" = '/api/media/d1a4f3dc-2192-4c67-9423-0d53dc09b700',
                    "DemoVideoLabel" = 'Regarder la démo',
                    "DemoVideoTitle" = 'Visite de Gestionnaire en 1 min 40',
                    "ContactPhone" = '+237696649233',
                    "ContactWhatsApp" = '+237696649233',
                    "ContactHours" = ''
                WHERE "Id" = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactHours",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "ContactWhatsApp",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "DemoVideoLabel",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "DemoVideoPosterUrl",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "DemoVideoTitle",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "DemoVideoUrl",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "demo_requests");
        }
    }
}
