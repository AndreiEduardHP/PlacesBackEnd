using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Places.Migrations
{
    /// <inheritdoc />
    public partial class CheckFunctionalityAndInterestEventMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckFunctionality",
                table: "Events",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Interest",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckFunctionality",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Interest",
                table: "Events");
        }
    }
}
