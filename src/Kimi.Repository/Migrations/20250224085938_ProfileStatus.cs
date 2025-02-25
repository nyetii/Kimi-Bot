using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kimi.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ProfileStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Default = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusMessage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    StatusUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    StatusType = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusActivityType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "Default", "StatusActivityType", "StatusMessage", "StatusType", "StatusUrl" },
                values: new object[] { -1, true, 4, "", 1, null });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Default",
                table: "Profiles",
                column: "Default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
