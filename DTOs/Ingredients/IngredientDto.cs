namespace CostQualityControl.API.DTOs.Ingredients;
public record IngredientDto(int Id, string Name, string Unit, decimal CurrentCost, int? SupplierId, string? SupplierName, DateTime LastUpdated);
public record CreateIngredientRequest(string Name, string Unit, decimal CurrentCost, int? SupplierId);
public record UpdateIngredientRequest(string Name, string Unit, decimal CurrentCost, int? SupplierId);
