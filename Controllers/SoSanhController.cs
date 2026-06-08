using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Extensions;
using ShopLaptop_v1.Services;
using ShopLaptop_v1.ViewModels;

namespace ShopLaptop_v1.Controllers
{
    public class SoSanhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AiCompareService _aiCompareService;
        private readonly LaptopScoringService _scoringService;
        private const string SO_SANH_KEY = "SoSanh_Session";

        public SoSanhController(
            ApplicationDbContext context,
            AiCompareService aiCompareService,
            LaptopScoringService scoringService)
        {
            _context = context;
            _aiCompareService = aiCompareService;
            _scoringService = scoringService;
        }

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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public IActionResult XoaTatCa()
        {
            HttpContext.Session.Remove(SO_SANH_KEY);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanTichAI(string? nhuCau, decimal? nganSach, string? mucDich, string? uuTien)
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

            var profile = new LaptopNeedProfile
            {
                Budget = nganSach,
                Purpose = string.IsNullOrWhiteSpace(mucDich) ? "general" : mucDich,
                Priority = string.IsNullOrWhiteSpace(uuTien) ? "balanced" : uuTien,
                Details = nhuCau?.Trim() ?? string.Empty
            };

            var advisorResult = _scoringService.Analyze(danhSachSP, profile);
            if (!advisorResult.Rankings.Any())
            {
                return Json(new { success = false, message = "Không có dữ liệu cấu hình để phân tích." });
            }

            var scoringReport = _scoringService.BuildMarkdownReport(advisorResult);
            var aiExplanation = await _aiCompareService.PhanTichTuVanAdvisorAsync(scoringReport);

            var finalContent = string.IsNullOrWhiteSpace(aiExplanation)
                ? scoringReport + "\n\n> Ghi chú: Chưa cấu hình API key AI hoặc AI tạm thời không phản hồi, nên hệ thống đang hiển thị kết quả từ thuật toán chấm điểm nội bộ."
                : scoringReport + "\n\n---\n\n## Tư vấn từ AI\n\n" + aiExplanation;

            return Json(new
            {
                success = true,
                content = finalContent,
                winnerId = advisorResult.BestOverall?.Product.Id,
                winnerName = advisorResult.BestOverall?.Product.Name
            });
        }
    }
}
