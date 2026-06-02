using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestHosts.Migrations.AgencyBankingDb
{
    /// <inheritdoc />
    public partial class somdbchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DuplicateOfId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDuplicate",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuplicateOfId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsDuplicate",
                table: "Transactions");
        }
    }
}
