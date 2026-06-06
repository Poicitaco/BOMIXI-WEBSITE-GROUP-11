using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.Services;
using System.Text;

namespace ShopLaptop_v1.Controllers
{
    public class SoSanhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AiCompareService _aiCompareService;
        private const string SO_SANH_KEY = "SoSanh_Session";

        public SoSanhController(ApplicationDbContext context, AiCompareService aiCompareService)
        {
            _context = context;
            _aiCompareService = aiCompareService;
        }

        // Lấy danh sách ID sản phẩm đang trong session
        private List<int> GetSoSanhList()
        {
            return HttpContext.Session.Get<List<int>>(SO_SANH_KEY) ?? new List<int>();
        }

        public async Task<IActionResult> Index()
        {
            var dsId = GetSoSanhList();
            var danhSachSP = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => dsId.Contains(p.Id))
                .ToListAsync();

            return View(danhSachSP);
        }

        [HttpPost]
        public IActionResult Them(int id)
        {
            var dsId = GetSoSanhList();
            if (!dsId.Contains(id))
            {
                if (dsId.Count >= 3)
                {
                    return Json(new { success = false, message = "Chỉ có thể so sánh tối đa 3 sản phẩm cùng lúc!" });
                }
                dsId.Add(id);
                HttpContext.Session.Set(SO_SANH_KEY, dsId);
            }
            return Json(new { success = true, count = dsId.Count });
        }

        [HttpPost]
        public IActionResult Xoa(int id)
        {
            var dsId = GetSoSanhList();
            if (dsId.Contains(id))
            {
                dsId.Remove(id);
                HttpContext.Session.Set(SO_SANH_KEY, dsId);
            }
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public IActionResult XoaTatCa()
        {
            HttpContext.Session.Remove(SO_SANH_KEY);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PhanTichAI(string nhuCau)
        {
            var dsId = GetSoSanhList();
            if (dsId.Count < 2)
            {
                return Json(new { success = false, message = "Cần ít nhất 2 sản phẩm để so sánh." });
            }

            var danhSachSP = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => dsId.Contains(p.Id))
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"--- NHU CẦU NGƯỜI DÙNG ---");
            sb.AppendLine($"- Chi tiết: {nhuCau ?? "Không có mô tả cụ thể."}");
            sb.AppendLine();
            sb.AppendLine($"--- DANH SÁCH SẢN PHẨM ---");

            foreach (var sp in danhSachSP)
            {
                var v = sp.Variants.FirstOrDefault();
                if (v != null)
                {
                    sb.AppendLine($"- Tên máy: {sp.Name} (Hãng: {sp.Category?.Name})");
                    sb.AppendLine($"  - Giá: {v.DiscountPrice ?? v.Price} VNĐ");
                    sb.AppendLine($"  - CPU: {v.CPU}");
                    sb.AppendLine($"  - RAM: {v.RAM}");
                    sb.AppendLine($"  - Ổ cứng: {v.Storage}");
                    sb.AppendLine($"  - Màn hình: {v.Screen}");
                    sb.AppendLine($"  - Card đồ họa: {v.GPU}");
                    sb.AppendLine($"  - Màu sắc: {v.Color}");
                    if (!string.IsNullOrEmpty(sp.Description))
                    {
                        var shortDesc = sp.Description.Length > 500 ? sp.Description.Substring(0, 500) + "..." : sp.Description;
                        sb.AppendLine($"  - Mô tả: {shortDesc}");
                    }
                    sb.AppendLine();
                }
            }

            string result = await _aiCompareService.PhanTichSoSanhAsync(sb.ToString());
            return Json(new { success = true, content = result });
        }
    }
}
