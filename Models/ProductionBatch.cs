namespace CostQualityControl.API.Models;
public class ProductionBatch
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public DateTime ProductionDate { get; set; }
    public decimal ExpectedWeight { get; set; }
    public decimal ActualWeight { get; set; }
    public decimal YieldPercentage { get; set; }
    public decimal WastePercentage { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public Product Product { get; set; } = null!;
    public ICollection<BatchIngredient> BatchIngredients { get; set; } = [];
}
