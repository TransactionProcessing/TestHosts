using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestHosts.Migrations.PataPawa
{
    /// <inheritdoc />
    public partial class pendingprepayflag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPending",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPending",
                table: "Transactions");
        }
    }
}
