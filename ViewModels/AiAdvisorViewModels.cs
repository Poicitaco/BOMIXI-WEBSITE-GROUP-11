using ShopLaptop_v1.Models;

namespace ShopLaptop_v1.ViewModels
{
    public class LaptopNeedProfile
    {
        public decimal? Budget { get; set; }
        public string Purpose { get; set; } = "general";
        public string Priority { get; set; } = "balanced";
        public string Details { get; set; } = string.Empty;
    }

    public class LaptopScoreResult
    {
        public Product Product { get; set; } = default!;
        public ProductVariant Variant { get; set; } = default!;
        public decimal Price { get; set; }
        public int TotalScore { get; set; }
        public Dictionary<string, int> Scores { get; set; } = new();
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public bool IsOverBudget { get; set; }
    }

    public class LaptopAdvisorResult
    {
        public LaptopNeedProfile Profile { get; set; } = new();
        public List<LaptopScoreResult> Rankings { get; set; } = new();
        public LaptopScoreResult? BestOverall => Rankings.OrderByDescending(r => r.TotalScore).FirstOrDefault();
        public LaptopScoreResult? BestValue => Rankings
            .Where(r => !r.IsOverBudget)
            .OrderByDescending(r => r.Scores.GetValueOrDefault("value"))
            .ThenByDescending(r => r.TotalScore)
            .FirstOrDefault();
        public LaptopScoreResult? StrongestPerformance => Rankings
            .OrderByDescending(r => r.Scores.GetValueOrDefault("cpu") + r.Scores.GetValueOrDefault("gpu"))
            .FirstOrDefault();
    }
}
