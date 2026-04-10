using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gasosa_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAvaliacoes5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_postos_cnpj",
                table: "postos");

            migrationBuilder.DropColumn(
                name: "cnpj",
                table: "postos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cnpj",
                table: "postos",
                type: "character varying(18)",
                maxLength: 18,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_postos_cnpj",
                table: "postos",
                column: "cnpj",
                unique: true);
        }
    }
}
