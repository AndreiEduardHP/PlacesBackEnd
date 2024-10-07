using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Places.Migrations
{
    /// <inheritdoc />
    public partial class ReadMessagesChangesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ChatUsers",
                newName: "SenderId");

            migrationBuilder.AddColumn<int>(
                name: "ReceiverId",
                table: "ChatUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "ChatUsers");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "ChatUsers",
                newName: "UserId");
        }
    }
}
