using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.Services;

namespace ShopLaptop_v1.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public GioHangController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            var (items, warnings) = await _cartService.SynchronizeAsync(gioHang);

            HttpContext.Session.Set(GIO_HANG_KEY, items);
            ViewData["CanhBaoGioHang"] = warnings;
            ViewData["SoLuongGioHang"] = items.Sum(m => m.SoLuong);

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> ThemVaoGioHang(int maBienThe, int soLuong = 1)
        {
            if (soLuong <= 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ." });
            }

            var bienTheSanPham = await _context.ProductVariants
                .Include(pv => pv.Product)
                .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(pv => pv.Id == maBienThe);

            if (bienTheSanPham == null)
            {
                return Json(new { success = false, message = "Không tìm thấy cấu hình sản phẩm này." });
            }

            if (bienTheSanPham.StockQuantity <= 0)
            {
                return Json(new { success = false, message = "Sản phẩm này hiện đã hết hàng." });
            }

            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            var (items, _) = await _cartService.SynchronizeAsync(gioHang);
            gioHang = items;

            var mucTonTai = gioHang.FirstOrDefault(m => m.MaBienThe == maBienThe);
            var soLuongMoi = (mucTonTai?.SoLuong ?? 0) + soLuong;

            if (soLuongMoi > bienTheSanPham.StockQuantity)
            {
                return Json(new
                {
                    success = false,
                    message = $"Chỉ còn {bienTheSanPham.StockQuantity} sản phẩm trong kho.",
                    tongSoLuong = gioHang.Sum(m => m.SoLuong)
                });
            }

            if (mucTonTai != null)
            {
                mucTonTai.SoLuong = soLuongMoi;
                mucTonTai.TonKho = bienTheSanPham.StockQuantity;
                mucTonTai.DonGia = bienTheSanPham.DiscountPrice ?? bienTheSanPham.Price;
            }
            else
            {
                gioHang.Add(_cartService.BuildCartItem(bienTheSanPham, soLuong));
            }

            HttpContext.Session.Set(GIO_HANG_KEY, gioHang);

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng",
                tongSoLuong = gioHang.Sum(m => m.SoLuong)
            });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatSoLuong(int maBienThe, int soLuong)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            var (items, _) = await _cartService.SynchronizeAsync(gioHang);
            gioHang = items;

            var muc = gioHang.FirstOrDefault(m => m.MaBienThe == maBienThe);
            if (muc == null)
            {
                HttpContext.Session.Set(GIO_HANG_KEY, gioHang);
                return Json(new { success = false, message = "Sản phẩm không còn trong giỏ hàng.", tongSoLuong = gioHang.Sum(m => m.SoLuong) });
            }

            var bienThe = await _context.ProductVariants.FindAsync(maBienThe);
            if (bienThe == null || bienThe.StockQuantity <= 0)
            {
                gioHang.Remove(muc);
                HttpContext.Session.Set(GIO_HANG_KEY, gioHang);
                return Json(new { success = false, message = "Sản phẩm đã hết hàng hoặc không còn tồn tại.", tongSoLuong = gioHang.Sum(m => m.SoLuong) });
            }

            var soLuongHopLe = Math.Max(1, soLuong);
            if (soLuongHopLe > bienThe.StockQuantity)
            {
                soLuongHopLe = bienThe.StockQuantity;
            }

            muc.SoLuong = soLuongHopLe;
            muc.TonKho = bienThe.StockQuantity;
            HttpContext.Session.Set(GIO_HANG_KEY, gioHang);

            return Json(new
            {
                success = soLuong <= bienThe.StockQuantity,
                message = soLuong > bienThe.StockQuantity ? $"Chỉ còn {bienThe.StockQuantity} sản phẩm trong kho." : "Đã cập nhật giỏ hàng.",
                soLuong = muc.SoLuong,
                thanhTien = muc.ThanhTien,
                tongCong = gioHang.Sum(m => m.ThanhTien),
                tongSoLuong = gioHang.Sum(m => m.SoLuong)
            });
        }

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

            return Json(new
            {
                success = true,
                message = "Đã xóa khỏi giỏ hàng",
                tongSoLuong = gioHang.Sum(m => m.SoLuong)
            });
        }
    }
}
