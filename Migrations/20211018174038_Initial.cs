using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApp.VendingMachine.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendingMachineViewModel",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendingMachineViewModel", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "Coins",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Denomination = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CountInVM = table.Column<int>(type: "int", nullable: false),
                    vendingMachineItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coins", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Coins_VendingMachineViewModel_vendingMachineItemId",
                        column: x => x.vendingMachineItemId,
                        principalTable: "VendingMachineViewModel",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drinks",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CountInVM = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vendingMachineItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drinks", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Drinks_VendingMachineViewModel_vendingMachineItemId",
                        column: x => x.vendingMachineItemId,
                        principalTable: "VendingMachineViewModel",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coins_vendingMachineItemId",
                table: "Coins",
                column: "vendingMachineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Drinks_vendingMachineItemId",
                table: "Drinks",
                column: "vendingMachineItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coins");

            migrationBuilder.DropTable(
                name: "Drinks");

            migrationBuilder.DropTable(
                name: "VendingMachineViewModel");
        }
    }
}
