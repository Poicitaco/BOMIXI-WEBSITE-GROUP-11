namespace ShopLaptop_v1.Models
{
    public class MucGioHang
    {
        public int MaBienThe { get; set; } // ProductVariantId
        public string TenSanPham { get; set; } = string.Empty;
        public string CauHinh { get; set; } = string.Empty; // CPU, RAM, v.v.
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public string HinhAnh { get; set; } = string.Empty;
        
        public decimal ThanhTien => DonGia * SoLuong;
    }
}
