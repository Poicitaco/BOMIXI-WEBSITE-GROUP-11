using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class QuanLyBannerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public QuanLyBannerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void ThietLapViewData()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
        }

        public async Task<IActionResult> Index()
        {
            ThietLapViewData();
            var banners = await _context.Banners.OrderBy(b => b.ThuTu).ToListAsync();
            return View(banners);
        }

        public IActionResult Tao()
        {
            ThietLapViewData();
            return View(new Banner());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tao(Banner banner)
        {
            if (ModelState.IsValid)
            {
                banner.NgayTao = DateTime.UtcNow;
                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = "Đã thêm banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            ThietLapViewData();
            return View(banner);
        }

        public async Task<IActionResult> Sua(int id)
        {
            ThietLapViewData();
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(Banner banner)
        {
            if (ModelState.IsValid)
            {
                _context.Banners.Update(banner);
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = "Đã cập nhật banner!";
                return RedirectToAction(nameof(Index));
            }
            ThietLapViewData();
            return View(banner);
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                banner.DangHoatDong = !banner.DangHoatDong;
                await _context.SaveChangesAsync();
                return Json(new { success = true, dangHoatDong = banner.DangHoatDong });
            }
            return Json(new { success = false });
        }
    }
}
