using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Data;

namespace ShopLaptop_v1.Services
{
    public class SystemSettingService
    {
        private readonly ApplicationDbContext _context;

        public SystemSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetValueAsync(string key, string defaultValue = "")
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value ?? defaultValue;
        }

        public async Task<bool> IsActiveAsync(string key)
        {
            var value = await GetValueAsync(key);
            return value.Equals("True", StringComparison.OrdinalIgnoreCase);
        }
    }
}
