namespace CostQualityControl.API.Models;
public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentCost { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? SupplierEntity { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ProductIngredient> ProductIngredients { get; set; } = [];
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = [];
    public ICollection<BatchIngredient> BatchIngredients { get; set; } = [];
}
