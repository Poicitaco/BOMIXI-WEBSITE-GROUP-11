using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.Services;

namespace ShopLaptop_v1.Tests;

public class CouponServiceTests
{
    [Fact]
    public async Task ValidateAsync_PercentCoupon_RespectsMaximumDiscount()
    {
        await using var context = CreateContext();
        context.Coupons.Add(ValidCoupon("SAVE20", "Percent", 20, maxDiscount: 500_000));
        await context.SaveChangesAsync();

        var result = await new CouponService(context).ValidateAsync(" save20 ", 5_000_000);

        Assert.True(result.Success);
        Assert.Equal(500_000, result.DiscountAmount);
    }

    [Fact]
    public async Task ValidateAsync_SubtotalBelowMinimum_ReturnsFailure()
    {
        await using var context = CreateContext();
        context.Coupons.Add(ValidCoupon("MINIMUM", "Fixed", 200_000, minimumOrder: 2_000_000));
        await context.SaveChangesAsync();

        var result = await new CouponService(context).ValidateAsync("MINIMUM", 1_000_000);

        Assert.False(result.Success);
        Assert.Equal(0, result.DiscountAmount);
    }

    [Fact]
    public async Task ValidateAsync_ExpiredCoupon_ReturnsFailure()
    {
        await using var context = CreateContext();
        var coupon = ValidCoupon("OLD", "Fixed", 100_000);
        coupon.StartDate = DateTime.Now.AddDays(-10);
        coupon.EndDate = DateTime.Now.AddDays(-1);
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();

        var result = await new CouponService(context).ValidateAsync("OLD", 2_000_000);

        Assert.False(result.Success);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Coupon ValidCoupon(
        string code,
        string type,
        decimal value,
        decimal? maxDiscount = null,
        decimal minimumOrder = 0)
    {
        return new Coupon
        {
            Code = code,
            Name = code,
            DiscountType = type,
            DiscountValue = value,
            MaxDiscountAmount = maxDiscount,
            MinimumOrderAmount = minimumOrder,
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(1),
            IsActive = true
        };
    }
}
