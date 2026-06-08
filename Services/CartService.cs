using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<MucGioHang> Items, List<string> Warnings)> SynchronizeAsync(
            List<MucGioHang> cart,
            bool clampQuantity = true)
        {
            var warnings = new List<string>();
            if (!cart.Any())
            {
                return (cart, warnings);
            }

            var variantIds = cart.Select(i => i.MaBienThe).Distinct().ToList();
            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .ThenInclude(p => p!.Images)
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            var synchronized = new List<MucGioHang>();
            foreach (var item in cart)
            {
                if (!variants.TryGetValue(item.MaBienThe, out var variant))
                {
                    warnings.Add($"{item.TenSanPham} không còn tồn tại và đã được bỏ khỏi giỏ hàng.");
                    continue;
                }

                if (variant.StockQuantity <= 0)
                {
                    warnings.Add($"{variant.Product?.Name ?? item.TenSanPham} hiện đã hết hàng.");
                    continue;
                }

                var quantity = Math.Max(1, item.SoLuong);
                if (clampQuantity && quantity > variant.StockQuantity)
                {
                    quantity = variant.StockQuantity;
                    warnings.Add($"{variant.Product?.Name ?? item.TenSanPham} chỉ còn {variant.StockQuantity} sản phẩm trong kho.");
                }

                synchronized.Add(BuildCartItem(variant, quantity));
            }

            return (synchronized, warnings);
        }

        public MucGioHang BuildCartItem(ProductVariant variant, int quantity)
        {
            var imageUrl = variant.Product?.Images.FirstOrDefault(i => i.IsMainImage)?.ImageUrl
                ?? variant.Product?.Images.FirstOrDefault()?.ImageUrl
                ?? string.Empty;

            return new MucGioHang
            {
                MaBienThe = variant.Id,
                TenSanPham = variant.Product?.Name ?? "Sản phẩm không xác định",
                CauHinh = $"{variant.CPU} | {variant.RAM} | {variant.Storage}",
                DonGia = variant.DiscountPrice ?? variant.Price,
                SoLuong = quantity,
                TonKho = variant.StockQuantity,
                HinhAnh = imageUrl
            };
        }
    }
}
