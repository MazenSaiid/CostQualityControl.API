using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Financials;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Services;

public class FinancialService(AppDbContext db) : IFinancialService
{
    public async Task<FinancialSummaryDto> GetSummaryAsync()
    {
        // All completed batches with product info
        var batches = await db.ProductionBatches
            .Include(b => b.Product)
            .Include(b => b.BatchIngredients).ThenInclude(bi => bi.Ingredient).ThenInclude(i => i.SupplierEntity)
            .Where(b => b.Status == "completed")
            .OrderByDescending(b => b.ProductionDate)
            .ToListAsync();

        // All invoices with supplier
        var invoices = await db.Invoices
            .Include(i => i.SupplierEntity)
            .Include(i => i.Items).ThenInclude(item => item.Ingredient)
            .ToListAsync();

        // Batch revenue breakdown
        var batchRevenue = batches.Select(b =>
        {
            var revenue = b.ActualWeight * (b.Product?.SellingPrice ?? 0);
            var cost = b.ActualWeight * (b.Product?.CostPerKg ?? 0);
            var profit = revenue - cost;
            var margin = revenue > 0 ? (profit / revenue) * 100 : 0;
            return new BatchRevenueDto(
                b.Id, b.BatchNumber, b.Product?.Name ?? "",
                b.ProductionDate, b.ActualWeight,
                b.Product?.SellingPrice ?? 0,
                revenue, cost, profit, Math.Round(margin, 2)
            );
        }).ToList();

        // Invoice totals
        var totalInvoiced = invoices.Sum(i => i.TotalAmount);
        var totalPaid = invoices.Where(i => i.IsPaid).Sum(i => i.TotalAmount);
        var totalOwed = invoices.Where(i => !i.IsPaid).Sum(i => i.TotalAmount);
        var totalRevenue = batchRevenue.Sum(b => b.Revenue);
        var totalIngredientCost = batchRevenue.Sum(b => b.IngredientCost);

        // Build ingredient → supplier map
        var ingredientSupplier = await db.Ingredients
            .Include(i => i.SupplierEntity)
            .ToDictionaryAsync(i => i.Id, i => i.SupplierEntity?.Name ?? "Unknown");

        // Revenue per supplier: from batch ingredients
        var revenueBySupplier = batches
            .SelectMany(b => b.BatchIngredients.Select(bi => new
            {
                Supplier = bi.Ingredient?.SupplierEntity?.Name ?? ingredientSupplier.GetValueOrDefault(bi.IngredientId, "Unknown"),
                Revenue = bi.ActualWeight * (b.Product?.SellingPrice ?? 0)
            }))
            .GroupBy(x => x.Supplier)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));

        var supplierBreakdown = invoices
            .GroupBy(i => i.SupplierEntity?.Name ?? "Unknown")
            .Select(g =>
            {
                var invoiced = g.Sum(i => i.TotalAmount);
                var paid = g.Where(i => i.IsPaid).Sum(i => i.TotalAmount);
                var owed = g.Where(i => !i.IsPaid).Sum(i => i.TotalAmount);
                var revenue = revenueBySupplier.GetValueOrDefault(g.Key, 0);
                return new SupplierFinancialDto(g.Key, invoiced, paid, owed, revenue, revenue - invoiced);
            })
            .OrderBy(s => s.Supplier)
            .ToList();

        return new FinancialSummaryDto(
            totalRevenue,
            totalInvoiced,
            totalPaid,
            totalOwed,
            totalRevenue - totalIngredientCost,
            totalRevenue - totalOwed,
            batchRevenue,
            supplierBreakdown
        );
    }
}
