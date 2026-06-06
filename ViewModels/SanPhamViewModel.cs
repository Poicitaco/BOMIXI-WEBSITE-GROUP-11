using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.ViewModels
{
    public class SanPhamViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string TenDanhMuc { get; set; } = string.Empty;
        public string SlugDanhMuc { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public decimal GiaThap { get; set; }
        public decimal GiaNiemYet { get; set; }
        public string CauHinhChính { get; set; } = string.Empty;
        public int TonKho { get; set; }
        public int MaBienTheChinhId { get; set; }
        public bool ConHang => TonKho > 0;
        public bool DangKhuyenMai => GiaThap < GiaNiemYet;
        public int PhanTramGiam => DangKhuyenMai
            ? (int)Math.Round((1 - GiaThap / GiaNiemYet) * 100)
            : 0;
    }

    public class TrangChuViewModel
    {
        public X.PagedList.IPagedList<SanPhamViewModel> DanhSachSanPham { get; set; } = null!;
        public List<Category> DanhSachDanhMuc { get; set; } = new();
        public string? DanhMucDangChon { get; set; }
        public string? TuKhoa { get; set; }
        public string? SapXep { get; set; }
        public string? RAM { get; set; }
        public string? DoPhanGiai { get; set; }
        public string? KhoangGia { get; set; }
        public string? CPU { get; set; }
        public string? VGA { get; set; }
        public List<Banner> DanhSachBanner { get; set; } = new();
        public List<SanPhamViewModel> DanhSachFlashSale { get; set; } = new();
    }
}
