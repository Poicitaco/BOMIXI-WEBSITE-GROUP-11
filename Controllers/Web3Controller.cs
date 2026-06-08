using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Controllers
{
    public class Web3Controller : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public Web3Controller(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Wallet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            ViewData["Wallet"] = await _context.WalletAddresses
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.IsPrimary)
                .ThenByDescending(w => w.LinkedAt)
                .FirstOrDefaultAsync();

            var certificates = await _context.WarrantyCertificates
                .Include(w => w.Order)
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.IssuedAt)
                .ToListAsync();

            return View(certificates);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkWallet(string address)
        {
            if (string.IsNullOrWhiteSpace(address) || !address.StartsWith("0x") || address.Length < 20)
            {
                return Json(new { success = false, message = "Địa chỉ ví không hợp lệ." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });

            address = address.Trim();
            var existing = await _context.WalletAddresses
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.Address == address);

            if (existing == null)
            {
                var oldWallets = await _context.WalletAddresses.Where(w => w.UserId == user.Id).ToListAsync();
                foreach (var wallet in oldWallets)
                {
                    wallet.IsPrimary = false;
                }

                _context.WalletAddresses.Add(new WalletAddress
                {
                    UserId = user.Id,
                    Address = address,
                    IsPrimary = true,
                    LinkedAt = DateTime.Now
                });
            }
            else
            {
                existing.IsPrimary = true;
                existing.LinkedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã liên kết ví Web3 demo.", address });
        }

        [HttpGet]
        public async Task<IActionResult> Verify(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return View(null);
            }

            var certificate = await _context.WarrantyCertificates
                .Include(w => w.Order)
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.CertificateCode == code.Trim() || w.TokenId == code.Trim());

            return View(certificate);
        }
    }
}
