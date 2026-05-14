using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gasosa_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBanimentoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Banido",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CreditosSociais",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banido",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreditosSociais",
                table: "AspNetUsers");
        }
    }
}
