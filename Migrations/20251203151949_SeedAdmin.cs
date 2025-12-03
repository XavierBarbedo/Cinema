using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Email", "Nome", "Password", "Role" },
                values: new object[] { 1, "cisco@gmail.com", "Admin", "cisco123", "Administrador" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
