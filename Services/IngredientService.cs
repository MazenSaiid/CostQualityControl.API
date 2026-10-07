using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Ingredients;
using CostQualityControl.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Services;

public class IngredientService(AppDbContext db, IProductService productService) : IIngredientService
{
    private static IngredientDto MapToDto(Ingredient i) =>
        new(i.Id, i.Name, i.Unit, i.CurrentCost, i.SupplierId, i.SupplierEntity?.Name, i.LastUpdated);

    public async Task<List<IngredientDto>> GetAllAsync() =>
        await db.Ingredients.Include(i => i.SupplierEntity).OrderBy(i => i.Name).Select(i => new IngredientDto(
            i.Id, i.Name, i.Unit, i.CurrentCost, i.SupplierId,
            i.SupplierEntity != null ? i.SupplierEntity.Name : null,
            i.LastUpdated)).ToListAsync();

    public async Task<IngredientDto?> GetByIdAsync(int id)
    {
        var i = await db.Ingredients.Include(i => i.SupplierEntity).FirstOrDefaultAsync(x => x.Id == id);
        return i is null ? null : MapToDto(i);
    }

    public async Task<IngredientDto> CreateAsync(CreateIngredientRequest req)
    {
        var ing = new Ingredient { Name = req.Name, Unit = req.Unit, CurrentCost = req.CurrentCost, SupplierId = req.SupplierId };
        db.Ingredients.Add(ing);
        await db.SaveChangesAsync();
        await db.Entry(ing).Reference(i => i.SupplierEntity).LoadAsync();
        return MapToDto(ing);
    }

    public async Task<IngredientDto?> UpdateAsync(int id, UpdateIngredientRequest req)
    {
        var ing = await db.Ingredients.Include(i => i.SupplierEntity).FirstOrDefaultAsync(x => x.Id == id);
        if (ing is null) return null;
        var costChanged = ing.CurrentCost != req.CurrentCost;
        ing.Name = req.Name; ing.Unit = req.Unit; ing.CurrentCost = req.CurrentCost; ing.SupplierId = req.SupplierId; ing.LastUpdated = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await db.Entry(ing).Reference(i => i.SupplierEntity).LoadAsync();
        if (costChanged) await productService.RecalculateCostsForIngredientAsync(id, req.CurrentCost);
        return MapToDto(ing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ing = await db.Ingredients.FindAsync(id);
        if (ing is null) return false;
        db.Ingredients.Remove(ing);
        await db.SaveChangesAsync();
        return true;
    }
}
