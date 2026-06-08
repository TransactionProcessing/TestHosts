using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestHosts.Migrations.PataPawa
{
    public partial class AddPostPaidBillSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApiKeyId",
                table: "postpaidaccounts",
                newName: "AccountId");

            migrationBuilder.CreateTable(
                name: "postpaidbill",
                columns: table => new
                {
                    PostPaidBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postpaidbill", x => x.PostPaidBillId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "postpaidbill");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "postpaidaccounts",
                newName: "ApiKeyId");
        }
    }
}
