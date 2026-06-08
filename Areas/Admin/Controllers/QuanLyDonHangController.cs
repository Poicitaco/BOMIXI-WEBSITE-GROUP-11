using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.Services;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class QuanLyDonHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private const string GIO_HANG_KEY = "GioHang_Session";

        private static readonly Dictionary<string, string[]> ChuyenTrangThaiHopLe = new()
        {
            ["ChoXacNhan"] = new[] { "DangXuLy", "DaHuy" },
            ["DangXuLy"] = new[] { "DangGiao", "DaHuy" },
            ["DangGiao"] = new[] { "HoanThanh" },
            ["HoanThanh"] = Array.Empty<string>(),
            ["DaHuy"] = Array.Empty<string>()
        };

        public QuanLyDonHangController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string? trangThai)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
            ViewData["TrangThaiLoc"] = trangThai;

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(o => o.Status == trangThai);

            var danhSach = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(danhSach);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var donHang = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .ThenInclude(p => p!.Images)
                .Include(o => o.StatusHistories)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThaiMoi)
        {
            var donHang = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (donHang == null) return NotFound();

            var trangThaiCu = donHang.Status;
            if (!ChuyenTrangThaiHopLe.TryGetValue(trangThaiCu, out var hopLe) || !hopLe.Contains(trangThaiMoi))
            {
                return Json(new { success = false, message = $"Không thể chuyển từ {trangThaiCu} sang {trangThaiMoi}." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (trangThaiMoi == "DaHuy")
            {
                foreach (var detail in donHang.OrderDetails)
                {
                    if (detail.ProductVariant != null)
                    {
                        detail.ProductVariant.StockQuantity += detail.Quantity;
                    }
                }

                donHang.CancelledAt = DateTime.Now;
                donHang.CancelReason = "Admin hủy đơn.";
            }

            donHang.Status = trangThaiMoi;
            donHang.StatusUpdatedAt = DateTime.Now;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = donHang.Id,
                FromStatus = trangThaiCu,
                ToStatus = trangThaiMoi,
                ChangedBy = User.Identity?.Name ?? "Admin",
                Note = trangThaiMoi == "DaHuy" ? "Admin hủy đơn và hoàn lại tồn kho." : "Admin cập nhật trạng thái đơn hàng.",
                ChangedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (trangThaiMoi == "DangXuLy" && trangThaiCu == "ChoXacNhan")
            {
                await _emailService.SendOrderConfirmationEmailAsync(donHang);
            }

            return Json(new { success = true, trangThai = trangThaiMoi });
        }
    }
}
