using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Data
{
    public static class DbSeeder
    {
        public static async Task KhachHang(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in new[] { "NguoiDung", "QuanTri", "Admin" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        public static async Task TaoTaiKhoanAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var emailAdmin = configuration["SeedAdmin:Email"];
            var matKhauAdmin = configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(emailAdmin) || string.IsNullOrWhiteSpace(matKhauAdmin))
            {
                return;
            }

            var adminTonTai = await userManager.FindByEmailAsync(emailAdmin);
            if (adminTonTai == null)
            {
                var taiKhoanAdmin = new ApplicationUser
                {
                    UserName = emailAdmin,
                    Email = emailAdmin,
                    FullName = "Quan tri vien BOMIXI",
                    EmailConfirmed = true
                };
                var ketQua = await userManager.CreateAsync(taiKhoanAdmin, matKhauAdmin);
                if (ketQua.Succeeded)
                {
                    await userManager.AddToRoleAsync(taiKhoanAdmin, "QuanTri");
                    await userManager.AddToRoleAsync(taiKhoanAdmin, "Admin");
                }
            }
        }

        private class LaptopSeedModel
        {
            public string Ten { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string MoTa { get; set; } = string.Empty;
            public string DanhMuc { get; set; } = string.Empty;
            public string CPU { get; set; } = string.Empty;
            public string RAM { get; set; } = string.Empty;
            public string Storage { get; set; } = string.Empty;
            public string GPU { get; set; } = string.Empty;
            public string Screen { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public decimal Gia { get; set; }
            public decimal? GiaKM { get; set; }
            public int TonKho { get; set; }
            public List<string> HinhAnh { get; set; } = new();
        }

        public static async Task DuLieuMauSanPham(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (await context.Categories.AnyAsync()) return;

            var danhMucs = new List<Category>
            {
                new() { Name = "Apple", Slug = "apple", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/f/fa/Apple_logo_black.svg" },
                new() { Name = "Dell", Slug = "dell", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/48/Dell_Logo.svg" },
                new() { Name = "Asus", Slug = "asus", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/2e/ASUS_Logo.svg" },
                new() { Name = "Lenovo", Slug = "lenovo", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/b/b8/Lenovo_logo_2015.svg" },
                new() { Name = "HP", Slug = "hp", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/a/ad/HP_logo_2012.svg" },
                new() { Name = "MSI", Slug = "msi", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/1a/MSI_logo.svg" },
                new() { Name = "Acer", Slug = "acer", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/00/Acer_2011.svg" },
                new() { Name = "Gigabyte", Slug = "gigabyte", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5d/GIGABYTE_logo.svg" },
                new() { Name = "Microsoft", Slug = "microsoft", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg" },
            };
            context.Categories.AddRange(danhMucs);
            await context.SaveChangesAsync();

            var danhMucDict = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

            var spMau = new List<LaptopSeedModel>
            {
                new() { 
                    Ten = "Laptop Lenovo V14 G5 IRL 83HD0062VA", Slug = "laptop-lenovo-v14-g5-irl-83hd0062va",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "I5 13420H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 18990000M, GiaKM = 18990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-lenovo-v14-g5-irl-83hd0062va.webp" }
                },
                new() { 
                    Ten = "Laptop Dell 15 DC15250 CPH99", Slug = "laptop-dell-15-dc15250-cph99",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "i5-1334U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 22490000M, GiaKM = 19490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-dell-15-dc15250-i5-cph99.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad E14 Gen 7 21SX002YVN", Slug = "laptop-lenovo-thinkpad-e14-gen-7-21sx002yvn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Unknown", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 29990000M, GiaKM = 28490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-lenovo-thinkpad-e14-gen-7-21sx002yvn.jpg" }
                },
                new() { 
                    Ten = "Laptop Dell 15 DC15250 DC5I5897W1", Slug = "laptop-dell-15-dc15250-dc5i5897w1",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "i5-1334U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 inch", Color = "Black",
                    Gia = 19590000M, GiaKM = 19590000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-15-dc15250-i5-dc5i5897w1.webp" }
                },
                new() { 
                    Ten = "Laptop Dell 15 DC15250 DC5I7952W1", Slug = "laptop-dell-15-dc15250-dc5i7952w1",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "i7-1355U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 inch", Color = "Black",
                    Gia = 22590000M, GiaKM = 22590000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-15-dc15250-i7-dc5i7952w1.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad E16 Gen 3 21SR00A6VA", Slug = "laptop-lenovo-thinkpad-e16-gen-3-21sr00a6va",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 5-135H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 29990000M, GiaKM = 23990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkpad--e16-gen-3-21sr00a6va.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkBook 16 G9 IRL 21US008FVN", Slug = "laptop-lenovo-thinkbook-16-g9-irl-21us008fvn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "i5-13420H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "16 inch", Color = "Black",
                    Gia = 22990000M, GiaKM = 21290000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkbook-16-g9-irl-21us008fvn.webp" }
                },
                new() { 
                    Ten = "Laptop Asus ExpertBook P1 P1403CVA-C5H16-63WS", Slug = "laptop-asus-expertbook-p1-p1403cva-c5h16-63ws",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Core 5 210H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 21990000M, GiaKM = 21990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-asus-expertbook-p1-p1403cva-c5h16-63ws.webp" }
                },
                new() { 
                    Ten = "Laptop Asus ExpertBook P1503CVA-C5H16-50W", Slug = "laptop-asus-expertbook-p1503cva-c5h16-50w",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Core 5-210H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 inch", Color = "Black",
                    Gia = 20490000M, GiaKM = 20490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-asus-expertbook-core-5-p1503cva-c5h16-50w.webp" }
                },
                new() { 
                    Ten = "Laptop Dell 14 DC14250 DC4C5386W", Slug = "laptop-dell-14-dc14250-dc4c5386w",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "Core 5 120U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 20990000M, GiaKM = 20990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-14-dc14250-core-5-dc4c5386w.webp" }
                },
                new() { 
                    Ten = "Laptop Asus ExpertBook P1 P1403CVA-C3U08-50W", Slug = "laptop-asus-expertbook-p1-p1403cva-c3u08-50w",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Core 3 100U", RAM = "8GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 14990000M, GiaKM = 14990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-asus-expertbook-p1-p1403cva-c3u08-50w.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad E16 Gen 3 21SR00AAVN", Slug = "laptop-lenovo-thinkpad-e16-gen-3-21sr00aavn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 5-135H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "16 inch", Color = "Black",
                    Gia = 24490000M, GiaKM = 23990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkpad-e16-gen-3-21sr00aavn.webp" }
                },
                new() { 
                    Ten = "Laptop Asus ExpertBook B3 B3405CCA-LY0077W", Slug = "laptop-asus-expertbook-b3-b3405cca-ly0077w",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Ultra 5-225H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 23990000M, GiaKM = 22990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-asus-expertbook-b3-b3405cca-ly0077w.webp" }
                },
                new() { 
                    Ten = "Laptop Dell 15 DC15250 71092479", Slug = "laptop-dell-15-dc15250-71092479",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "i5-1334U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 inch", Color = "Black",
                    Gia = 22590000M, GiaKM = 19990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-15-dc15250-71092479.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad E14 Gen 7 21U2006GVA", Slug = "laptop-lenovo-thinkpad-e14-gen-7-21u2006gva",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 7-258V", RAM = "32GB", Storage = "SSD 1TB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 31890000M, GiaKM = 30990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkpad-e14-gen-7-21u2006gva.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad E14 Gen 7 21SX00BNVN", Slug = "laptop-lenovo-thinkpad-e14-gen-7-21sx00bnvn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 5-135H", RAM = "16GB", Storage = "SSD 512B", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 24990000M, GiaKM = 23990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkpad-e14-gen-7-u5-21sx00bnvn.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad T14s Gen 6 21QX00LHVA", Slug = "laptop-lenovo-thinkpad-t14s-gen-6-21qx00lhva",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 5-228V", RAM = "32GB", Storage = "SSD 1TB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 38990000M, GiaKM = 36990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkpad-t14s-gen-6-21qx00lhva.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad E14 Gen 7 MTL 21SX00BJVA", Slug = "laptop-lenovo-thinkpad-e14-gen-7-mtl-21sx00bjva",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 5-135H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 23990000M, GiaKM = 23990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkpad-e14-gen-7-mtl-21sx00bjva.webp" }
                },
                new() { 
                    Ten = "Laptop Asus ExpertBook B3 B3405CCA-LY0080W", Slug = "laptop-asus-expertbook-b3-b3405cca-ly0080w",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Ultra 7-255H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 28990000M, GiaKM = 26490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-asus-expertbook-b3-b3405cca-ly0080w.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo Legion 5 15IRX10 83LY00HQVN", Slug = "laptop-lenovo-legion-5-15irx10-83ly00hqvn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "i7-13650HX", RAM = "16GB", Storage = "RTX 5060 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 41990000M, GiaKM = 41990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-lenovo-legion-5-15irx10-i7-83ly00hqvn.webp" }
                },
                new() { 
                    Ten = "Laptop Dell Pro 14 Essential PV14250-120U-16512U", Slug = "laptop-dell-pro-14-essential-pv14250-120u-16512u",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "Core 5 120U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 24490000M, GiaKM = 20290000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-pro-14-essential-core-5-pv14250-120u-16512u.webp" }
                },
                new() { 
                    Ten = "Laptop Dell Pro 14 PC14250 PC14250-225U-16512WH", Slug = "laptop-dell-pro-14-pc14250-pc14250-225u-16512wh",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "Ultra 5 225U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 25790000M, GiaKM = 24990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-pro-14--pc14250-u5-pc14250-225u-16512wh.webp" }
                },
                new() { 
                    Ten = "Laptop Dell 14 DC14250 71092478", Slug = "laptop-dell-14-dc14250-71092478",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "Core 7-150U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 28390000M, GiaKM = 24990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-14-dc14250-71092478.webp" }
                },
                new() { 
                    Ten = "Laptop Asus Vivobook S14 S3407CA-SF913W", Slug = "laptop-asus-vivobook-s14-s3407ca-sf913w",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Ultra 5-225H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 29990000M, GiaKM = 29490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-asus-vivobook-s14-s3407ca-sf913w.webp" }
                },
                new() { 
                    Ten = "Laptop Asus Vivobook S14 S3407CA-SF923W", Slug = "laptop-asus-vivobook-s14-s3407ca-sf923w",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Ultra 7-255H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 31990000M, GiaKM = 31490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-asus-vivobook-s14-s3407ca-sf923w.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo IdeaPad Slim 3 14ARP10 83K6005VVN", Slug = "laptop-lenovo-ideapad-slim-3-14arp10-83k6005vvn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ryzen 5-7535HS", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 20990000M, GiaKM = 18990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-ideapad-slim-3-14arp10-83k6005vvn.webp" }
                },
                new() { 
                    Ten = "Laptop Dell Vostro 5630 V5630-i5P085W11GRU", Slug = "laptop-dell-vostro-5630-v5630-i5p085w11gru",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "i5 1340P", RAM = "8GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 20990000M, GiaKM = 19990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/2023_Xuyen_Audit/thumbs/370x200_laptop-dell-vostro-5630-v5630-i5p085w11gru-xam.jpg" }
                },
                new() { 
                    Ten = "Laptop Dell Latitude 3450 L3450-1335U-08512U", Slug = "laptop-dell-latitude-3450-l3450-1335u-08512u",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "i5 1335U", RAM = "8GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 17990000M, GiaKM = 15990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2024/thumbs/370x200_laptop-dell-latitude-3450-i5-l3450-1335u-08512u.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Aspire Lite 15 AL15-41P-R3QL NX.J54SV.001", Slug = "laptop-acer-aspire-lite-15-al15-41p-r3ql-nx-j54sv-001",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Ryzen 7 5700U", RAM = "8GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 12790000M, GiaKM = 12790000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-aspire-lite-15-al15-41p-r3ql-r7-nx.j54sv.001.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Nitro Lite NL16-71G-71UJ NH.D59SV.002", Slug = "laptop-acer-nitro-lite-nl16-71g-71uj-nh-d59sv-002",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "i7-13620H", RAM = "16GB", Storage = "RTX 3050 6GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 27190000M, GiaKM = 27190000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-nitro-lite-nl16-71g-71uj-i7-nh.d59sv.002.jpg" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad E16 Gen 3 21SR002JVA", Slug = "laptop-lenovo-thinkpad-e16-gen-3-21sr002jva",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Unknown", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 29490000M, GiaKM = 29490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-lenovo-thinkpad-e16-gen-3-21sr002jva.jpg" }
                },
                new() { 
                    Ten = "Laptop Gigabyte Gaming A16 CMHH2VN893SH", Slug = "laptop-gigabyte-gaming-a16-cmhh2vn893sh",
                    MoTa = "Laptop Gigabyte chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Gigabyte", CPU = "i5-13420H", RAM = "16GB", Storage = "RTX 4050 6GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 25790000M, GiaKM = 25790000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-gigabyte-gaming-a16-i5-gaming-a16-cmhh2vn893sh.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Aspire A14-11M-X8FP NX.JP3SV.001", Slug = "laptop-acer-aspire-a14-11m-x8fp-nx-jp3sv-001",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Unknown", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 21990000M, GiaKM = 19990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-aspire-a14-11m-x8fp-x1-nx.jp3sv.001.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Gaming Nitro V ProPanel ANV15-41-R9M1 NH.QPFSV.004", Slug = "laptop-acer-gaming-nitro-v-propanel-anv15-41-r9m1-nh-qpfsv-004",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Unknown", RAM = "16GB", Storage = "RTX 3050 6GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 24690000M, GiaKM = 24690000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-gaming-nitro-v-propanel-anv15-41-r9m1-r5-nh.qpfsv.004.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Gaming Nitro V ProPanel ANV15-41-R0Y4 NH.QPESV.004", Slug = "laptop-acer-gaming-nitro-v-propanel-anv15-41-r0y4-nh-qpesv-004",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Unknown", RAM = "16GB", Storage = "RTX 4050 6GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 28990000M, GiaKM = 28490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-gaming-nitro-v-propanel-anv15-41-r0y4-r7-nh.qpesv.004.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Aspire Go AG15-72P-776J NX.JRRSV.006", Slug = "laptop-acer-aspire-go-ag15-72p-776j-nx-jrrsv-006",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Core 7 150U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 21990000M, GiaKM = 21990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-aspire-go-ag15-72p-776j-core-7-nx.jrrsv.006.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Aspire 5 A515-58P-9841 NX.KVGSV.003", Slug = "laptop-acer-aspire-5-a515-58p-9841-nx-kvgsv-003",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "i9-13900H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 21690000M, GiaKM = 20990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-aspire-5-a515-58p-9841-i9-nx.kvgsv.003.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Aspire Go AG14-72P-563L NX.JSBSV.002", Slug = "laptop-acer-aspire-go-ag14-72p-563l-nx-jsbsv-002",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Core 5 120U", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 18990000M, GiaKM = 18990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-aspire-go-ag14-72p-563l-core-5-nx.jsbsv.002.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Nitro V ProPanel ANV15-52-74UM NH.QUBSV.003", Slug = "laptop-acer-nitro-v-propanel-anv15-52-74um-nh-qubsv-003",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Core 7 240H", RAM = "16GB", Storage = "RTX 3050 6GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 29990000M, GiaKM = 29990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-nitro-v-propanel-anv15-52-74um-core-7-nh.qubsv.003.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Nitro V ProPanel ANV15-52-72BM NH.QZ9SV.004", Slug = "laptop-acer-nitro-v-propanel-anv15-52-72bm-nh-qz9sv-004",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "i7-13620H", RAM = "16GB", Storage = "RTX 5050 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 33990000M, GiaKM = 33990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-nitro-v-propanel-anv15-52-72bm-i7-nh.qz9sv.004.webp" }
                },
                new() { 
                    Ten = "Laptop Dell Precision 3590 Mobile Workstation", Slug = "laptop-dell-precision-3590-mobile-workstation",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "Ultra 7 165H vPro Ent", RAM = "16GB", Storage = "SSD 512GB", GPU = "RTX 500 Ada", Screen = "15.6 FHD", Color = "Black",
                    Gia = 50000000M, GiaKM = 46990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-dell-precision-3590-ultra-7-165h-vpro-rtx-500-ada.webp" }
                },
                new() { 
                    Ten = "Laptop Dell 14 DC14255-R7165W11SLU", Slug = "laptop-dell-14-dc14255-r7165w11slu",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "Ryzen 7 250", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 22490000M, GiaKM = 22490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-dell-14-dc14255-r7-dc14255-r7165w11slu.webp" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad P14s Gen 6 21QT005FVN", Slug = "laptop-lenovo-thinkpad-p14s-gen-6-21qt005fvn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 7 255H", RAM = "32GB", Storage = "SSD 1 TB", GPU = "RTX PRO 500", Screen = "14.5 inch", Color = "Black",
                    Gia = 59000000M, GiaKM = 55990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-lenovo-thinkpad-p14s-gen-6-ultra-7-255h-rtx-pro-500-21qt005fvn.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Nitro V 16S ProPanel ANV16S-71-58WQ NH.QXBSV.001", Slug = "laptop-acer-nitro-v-16s-propanel-anv16s-71-58wq-nh-qxbsv-001",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Core 5-210H", RAM = "32GB", Storage = "RTX 5050 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 38990000M, GiaKM = 38990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-acer-nitro-v-16s-propanel-anv16s-71-58wq.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Aspire Lite 14 AL14-52P-309T", Slug = "laptop-acer-aspire-lite-14-al14-52p-309t",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "i3-1305U", RAM = "8GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "14 inch", Color = "Black",
                    Gia = 14890000M, GiaKM = 13990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2026/thumbs/370x200_laptop-acer-aspire-lite-14-al14-52p-309t.webp" }
                },
                new() { 
                    Ten = "Laptop Asus ExpertBook B5 B5404CMA-Q70250W", Slug = "laptop-asus-expertbook-b5-b5404cma-q70250w",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "Ultra 7 155H", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 27490000M, GiaKM = 27490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2024/thumbs/370x200_laptop-asus-expertbook-b5-ultra-7-b5404cma-q70250w.jpg" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad Z13 Gen 2 21JV001HVN", Slug = "laptop-lenovo-thinkpad-z13-gen-2-21jv001hvn",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Unknown", RAM = "64GB", Storage = "SSD 1TB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 61190000M, GiaKM = 58490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2024/thumbs/370x200_laptop-lenovo-thinkpad-z13-gen-2-r7-21jv001hvn.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Predator Triton Neo 16 PTN16-51-78JQ", Slug = "laptop-acer-predator-triton-neo-16-ptn16-51-78jq",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Ultra 7 155H", RAM = "32GB", Storage = "RTX 4060 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 51590000M, GiaKM = 46990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-predator-triton-neo-16-ptn16-51-78jq-u7-nh.qpnsv.004.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Swift X 14 AI SFX14-72G-79UW", Slug = "laptop-acer-swift-x-14-ai-sfx14-72g-79uw",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Ultra 7 155H", RAM = "32GB", Storage = "RTX 4070 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 53090000M, GiaKM = 51490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-swift-x-14-ai-sfx14-72g-79uw-u7-nx.ktusv.001.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Swift X 14 AI SFX14-72G-708X", Slug = "laptop-acer-swift-x-14-ai-sfx14-72g-708x",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Ultra 7 155H", RAM = "32GB", Storage = "RTX 4060 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 46690000M, GiaKM = 45490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-swift-x-14-ai-sfx14-72g-708x-u7-nx.kr8sv.003.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Swift X 14 AI SFX14-72G-77F9", Slug = "laptop-acer-swift-x-14-ai-sfx14-72g-77f9",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Ultra 7 155H", RAM = "32GB", Storage = "RTX 4050 6GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 44190000M, GiaKM = 42990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-swift-x-14-ai-sfx14-72g-77f9-u7-nx.kr7sv.004.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Swift X 14 SFX14-71G-78SY", Slug = "laptop-acer-swift-x-14-sfx14-71g-78sy",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "i7 13700H", RAM = "32GB", Storage = "RTX 4050 6GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 42290000M, GiaKM = 40990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-swift-x-14-sfx14-71g-78sy-i7-nx.kevsv.006.jpg" }
                },
                new() { 
                    Ten = "Laptop Asus ExpertBook B9 OLed B9403CVAR-KM0837X", Slug = "laptop-asus-expertbook-b9-oled-b9403cvar-km0837x",
                    MoTa = "Laptop Asus chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Asus", CPU = "i7-150U", RAM = "32GB", Storage = "SSD 2TB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 47990000M, GiaKM = 47990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-asus-expertbook-b9-oled-b9403cvar-km0837x.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Swift Go SFG14-74T-55HD NX.JF9SV.001", Slug = "laptop-acer-swift-go-sfg14-74t-55hd-nx-jf9sv-001",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Ultra 5 225H", RAM = "16GB", Storage = "SSD 1TB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 30990000M, GiaKM = 30990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-swift-go-sfg14-74t-55hd-u5-nx.jf9sv.001.jpg" }
                },
                new() { 
                    Ten = "Laptop Dell Inspiron 14 5441 5MNK1", Slug = "laptop-dell-inspiron-14-5441-5mnk1",
                    MoTa = "Laptop Dell chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Dell", CPU = "Unknown", RAM = "16GB", Storage = "SSD 512GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 28990000M, GiaKM = 26490000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-dell-inspiron-14-5441-5mnk1.jpg" }
                },
                new() { 
                    Ten = "Laptop Acer Predator Helios 18 AI PH18-73-98AQ", Slug = "laptop-acer-predator-helios-18-ai-ph18-73-98aq",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Unknown", RAM = "192GB", Storage = "RTX 5090 24GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 166990000M, GiaKM = 166990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-predator-helios-18-ai-u9-ph18-73-98aq.jpg" }
                },
                new() { 
                    Ten = "Laptop Lenovo ThinkPad P16s Gen3 21KS003HVA", Slug = "laptop-lenovo-thinkpad-p16s-gen3-21ks003hva",
                    MoTa = "Laptop Lenovo chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Lenovo", CPU = "Ultra 7 155H", RAM = "32GB", Storage = "RTX500ADA 4GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 45990000M, GiaKM = 45990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-lenovo-thinkpad-p16s-gen3-ultra-7-21ks003hva.webp" }
                },
                new() { 
                    Ten = "Laptop Gigabyte Gaming-A16-CTHH3VN893SH", Slug = "laptop-gigabyte-gaming-a16-cthh3vn893sh",
                    MoTa = "Laptop Gigabyte chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Gigabyte", CPU = "i5-13420H", RAM = "16GB", Storage = "RTX 5050 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 30590000M, GiaKM = 28990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-gigabyte-gaming-a16-i5-cthh3vn893sh.webp" }
                },
                new() { 
                    Ten = "Laptop Gigabyte Gaming-A16-CVHI3VN893SH", Slug = "laptop-gigabyte-gaming-a16-cvhi3vn893sh",
                    MoTa = "Laptop Gigabyte chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Gigabyte", CPU = "i7-13620H", RAM = "16GB", Storage = "RTX 5060 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 34590000M, GiaKM = 33290000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-gigabyte-gaming-a16-i7-cvhi3vn893sh.webp" }
                },
                new() { 
                    Ten = "Laptop Gigabyte Gaming-A16-CWHI3VNC94SH", Slug = "laptop-gigabyte-gaming-a16-cwhi3vnc94sh",
                    MoTa = "Laptop Gigabyte chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Gigabyte", CPU = "i7-13620H", RAM = "16GB", Storage = "RTX 5070 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 42590000M, GiaKM = 39990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-gigabyte-gaming-a16-i7-cwhi3vnc94sh.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Nitro V 16 AI ProPanel ANV16S-61-R0B8 NH.QXQSV.001", Slug = "laptop-acer-nitro-v-16-ai-propanel-anv16s-61-r0b8-nh-qxqsv-001",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Unknown", RAM = "16GB", Storage = "RTX 5050 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 39990000M, GiaKM = 39990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-nitro-v-16-ai-propanel-anv16s-61-r0b8-r5-nh.qxqsv.001.webp" }
                },
                new() { 
                    Ten = "Laptop Acer Nitro V 16 AI ProPanel ANV16S-61-R9ZV NH.QXPSV.002", Slug = "laptop-acer-nitro-v-16-ai-propanel-anv16s-61-r9zv-nh-qxpsv-002",
                    MoTa = "Laptop Acer chinh hang. 1 KHUYẾN MÃI",
                    DanhMuc = "Acer", CPU = "Unknown", RAM = "32GB", Storage = "RTX 5060 8GB", GPU = "Intel Iris Xe", Screen = "15.6 FHD", Color = "Black",
                    Gia = 49990000M, GiaKM = 49990000M, TonKho = 15,
                    HinhAnh = new List<string> { "https://www.tnc.com.vn/uploads/product/sp2025/thumbs/370x200_laptop-acer-nitro-v-16-ai-propanel-anv16s-61-r9zv-r7-nh.qxpsv.002.webp" }
                },
            };

            foreach (var item in spMau)
            {
                if (!danhMucDict.ContainsKey(item.DanhMuc)) continue;

                var p = new Product
                {
                    Name = item.Ten,
                    Slug = item.Slug,
                    Description = item.MoTa,
                    CategoryId = danhMucDict[item.DanhMuc]
                };
                context.Products.Add(p);
                await context.SaveChangesAsync();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductId = p.Id,
                    SKU = "TNC-" + p.Id.ToString("D4"),
                    CPU = item.CPU,
                    RAM = item.RAM,
                    Storage = item.Storage,
                    GPU = item.GPU,
                    Screen = item.Screen,
                    Color = item.Color,
                    Price = item.Gia,
                    DiscountPrice = item.GiaKM,
                    StockQuantity = item.TonKho
                });

                bool isMain = true;
                foreach (var url in item.HinhAnh)
                {
                    context.ProductImages.Add(new ProductImage
                    {
                        ProductId = p.Id,
                        ImageUrl = url,
                        IsMainImage = isMain
                    });
                    isMain = false;
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task DuLieuMauBanner(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (await context.Banners.AnyAsync()) return;

            var banners = new List<Banner>
            {
                new() { 
                    TieuDe = "RAZER BLADE 16", 
                    NhanTag = "PREMIUM GAMING",
                    MoTa = "The ultimate gaming laptop with OLED display.", 
                    DuongDanAnh = "https://pub-c81cd288a1ce41e0902f9cf43fb226d1.r2.dev/Img/RAZER%20BLADE16.jpg", 
                    DuongDanLink = "/", 
                    ThuTu = 1, 
                    DangHoatDong = true 
                },
                new() { 
                    TieuDe = "TRADE-IN PROGRAM", 
                    NhanTag = "UPGRADE DEAL",
                    MoTa = "Upgrade your gear today.", 
                    DuongDanAnh = "https://pub-c81cd288a1ce41e0902f9cf43fb226d1.r2.dev/Img/TRADE-IN%20PROGRAM.png", 
                    DuongDanLink = "/", 
                    ThuTu = 2, 
                    DangHoatDong = true 
                },
                new() { 
                    TieuDe = "GAMING STATION", 
                    NhanTag = "WORK & PLAY",
                    MoTa = "Power up your productivity.", 
                    DuongDanAnh = "https://pub-c81cd288a1ce41e0902f9cf43fb226d1.r2.dev/Img/gammingstation.png", 
                    DuongDanLink = "/", 
                    ThuTu = 3, 
                    DangHoatDong = true 
                }
            };

            context.Banners.AddRange(banners);
            await context.SaveChangesAsync();
        }

        public static async Task DuLieuMauSetting(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (await context.SystemSettings.AnyAsync()) return;

            var settings = new List<SystemSetting>
            {
                new() { Key = "FlashSale_Text", Value = "FLASH SALE - GIẢM 20% CHO ĐƠN HÀNG TRÊN 20 TRIỆU", Description = "Nội dung hiển thị trên thanh Flash Sale" },
                new() { Key = "FlashSale_EndDate", Value = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"), Description = "Thời gian kết thúc Flash Sale" },
                new() { Key = "FlashSale_IsActive", Value = "True", Description = "Trạng thái bật/tắt thanh Flash Sale" }
            };

            context.SystemSettings.AddRange(settings);
            await context.SaveChangesAsync();
        }
    }
}
