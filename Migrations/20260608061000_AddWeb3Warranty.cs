using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopLaptop_v1.Migrations
{
    public partial class AddWeb3Warranty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletAddresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CertificateCode = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    TokenId = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    BlockchainTxHash = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderDetailId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    WalletAddress = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    VariantSnapshot = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyCertificates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarrantyCertificates_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarrantyCertificates_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletAddresses_UserId_Address",
                table: "WalletAddresses",
                columns: new[] { "UserId", "Address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCertificates_CertificateCode",
                table: "WarrantyCertificates",
                column: "CertificateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCertificates_OrderDetailId",
                table: "WarrantyCertificates",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCertificates_OrderId",
                table: "WarrantyCertificates",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCertificates_TokenId",
                table: "WarrantyCertificates",
                column: "TokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCertificates_UserId",
                table: "WarrantyCertificates",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "WarrantyCertificates");
            migrationBuilder.DropTable(name: "WalletAddresses");
        }
    }
}
