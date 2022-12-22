using Microsoft.EntityFrameworkCore.Migrations;

namespace Web_Parent_Control.Migrations
{
    public partial class UpdateBlockedItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "BlockedItems",
                newName: "BlockDate");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "BlockedItems",
                newName: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "BlockedItems",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "BlockDate",
                table: "BlockedItems",
                newName: "Date");
        }
    }
}
