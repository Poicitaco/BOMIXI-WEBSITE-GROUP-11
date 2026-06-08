using System.Text;
using System.Text.RegularExpressions;
using ShopLaptop_v1.Models;
using ShopLaptop_v1.ViewModels;

namespace ShopLaptop_v1.Services
{
    public class LaptopScoringService
    {
        public LaptopAdvisorResult Analyze(IEnumerable<Product> products, LaptopNeedProfile profile)
        {
            var result = new LaptopAdvisorResult { Profile = profile };

            foreach (var product in products)
            {
                var variant = product.Variants
                    .OrderBy(v => v.DiscountPrice ?? v.Price)
                    .FirstOrDefault();

                if (variant == null) continue;

                var item = ScoreProduct(product, variant, profile);
                result.Rankings.Add(item);
            }

            result.Rankings = result.Rankings
                .OrderByDescending(r => r.TotalScore)
                .ThenBy(r => r.Price)
                .ToList();

            return result;
        }

        public string BuildMarkdownReport(LaptopAdvisorResult result)
        {
            var sb = new StringBuilder();
            var profile = result.Profile;

            sb.AppendLine("## AI Laptop Advisor - Kết quả chấm điểm");
            sb.AppendLine();
            sb.AppendLine($"**Nhu cầu:** {PurposeName(profile.Purpose)}");
            sb.AppendLine($"**Ưu tiên:** {PriorityName(profile.Priority)}");
            if (profile.Budget.HasValue && profile.Budget.Value > 0)
            {
                sb.AppendLine($"**Ngân sách tối đa:** {profile.Budget.Value:N0} VNĐ");
            }
            if (!string.IsNullOrWhiteSpace(profile.Details))
            {
                sb.AppendLine($"**Mô tả thêm:** {profile.Details}");
            }
            sb.AppendLine();

            sb.AppendLine("| Xếp hạng | Laptop | Giá | Điểm phù hợp | Điểm mạnh | Lưu ý |");
            sb.AppendLine("|---|---|---:|---:|---|---|");
            var rank = 1;
            foreach (var item in result.Rankings)
            {
                sb.AppendLine($"| {rank++} | **{item.Product.Name}** | {item.Price:N0} VNĐ | **{item.TotalScore}/100** | {string.Join(", ", item.Strengths.Take(2))} | {string.Join(", ", item.Weaknesses.Take(2))} |");
            }
            sb.AppendLine();

            if (result.BestOverall != null)
            {
                sb.AppendLine($"### Lựa chọn tốt nhất tổng thể: {result.BestOverall.Product.Name}");
                sb.AppendLine($"Máy này đạt **{result.BestOverall.TotalScore}/100**, cân bằng tốt nhất giữa hiệu năng, giá trị và độ phù hợp với nhu cầu đã chọn.");
                sb.AppendLine();
            }

            if (result.BestValue != null)
            {
                sb.AppendLine($"### Lựa chọn tiết kiệm nhất: {result.BestValue.Product.Name}");
                sb.AppendLine("Đây là lựa chọn có tỷ lệ giá/hiệu năng tốt nhất trong nhóm sản phẩm không vượt ngân sách.");
                sb.AppendLine();
            }

            if (result.StrongestPerformance != null)
            {
                sb.AppendLine($"### Lựa chọn mạnh nhất về hiệu năng: {result.StrongestPerformance.Product.Name}");
                sb.AppendLine("Phù hợp nếu bạn ưu tiên tác vụ nặng như gaming, dựng video, render, máy ảo hoặc lập trình đa nhiệm.");
                sb.AppendLine();
            }

            sb.AppendLine("### Chi tiết điểm thành phần");
            foreach (var item in result.Rankings)
            {
                sb.AppendLine();
                sb.AppendLine($"**{item.Product.Name}**");
                sb.AppendLine($"- CPU: {item.Scores["cpu"]}/100");
                sb.AppendLine($"- GPU: {item.Scores["gpu"]}/100");
                sb.AppendLine($"- RAM/Lưu trữ: {item.Scores["memory"]}/100");
                sb.AppendLine($"- Màn hình: {item.Scores["screen"]}/100");
                sb.AppendLine($"- Giá trị/giá tiền: {item.Scores["value"]}/100");
                sb.AppendLine($"- Tồn kho: {item.Scores["stock"]}/100");
            }

            return sb.ToString();
        }

        private LaptopScoreResult ScoreProduct(Product product, ProductVariant variant, LaptopNeedProfile profile)
        {
            var price = variant.DiscountPrice ?? variant.Price;
            var scores = new Dictionary<string, int>
            {
                ["cpu"] = ScoreCpu(variant.CPU),
                ["gpu"] = ScoreGpu(variant.GPU),
                ["memory"] = ScoreMemory(variant.RAM, variant.Storage),
                ["screen"] = ScoreScreen(variant.Screen),
                ["value"] = ScoreValue(price, profile.Budget),
                ["stock"] = variant.StockQuantity > 5 ? 100 : variant.StockQuantity > 0 ? 70 : 0
            };

            var weights = GetWeights(profile);
            var total = (int)Math.Round(scores.Sum(s => s.Value * weights[s.Key]));
            total = Math.Clamp(total, 0, 100);

            var item = new LaptopScoreResult
            {
                Product = product,
                Variant = variant,
                Price = price,
                Scores = scores,
                TotalScore = total,
                IsOverBudget = profile.Budget.HasValue && profile.Budget.Value > 0 && price > profile.Budget.Value
            };

            AddInsights(item, profile);
            return item;
        }

        private static Dictionary<string, decimal> GetWeights(LaptopNeedProfile profile)
        {
            var weights = profile.Purpose switch
            {
                "gaming" => new Dictionary<string, decimal> { ["cpu"] = .18m, ["gpu"] = .34m, ["memory"] = .14m, ["screen"] = .16m, ["value"] = .13m, ["stock"] = .05m },
                "design" => new Dictionary<string, decimal> { ["cpu"] = .22m, ["gpu"] = .26m, ["memory"] = .18m, ["screen"] = .20m, ["value"] = .09m, ["stock"] = .05m },
                "coding" => new Dictionary<string, decimal> { ["cpu"] = .30m, ["gpu"] = .10m, ["memory"] = .25m, ["screen"] = .12m, ["value"] = .18m, ["stock"] = .05m },
                "office" => new Dictionary<string, decimal> { ["cpu"] = .16m, ["gpu"] = .04m, ["memory"] = .16m, ["screen"] = .14m, ["value"] = .45m, ["stock"] = .05m },
                _ => new Dictionary<string, decimal> { ["cpu"] = .23m, ["gpu"] = .18m, ["memory"] = .18m, ["screen"] = .15m, ["value"] = .21m, ["stock"] = .05m }
            };

            if (profile.Priority == "performance")
            {
                weights["cpu"] += .06m; weights["gpu"] += .06m; weights["value"] -= .08m; weights["screen"] -= .04m;
            }
            else if (profile.Priority == "budget")
            {
                weights["value"] += .12m; weights["gpu"] -= .05m; weights["cpu"] -= .04m; weights["screen"] -= .03m;
            }
            else if (profile.Priority == "display")
            {
                weights["screen"] += .12m; weights["value"] -= .06m; weights["gpu"] -= .03m; weights["cpu"] -= .03m;
            }

            return weights;
        }

        private static int ScoreCpu(string? cpu)
        {
            var text = (cpu ?? string.Empty).ToLowerInvariant();
            if (text.Contains("m3 max") || text.Contains("m4") || text.Contains("i9") || text.Contains("ryzen 9")) return 96;
            if (text.Contains("m3") || text.Contains("m2") || text.Contains("i7") || text.Contains("ryzen 7")) return 84;
            if (text.Contains("i5") || text.Contains("ryzen 5")) return 70;
            if (text.Contains("i3") || text.Contains("ryzen 3")) return 52;
            return 60;
        }

        private static int ScoreGpu(string? gpu)
        {
            var text = (gpu ?? string.Empty).ToLowerInvariant();
            if (text.Contains("4090")) return 100;
            if (text.Contains("4080")) return 94;
            if (text.Contains("4070")) return 86;
            if (text.Contains("4060")) return 76;
            if (text.Contains("4050")) return 68;
            if (text.Contains("rtx")) return 70;
            if (text.Contains("iris") || text.Contains("radeon") || text.Contains("integrated")) return 45;
            if (string.IsNullOrWhiteSpace(text)) return 40;
            return 55;
        }

        private static int ScoreMemory(string? ram, string? storage)
        {
            var ramGb = ExtractNumber(ram);
            var storageText = (storage ?? string.Empty).ToLowerInvariant();
            var storageScore = storageText.Contains("2tb") ? 100 : storageText.Contains("1tb") ? 85 : storageText.Contains("512") ? 70 : 55;
            var ramScore = ramGb >= 32 ? 95 : ramGb >= 16 ? 80 : ramGb >= 8 ? 60 : 45;
            return (ramScore + storageScore) / 2;
        }

        private static int ScoreScreen(string? screen)
        {
            var text = (screen ?? string.Empty).ToLowerInvariant();
            var score = 60;
            if (text.Contains("oled") || text.Contains("xdr") || text.Contains("mini")) score += 20;
            if (text.Contains("4k") || text.Contains("qhd") || text.Contains("2.8k")) score += 10;
            if (text.Contains("240hz") || text.Contains("165hz") || text.Contains("144hz")) score += 10;
            return Math.Clamp(score, 40, 100);
        }

        private static int ScoreValue(decimal price, decimal? budget)
        {
            if (!budget.HasValue || budget.Value <= 0)
            {
                return price <= 25_000_000 ? 95 : price <= 40_000_000 ? 80 : price <= 60_000_000 ? 65 : 50;
            }

            if (price <= budget.Value * 0.8m) return 100;
            if (price <= budget.Value) return 85;
            if (price <= budget.Value * 1.15m) return 55;
            return 30;
        }

        private static int ExtractNumber(string? text)
        {
            var match = Regex.Match(text ?? string.Empty, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        private static void AddInsights(LaptopScoreResult item, LaptopNeedProfile profile)
        {
            if (item.Scores["cpu"] >= 85) item.Strengths.Add("CPU mạnh");
            if (item.Scores["gpu"] >= 80) item.Strengths.Add("GPU tốt cho tác vụ nặng");
            if (item.Scores["memory"] >= 80) item.Strengths.Add("RAM/lưu trữ thoải mái");
            if (item.Scores["screen"] >= 80) item.Strengths.Add("Màn hình nổi bật");
            if (item.Scores["value"] >= 85) item.Strengths.Add("Giá trị/giá tiền tốt");
            if (item.Variant.StockQuantity > 0) item.Strengths.Add("Còn hàng");

            if (item.IsOverBudget) item.Weaknesses.Add("Vượt ngân sách");
            if (item.Scores["gpu"] < 60 && profile.Purpose == "gaming") item.Weaknesses.Add("GPU chưa lý tưởng cho gaming");
            if (item.Scores["memory"] < 70 && profile.Purpose == "coding") item.Weaknesses.Add("RAM/lưu trữ có thể hạn chế khi đa nhiệm");
            if (item.Scores["stock"] < 80) item.Weaknesses.Add("Tồn kho thấp");
            if (!item.Weaknesses.Any()) item.Weaknesses.Add("Không có điểm trừ lớn");
        }

        private static string PurposeName(string value) => value switch
        {
            "gaming" => "Gaming",
            "design" => "Đồ họa / render / sáng tạo nội dung",
            "coding" => "Lập trình / học CNTT",
            "office" => "Văn phòng / học tập",
            _ => "Đa dụng"
        };

        private static string PriorityName(string value) => value switch
        {
            "performance" => "Hiệu năng mạnh nhất",
            "budget" => "Tối ưu ngân sách",
            "display" => "Màn hình đẹp",
            _ => "Cân bằng"
        };
    }
}
