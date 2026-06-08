using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopLaptop_v1.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [StringLength(50)]
        public string FromStatus { get; set; } = string.Empty;

        [StringLength(50)]
        public string ToStatus { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Note { get; set; }

        [StringLength(100)]
        public string ChangedBy { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
