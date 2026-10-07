namespace CostQualityControl.API.DTOs.Financials;

public record BatchRevenueDto(
    int BatchId, string BatchNumber, string ProductName, DateTime ProductionDate,
    decimal ActualWeight, decimal SellingPrice, decimal Revenue,
    decimal IngredientCost, decimal GrossProfit, decimal GrossMarginPct
);

public record SupplierFinancialDto(
    string Supplier,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal TotalOwed,
    decimal RevenueGenerated,
    decimal NetPosition
);

public record FinancialSummaryDto(
    decimal TotalRevenue,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal TotalOwed,
    decimal GrossProfit,
    decimal NetCashPosition,
    List<BatchRevenueDto> BatchBreakdown,
    List<SupplierFinancialDto> SupplierBreakdown
);
