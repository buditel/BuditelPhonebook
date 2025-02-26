using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuditelPhonebook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditedChangesDescriptionsPropertyTypeOfChangeLogToJSON : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeDescription",
                table: "ChangeLogs");

            migrationBuilder.AddColumn<string>(
                name: "ChangesDescriptions",
                table: "ChangeLogs",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangesDescriptions",
                table: "ChangeLogs");

            migrationBuilder.AddColumn<string>(
                name: "ChangeDescription",
                table: "ChangeLogs",
                type: "text",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
