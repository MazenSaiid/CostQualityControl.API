namespace CostQualityControl.API.Models;
public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal ProfitAmount { get; set; }
    public decimal ProfitPercentage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ProductIngredient> ProductIngredients { get; set; } = [];
    public ICollection<ProductionBatch> ProductionBatches { get; set; } = [];
}
