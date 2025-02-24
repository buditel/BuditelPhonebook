using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuditelPhonebook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncreasedMaxLengthOfPersonBirthdateTo6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Birthdate",
                table: "People",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Birthdate",
                table: "People",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);
        }
    }
}
