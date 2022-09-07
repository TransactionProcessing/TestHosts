using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestHosts.Migrations.PataPawa
{
    public partial class RenameAccountTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prepaidaccounts");

            migrationBuilder.CreateTable(
                name: "postpaidaccounts",
                columns: table => new
                {
                    ApiKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postpaidaccounts", x => x.ApiKeyId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "postpaidaccounts");

            migrationBuilder.CreateTable(
                name: "prepaidaccounts",
                columns: table => new
                {
                    ApiKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prepaidaccounts", x => x.ApiKeyId);
                });
        }
    }
}
