namespace CostQualityControl.API.DTOs.Products;
public record ProductIngredientDto(int IngredientId, string IngredientName, decimal Weight, string Unit, decimal Cost);
public record ProductDto(int Id, string Code, string Name, string Category, decimal SellingPrice, decimal TotalCost, decimal TotalWeight, decimal CostPerKg, decimal ProfitAmount, decimal ProfitPercentage, List<ProductIngredientDto> Ingredients, DateTime CreatedAt);
public record CreateProductRequest(string Code, string Name, string Category, decimal SellingPrice, List<CreateProductIngredientRequest> Ingredients);
public record CreateProductIngredientRequest(int IngredientId, decimal Weight);
public record UpdateProductRequest(string Code, string Name, string Category, decimal SellingPrice, List<CreateProductIngredientRequest> Ingredients);
