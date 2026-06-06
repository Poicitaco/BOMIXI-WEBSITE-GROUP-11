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
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public DanhGiaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Gửi đánh giá sản phẩm
        [HttpPost]
        public async Task<IActionResult> GuiDanhGia(int maSanPham, int soSao, string? nhanXet)
        {
            var nguoiDung = await _userManager.GetUserAsync(User);
            if (nguoiDung == null) return Json(new { success = false, thongBao = "Vui lòng đăng nhập" });

            if (soSao < 1 || soSao > 5)
                return Json(new { success = false, thongBao = "Số sao không hợp lệ" });

            // Kiểm tra đã đánh giá chưa
            var danhGiaCu = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.ProductId == maSanPham && d.UserId == nguoiDung.Id);

            if (danhGiaCu != null)
            {
                // Cập nhật đánh giá cũ
                danhGiaCu.SoSao = soSao;
                danhGiaCu.NhanXet = nhanXet;
                danhGiaCu.NgayDanhGia = DateTime.Now;
            }
            else
            {
                _context.DanhGias.Add(new DanhGia
                {
                    ProductId = maSanPham,
                    UserId = nguoiDung.Id,
                    SoSao = soSao,
                    NhanXet = nhanXet,
                    NgayDanhGia = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            // Tính điểm trung bình mới
            var trungBinh = await _context.DanhGias
                .Where(d => d.ProductId == maSanPham)
                .AverageAsync(d => (double)d.SoSao);

            var tongDanhGia = await _context.DanhGias
                .CountAsync(d => d.ProductId == maSanPham);

            return Json(new {
                success = true,
                thongBao = "Cảm ơn bạn đã đánh giá!",
                trungBinh = Math.Round(trungBinh, 1),
                tongDanhGia
            });
        }
    }
}
