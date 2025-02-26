using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuditelPhonebook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCommentOnDeletePropertyToPersonAndMadePersonalPhoneNumberRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PersonalPhoneNumber",
                table: "People",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentOnDeletion",
                table: "People",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentOnDeletion",
                table: "People");

            migrationBuilder.AlterColumn<string>(
                name: "PersonalPhoneNumber",
                table: "People",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20);
        }
    }
}
