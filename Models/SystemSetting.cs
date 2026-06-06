using System.ComponentModel.DataAnnotations;

namespace ShopLaptop_v1.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
