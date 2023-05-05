using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeleteDt",
                table: "Sections",
                newName: "DeletedDt");

            migrationBuilder.RenameColumn(
                name: "CreateDt",
                table: "Sections",
                newName: "CreatedDt");

            migrationBuilder.RenameColumn(
                name: "AutorId",
                table: "Sections",
                newName: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_AuthorId",
                table: "Sections",
                column: "AuthorId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_AuthorId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_AuthorId",
                table: "Sections");

            migrationBuilder.RenameColumn(
                name: "DeletedDt",
                table: "Sections",
                newName: "DeleteDt");

            migrationBuilder.RenameColumn(
                name: "CreatedDt",
                table: "Sections",
                newName: "CreateDt");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "Sections",
                newName: "AutorId");
        }
    }
}
