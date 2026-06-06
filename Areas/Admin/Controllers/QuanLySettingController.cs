using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,QuanTri")]
    public class QuanLySettingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLySettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.ToListAsync();
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, string value)
        {
            var setting = await _context.SystemSettings.FindAsync(id);
            if (setting == null) return NotFound();

            setting.Value = value;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
