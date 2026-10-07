namespace CostQualityControl.API.Models;
public class ProductIngredient
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int IngredientId { get; set; }
    public decimal Weight { get; set; }
    public decimal Cost { get; set; }
    public Product Product { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
