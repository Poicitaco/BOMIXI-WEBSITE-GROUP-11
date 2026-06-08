using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ShopLaptop_v1.Services
{
    public class AiCompareService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiCompareService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> PhanTichSoSanhAsync(string productInfo)
        {
            var apiKey = _configuration["GroqApi:ApiKey"];
            var model = _configuration["GroqApi:Model"] ?? "llama3-70b-8192";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Chưa cấu hình API key AI. Hệ thống vẫn có thể dùng chế độ chấm điểm nội bộ.";
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = @"Bạn là chuyên gia tư vấn laptop của BOMIXI.
So sánh khách quan, không bịa thông số ngoài dữ liệu được cung cấp.
Trả lời bằng tiếng Việt, dùng Markdown, có kết luận rõ sản phẩm phù hợp nhất."
                    },
                    new
                    {
                        role = "user",
                        content = $"Hãy so sánh các laptop sau:\n\n{productInfo}"
                    }
                },
                temperature = 0.45,
                max_tokens = 1000
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var jsonDocument = JsonDocument.Parse(responseString);
                if (jsonDocument.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    return choices[0].GetProperty("message").GetProperty("content").GetString()
                        ?? "AI không trả về nội dung phân tích.";
                }
            }

            return "AI tạm thời không phản hồi. Vui lòng thử lại sau.";
        }

        public async Task<string?> PhanTichTuVanAdvisorAsync(string scoringReport)
        {
            var apiKey = _configuration["GroqApi:ApiKey"];
            var model = _configuration["GroqApi:Model"] ?? "llama3-70b-8192";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = @"Bạn là AI Laptop Advisor của BOMIXI.
Bạn nhận một báo cáo chấm điểm đã được backend tính sẵn. Không bịa thông số ngoài báo cáo.
Hãy viết bằng tiếng Việt, gọn, rõ, có tính tư vấn.
Cấu trúc bắt buộc:
1. Kết luận nhanh
2. Vì sao máy thắng phù hợp nhất
3. Khi nào nên chọn máy khác
4. Cảnh báo ngân sách/tồn kho nếu có
5. Gợi ý mua hàng cuối cùng"
                    },
                    new
                    {
                        role = "user",
                        content = scoringReport
                    }
                },
                temperature = 0.35,
                max_tokens = 900
            };

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var jsonDocument = JsonDocument.Parse(responseString);
                if (jsonDocument.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    return choices[0].GetProperty("message").GetProperty("content").GetString();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
