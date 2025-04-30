using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Festpay.Onboarding.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Removing_AlternateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Accounts_AlternateId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_AlternateId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AlternateId",
                table: "Accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlternateId",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Accounts_AlternateId",
                table: "Accounts",
                column: "AlternateId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AlternateId",
                table: "Accounts",
                column: "AlternateId",
                unique: true);
        }
    }
}
