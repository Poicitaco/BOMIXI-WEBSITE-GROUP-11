using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopLaptop_v1.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string DiscountType { get; set; } = "Percent";

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumOrderAmount { get; set; }

        public int UsageLimit { get; set; }

        public int UsedCount { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        public bool IsActive { get; set; } = true;

        public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
    }
}
