using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Services;

public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var products = await db.Products.Where(p => p.IsActive).ToListAsync();
        var ingredients = await db.Ingredients.CountAsync();
        var batches = await db.ProductionBatches.ToListAsync();
        var completedBatches = batches.Where(b => b.Status == "completed").ToList();
        var avgYield = completedBatches.Any() ? completedBatches.Average(b => (double)b.YieldPercentage) : 0;
        var avgMargin = products.Any() ? products.Average(p => (double)p.ProfitPercentage) : 0;
        const double passRate = 0;

        var recentBatches = await db.ProductionBatches.Include(b => b.Product).OrderByDescending(b => b.ProductionDate).Take(5)
            .Select(b => new RecentBatchDto(b.BatchNumber, b.Product.Name, b.YieldPercentage, b.Status)).ToListAsync();

        var topProducts = products.OrderByDescending(p => p.ProfitPercentage).Take(5)
            .Select(p => new TopProductDto(p.Name, p.ProfitPercentage, p.SellingPrice, p.TotalCost)).ToList();

        return new DashboardStatsDto(products.Count, ingredients, batches.Count,
            (decimal)avgMargin, (decimal)avgYield, (decimal)passRate, recentBatches, topProducts);
    }
}
