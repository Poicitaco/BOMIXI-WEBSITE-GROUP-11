using ShopLaptop_v1.Models;
using ShopLaptop_v1.Services;
using ShopLaptop_v1.ViewModels;

namespace ShopLaptop_v1.Tests;

public class LaptopScoringServiceTests
{
    private readonly LaptopScoringService _service = new();

    [Fact]
    public void Analyze_GamingProfile_RanksDedicatedGpuFirst()
    {
        var gaming = Product("Gaming RTX", "Intel Core i7", "RTX 4070", "16GB", "1TB SSD", 35_000_000, 10);
        var office = Product("Office Iris", "Intel Core i7", "Iris Xe", "16GB", "1TB SSD", 28_000_000, 10);

        var result = _service.Analyze([office, gaming], new LaptopNeedProfile
        {
            Purpose = "gaming",
            Priority = "performance",
            Budget = 40_000_000
        });

        Assert.Equal("Gaming RTX", result.BestOverall?.Product.Name);
        Assert.True(result.Rankings[0].Scores["gpu"] > result.Rankings[1].Scores["gpu"]);
    }

    [Fact]
    public void Analyze_BudgetProfile_MarksExpensiveLaptopOverBudget()
    {
        var affordable = Product("Student Laptop", "Intel Core i5", "Iris Xe", "16GB", "512GB SSD", 18_000_000, 8);
        var expensive = Product("Premium Laptop", "Intel Core i9", "RTX 4080", "32GB", "2TB SSD", 70_000_000, 8);

        var result = _service.Analyze([expensive, affordable], new LaptopNeedProfile
        {
            Purpose = "office",
            Priority = "budget",
            Budget = 20_000_000
        });

        Assert.Equal("Student Laptop", result.BestValue?.Product.Name);
        Assert.True(result.Rankings.Single(x => x.Product.Name == "Premium Laptop").IsOverBudget);
    }

    [Fact]
    public void Analyze_OutOfStockLaptop_HasZeroStockScoreAndWarning()
    {
        var product = Product("Unavailable Laptop", "Intel Core i7", "RTX 4060", "16GB", "1TB SSD", 30_000_000, 0);

        var item = _service.Analyze([product], new LaptopNeedProfile { Purpose = "gaming" }).Rankings.Single();

        Assert.Equal(0, item.Scores["stock"]);
        Assert.Contains(item.Weaknesses, warning => warning.Contains("kho", StringComparison.OrdinalIgnoreCase));
    }

    private static Product Product(
        string name,
        string cpu,
        string gpu,
        string ram,
        string storage,
        decimal price,
        int stock)
    {
        return new Product
        {
            Name = name,
            Variants =
            [
                new ProductVariant
                {
                    CPU = cpu,
                    GPU = gpu,
                    RAM = ram,
                    Storage = storage,
                    Screen = "15.6 QHD 165Hz",
                    Price = price,
                    StockQuantity = stock
                }
            ]
        };
    }
}
