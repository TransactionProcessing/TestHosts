using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TestHosts.Migrations
{
    public partial class InitialTestBankDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deposit",
                columns: table => new
                {
                    HostIdentifier = table.Column<Guid>(nullable: false),
                    DepositId = table.Column<Guid>(nullable: false),
                    SortCode = table.Column<string>(nullable: true),
                    AccountNumber = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Reference = table.Column<string>(nullable: true),
                    SentToHost = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit", x => new { x.HostIdentifier, x.DepositId });
                });

            migrationBuilder.CreateTable(
                name: "hostconfiguration",
                columns: table => new
                {
                    HostIdentifier = table.Column<Guid>(nullable: false),
                    CallbackUri = table.Column<string>(nullable: true),
                    SortCode = table.Column<string>(nullable: true),
                    AccountNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostconfiguration", x => x.HostIdentifier);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposit");

            migrationBuilder.DropTable(
                name: "hostconfiguration");
        }
    }
}
