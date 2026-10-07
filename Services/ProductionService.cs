using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Production;
using CostQualityControl.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Services;

public class ProductionService(AppDbContext db) : IProductionService
{
    public async Task<List<BatchDto>> GetAllAsync()
    {
        var batches = await db.ProductionBatches.Include(b => b.Product).Include(b => b.BatchIngredients).ThenInclude(bi => bi.Ingredient).OrderByDescending(b => b.ProductionDate).ToListAsync();
        return batches.Select(MapToDto).ToList();
    }

    public async Task<BatchDto?> GetByIdAsync(int id)
    {
        var b = await db.ProductionBatches.Include(b => b.Product).Include(b => b.BatchIngredients).ThenInclude(bi => bi.Ingredient).FirstOrDefaultAsync(b => b.Id == id);
        return b is null ? null : MapToDto(b);
    }

    public async Task<BatchDto> CreateAsync(CreateBatchRequest req, string createdBy)
    {
        var batch = new ProductionBatch
        {
            BatchNumber = req.BatchNumber, ProductId = req.ProductId, ProductionDate = req.ProductionDate,
            ExpectedWeight = req.ExpectedWeight, ActualWeight = req.ActualWeight,
            Status = req.Status, Notes = req.Notes, CreatedBy = createdBy
        };
        batch.YieldPercentage = batch.ExpectedWeight > 0 ? (batch.ActualWeight / batch.ExpectedWeight) * 100 : 0;
        batch.WastePercentage = 100 - batch.YieldPercentage;
        foreach (var item in req.IngredientsUsed)
            batch.BatchIngredients.Add(new BatchIngredient { IngredientId = item.IngredientId, ExpectedWeight = item.ExpectedWeight, ActualWeight = item.ActualWeight });
        db.ProductionBatches.Add(batch);

        // Auto-populate ingredients from product formula
        var productWithIngredients = await db.Products
            .Include(p => p.ProductIngredients)
            .FirstOrDefaultAsync(p => p.Id == req.ProductId);

        if (productWithIngredients != null && !req.IngredientsUsed.Any())
        {
            var yieldFactor = req.ExpectedWeight > 0 ? req.ActualWeight / req.ExpectedWeight : 1m;
            foreach (var pi in productWithIngredients.ProductIngredients)
            {
                batch.BatchIngredients.Add(new BatchIngredient
                {
                    IngredientId = pi.IngredientId,
                    ExpectedWeight = pi.Weight,
                    ActualWeight = pi.Weight * yieldFactor  // yield-adjusted
                });
            }
        }

        await db.SaveChangesAsync();
        await db.Entry(batch).Reference(b => b.Product).LoadAsync();
        await db.Entry(batch).Collection(b => b.BatchIngredients).Query().Include(bi => bi.Ingredient).LoadAsync();
        return MapToDto(batch);
    }

    public async Task<BatchDto?> UpdateAsync(int id, UpdateBatchRequest req)
    {
        var batch = await db.ProductionBatches.Include(b => b.Product).Include(b => b.BatchIngredients).FirstOrDefaultAsync(b => b.Id == id);
        if (batch is null) return null;
        batch.ActualWeight = req.ActualWeight; batch.Status = req.Status; batch.Notes = req.Notes;
        batch.YieldPercentage = batch.ExpectedWeight > 0 ? (batch.ActualWeight / batch.ExpectedWeight) * 100 : 0;
        batch.WastePercentage = 100 - batch.YieldPercentage;
        // Only touch BatchIngredients if caller explicitly provided them
        if (req.IngredientsUsed.Any())
        {
            db.BatchIngredients.RemoveRange(batch.BatchIngredients);
            foreach (var item in req.IngredientsUsed)
                batch.BatchIngredients.Add(new BatchIngredient { BatchId = id, IngredientId = item.IngredientId, ExpectedWeight = item.ExpectedWeight, ActualWeight = item.ActualWeight });
        }
        else
        {
            // Recalculate ActualWeight on existing BatchIngredients based on new yield
            var yieldFactor = batch.ExpectedWeight > 0 ? batch.ActualWeight / batch.ExpectedWeight : 1m;
            foreach (var bi in batch.BatchIngredients)
                bi.ActualWeight = bi.ExpectedWeight * yieldFactor;
        }
        await db.SaveChangesAsync();
        await db.Entry(batch).Collection(b => b.BatchIngredients).Query().Include(bi => bi.Ingredient).LoadAsync();
        return MapToDto(batch);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var batch = await db.ProductionBatches.FindAsync(id);
        if (batch is null) return false;
        db.ProductionBatches.Remove(batch);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<SupplierConsumptionDto>> GetSupplierConsumptionAsync()
    {
        // Load all invoices grouped by supplier
        var invoices = await db.Invoices
            .Include(inv => inv.Items).ThenInclude(item => item.Ingredient)
            .Include(inv => inv.SupplierEntity)
            .ToListAsync();

        // Load all completed batch ingredient usage
        var usedIngredients = await db.BatchIngredients
            .Include(bi => bi.Batch)
            .Include(bi => bi.Ingredient)
            .Where(bi => bi.Batch.Status == "completed")
            .ToListAsync();

        // Build a lookup: ingredientId → total used kg across all completed batches
        var usedByIngredient = usedIngredients
            .GroupBy(bi => bi.IngredientId)
            .ToDictionary(g => g.Key, g => g.Sum(bi => bi.ActualWeight));

        var result = invoices
            .GroupBy(inv => inv.SupplierEntity?.Name ?? "Unknown")
            .Select(supplierGroup =>
            {
                // All invoice items for this supplier
                var allItems = supplierGroup
                    .SelectMany(inv => inv.Items)
                    .GroupBy(item => item.IngredientId)
                    .Select(ingGroup =>
                    {
                        var ingName = ingGroup.First().Ingredient?.Name ?? $"Ingredient #{ingGroup.Key}";
                        var unit = ingGroup.First().Ingredient?.Unit ?? "kg";
                        var totalPurchasedKg = ingGroup.Sum(i => i.Weight);
                        var totalPurchasedValue = ingGroup.Sum(i => i.TotalPrice);
                        var avgPricePerKg = totalPurchasedKg > 0 ? totalPurchasedValue / totalPurchasedKg : 0;

                        var totalUsedKg = usedByIngredient.TryGetValue(ingGroup.Key, out var used) ? used : 0;
                        // Cap used at purchased — can't use more than bought from this supplier
                        totalUsedKg = Math.Min(totalUsedKg, totalPurchasedKg);
                        var totalUsedValue = totalUsedKg * avgPricePerKg;
                        var remainingKg = totalPurchasedKg - totalUsedKg;
                        var remainingValue = remainingKg * avgPricePerKg;
                        var usagePct = totalPurchasedKg > 0
                            ? Math.Round((totalUsedKg / totalPurchasedKg) * 100, 2)
                            : 0;

                        return new IngredientConsumptionDto(
                            ingName, unit,
                            totalPurchasedKg, totalPurchasedValue,
                            totalUsedKg, totalUsedValue,
                            remainingKg, remainingValue,
                            usagePct
                        );
                    })
                    .ToList();

                var totalP_Kg = allItems.Sum(i => i.TotalPurchasedKg);
                var totalP_Val = allItems.Sum(i => i.TotalPurchasedValue);
                var totalU_Kg = allItems.Sum(i => i.TotalUsedKg);
                var totalU_Val = allItems.Sum(i => i.TotalUsedValue);
                var rem_Kg = allItems.Sum(i => i.RemainingKg);
                var rem_Val = allItems.Sum(i => i.RemainingValue);
                var pct = totalP_Kg > 0 ? Math.Round((totalU_Kg / totalP_Kg) * 100, 2) : 0;

                return new SupplierConsumptionDto(
                    supplierGroup.Key,
                    allItems,
                    totalP_Kg, totalP_Val,
                    totalU_Kg, totalU_Val,
                    rem_Kg, rem_Val,
                    pct
                );
            })
            .OrderBy(s => s.Supplier)
            .ToList();

        return result;
    }

    private static BatchDto MapToDto(ProductionBatch b)
    {
        var wasteWeight = b.ExpectedWeight - b.ActualWeight;
        var sellingValue = b.ActualWeight * (b.Product?.SellingPrice ?? 0m);
        var costValue = b.ActualWeight * (b.Product?.CostPerKg ?? 0m);
        var profitValue = sellingValue - costValue;
        return new BatchDto(
            b.Id, b.BatchNumber, b.ProductId, b.Product?.Name ?? "", b.ProductionDate,
            b.ExpectedWeight, b.ActualWeight, b.YieldPercentage, b.WastePercentage, b.Status, b.Notes,
            b.BatchIngredients.Select(bi => new BatchIngredientDto(bi.IngredientId, bi.Ingredient?.Name ?? "", bi.ExpectedWeight, bi.ActualWeight)).ToList(),
            wasteWeight, sellingValue, costValue, profitValue);
    }
}
