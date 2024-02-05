using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestHosts.Migrations.PataPawa
{
    /// <inheritdoc />
    public partial class storeprepaytransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Messaage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeterNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StandardTokenTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StandardTokenAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StandardTokenRctNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCharges",
                columns: table => new
                {
                    TransactionChargeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    REPCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyFC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ERCCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FuelIndexCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForexCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InflationAdjustment = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCharges", x => x.TransactionChargeId);
                    table.ForeignKey(
                        name: "FK_TransactionCharges_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCharges_TransactionId",
                table: "TransactionCharges",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionCharges");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
