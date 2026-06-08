using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopLaptop_v1.Models
{
    public class DanhGia
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Range(1, 5)]
        public int SoSao { get; set; }

        [StringLength(500)]
        public string? NhanXet { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        public bool DaMuaHang { get; set; }

        public bool DangHienThi { get; set; } = true;

        [StringLength(300)]
        public string? GhiChuKiemDuyet { get; set; }
    }
}
