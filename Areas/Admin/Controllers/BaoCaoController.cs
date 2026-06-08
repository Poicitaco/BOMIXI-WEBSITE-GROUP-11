using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class BaoCaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BaoCaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Revenue"] = await _context.Orders
                .Where(o => o.Status == "HoanThanh")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            ViewData["Discount"] = await _context.Orders.SumAsync(o => (decimal?)o.DiscountAmount) ?? 0;
            ViewData["Customers"] = await _context.Users.CountAsync();
            ViewData["Certificates"] = await _context.WarrantyCertificates.CountAsync();
            ViewData["LowStock"] = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.StockQuantity <= 5)
                .OrderBy(v => v.StockQuantity)
                .ToListAsync();
            return View();
        }

        public async Task<FileResult> XuatDoanhThuCsv()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("OrderNumber,Customer,Date,Status,PaymentMethod,Subtotal,Discount,Total");
            foreach (var order in orders)
            {
                sb.AppendLine(string.Join(",",
                    Csv(order.OrderNumber ?? order.Id.ToString()),
                    Csv(order.User?.Email ?? string.Empty),
                    Csv(order.OrderDate.ToString("yyyy-MM-dd HH:mm")),
                    Csv(order.Status),
                    Csv(order.PaymentMethod),
                    Csv(order.SubtotalAmount.ToString(CultureInfo.InvariantCulture)),
                    Csv(order.DiscountAmount.ToString(CultureInfo.InvariantCulture)),
                    Csv(order.TotalAmount.ToString(CultureInfo.InvariantCulture))));
            }

            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(),
                "text/csv", $"bomixi-revenue-{DateTime.Now:yyyyMMdd}.csv");
        }

        private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
