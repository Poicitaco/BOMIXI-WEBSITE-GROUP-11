using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class QuanLyKhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuanLyKhachHangController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? tuKhoa)
        {
            ViewData["TuKhoa"] = tuKhoa;
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(u =>
                    (u.Email != null && u.Email.Contains(tuKhoa)) ||
                    (u.FullName != null && u.FullName.Contains(tuKhoa)));
            }

            var users = await query.OrderByDescending(u => u.Id).ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();
            ViewData["OrderCounts"] = await _context.Orders
                .Where(o => userIds.Contains(o.UserId))
                .GroupBy(o => o.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewData["SpentTotals"] = await _context.Orders
                .Where(o => userIds.Contains(o.UserId) && o.Status == "HoanThanh")
                .GroupBy(o => o.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(o => o.TotalAmount));

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            if (user.UserName == User.Identity?.Name)
                return Json(new { success = false, message = "Không thể khóa tài khoản đang đăng nhập." });

            var isLocked = await _userManager.IsLockedOutAsync(user);
            var result = await _userManager.SetLockoutEndDateAsync(
                user,
                isLocked ? null : DateTimeOffset.UtcNow.AddYears(10));

            return Json(new
            {
                success = result.Succeeded,
                locked = !isLocked,
                message = result.Succeeded ? (!isLocked ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản.") : "Không thể cập nhật tài khoản."
            });
        }
    }
}
