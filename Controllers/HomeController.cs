using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;
using System.Diagnostics;

namespace ShopLaptop_v1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string GIO_HANG_KEY = "GioHang_Session";
        private const string DA_XEM_KEY = "DaXem_Session";

        public HomeController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Trang chủ - Danh sách sản phẩm có lọc và tìm kiếm (đã thêm phân trang)
        public async Task<IActionResult> Index(string? tuKhoa, string? danhMuc, string? sapXep, string? khoangGia, string? cpu, string? vga, string? ram, string? doPhanGiai, int? page)
        {
            int pageSize = 12; // Hiển thị 12 sản phẩm mỗi trang
            int pageNumber = (page ?? 1);
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
            ViewData["TuKhoa"] = tuKhoa;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .AsQueryable();

            // Lọc theo tên danh mục
            if (!string.IsNullOrEmpty(danhMuc))
            {
                query = query.Where(p => p.Category != null && p.Category.Slug == danhMuc);
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(p => p.Name.Contains(tuKhoa));
            }

            // Lọc theo RAM
            if (!string.IsNullOrEmpty(ram))
            {
                query = query.Where(p => p.Variants.Any(v => v.RAM != null && v.RAM.Contains(ram)));
            }

            // Lọc theo CPU
            if (!string.IsNullOrEmpty(cpu))
            {
                query = query.Where(p => p.Variants.Any(v => v.CPU != null && v.CPU.Contains(cpu)));
            }

            // Lọc theo VGA
            if (!string.IsNullOrEmpty(vga))
            {
                query = query.Where(p => p.Variants.Any(v => v.GPU != null && v.GPU.Contains(vga)));
            }

            // Lọc theo Độ phân giải/Màn hình
            if (!string.IsNullOrEmpty(doPhanGiai))
            {
                query = query.Where(p => p.Variants.Any(v => v.Screen != null && v.Screen.Contains(doPhanGiai)));
            }

            // Cache danh muc 5 phut
            var danhSachDanhMuc = await _cache.GetOrCreateAsync("DanhMuc_TatCa", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Categories.ToListAsync();
            });

            var danhSachSanPham = await query.ToListAsync();

            // Lấy danh sách Banner
            var danhSachBanner = await _context.Banners
                .Where(b => b.DangHoatDong)
                .OrderBy(b => b.ThuTu)
                .ToListAsync();

            // Ánh xạ sang ViewModel (Sản phẩm chính)
            var ketQua = danhSachSanPham
                .Where(p => p.Variants.Any())
                .Select(p => new SanPhamViewModel
                {
                    MaSanPham = p.Id,
                    TenSanPham = p.Name,
                    TenDanhMuc = p.Category?.Name ?? "",
                    SlugDanhMuc = p.Category?.Slug ?? "",
                    HinhAnh = p.Images.FirstOrDefault(i => i.IsMainImage)?.ImageUrl
                              ?? p.Images.FirstOrDefault()?.ImageUrl ?? "",
                    GiaThap = p.Variants.Min(v => v.DiscountPrice ?? v.Price),
                    GiaNiemYet = p.Variants.Min(v => v.Price),
                    CauHinhChính = p.Variants.OrderBy(v => v.Price).Select(v => $"{v.CPU} | {v.RAM} | {v.Storage}").FirstOrDefault() ?? "",
                    TonKho = p.Variants.Sum(v => v.StockQuantity),
                    MaBienTheChinhId = p.Variants.OrderBy(v => v.Price).Select(v => v.Id).FirstOrDefault()
                }).ToList();

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(khoangGia))
            {
                switch (khoangGia)
                {
                    case "duoi-15": ketQua = ketQua.Where(p => p.GiaThap < 15000000).ToList(); break;
                    case "15-20": ketQua = ketQua.Where(p => p.GiaThap >= 15000000 && p.GiaThap <= 20000000).ToList(); break;
                    case "20-25": ketQua = ketQua.Where(p => p.GiaThap >= 20000000 && p.GiaThap <= 25000000).ToList(); break;
                    case "25-30": ketQua = ketQua.Where(p => p.GiaThap >= 25000000 && p.GiaThap <= 30000000).ToList(); break;
                    case "tren-30": ketQua = ketQua.Where(p => p.GiaThap > 30000000).ToList(); break;
                }
            }

            // Sắp xếp
            ketQua = sapXep switch
            {
                "gia-tang" => ketQua.OrderBy(p => p.GiaThap).ToList(),
                "gia-giam" => ketQua.OrderByDescending(p => p.GiaThap).ToList(),
                "ten" => ketQua.OrderBy(p => p.TenSanPham).ToList(),
                _ => ketQua
            };

            // Lấy danh sách Flash Sale (Sản phẩm đang khuyến mãi) ngẫu nhiên 4 cái
            var flashSaleList = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.Variants.Any(v => v.DiscountPrice != null && v.DiscountPrice < v.Price))
                .ToListAsync();

            var flashSaleViewModels = flashSaleList.Select(p => {
                var v = p.Variants.OrderBy(x => x.Price).First();
                return new SanPhamViewModel {
                    MaSanPham = p.Id,
                    TenSanPham = p.Name,
                    TenDanhMuc = p.Category?.Name ?? "",
                    HinhAnh = p.Images.FirstOrDefault(i => i.IsMainImage)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl ?? "",
                    GiaThap = v.DiscountPrice ?? v.Price,
                    GiaNiemYet = v.Price,
                    CauHinhChính = $"{v.CPU} | {v.RAM} | {v.Storage}",
                    TonKho = v.StockQuantity,
                    MaBienTheChinhId = v.Id
                };
            })
            .OrderBy(x => Guid.NewGuid())
            .Take(4)
            .ToList();

            var model = new TrangChuViewModel
            {
                DanhSachSanPham = ketQua.ToPagedList(pageNumber, pageSize),
                DanhSachDanhMuc = danhSachDanhMuc ?? new List<Category>(),
                DanhMucDangChon = danhMuc,
                TuKhoa = tuKhoa,
                SapXep = sapXep,
                RAM = ram,
                DoPhanGiai = doPhanGiai,
                KhoangGia = khoangGia,
                CPU = cpu,
                VGA = vga,
                DanhSachBanner = danhSachBanner,
                DanhSachFlashSale = flashSaleViewModels
            };

            // SEO meta dong
            ViewData["MetaDescription"] = string.IsNullOrEmpty(danhMuc)
                ? "Khám phá laptop gaming, văn phòng, doanh nhân chính hãng tại BOMIXI. Giá tốt nhất, bảo hành 24 tháng."
                : $"Laptop {danhMuc} chính hãng giá tốt tại BOMIXI. Bảo hành 24 tháng, đổi trả 30 ngày.";
            ViewData["MetaTitle"] = string.IsNullOrEmpty(danhMuc) ? "BOMIXI - Laptop Gaming & Văn Phòng Chính Hãng" : $"Laptop {danhMuc} - BOMIXI";

            return View(model);
        }

        // Trang chi tiết sản phẩm
        public async Task<IActionResult> ChiTiet(int id)
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            var sanPham = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sanPham == null) return NotFound();

            // Load danh sách đánh giá
            var danhSachDanhGia = await _context.DanhGias
                .Include(d => d.User)
                .Where(d => d.ProductId == id && d.DangHienThi)
                .OrderByDescending(d => d.NgayDanhGia)
                .ToListAsync();

            double diemTrungBinh = danhSachDanhGia.Any() ? danhSachDanhGia.Average(d => d.SoSao) : 0;

            // Kiểm tra user hiện tại đã đánh giá chưa
            string? danhGiaCuaUser = null;
            int soSaoCuaUser = 0;
            if (User.Identity?.IsAuthenticated == true)
            {
                var maNguoiDung = _context.Users.Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).FirstOrDefault();
                var danhGiaCu = danhSachDanhGia.FirstOrDefault(d => d.UserId == maNguoiDung);
                if (danhGiaCu != null)
                {
                    danhGiaCuaUser = danhGiaCu.NhanXet;
                    soSaoCuaUser = danhGiaCu.SoSao;
                }
            }

            // Luu san pham da xem vao session
            var danhSachDaXem = HttpContext.Session.Get<List<int>>(DA_XEM_KEY) ?? new List<int>();
            danhSachDaXem.Remove(id);
            danhSachDaXem.Insert(0, id);
            if (danhSachDaXem.Count > 10) danhSachDaXem = danhSachDaXem.Take(10).ToList();
            HttpContext.Session.Set(DA_XEM_KEY, danhSachDaXem);

            // Goi y san pham lien quan (cung danh muc, loai tru san pham hien tai)
            var dsGoiYId = await _context.Products
                .Where(p => p.CategoryId == sanPham.CategoryId && p.Id != id && p.Variants.Any())
                .Select(p => p.Id)
                .ToListAsync();

            var random = new Random();
            var randomIds = dsGoiYId.OrderBy(x => random.Next()).Take(4).ToList();

            var goiY = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => randomIds.Contains(p.Id))
                .ToListAsync();

            // SEO
            ViewData["MetaTitle"] = $"{sanPham.Name} - BOMIXI";
            ViewData["MetaDescription"] = $"Mua {sanPham.Name} chính hãng tại BOMIXI. Bảo hành 24 tháng, giao hàng hỏa tốc, đổi trả 30 ngày.";
            ViewData["Title"] = sanPham.Name;
            ViewData["GoiY"] = goiY;
            ViewData["DanhSachDanhGia"] = danhSachDanhGia;
            ViewData["DiemTrungBinh"] = Math.Round(diemTrungBinh, 1);
            ViewData["TongDanhGia"] = danhSachDanhGia.Count;
            ViewData["DanhGiaCuaUser"] = danhGiaCuaUser;
            ViewData["SoSaoCuaUser"] = soSaoCuaUser;
            return View(sanPham);
        }

        public IActionResult Privacy()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GoiYTimKiem(string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                return Json(new List<object>());
            }

            var ketQua = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => p.Name.ToLower().Contains(tuKhoa.ToLower()))
                .Take(5)
                .Select(p => new
                {
                    id = p.Id,
                    ten = p.Name,
                    hinhAnh = p.Images.FirstOrDefault(i => i.IsMainImage) != null 
                                ? p.Images.FirstOrDefault(i => i.IsMainImage)!.ImageUrl 
                                : (p.Images.FirstOrDefault() != null ? p.Images.FirstOrDefault()!.ImageUrl : ""),
                    gia = p.Variants.Any() ? p.Variants.Min(v => v.DiscountPrice ?? v.Price) : 0
                })
                .ToListAsync();

            return Json(ketQua);
        }

        [HttpGet]
        public async Task<IActionResult> LaySanPhamDaXem()
        {
            var danhSachId = HttpContext.Session.Get<List<int>>(DA_XEM_KEY) ?? new List<int>();
            if (!danhSachId.Any()) return Json(new List<object>());

            var sanPhams = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => danhSachId.Contains(p.Id) && p.Variants.Any())
                .ToListAsync();

            // Sap xep theo thu tu da xem
            var ketQua = danhSachId
                .Where(id => sanPhams.Any(p => p.Id == id))
                .Select(id =>
                {
                    var p = sanPhams.First(x => x.Id == id);
                    var giaThap = p.Variants.Min(v => v.DiscountPrice ?? v.Price);
                    return new
                    {
                        id = p.Id,
                        ten = p.Name,
                        hinhAnh = p.Images.FirstOrDefault(i => i.IsMainImage)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl ?? "",
                        gia = giaThap,
                        giaFormat = giaThap.ToString("N0") + " VNĐ"
                    };
                })
                .Take(6)
                .ToList();

            return Json(ketQua);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
