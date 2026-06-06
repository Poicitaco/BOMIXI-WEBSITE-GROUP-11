using System.ComponentModel.DataAnnotations;

namespace ShopLaptop_v1.Models
{
    public class Banner
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TieuDe { get; set; } = string.Empty;

        [StringLength(200)]
        public string MoTa { get; set; } = string.Empty;

        [Required]
        public string DuongDanAnh { get; set; } = string.Empty;

        public string DuongDanLink { get; set; } = "#";

        [StringLength(50)]
        public string NhanTag { get; set; } = string.Empty;

        public bool DangHoatDong { get; set; } = true;

        public int ThuTu { get; set; } = 0;

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}
