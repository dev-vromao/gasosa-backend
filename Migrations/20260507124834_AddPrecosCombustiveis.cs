using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace gasosa_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecosCombustiveis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "precos_combustiveis",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    posto_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_combustivel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    preco = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precos_combustiveis", x => x.id);
                    table.ForeignKey(
                        name: "FK_precos_combustiveis_postos_posto_id",
                        column: x => x.posto_id,
                        principalTable: "postos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_precos_combustiveis_posto_id",
                table: "precos_combustiveis",
                column: "posto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "precos_combustiveis");
        }
    }
}
