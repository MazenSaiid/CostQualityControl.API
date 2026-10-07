namespace CostQualityControl.API.DTOs.Suppliers;

public record SupplierDto(int Id, string Name, string? ContactPerson, string? Phone, string? Email, string? Notes, DateTime CreatedAt);
public record CreateSupplierRequest(string Name, string? ContactPerson, string? Phone, string? Email, string? Notes);
