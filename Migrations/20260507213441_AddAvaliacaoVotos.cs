using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace gasosa_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAvaliacaoVotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avaliacao_votos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvaliacaoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    IsLike = table.Column<bool>(type: "boolean", nullable: false),
                    DataVoto = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacao_votos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_avaliacao_votos_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_avaliacao_votos_avaliacoes_AvaliacaoId",
                        column: x => x.AvaliacaoId,
                        principalTable: "avaliacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacao_votos_AvaliacaoId",
                table: "avaliacao_votos",
                column: "AvaliacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacao_votos_UsuarioId_AvaliacaoId",
                table: "avaliacao_votos",
                columns: new[] { "UsuarioId", "AvaliacaoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avaliacao_votos");
        }
    }
}
