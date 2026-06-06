using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.Services
{
    public class EmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EmailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(Order order)
        {
            var apiKey = _configuration["Resend:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return false;

            var emailTo = order.User?.Email;
            if (string.IsNullOrEmpty(emailTo)) return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Xây dựng nội dung Email (Cyberpunk Style HTML)
            var sb = new StringBuilder();
            sb.Append(@"
                <div style='background-color:#050505; color:#fff; font-family:""Inter"",sans-serif; padding:40px; border:1px solid #1A1A1A; border-radius:16px; max-width:600px; margin:0 auto;'>
                    <div style='text-align:center; margin-bottom:30px;'>
                        <h1 style='color:#00FF66; margin:0; font-size:24px; text-transform:uppercase; letter-spacing:2px;'>BOMIXI STORE</h1>
                        <p style='color:#888; font-size:12px; letter-spacing:4px; text-transform:uppercase; margin-top:5px;'>Xác Nhận Đơn Hàng</p>
                    </div>
                    
                    <div style='background-color:#111; padding:20px; border-radius:8px; border-left:4px solid #00FF66; margin-bottom:30px;'>
                        <p style='margin:0 0 10px 0; color:#ddd;'>Xin chào <strong>" + (order.User?.UserName ?? "Khách hàng") + @"</strong>,</p>
                        <p style='margin:0; color:#aaa; font-size:14px;'>Đơn hàng <strong>#" + order.Id + @"</strong> của bạn đã được xác nhận và đang được xử lý.</p>
                    </div>
                    
                    <h3 style='color:#fff; text-transform:uppercase; font-size:14px; letter-spacing:1px; border-bottom:1px solid #222; padding-bottom:10px; margin-bottom:20px;'>Chi tiết đơn hàng</h3>
                    
                    <table style='width:100%; border-collapse:collapse; margin-bottom:30px;'>
                        <tr style='background-color:#111;'>
                            <th style='text-align:left; padding:12px; color:#888; font-size:12px; text-transform:uppercase; border-bottom:1px solid #222;'>Sản phẩm</th>
                            <th style='text-align:right; padding:12px; color:#888; font-size:12px; text-transform:uppercase; border-bottom:1px solid #222;'>SL</th>
                            <th style='text-align:right; padding:12px; color:#888; font-size:12px; text-transform:uppercase; border-bottom:1px solid #222;'>Đơn giá</th>
                        </tr>");

            foreach (var detail in order.OrderDetails)
            {
                var pName = detail.ProductVariant?.Product?.Name ?? "Laptop";
                var pPrice = detail.UnitPrice.ToString("N0") + " VNĐ";
                sb.Append($@"
                        <tr>
                            <td style='padding:12px; border-bottom:1px solid #1A1A1A; font-size:14px;'>{pName}</td>
                            <td style='text-align:right; padding:12px; border-bottom:1px solid #1A1A1A; font-size:14px;'>{detail.Quantity}</td>
                            <td style='text-align:right; padding:12px; border-bottom:1px solid #1A1A1A; color:#00FF66; font-weight:bold;'>{pPrice}</td>
                        </tr>");
            }

            sb.Append($@"
                        <tr>
                            <td colspan='2' style='text-align:right; padding:15px 12px; font-weight:bold; color:#fff;'>Tổng thanh toán:</td>
                            <td style='text-align:right; padding:15px 12px; color:#00FF66; font-size:18px; font-weight:bold;'>{order.TotalAmount.ToString("N0")} VNĐ</td>
                        </tr>
                    </table>
                    
                    <div style='text-align:center; padding-top:20px; border-top:1px solid #1A1A1A;'>
                        <p style='color:#666; font-size:11px; margin:0;'>Cảm ơn bạn đã tin tưởng BOMIXI. Hệ thống sẽ tự động cập nhật trạng thái giao hàng.</p>
                    </div>
                </div>");

            var requestBody = new
            {
                from = "BOMIXI <onboarding@resend.dev>", // Resend yêu cầu domain đã verify, mặc định dùng onboarding
                to = new[] { emailTo },
                reply_to = "itentad.work@gmail.com", // Khi khách hàng bấm Reply, thư sẽ gửi về đây
                subject = $"[BOMIXI] Đơn hàng #{order.Id} đã được xác nhận!",
                html = sb.ToString()
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
