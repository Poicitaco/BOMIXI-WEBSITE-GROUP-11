using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopLaptop_v1.Migrations
{
    /// <inheritdoc />
    public partial class CapNhatDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TieuDe = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DuongDanAnh = table.Column<string>(type: "TEXT", nullable: false),
                    DuongDanLink = table.Column<string>(type: "TEXT", nullable: false),
                    NhanTag = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    ThuTu = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YeuThichs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuThichs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuThichs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YeuThichs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_ProductId",
                table: "YeuThichs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_UserId",
                table: "YeuThichs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "YeuThichs");
        }
    }
}
