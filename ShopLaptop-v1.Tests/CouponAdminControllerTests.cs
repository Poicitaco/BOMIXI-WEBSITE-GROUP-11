using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Areas.Admin.Controllers;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Tests;

public class CouponAdminControllerTests
{
    [Fact]
    public async Task Tao_ValidCoupon_NormalizesAndSaves()
    {
        await using var context = CreateContext();
        var controller = new QuanLyCouponController(context);

        var result = await controller.Tao(ValidCoupon(" student "));

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await context.Coupons.SingleAsync();
        Assert.Equal("STUDENT", saved.Code);
    }

    [Fact]
    public async Task Tao_DuplicateCode_ReturnsViewWithoutSaving()
    {
        await using var context = CreateContext();
        context.Coupons.Add(ValidCoupon("STUDENT"));
        await context.SaveChangesAsync();
        var controller = new QuanLyCouponController(context);

        var result = await controller.Tao(ValidCoupon("student"));

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, await context.Coupons.CountAsync());
    }

    [Fact]
    public async Task DoiTrangThai_TogglesCoupon()
    {
        await using var context = CreateContext();
        var coupon = ValidCoupon("ACTIVE");
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();
        var controller = new QuanLyCouponController(context);

        var result = await controller.DoiTrangThai(coupon.Id);

        Assert.IsType<JsonResult>(result);
        Assert.False(coupon.IsActive);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Coupon ValidCoupon(string code)
    {
        return new Coupon
        {
            Code = code,
            Name = "Student discount",
            DiscountType = "Percent",
            DiscountValue = 10,
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(10),
            IsActive = true
        };
    }
}
