namespace CostQualityControl.API.DTOs.Dashboard;
public record DashboardStatsDto(
    int TotalProducts, int TotalIngredients, int TotalBatches,
    decimal AvgProfitMargin, decimal AvgYield, decimal QualityPassRate,
    List<RecentBatchDto> RecentBatches, List<TopProductDto> TopProducts
);
public record RecentBatchDto(string BatchNumber, string ProductName, decimal YieldPercentage, string Status);
public record TopProductDto(string Name, decimal ProfitPercentage, decimal SellingPrice, decimal TotalCost);
