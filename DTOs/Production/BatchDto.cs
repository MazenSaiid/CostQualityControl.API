namespace CostQualityControl.API.DTOs.Production;
public record BatchIngredientDto(int IngredientId, string IngredientName, decimal ExpectedWeight, decimal ActualWeight);
public record BatchDto(
    int Id, string BatchNumber, int ProductId, string ProductName, DateTime ProductionDate,
    decimal ExpectedWeight, decimal ActualWeight, decimal YieldPercentage, decimal WastePercentage,
    string Status, string? Notes, List<BatchIngredientDto> IngredientsUsed,
    decimal WasteWeight, decimal SellingValue, decimal CostValue, decimal ProfitValue);
public record CreateBatchIngredientRequest(int IngredientId, decimal ExpectedWeight, decimal ActualWeight);
public record CreateBatchRequest(string BatchNumber, int ProductId, DateTime ProductionDate, decimal ExpectedWeight, decimal ActualWeight, string Status, string? Notes, List<CreateBatchIngredientRequest> IngredientsUsed);
public record UpdateBatchRequest(decimal ActualWeight, string Status, string? Notes, List<CreateBatchIngredientRequest> IngredientsUsed);
public record IngredientConsumptionDto(
    string IngredientName,
    string Unit,
    decimal TotalPurchasedKg,
    decimal TotalPurchasedValue,
    decimal TotalUsedKg,
    decimal TotalUsedValue,
    decimal RemainingKg,
    decimal RemainingValue,
    decimal UsagePercent
);

public record SupplierConsumptionDto(
    string Supplier,
    List<IngredientConsumptionDto> Ingredients,
    decimal TotalPurchasedKg,
    decimal TotalPurchasedValue,
    decimal TotalUsedKg,
    decimal TotalUsedValue,
    decimal RemainingKg,
    decimal RemainingValue,
    decimal UsagePercent
);
