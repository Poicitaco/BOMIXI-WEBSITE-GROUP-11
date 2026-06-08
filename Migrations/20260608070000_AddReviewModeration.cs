using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopLaptop_v1.Migrations
{
    public partial class AddReviewModeration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaMuaHang",
                table: "DanhGias",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DangHienThi",
                table: "DanhGias",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChuKiemDuyet",
                table: "DanhGias",
                type: "TEXT",
                maxLength: 300,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DaMuaHang", table: "DanhGias");
            migrationBuilder.DropColumn(name: "DangHienThi", table: "DanhGias");
            migrationBuilder.DropColumn(name: "GhiChuKiemDuyet", table: "DanhGias");
        }
    }
}
