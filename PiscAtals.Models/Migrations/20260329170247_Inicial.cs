using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscAtlas.Models.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Especies",
                columns: table => new
                {
                    EspecieId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomeCientifico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PesoRecordPt = table.Column<double>(type: "float", nullable: true),
                    TamanhoRecordPt = table.Column<double>(type: "float", nullable: true),
                    Habitat = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especies", x => x.EspecieId);
                });

            migrationBuilder.CreateTable(
                name: "Pesqueiros",
                columns: table => new
                {
                    PesqueiroId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    FotografiaUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pesqueiros", x => x.PesqueiroId);
                });

            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    EventoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EspecieAlvoId = table.Column<int>(type: "int", nullable: false),
                    PesoMinimo = table.Column<double>(type: "float", nullable: true),
                    TamanhoMinimo = table.Column<double>(type: "float", nullable: true),
                    PrecoInscricao = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.EventoId);
                    table.ForeignKey(
                        name: "FK_Eventos_Especies_EspecieAlvoId",
                        column: x => x.EspecieAlvoId,
                        principalTable: "Especies",
                        principalColumn: "EspecieId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Capturas",
                columns: table => new
                {
                    CapturaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EspecieId = table.Column<int>(type: "int", nullable: false),
                    PesqueiroId = table.Column<int>(type: "int", nullable: false),
                    FotografiaUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Peso = table.Column<double>(type: "float", nullable: true),
                    Tamanho = table.Column<double>(type: "float", nullable: true),
                    PossuiProvasVisuais = table.Column<bool>(type: "bit", nullable: false),
                    AprovadaPeloAdmin = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PescadorNome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capturas", x => x.CapturaId);
                    table.ForeignKey(
                        name: "FK_Capturas_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "EspecieId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Capturas_Pesqueiros_PesqueiroId",
                        column: x => x.PesqueiroId,
                        principalTable: "Pesqueiros",
                        principalColumn: "PesqueiroId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscricoes",
                columns: table => new
                {
                    InscricaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventoId = table.Column<int>(type: "int", nullable: false),
                    PescadorEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PescadorNome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoPagamento = table.Column<int>(type: "int", nullable: false),
                    ValorPago = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MelhorCapturaId = table.Column<int>(type: "int", nullable: true),
                    Pontuacao = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscricoes", x => x.InscricaoId);
                    table.ForeignKey(
                        name: "FK_Inscricoes_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "EventoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Denuncias",
                columns: table => new
                {
                    DenunciaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapturaId = table.Column<int>(type: "int", nullable: false),
                    DenuncianteEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    DecisaoAdmin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataDecisao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncias", x => x.DenunciaId);
                    table.ForeignKey(
                        name: "FK_Denuncias_Capturas_CapturaId",
                        column: x => x.CapturaId,
                        principalTable: "Capturas",
                        principalColumn: "CapturaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Capturas_EspecieId",
                table: "Capturas",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_Capturas_PesqueiroId",
                table: "Capturas",
                column: "PesqueiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_CapturaId",
                table: "Denuncias",
                column: "CapturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_EspecieAlvoId",
                table: "Eventos",
                column: "EspecieAlvoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_EventoId",
                table: "Inscricoes",
                column: "EventoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Denuncias");

            migrationBuilder.DropTable(
                name: "Inscricoes");

            migrationBuilder.DropTable(
                name: "Capturas");

            migrationBuilder.DropTable(
                name: "Eventos");

            migrationBuilder.DropTable(
                name: "Pesqueiros");

            migrationBuilder.DropTable(
                name: "Especies");
        }
    }
}
