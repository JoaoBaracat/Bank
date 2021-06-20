using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bank.Infra.Data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AccountOrigin = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: false),
                    AccountDestination = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
