using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Products;
using CostQualityControl.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Services;

public class ProductService(AppDbContext db) : IProductService
{
    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await db.Products
            .Include(p => p.ProductIngredients).ThenInclude(pi => pi.Ingredient)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await db.Products.Include(p => p.ProductIngredients).ThenInclude(pi => pi.Ingredient).FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? null : MapToDto(p);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest req)
    {
        var product = new Product { Code = req.Code, Name = req.Name, Category = req.Category, SellingPrice = req.SellingPrice };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        await SetIngredientsAsync(product, req.Ingredients);
        RecalcProduct(product);
        await db.SaveChangesAsync();
        await db.Entry(product).Collection(p => p.ProductIngredients).Query().Include(pi => pi.Ingredient).LoadAsync();
        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest req)
    {
        var product = await db.Products.Include(p => p.ProductIngredients).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return null;
        product.Code = req.Code; product.Name = req.Name; product.Category = req.Category;
        product.SellingPrice = req.SellingPrice; product.UpdatedAt = DateTime.UtcNow;
        db.ProductIngredients.RemoveRange(product.ProductIngredients);
        await db.SaveChangesAsync();
        await SetIngredientsAsync(product, req.Ingredients);
        RecalcProduct(product);
        await db.SaveChangesAsync();
        await db.Entry(product).Collection(p => p.ProductIngredients).Query().Include(pi => pi.Ingredient).LoadAsync();
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return false;
        product.IsActive = false;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task RecalculateCostsForIngredientAsync(int ingredientId, decimal newCost)
    {
        var affected = await db.Products
            .Include(p => p.ProductIngredients).ThenInclude(pi => pi.Ingredient)
            .Where(p => p.ProductIngredients.Any(pi => pi.IngredientId == ingredientId))
            .ToListAsync();
        foreach (var product in affected)
        {
            foreach (var pi in product.ProductIngredients.Where(pi => pi.IngredientId == ingredientId))
                pi.Cost = pi.Weight * newCost;
            RecalcProduct(product);
        }
        await db.SaveChangesAsync();
    }

    private async Task SetIngredientsAsync(Product product, List<CreateProductIngredientRequest> items)
    {
        foreach (var item in items)
        {
            var ing = await db.Ingredients.FindAsync(item.IngredientId);
            if (ing is null) continue;
            db.ProductIngredients.Add(new ProductIngredient
            {
                ProductId = product.Id, IngredientId = item.IngredientId,
                Weight = item.Weight, Cost = item.Weight * ing.CurrentCost
            });
        }
        await db.SaveChangesAsync();
    }

    private static void RecalcProduct(Product product)
    {
        product.TotalCost = product.ProductIngredients.Sum(pi => pi.Cost);
        product.TotalWeight = product.ProductIngredients.Sum(pi => pi.Weight);
        product.CostPerKg = product.TotalWeight > 0 ? product.TotalCost / product.TotalWeight : 0;
        // Profit and margin are calculated per kg against the selling price per kg
        product.ProfitAmount = product.SellingPrice - product.CostPerKg;
        product.ProfitPercentage = product.SellingPrice > 0 ? (product.ProfitAmount / product.SellingPrice) * 100 : 0;
        product.UpdatedAt = DateTime.UtcNow;
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id, p.Code, p.Name, p.Category, p.SellingPrice, p.TotalCost, p.TotalWeight, p.CostPerKg, p.ProfitAmount, p.ProfitPercentage,
        p.ProductIngredients.Select(pi => new ProductIngredientDto(pi.IngredientId, pi.Ingredient?.Name ?? "", pi.Weight, pi.Ingredient?.Unit ?? "", pi.Cost)).ToList(),
        p.CreatedAt);
}
