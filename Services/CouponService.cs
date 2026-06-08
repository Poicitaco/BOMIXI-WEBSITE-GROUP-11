using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Services
{
    public class CouponValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Coupon? Coupon { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class CouponService
    {
        private readonly ApplicationDbContext _context;

        public CouponService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CouponValidationResult> ValidateAsync(string? code, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new CouponValidationResult { Success = true };
            }

            var normalized = code.Trim().ToUpperInvariant();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalized);
            if (coupon == null)
            {
                return new CouponValidationResult { Success = false, Message = "Mã giảm giá không tồn tại." };
            }

            var now = DateTime.Now;
            if (!coupon.IsActive || coupon.StartDate > now || coupon.EndDate < now)
            {
                return new CouponValidationResult { Success = false, Message = "Mã giảm giá đã hết hiệu lực." };
            }

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
            {
                return new CouponValidationResult { Success = false, Message = "Mã giảm giá đã hết lượt sử dụng." };
            }

            if (subtotal < coupon.MinimumOrderAmount)
            {
                return new CouponValidationResult
                {
                    Success = false,
                    Message = $"Đơn hàng cần tối thiểu {coupon.MinimumOrderAmount:N0} VNĐ để dùng mã này."
                };
            }

            var discount = coupon.DiscountType == "Fixed"
                ? coupon.DiscountValue
                : subtotal * coupon.DiscountValue / 100m;

            if (coupon.MaxDiscountAmount.HasValue)
            {
                discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
            }

            discount = Math.Min(discount, subtotal);

            return new CouponValidationResult
            {
                Success = true,
                Message = $"Áp dụng mã {coupon.Code} thành công.",
                Coupon = coupon,
                DiscountAmount = discount
            };
        }
    }
}
