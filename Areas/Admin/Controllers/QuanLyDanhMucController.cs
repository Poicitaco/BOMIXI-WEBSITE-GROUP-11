using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;
using X.PagedList.Extensions;
using X.PagedList;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class QuanLyDanhMucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyDanhMucController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách danh mục
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var danhSach = await _context.Categories
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(danhSach.ToPagedList(pageNumber, pageSize));
        }

        // Form thêm danh mục
        [HttpGet]
        public IActionResult Tao()
        {
            return View();
        }

        // Xử lý thêm danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tao(string tenDanhMuc, string? moTa, string? hinhAnh)
        {
            if (string.IsNullOrWhiteSpace(tenDanhMuc))
            {
                TempData["LoiThaoTac"] = "Tên danh mục không được để trống!";
                return View();
            }

            var category = new Category
            {
                Name = tenDanhMuc,
                Slug = tenDanhMuc.ToLower().Replace(" ", "-").Replace("đ", "d"),
                Description = moTa,
                ImageUrl = hinhAnh
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã thêm danh mục '{tenDanhMuc}' thành công!";
            return RedirectToAction("Index");
        }

        // Form sửa danh mục
        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // Xử lý sửa danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(int id, string tenDanhMuc, string? moTa, string? hinhAnh)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            if (string.IsNullOrWhiteSpace(tenDanhMuc))
            {
                TempData["LoiThaoTac"] = "Tên danh mục không được để trống!";
                return View(category);
            }

            category.Name = tenDanhMuc;
            category.Slug = tenDanhMuc.ToLower().Replace(" ", "-").Replace("đ", "d");
            category.Description = moTa;
            category.ImageUrl = hinhAnh;

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã cập nhật danh mục '{tenDanhMuc}' thành công!";
            return RedirectToAction("Index");
        }

        // Xóa danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();

            if (category.Products != null && category.Products.Any())
            {
                TempData["LoiThaoTac"] = $"Không thể xóa hãng '{category.Name}' vì đang có {category.Products.Count} sản phẩm thuộc hãng này!";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xóa hãng '{category.Name}' thành công!";
            return RedirectToAction("Index");
        }
    }
}
