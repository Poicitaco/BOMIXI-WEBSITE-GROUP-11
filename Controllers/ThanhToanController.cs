using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Controllers
{
    [Authorize] // Bắt buộc đăng nhập để thanh toán
    public class ThanhToanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public ThanhToanController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiện trang nhập thông tin thanh toán
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            if (!gioHang.Any())
            {
                return RedirectToAction("Index", "GioHang");
            }

            var user = await _userManager.GetUserAsync(User);
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
            ViewData["GioHang"] = gioHang;
            ViewData["UserAddress"] = user?.Address;
            return View();
        }

        // Xử lý nút Đặt Hàng
        [HttpPost]
        public async Task<IActionResult> DatHang(string diaChiGiaoHang, string soDienThoai)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            if (!gioHang.Any())
            {
                return BadRequest("Giỏ hàng trống!");
            }

            if (string.IsNullOrEmpty(diaChiGiaoHang) || string.IsNullOrEmpty(soDienThoai))
            {
                return BadRequest("Vui lòng điền đầy đủ địa chỉ và số điện thoại.");
            }

            var user = await _userManager.GetUserAsync(User);

            // 1. Tạo Đơn hàng mới
            var donHang = new Order
            {
                UserId = user?.Id ?? "",
                OrderDate = DateTime.Now,
                ShippingAddress = diaChiGiaoHang,
                PhoneNumber = soDienThoai,
                Status = "ChoXacNhan", // Mặc định là chờ xác nhận
                TotalAmount = gioHang.Sum(m => m.ThanhTien)
            };

            _context.Orders.Add(donHang);
            await _context.SaveChangesAsync(); // Lưu để lấy donHang.Id

            // 2. Tạo Chi tiết đơn hàng & Trừ tồn kho
            foreach (var muc in gioHang)
            {
                var chiTiet = new OrderDetail
                {
                    OrderId = donHang.Id,
                    ProductVariantId = muc.MaBienThe,
                    Quantity = muc.SoLuong,
                    UnitPrice = muc.DonGia
                };
                _context.OrderDetails.Add(chiTiet);

                // Trừ tồn kho logic
                var bienThe = await _context.ProductVariants.FindAsync(muc.MaBienThe);
                if (bienThe != null)
                {
                    bienThe.StockQuantity -= muc.SoLuong;
                }
            }

            await _context.SaveChangesAsync();

            // 3. Xóa giỏ hàng
            HttpContext.Session.Remove(GIO_HANG_KEY);

            return Json(new { success = true, message = "Đặt hàng thành công!", maDonHang = donHang.Id });
        }
    }
}
