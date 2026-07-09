using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscAtlas.Models.Migrations
{
    /// <inheritdoc />
    public partial class MelhoriasBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotografiaUrl",
                table: "Eventos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CapturaFotografia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapturaId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataAdicao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapturaFotografia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapturaFotografia_Capturas_CapturaId",
                        column: x => x.CapturaId,
                        principalTable: "Capturas",
                        principalColumn: "CapturaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapturaFotografia_CapturaId",
                table: "CapturaFotografia",
                column: "CapturaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapturaFotografia");

            migrationBuilder.DropColumn(
                name: "FotografiaUrl",
                table: "Eventos");
        }
    }
}
