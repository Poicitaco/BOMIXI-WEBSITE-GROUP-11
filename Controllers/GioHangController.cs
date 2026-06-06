using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem giỏ hàng
        public IActionResult Index()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
            return View(gioHang);
        }

        // Thêm sản phẩm vào giỏ
        [HttpPost]
        public async Task<IActionResult> ThemVaoGioHang(int maBienThe, int soLuong = 1)
        {
            var bienTheSanPham = await _context.ProductVariants
                .Include(pv => pv.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(pv => pv.Id == maBienThe);

            if (bienTheSanPham == null)
            {
                return NotFound("Không tìm thấy cấu hình sản phẩm này.");
            }

            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            var mucTonTai = gioHang.FirstOrDefault(m => m.MaBienThe == maBienThe);

            if (mucTonTai != null)
            {
                mucTonTai.SoLuong += soLuong;
            }
            else
            {
                var hinhAnh = bienTheSanPham.Product?.Images.FirstOrDefault(i => i.IsMainImage)?.ImageUrl ?? "";
                
                gioHang.Add(new MucGioHang
                {
                    MaBienThe = bienTheSanPham.Id,
                    TenSanPham = bienTheSanPham.Product?.Name ?? "Sản phẩm không xác định",
                    CauHinh = $"{bienTheSanPham.CPU} | {bienTheSanPham.RAM} | {bienTheSanPham.Storage}",
                    DonGia = bienTheSanPham.DiscountPrice ?? bienTheSanPham.Price,
                    SoLuong = soLuong,
                    HinhAnh = hinhAnh
                });
            }

            HttpContext.Session.Set(GIO_HANG_KEY, gioHang);
            
            // Tạm thời trả về JSON để dễ test chức năng
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng", tongSoLuong = gioHang.Sum(m => m.SoLuong) });
        }

        // Cập nhật số lượng sản phẩm trong giỏ
        [HttpPost]
        public IActionResult CapNhatSoLuong(int maBienThe, int soLuong)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            var muc = gioHang.FirstOrDefault(m => m.MaBienThe == maBienThe);
            if (muc != null)
            {
                muc.SoLuong = Math.Max(1, soLuong);
                HttpContext.Session.Set(GIO_HANG_KEY, gioHang);
            }
            return Json(new { success = true, tongSoLuong = gioHang.Sum(m => m.SoLuong) });
        }

        // Xóa khỏi giỏ hàng
        [HttpPost]
        public IActionResult XoaKhoiGioHang(int maBienThe)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            var mucTonTai = gioHang.FirstOrDefault(m => m.MaBienThe == maBienThe);

            if (mucTonTai != null)
            {
                gioHang.Remove(mucTonTai);
                HttpContext.Session.Set(GIO_HANG_KEY, gioHang);
            }

            return Json(new { success = true, message = "Đã xóa khỏi giỏ hàng" });
        }
    }
}
