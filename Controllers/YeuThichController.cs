using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Controllers
{
    [Authorize]
    public class YeuThichController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public YeuThichController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<string?> LayMaNguoiDung()
        {
            return await _context.Users
                .Where(u => u.UserName == User.Identity!.Name)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        // GET: /YeuThich
        public async Task<IActionResult> Index()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            var maNguoiDung = await LayMaNguoiDung();
            var danhSach = await _context.YeuThichs
                .Include(y => y.Product)
                    .ThenInclude(p => p!.Images)
                .Include(y => y.Product)
                    .ThenInclude(p => p!.Variants)
                .Where(y => y.UserId == maNguoiDung)
                .OrderByDescending(y => y.NgayThem)
                .ToListAsync();

            return View(danhSach);
        }

        // POST: /YeuThich/ThemHoacXoa/5
        [HttpPost]
        public async Task<IActionResult> ThemHoacXoa(int id)
        {
            var maNguoiDung = await LayMaNguoiDung();
            if (maNguoiDung == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var yeuThichCu = await _context.YeuThichs
                .FirstOrDefaultAsync(y => y.UserId == maNguoiDung && y.ProductId == id);

            bool dangYeuThich;
            if (yeuThichCu != null)
            {
                _context.YeuThichs.Remove(yeuThichCu);
                dangYeuThich = false;
            }
            else
            {
                _context.YeuThichs.Add(new YeuThich
                {
                    UserId = maNguoiDung,
                    ProductId = id
                });
                dangYeuThich = true;
            }

            await _context.SaveChangesAsync();

            int tongSoYeuThich = await _context.YeuThichs
                .CountAsync(y => y.UserId == maNguoiDung);

            return Json(new { success = true, dangYeuThich, tongSo = tongSoYeuThich });
        }

        // POST: /YeuThich/Xoa/5
        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            var maNguoiDung = await LayMaNguoiDung();
            var yeuThich = await _context.YeuThichs
                .FirstOrDefaultAsync(y => y.UserId == maNguoiDung && y.ProductId == id);

            if (yeuThich != null)
            {
                _context.YeuThichs.Remove(yeuThich);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /YeuThich/KiemTraTrangThai?id=5
        [HttpGet]
        public async Task<IActionResult> KiemTraTrangThai(int id)
        {
            var maNguoiDung = await LayMaNguoiDung();
            bool dangYeuThich = await _context.YeuThichs
                .AnyAsync(y => y.UserId == maNguoiDung && y.ProductId == id);
            int tongSo = await _context.YeuThichs.CountAsync(y => y.UserId == maNguoiDung);
            return Json(new { dangYeuThich, tongSo });
        }
    }
}
