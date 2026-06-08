using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class QuanLyDanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyDanhGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.DanhGias
                .Include(d => d.User)
                .Include(d => d.Product)
                .OrderByDescending(d => d.NgayDanhGia)
                .ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiHienThi(int id, string? ghiChu)
        {
            var review = await _context.DanhGias.FindAsync(id);
            if (review == null) return Json(new { success = false, message = "Không tìm thấy đánh giá." });

            review.DangHienThi = !review.DangHienThi;
            review.GhiChuKiemDuyet = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();
            await _context.SaveChangesAsync();

            return Json(new { success = true, visible = review.DangHienThi });
        }
    }
}
