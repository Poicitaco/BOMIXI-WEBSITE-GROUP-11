using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.Extensions;
using X.PagedList.Extensions;
using X.PagedList;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class QuanLySanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public QuanLySanPhamController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            var danhSach = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            
            return View(danhSach.ToPagedList(pageNumber, pageSize));
        }

        // Form tạo sản phẩm
        [HttpGet]
        public async Task<IActionResult> Tao()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
            ViewData["DanhSachDanhMuc"] = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Tao(string tenSanPham, string? moTa, int maDanhMuc,
            string cpu, string ram, string storage, string? gpu, string? screen, string? mauSac,
            decimal gia, decimal? giaKhuyenMai, int soLuong, string? urlHinhAnh)
        {
            var validationError = await ValidateProductInput(
                tenSanPham, maDanhMuc, cpu, ram, storage, gia, giaKhuyenMai, soLuong);
            if (validationError != null)
            {
                TempData["LoiThaoTac"] = validationError;
                return RedirectToAction("Tao");
            }

            if (string.IsNullOrEmpty(tenSanPham))
            {
                TempData["LỗI"] = "Tên sản phẩm không được để trống!";
                return RedirectToAction("Tao");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var sanPham = new Product
            {
                Name = tenSanPham.Trim(),
                Slug = CreateSlug(tenSanPham),
                Description = moTa?.Trim(),
                CategoryId = maDanhMuc
            };
            _context.Products.Add(sanPham);
            await _context.SaveChangesAsync();

            var bienThe = new ProductVariant
            {
                ProductId = sanPham.Id,
                SKU = $"SP-{sanPham.Id}-001",
                CPU = cpu.Trim(),
                RAM = ram.Trim(),
                Storage = storage.Trim(),
                GPU = gpu?.Trim(),
                Screen = screen?.Trim(),
                Color = mauSac?.Trim(),
                Price = gia,
                DiscountPrice = giaKhuyenMai,
                StockQuantity = soLuong
            };
            _context.ProductVariants.Add(bienThe);

            if (!string.IsNullOrEmpty(urlHinhAnh))
            {
                var urls = urlHinhAnh.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool isFirst = true;
                foreach (var url in urls)
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = sanPham.Id,
                        ImageUrl = url.Trim(),
                        IsMainImage = isFirst
                    });
                    isFirst = false;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["ThanhCong"] = $"Đã thêm sản phẩm '{tenSanPham}' thành công!";
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm
        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            var sanPham = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (sanPham != null)
            {
                var variantIds = sanPham.Variants.Select(v => v.Id).ToList();
                var daCoTrongDonHang = await _context.OrderDetails
                    .AnyAsync(d => variantIds.Contains(d.ProductVariantId));
                if (daCoTrongDonHang)
                {
                    TempData["LoiThaoTac"] = "Khong the xoa san pham da phat sinh don hang. Hay dat ton kho ve 0 thay vi xoa.";
                    return RedirectToAction("Index");
                }

                _context.Products.Remove(sanPham);
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction("Index");
        }

        // Form sửa sản phẩm
        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
            ViewData["DanhSachDanhMuc"] = await _context.Categories.ToListAsync();

            var sanPham = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sanPham == null) return NotFound();
            ViewData["SanPham"] = sanPham;
            return View("Sua");
        }

        // Lưu chỉnh sửa sản phẩm
        [HttpPost]
        public async Task<IActionResult> CapNhat(int id, int maBienThe, string tenSanPham, string? moTa,
            int maDanhMuc, string cpu, string ram, string storage, string? gpu, string? screen,
            string? mauSac, decimal gia, decimal? giaKhuyenMai, int soLuong, string? urlHinhAnh)
        {
            var validationError = await ValidateProductInput(
                tenSanPham, maDanhMuc, cpu, ram, storage, gia, giaKhuyenMai, soLuong);
            if (validationError != null)
            {
                TempData["LoiThaoTac"] = validationError;
                return RedirectToAction("Sua", new { id });
            }

            var sanPham = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            var bienThe = await _context.ProductVariants.FindAsync(maBienThe);
            if (sanPham == null || bienThe == null || bienThe.ProductId != id) return NotFound();

            // Cập nhật thông tin sản phẩm
            sanPham.Name = tenSanPham.Trim();
            sanPham.Slug = CreateSlug(tenSanPham);
            sanPham.Description = moTa?.Trim();
            sanPham.CategoryId = maDanhMuc;

            // Cập nhật biến thể
            bienThe.CPU = cpu.Trim(); bienThe.RAM = ram.Trim(); bienThe.Storage = storage.Trim();
            bienThe.GPU = gpu?.Trim(); bienThe.Screen = screen?.Trim(); bienThe.Color = mauSac?.Trim();
            bienThe.Price = gia; bienThe.DiscountPrice = giaKhuyenMai;
            bienThe.StockQuantity = soLuong;

            // Cập nhật hình ảnh
            _context.ProductImages.RemoveRange(sanPham.Images);
            if (!string.IsNullOrEmpty(urlHinhAnh))
            {
                var urls = urlHinhAnh.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool isFirst = true;
                foreach (var url in urls)
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = sanPham.Id,
                        ImageUrl = url.Trim(),
                        IsMainImage = isFirst
                    });
                    isFirst = false;
                }
            }

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã cập nhật sản phẩm '{tenSanPham}' thành công!";
            return RedirectToAction("Index");
        }

        // Seed dữ liệu mẫu cho demo
        private async Task<string?> ValidateProductInput(
            string tenSanPham,
            int maDanhMuc,
            string cpu,
            string ram,
            string storage,
            decimal gia,
            decimal? giaKhuyenMai,
            int soLuong)
        {
            if (string.IsNullOrWhiteSpace(tenSanPham) || tenSanPham.Trim().Length > 200)
                return "Ten san pham phai co tu 1 den 200 ky tu.";
            if (!await _context.Categories.AnyAsync(c => c.Id == maDanhMuc))
                return "Danh muc da chon khong ton tai.";
            if (string.IsNullOrWhiteSpace(cpu) || string.IsNullOrWhiteSpace(ram) || string.IsNullOrWhiteSpace(storage))
                return "CPU, RAM va luu tru khong duoc de trong.";
            if (gia <= 0)
                return "Gia ban phai lon hon 0.";
            if (giaKhuyenMai.HasValue && (giaKhuyenMai.Value <= 0 || giaKhuyenMai.Value > gia))
                return "Gia khuyen mai phai lon hon 0 va khong vuot gia niem yet.";
            if (soLuong < 0)
                return "Ton kho khong the la so am.";
            return null;
        }

        private static string CreateSlug(string value)
        {
            return string.Join("-", value.Trim().ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        [HttpPost]
        public async Task<IActionResult> SeedDuLieuMau()
        {
            if (await _context.Products.AnyAsync())
                return Json(new { message = "Dữ liệu đã tồn tại, bỏ qua seed." });

            var danhMucs = new List<Category>
            {
                new() { Name = "Dell", Slug = "dell" },
                new() { Name = "Apple", Slug = "apple" },
                new() { Name = "ASUS", Slug = "asus" },
                new() { Name = "Razer", Slug = "razer" },
                new() { Name = "Lenovo", Slug = "lenovo" }
            };
            _context.Categories.AddRange(danhMucs);
            await _context.SaveChangesAsync();

            var sanPhams = new List<(string ten, string slug, string mo_ta, string danh_muc_slug, string cpu, string ram, string storage, string? gpu, string screen, decimal gia, decimal? gia_km, int ton_kho, string? hinh)>
            {
                ("Dell XPS 13 Plus", "dell-xps-13-plus", "Laptop doanh nhân cao cấp, thiết kế siêu mỏng nhẹ với màn hình OLED rực rỡ.", "dell", "Intel Core i7-1360P", "16GB LPDDR5", "512GB NVMe SSD", "Intel Iris Xe", "13.4\" OLED FHD+ 60Hz", 32_990_000, 29_990_000, 8, "https://lh3.googleusercontent.com/aida-public/AB6AXuBbpo2GSU7lgTbHiF50q5UEban8MtjocDKyGpKWTYvlh1FcR9rlEfEUreZNd4b1228hvVuljoaxTjq9C-hDXuM_ovybHLyVLzbEJURKX8yxo08G6y5xzTsIWDJ4CxvsJ3HBT6j_4uny3rCQtDuIG3rWkDmfTzElYk9Ht7Z9EFpCxT8TUQGSbJasvymD4GziBesrnrtTaxwA9uO3QW3ZAK2Pjoff-r6_mkjrPiWyF0hq_SWRgyxQy8-F0SWffA2pbu9sCmQ2YUe6lwoA"),
                ("MacBook Pro 16\"", "macbook-pro-16", "Hiệu năng vượt trội với chip M3 Max, màn hình Liquid Retina XDR sắc nét.", "apple", "Apple M3 Max", "36GB Unified Memory", "1TB SSD", null, "16.2\" Liquid Retina XDR", 79_990_000, null, 5, "https://lh3.googleusercontent.com/aida-public/AB6AXuBbpo2GSU7lgTbHiF50q5UEban8MtjocDKyGpKWTYvlh1FcR9rlEfEUreZNd4b1228hvVuljoaxTjq9C-hDXuM_ovybHLyVLzbEJURKX8yxo08G6y5xzTsIWDJ4CxvsJ3HBT6j_4uny3rCQtDuIG3rWkDmfTzElYk9Ht7Z9EFpCxT8TUQGSbJasvymD4GziBesrnrtTaxwA9uO3QW3ZAK2Pjoff-r6_mkjrPiWyF0hq_SWRgyxQy8-F0SWffA2pbu9sCmQ2YUe6lwoA"),
                ("ASUS ROG Strix SCAR 16", "asus-rog-strix-scar-16", "Cỗ máy gaming đỉnh cao với GPU RTX 4080, màn hình 240Hz mướt mà.", "asus", "AMD Ryzen 9 7945HX", "32GB DDR5", "2TB NVMe SSD", "NVIDIA RTX 4080 12GB", "16\" QHD+ 240Hz", 59_990_000, 54_990_000, 3, "https://lh3.googleusercontent.com/aida-public/AB6AXuA2vIuEhvYQUo4_hajpubw57f0n6H2Vo2VCEVkqbeZ0034kA3A_xPasGpthhTbMnTNulELHrbVZ_oeKk-eA-l0OYjddj5SJueKHTPjwCWN4KGVERSqRMgz7ub6TGSjoT6EIog8xgWh9bpFi-B6f88pSglV3wi4BmsjqRlTkq8FniOlCu4w-NE_N9HK0qWfmpzYA0SFH3BtUB6D7B7iOioCX_WO5_I93D9lqd33F8LPGV1Q8qNq7XQ2EmAhRgWEBK_Hp-51eTeUPuTan"),
                ("Razer Blade 16", "razer-blade-16", "Laptop gaming mỏng nhất thế giới với RTX 4090, dành cho game thủ chuyên nghiệp.", "razer", "Intel Core i9-13950HX", "32GB DDR5", "2TB NVMe SSD", "NVIDIA RTX 4090 16GB", "16\" WOLED 240Hz", 89_990_000, null, 2, "https://lh3.googleusercontent.com/aida-public/AB6AXuAcft7E-WDf7yvhhFXxR2f4fyDR-AqebVECZfsNGy8JYJBjirzU3djhwqmejPMy46Is3ki5PCHMyDOru0oCaW_ryMIQHSFcukNkDptcZgKJzw4qq_R_vRWTMCxoMFIlMT0oDcpCwbz47hg5_zwfPi46eCbfu31bLBXCZSEgFpUta0AbiD59RZSGirCMn1SE7FQUSmJUJMWHS1H_ykxqziXJVbpGEWC0uSIM6zI6zJ4mk0_OBHnxbfBcGn8D5l3z1uwn5f0KMg4fuIQE"),
                ("Lenovo ThinkPad X1 Carbon Gen 11", "thinkpad-x1-carbon-gen11", "Laptop doanh nhân bền bỉ, nhẹ nhất phân khúc với chứng nhận MIL-SPEC.", "lenovo", "Intel Core i7-1365U", "16GB LPDDR5", "512GB SSD", "Intel Iris Xe", "14\" WUXGA IPS Anti-glare", 41_990_000, 37_990_000, 10, "https://lh3.googleusercontent.com/aida-public/AB6AXuD5fKkG4lZ8-BFr7U2rb6cYSIt-x1o_yNp6gcAxXgRbjxR7HM3OInF6g3d7MNJEeTvHo0vHK5TTuROp5d51nqnJBy4VY9_Lth56PjeNjhfPPPE5mPmTsx4lOyMo65cO42tmFsXRoWrfGGrI_auzOAU2kybVJvjwBsvJuWGu2DVslH5CjsuR4xhlk6ceBzdh6LF1rbIJtDN_MsZBQK_bsJCHiEFwgYYAebeEJe8jHbftfJWLzHCTzZTP0VhfHX_rasjy23PUqgZapvCO"),
                ("Dell XPS 17 9730", "dell-xps-17-9730", "Màn hình 17 inch 4K tuyệt đẹp kết hợp GPU rời mạnh mẽ cho nhà sáng tạo.", "dell", "Intel Core i9-13900H", "32GB DDR5", "1TB NVMe SSD", "NVIDIA RTX 4070 8GB", "17\" 4K UHD+ OLED Touch", 69_990_000, 64_990_000, 4, "https://lh3.googleusercontent.com/aida-public/AB6AXuCmF2qLnjjI0P-f6gxvuyDblUyRQWex_pgQiTMxiUHXZ9B6ZyG3-l_rsJHQVWG8yX89UipmnM_XQhoQKpAIUyqQC0AgcLLFo0upC8qjD4gvztXHYWTIYMIOKyWZF618evJgjEUXrqjUoetfkWJgCnEoqXJyUYdkdVsjEQhq1Bfv8xi7Kpm755lXSUm4O8QciZooQPrw7AOKEmBIrbhju6AxR5J4GeyF8TUDBEKrm5ckBVgL2EATWD2TAu7jnAk3A0PgZGEWSEKJ_uNH")
            };

            foreach (var sp in sanPhams)
            {
                var dm = danhMucs.First(d => d.Slug == sp.danh_muc_slug);
                var product = new Product { Name = sp.ten, Slug = sp.slug, Description = sp.mo_ta, CategoryId = dm.Id };
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _context.ProductVariants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    SKU = $"{sp.slug.Replace("-", "").ToUpper()[..8]}-001",
                    CPU = sp.cpu, RAM = sp.ram, Storage = sp.storage,
                    GPU = sp.gpu, Screen = sp.screen,
                    Price = sp.gia, DiscountPrice = sp.gia_km, StockQuantity = sp.ton_kho
                });

                if (sp.hinh != null)
                {
                    _context.ProductImages.Add(new ProductImage { ProductId = product.Id, ImageUrl = sp.hinh, IsMainImage = true });
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = $"Đã tạo {sanPhams.Count} sản phẩm mẫu thành công!" });
        }
    }
}
