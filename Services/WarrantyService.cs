using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Services
{
    public class WarrantyService
    {
        private readonly ApplicationDbContext _context;

        public WarrantyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> IssueForCompletedOrderAsync(Order order)
        {
            if (order.Status != "HoanThanh")
            {
                return 0;
            }

            var wallet = await _context.WalletAddresses
                .Where(w => w.UserId == order.UserId)
                .OrderByDescending(w => w.IsPrimary)
                .ThenByDescending(w => w.LinkedAt)
                .FirstOrDefaultAsync();

            var created = 0;
            foreach (var detail in order.OrderDetails)
            {
                var exists = await _context.WarrantyCertificates
                    .AnyAsync(w => w.OrderDetailId == detail.Id);
                if (exists) continue;

                var certificate = new WarrantyCertificate
                {
                    CertificateCode = $"BMX-WRT-{DateTime.Now:yyyyMMdd}-{detail.Id:D5}",
                    TokenId = $"BOMIXI-{order.Id}-{detail.Id}",
                    BlockchainTxHash = $"0x{Guid.NewGuid():N}{Guid.NewGuid():N}"[..66],
                    OrderId = order.Id,
                    OrderDetailId = detail.Id,
                    UserId = order.UserId,
                    WalletAddress = wallet?.Address,
                    ProductName = !string.IsNullOrWhiteSpace(detail.ProductName)
                        ? detail.ProductName
                        : detail.ProductVariant?.Product?.Name ?? "Laptop BOMIXI",
                    VariantSnapshot = !string.IsNullOrWhiteSpace(detail.VariantSnapshot)
                        ? detail.VariantSnapshot
                        : $"{detail.ProductVariant?.CPU} | {detail.ProductVariant?.RAM} | {detail.ProductVariant?.Storage}",
                    IssuedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddYears(2)
                };

                _context.WarrantyCertificates.Add(certificate);
                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync();
            }

            return created;
        }
    }
}
