using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscAtlas.Models.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDescricaoCaptura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Capturas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Capturas");
        }
    }
}
