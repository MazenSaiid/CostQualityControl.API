namespace CostQualityControl.API.DTOs.Invoices;

public record InvoiceItemDto(int Id, int IngredientId, string IngredientName, decimal Weight, decimal UnitPrice, decimal TotalPrice);

public record InvoicePaymentDto(int Id, decimal Amount, DateTime Date, string? Notes, string CreatedBy, DateTime CreatedAt);

public record InvoiceDto(
    int Id,
    string InvoiceNumber,
    int? SupplierId,
    string SupplierName,
    DateTime Date,
    List<InvoiceItemDto> Items,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalDue,
    DateTime CreatedAt,
    bool IsPaid,
    List<InvoicePaymentDto> Payments
);

public record CreateInvoiceItemRequest(int IngredientId, decimal Weight, decimal UnitPrice);
public record CreateInvoiceRequest(string InvoiceNumber, int? SupplierId, DateTime Date, List<CreateInvoiceItemRequest> Items);
public record MarkPaidRequest(bool IsPaid);
public record AddPaymentRequest(decimal Amount, DateTime Date, string? Notes);
