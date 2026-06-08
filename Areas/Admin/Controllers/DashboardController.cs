using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string GIO_HANG_KEY = "GioHang_Session";

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var gioHang = HttpContext.Session.Get<List<MucGioHang>>(GIO_HANG_KEY) ?? new List<MucGioHang>();
            ViewData["SoLuongGioHang"] = gioHang.Sum(m => m.SoLuong);

            // Thống kê tổng quan
            ViewData["TongSanPham"] = await _context.Products.CountAsync();
            ViewData["TongDonHang"] = await _context.Orders.CountAsync();
            
            var today = DateTime.Now;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);

            ViewData["TongDoanhThu"] = await _context.Orders
                .Where(o => o.Status == "HoanThanh")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            ViewData["DoanhThuThangNay"] = await _context.Orders
                .Where(o => o.Status == "HoanThanh" && o.OrderDate >= startOfMonth)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            ViewData["DonChoXacNhan"] = await _context.Orders
                .Where(o => o.Status == "ChoXacNhan")
                .CountAsync();

            // Top 5 sản phẩm bán chạy
            var topSanPham = await _context.OrderDetails
                .Include(od => od.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
                .Where(od => od.ProductVariant != null && od.ProductVariant.Product != null)
                .GroupBy(od => od.ProductVariant!.ProductId)
                .Select(g => new {
                    Ten = g.First().ProductVariant!.Product!.Name,
                    SoLuong = g.Sum(od => od.Quantity),
                    DoanhThu = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToListAsync();
            ViewData["TopSanPham"] = topSanPham;

            // Thống kê doanh thu 7 ngày gần nhất
            var bayNgayTruoc = today.Date.AddDays(-6);
            var doanhThuBayNgay = await _context.Orders
                .Where(o => o.Status == "HoanThanh" && o.OrderDate >= bayNgayTruoc)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Ngay = g.Key, DoanhThu = g.Sum(o => o.TotalAmount) })
                .OrderBy(g => g.Ngay)
                .ToListAsync();
            
            var labels = new List<string>();
            var data = new List<decimal>();
            for (int i = 0; i < 7; i++)
            {
                var ngay = bayNgayTruoc.AddDays(i);
                labels.Add(ngay.ToString("dd/MM"));
                data.Add(doanhThuBayNgay.FirstOrDefault(x => x.Ngay == ngay)?.DoanhThu ?? 0);
            }
            ViewData["ChartLabels"] = labels;
            ViewData["ChartData"] = data;

            // Đơn hàng gần nhất
            var donHangGanNhat = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(8)
                .ToListAsync();
            ViewData["DonHangGanNhat"] = donHangGanNhat;

            // Sản phẩm sắp hết hàng
            var sapHetHang = await _context.ProductVariants
                .Include(pv => pv.Product)
                .Where(pv => pv.StockQuantity <= 5)
                .OrderBy(pv => pv.StockQuantity)
                .Take(5)
                .ToListAsync();
            ViewData["SapHetHang"] = sapHetHang;

            return View();
        }
    }
}
