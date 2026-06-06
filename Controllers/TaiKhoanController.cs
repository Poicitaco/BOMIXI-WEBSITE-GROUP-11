using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Controllers
{
    [Authorize]
    public class TaiKhoanController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public TaiKhoanController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Trang cá nhân
        public async Task<IActionResult> HoSo()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            var nguoiDung = await _userManager.GetUserAsync(User);
            if (nguoiDung == null) return RedirectToAction("Index", "Home");

            return View(nguoiDung);
        }

        // Cập nhật thông tin cá nhân
        [HttpPost]
        public async Task<IActionResult> CapNhatHoSo(string? hoTen, string? diaChi, string? soDienThoai)
        {
            var nguoiDung = await _userManager.GetUserAsync(User);
            if (nguoiDung == null) return RedirectToAction("Index", "Home");

            nguoiDung.FullName = hoTen;
            nguoiDung.Address = diaChi;
            nguoiDung.PhoneNumber = soDienThoai;

            var ketQua = await _userManager.UpdateAsync(nguoiDung);
            TempData[ketQua.Succeeded ? "ThanhCong" : "ThatBai"] = ketQua.Succeeded
                ? "Cập nhật thông tin thành công!"
                : "Có lỗi xảy ra, vui lòng thử lại.";

            return RedirectToAction("HoSo");
        }

        // Form đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            if (matKhauMoi != xacNhanMatKhau)
            {
                TempData["ThatBai"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("HoSo");
            }

            var nguoiDung = await _userManager.GetUserAsync(User);
            if (nguoiDung == null) return RedirectToAction("Index", "Home");

            var ketQua = await _userManager.ChangePasswordAsync(nguoiDung, matKhauCu, matKhauMoi);
            if (ketQua.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(nguoiDung);
                TempData["ThanhCong"] = "Đổi mật khẩu thành công!";
            }
            else
            {
                TempData["ThatBai"] = "Mật khẩu cũ không đúng!";
            }

            return RedirectToAction("HoSo");
        }
    }
}
