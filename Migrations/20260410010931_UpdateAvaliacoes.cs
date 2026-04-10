using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace gasosa_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    PostoId = table.Column<int>(type: "integer", nullable: false),
                    NotaGeral = table.Column<int>(type: "integer", nullable: false),
                    NotaPrecos = table.Column<int>(type: "integer", nullable: true),
                    NotaAtendimento = table.Column<int>(type: "integer", nullable: true),
                    TemLojaConveniencia = table.Column<bool>(type: "boolean", nullable: false),
                    TemCalibrador = table.Column<bool>(type: "boolean", nullable: false),
                    TemLavaRapido = table.Column<bool>(type: "boolean", nullable: false),
                    TemTrocaOleo = table.Column<bool>(type: "boolean", nullable: false),
                    TemAreaDescanso = table.Column<bool>(type: "boolean", nullable: false),
                    TemCarregadorEletrico = table.Column<bool>(type: "boolean", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataAvaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_avaliacoes_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_avaliacoes_postos_PostoId",
                        column: x => x.PostoId,
                        principalTable: "postos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_PostoId",
                table: "avaliacoes",
                column: "PostoId");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_UsuarioId",
                table: "avaliacoes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avaliacoes");
        }
    }
}
