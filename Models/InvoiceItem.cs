namespace CostQualityControl.API.Models;
public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int IngredientId { get; set; }
    public decimal Weight { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
