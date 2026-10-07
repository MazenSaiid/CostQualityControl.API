namespace CostQualityControl.API.Models;
public class BatchIngredient
{
    public int Id { get; set; }
    public int BatchId { get; set; }
    public int IngredientId { get; set; }
    public decimal ExpectedWeight { get; set; }
    public decimal ActualWeight { get; set; }
    public ProductionBatch Batch { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
