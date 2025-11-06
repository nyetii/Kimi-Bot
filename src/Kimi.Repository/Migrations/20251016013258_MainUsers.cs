using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kimi.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MainUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "MainUserId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MainUserId",
                table: "Users",
                column: "MainUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_MainUserId",
                table: "Users",
                column: "MainUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_MainUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_MainUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MainUserId",
                table: "Users");
        }
    }
}
