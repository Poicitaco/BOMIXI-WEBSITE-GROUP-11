using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public DonHangController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> DanhSach()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHangs = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(donHangs);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHang = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .ThenInclude(p => p!.Images)
                .Include(o => o.StatusHistories)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id, string? lyDo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var donHang = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (donHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            if (donHang.Status != "ChoXacNhan")
            {
                return Json(new { success = false, message = "Chỉ có thể hủy đơn đang chờ xác nhận." });
            }

            foreach (var detail in donHang.OrderDetails)
            {
                if (detail.ProductVariant != null)
                {
                    detail.ProductVariant.StockQuantity += detail.Quantity;
                }
            }

            var trangThaiCu = donHang.Status;
            donHang.Status = "DaHuy";
            donHang.StatusUpdatedAt = DateTime.Now;
            donHang.CancelledAt = DateTime.Now;
            donHang.CancelReason = string.IsNullOrWhiteSpace(lyDo) ? "Khách hàng hủy đơn." : lyDo.Trim();

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = donHang.Id,
                FromStatus = trangThaiCu,
                ToStatus = "DaHuy",
                ChangedBy = user.Email ?? user.UserName ?? "Khách hàng",
                Note = donHang.CancelReason,
                ChangedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = "Đã hủy đơn và hoàn lại tồn kho." });
        }
    }
}
