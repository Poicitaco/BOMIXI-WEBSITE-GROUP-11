using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.Services;

namespace ShopLaptop_v1.Controllers
{
    [Authorize]
    public class ThanhToanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public ThanhToanController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            var (items, warnings) = await _cartService.SynchronizeAsync(gioHang);

            HttpContext.Session.Set(GIO_HANG_KEY, items);
            if (!items.Any())
            {
                return RedirectToAction("Index", "GioHang");
            }

            var user = await _userManager.GetUserAsync(User);
            ViewData["SoLuongGioHang"] = items.Sum(m => m.SoLuong);
            ViewData["GioHang"] = items;
            ViewData["CanhBaoGioHang"] = warnings;
            ViewData["UserAddress"] = user?.Address;
            ViewData["UserPhone"] = user?.PhoneNumber;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHang(string diaChiGiaoHang, string soDienThoai)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            if (!gioHang.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống." });
            }

            if (string.IsNullOrWhiteSpace(diaChiGiaoHang) || string.IsNullOrWhiteSpace(soDienThoai))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ địa chỉ và số điện thoại." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            var (items, warnings) = await _cartService.SynchronizeAsync(gioHang, clampQuantity: false);
            if (warnings.Any())
            {
                HttpContext.Session.Set(GIO_HANG_KEY, items);
                return Json(new { success = false, message = string.Join(" ", warnings) });
            }

            var khongDuHang = items.FirstOrDefault(i => i.SoLuong > i.TonKho);
            if (khongDuHang != null)
            {
                return Json(new { success = false, message = $"{khongDuHang.TenSanPham} chỉ còn {khongDuHang.TonKho} sản phẩm trong kho." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var variantIds = items.Select(i => i.MaBienThe).ToList();
            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .ThenInclude(p => p!.Images)
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            foreach (var item in items)
            {
                if (!variants.TryGetValue(item.MaBienThe, out var variant) || variant.StockQuantity < item.SoLuong)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"{item.TenSanPham} không còn đủ hàng để đặt." });
                }
            }

            var donHang = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                StatusUpdatedAt = DateTime.Now,
                ShippingAddress = diaChiGiaoHang.Trim(),
                PhoneNumber = soDienThoai.Trim(),
                Status = "ChoXacNhan",
                TotalAmount = items.Sum(m => m.ThanhTien)
            };

            _context.Orders.Add(donHang);
            await _context.SaveChangesAsync();

            donHang.OrderNumber = TaoMaDonHang(donHang.Id, donHang.OrderDate);

            foreach (var item in items)
            {
                var variant = variants[item.MaBienThe];
                var imageUrl = variant.Product?.Images.FirstOrDefault(i => i.IsMainImage)?.ImageUrl
                    ?? variant.Product?.Images.FirstOrDefault()?.ImageUrl
                    ?? string.Empty;

                variant.StockQuantity -= item.SoLuong;

                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = donHang.Id,
                    ProductVariantId = item.MaBienThe,
                    ProductName = variant.Product?.Name ?? item.TenSanPham,
                    VariantSnapshot = $"{variant.CPU} | {variant.RAM} | {variant.Storage} | {variant.GPU}",
                    ProductImageUrl = imageUrl,
                    Quantity = item.SoLuong,
                    UnitPrice = variant.DiscountPrice ?? variant.Price
                });
            }

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = donHang.Id,
                FromStatus = string.Empty,
                ToStatus = "ChoXacNhan",
                ChangedBy = user.Email ?? user.UserName ?? "Khách hàng",
                Note = "Khách hàng tạo đơn hàng.",
                ChangedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            HttpContext.Session.Remove(GIO_HANG_KEY);

            return Json(new
            {
                success = true,
                message = "Đặt hàng thành công!",
                maDonHang = donHang.OrderNumber,
                id = donHang.Id
            });
        }

        private static string TaoMaDonHang(int id, DateTime orderDate)
        {
            return $"BMX-{orderDate:yyyyMMdd}-{id:D4}";
        }
    }
}
