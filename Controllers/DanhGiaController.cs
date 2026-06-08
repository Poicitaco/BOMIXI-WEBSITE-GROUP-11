using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Controllers
{
    [Authorize]
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DanhGiaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiDanhGia(int maSanPham, int soSao, string? nhanXet)
        {
            var nguoiDung = await _userManager.GetUserAsync(User);
            if (nguoiDung == null)
                return Json(new { success = false, thongBao = "Vui lòng đăng nhập." });

            if (soSao < 1 || soSao > 5)
                return Json(new { success = false, thongBao = "Số sao không hợp lệ." });

            if (!string.IsNullOrWhiteSpace(nhanXet) && nhanXet.Length > 500)
                return Json(new { success = false, thongBao = "Nhận xét tối đa 500 ký tự." });

            var daMuaHang = await _context.Orders
                .Where(o => o.UserId == nguoiDung.Id && o.Status == "HoanThanh")
                .SelectMany(o => o.OrderDetails)
                .AnyAsync(d => d.ProductVariant != null && d.ProductVariant.ProductId == maSanPham);

            if (!daMuaHang)
            {
                return Json(new { success = false, thongBao = "Chỉ khách hàng đã mua và nhận sản phẩm mới có thể đánh giá." });
            }

            var danhGiaCu = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.ProductId == maSanPham && d.UserId == nguoiDung.Id);

            if (danhGiaCu != null)
            {
                danhGiaCu.SoSao = soSao;
                danhGiaCu.NhanXet = nhanXet?.Trim();
                danhGiaCu.NgayDanhGia = DateTime.Now;
                danhGiaCu.DaMuaHang = true;
                danhGiaCu.DangHienThi = true;
                danhGiaCu.GhiChuKiemDuyet = null;
            }
            else
            {
                _context.DanhGias.Add(new DanhGia
                {
                    ProductId = maSanPham,
                    UserId = nguoiDung.Id,
                    SoSao = soSao,
                    NhanXet = nhanXet?.Trim(),
                    NgayDanhGia = DateTime.Now,
                    DaMuaHang = true,
                    DangHienThi = true
                });
            }

            await _context.SaveChangesAsync();

            var visibleReviews = _context.DanhGias.Where(d => d.ProductId == maSanPham && d.DangHienThi);
            var tongDanhGia = await visibleReviews.CountAsync();
            var trungBinh = tongDanhGia > 0 ? await visibleReviews.AverageAsync(d => (double)d.SoSao) : 0;

            return Json(new
            {
                success = true,
                thongBao = "Cảm ơn bạn đã đánh giá sản phẩm!",
                trungBinh = Math.Round(trungBinh, 1),
                tongDanhGia
            });
        }
    }
}
