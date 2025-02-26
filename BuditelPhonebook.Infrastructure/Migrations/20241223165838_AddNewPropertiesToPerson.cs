using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuditelPhonebook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPropertiesToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "People",
                newName: "SubjectGroup");

            migrationBuilder.AddColumn<string>(
                name: "Birthdate",
                table: "People",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPhoneNumber",
                table: "People",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalPhoneNumber",
                table: "People",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "People",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Birthdate",
                table: "People");

            migrationBuilder.DropColumn(
                name: "BusinessPhoneNumber",
                table: "People");

            migrationBuilder.DropColumn(
                name: "PersonalPhoneNumber",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "People");

            migrationBuilder.RenameColumn(
                name: "SubjectGroup",
                table: "People",
                newName: "PhoneNumber");
        }
    }
}
