using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopLaptop_v1.Models
{
    public class WarrantyCertificate
    {
        public int Id { get; set; }

        [StringLength(40)]
        public string CertificateCode { get; set; } = string.Empty;

        [StringLength(80)]
        public string TokenId { get; set; } = string.Empty;

        [StringLength(120)]
        public string BlockchainTxHash { get; set; } = string.Empty;

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        public int OrderDetailId { get; set; }

        [ForeignKey("OrderDetailId")]
        public OrderDetail? OrderDetail { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [StringLength(80)]
        public string? WalletAddress { get; set; }

        [StringLength(160)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(300)]
        public string VariantSnapshot { get; set; } = string.Empty;

        public DateTime IssuedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddYears(2);
    }
}
