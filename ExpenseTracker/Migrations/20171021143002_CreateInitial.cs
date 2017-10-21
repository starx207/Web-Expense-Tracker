using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Migrations
{
    public partial class CreateInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetCategories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    BeginEffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndEffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCategories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Payees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeginEffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BudgetCategoryID = table.Column<int>(type: "INTEGER", nullable: true),
                    EndEffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payees", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Payees_BudgetCategories_BudgetCategoryID",
                        column: x => x.BudgetCategoryID,
                        principalTable: "BudgetCategories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OverrideCategoryID = table.Column<int>(type: "INTEGER", nullable: true),
                    PayeeID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Transactions_BudgetCategories_OverrideCategoryID",
                        column: x => x.OverrideCategoryID,
                        principalTable: "BudgetCategories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Payees_PayeeID",
                        column: x => x.PayeeID,
                        principalTable: "Payees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payees_BudgetCategoryID",
                table: "Payees",
                column: "BudgetCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OverrideCategoryID",
                table: "Transactions",
                column: "OverrideCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PayeeID",
                table: "Transactions",
                column: "PayeeID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Payees");

            migrationBuilder.DropTable(
                name: "BudgetCategories");
        }
    }
}
