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

        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThaiMoi)
        {
            var donHang = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (donHang == null) return NotFound();

            var trangThaiCu = donHang.Status;
            donHang.Status = trangThaiMoi;
            await _context.SaveChangesAsync();

            // Gửi email xác nhận nếu đơn hàng được admin xác nhận (chuyển sang trạng thái Đang xử lý)
            if (trangThaiMoi == "DangXuLy" && trangThaiCu == "ChoXacNhan")
            {
                await _emailService.SendOrderConfirmationEmailAsync(donHang);
            }

            return Json(new { success = true, trangThai = trangThaiMoi });
        }
    }
}
