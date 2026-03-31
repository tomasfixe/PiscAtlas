using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscAtlas.Models.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarCapturasFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PescadorNome",
                table: "Capturas");

            migrationBuilder.AddColumn<string>(
                name: "UtilizadorId",
                table: "Inscricoes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCaptura",
                table: "Capturas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UtilizadorId",
                table: "Capturas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Utilizador",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NomeUtilizador = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrimeiroNome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UltimoNome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FotografiaPerfilUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizador", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_UtilizadorId",
                table: "Inscricoes",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Capturas_UtilizadorId",
                table: "Capturas",
                column: "UtilizadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Capturas_Utilizador_UtilizadorId",
                table: "Capturas",
                column: "UtilizadorId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inscricoes_Utilizador_UtilizadorId",
                table: "Inscricoes",
                column: "UtilizadorId",
                principalTable: "Utilizador",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Capturas_Utilizador_UtilizadorId",
                table: "Capturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Inscricoes_Utilizador_UtilizadorId",
                table: "Inscricoes");

            migrationBuilder.DropTable(
                name: "Utilizador");

            migrationBuilder.DropIndex(
                name: "IX_Inscricoes_UtilizadorId",
                table: "Inscricoes");

            migrationBuilder.DropIndex(
                name: "IX_Capturas_UtilizadorId",
                table: "Capturas");

            migrationBuilder.DropColumn(
                name: "UtilizadorId",
                table: "Inscricoes");

            migrationBuilder.DropColumn(
                name: "DataCaptura",
                table: "Capturas");

            migrationBuilder.DropColumn(
                name: "UtilizadorId",
                table: "Capturas");

            migrationBuilder.AddColumn<string>(
                name: "PescadorNome",
                table: "Capturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
