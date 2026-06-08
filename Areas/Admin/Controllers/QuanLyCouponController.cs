using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class QuanLyCouponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyCouponController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons
                .OrderByDescending(c => c.IsActive)
                .ThenByDescending(c => c.EndDate)
                .ToListAsync();
            return View(coupons);
        }

        [HttpGet]
        public IActionResult Tao()
        {
            return View(new Coupon
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                IsActive = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Tao(Coupon coupon)
        {
            Normalize(coupon);
            await ValidateCouponAsync(coupon);
            if (!ModelState.IsValid) return View(coupon);

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Da tao ma giam gia {coupon.Code}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            return coupon == null ? NotFound() : View(coupon);
        }

        [HttpPost]
        public async Task<IActionResult> Sua(int id, Coupon input)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            Normalize(input);
            await ValidateCouponAsync(input, id);
            if (!ModelState.IsValid) return View(input);

            coupon.Code = input.Code;
            coupon.Name = input.Name;
            coupon.DiscountType = input.DiscountType;
            coupon.DiscountValue = input.DiscountValue;
            coupon.MaxDiscountAmount = input.MaxDiscountAmount;
            coupon.MinimumOrderAmount = input.MinimumOrderAmount;
            coupon.UsageLimit = input.UsageLimit;
            coupon.StartDate = input.StartDate;
            coupon.EndDate = input.EndDate;
            coupon.IsActive = input.IsActive;

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Da cap nhat ma giam gia {coupon.Code}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return Json(new { success = false, message = "Khong tim thay ma giam gia." });

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, active = coupon.IsActive });
        }

        private static void Normalize(Coupon coupon)
        {
            coupon.Code = (coupon.Code ?? string.Empty).Trim().ToUpperInvariant();
            coupon.Name = (coupon.Name ?? string.Empty).Trim();
            coupon.DiscountType = coupon.DiscountType is "Fixed" ? "Fixed" : "Percent";
        }

        private async Task ValidateCouponAsync(Coupon coupon, int? currentId = null)
        {
            if (string.IsNullOrWhiteSpace(coupon.Code))
                ModelState.AddModelError(nameof(Coupon.Code), "Ma giam gia khong duoc de trong.");
            if (string.IsNullOrWhiteSpace(coupon.Name))
                ModelState.AddModelError(nameof(Coupon.Name), "Ten chuong trinh khong duoc de trong.");
            if (coupon.DiscountValue <= 0 || coupon.DiscountType == "Percent" && coupon.DiscountValue > 100)
                ModelState.AddModelError(nameof(Coupon.DiscountValue), "Gia tri giam khong hop le.");
            if (coupon.MinimumOrderAmount < 0 || coupon.MaxDiscountAmount < 0 || coupon.UsageLimit < 0)
                ModelState.AddModelError(string.Empty, "Gia tri don toi thieu, giam toi da va luot dung khong the am.");
            if (coupon.EndDate <= coupon.StartDate)
                ModelState.AddModelError(nameof(Coupon.EndDate), "Ngay ket thuc phai sau ngay bat dau.");
            if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code && c.Id != currentId))
                ModelState.AddModelError(nameof(Coupon.Code), "Ma giam gia da ton tai.");
        }
    }
}
