using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscAtlas.Models.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDefinicoesUtilizador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlertasSignalR",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CadernetaPrivada",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ContaPrivada",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ListaSeguidoresPrivada",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TemaVisual",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertasSignalR",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CadernetaPrivada",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ContaPrivada",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ListaSeguidoresPrivada",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TemaVisual",
                table: "AspNetUsers");
        }
    }
}
