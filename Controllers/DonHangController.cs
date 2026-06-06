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

        // Danh sách đơn hàng của người dùng
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

        // Chi tiết 1 đơn hàng
        public async Task<IActionResult> ChiTiet(int id)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            var user = await _userManager.GetUserAsync(User);
            var donHang = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }
    }
}
