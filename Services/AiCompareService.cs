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

            if (string.IsNullOrEmpty(apiKey))
            {
                return "Chưa cấu hình API Key. Vui lòng kiểm tra lại cấu hình.";
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = @"Bạn là một chuyên gia tư vấn Laptop cao cấp từ hệ thống BOMIXI.
Nhiệm vụ của bạn là so sánh các sản phẩm người dùng đã chọn và đưa ra lời khuyên dựa trên NHU CẦU CỤ THỂ của họ.
1. Phải so sánh chi tiết các thông số (CPU, RAM, GPU, Màn hình...).
2. Phải đánh giá xem sản phẩm nào tối ưu nhất cho nhu cầu đã nêu.
3. Câu trả lời phải chuyên nghiệp, khách quan, sử dụng Markdown (bao gồm cả bảng so sánh nếu cần).
4. Ngôn ngữ: Tiếng Việt."
                    },
                    new
                    {
                        role = "user",
                        content = $"Hãy so sánh các laptop sau dựa trên thông số:\n\n{productInfo}"
                    }
                },
                temperature = 0.7,
                max_tokens = 1000
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseString);
                if (jsonDocument.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var content = choices[0]
                                    .GetProperty("message")
                                    .GetProperty("content")
                                    .GetString();
                    return content ?? "Không thể phân tích dữ liệu từ phản hồi của AI.";
                }
                return "Phản hồi từ AI không đúng cấu trúc mong đợi.";
            }

            var error = await response.Content.ReadAsStringAsync();
            return $"Lỗi khi kết nối với AI: {response.StatusCode} - {error}";
        }
    }
}
