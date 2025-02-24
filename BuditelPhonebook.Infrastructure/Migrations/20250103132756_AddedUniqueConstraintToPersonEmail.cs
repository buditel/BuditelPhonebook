using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuditelPhonebook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUniqueConstraintToPersonEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_People_Email",
                table: "People",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_People_Email",
                table: "People");
        }
    }
}
