using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopLaptop_v1.Models
{
    public class WalletAddress
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(80)]
        public string Address { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = true;

        public DateTime LinkedAt { get; set; } = DateTime.Now;
    }
}
