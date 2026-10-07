using CostQualityControl.API.DTOs.Suppliers;

namespace CostQualityControl.API.Services;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync();
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest req);
    Task<SupplierDto?> UpdateAsync(int id, CreateSupplierRequest req);
    Task<bool> DeleteAsync(int id);
}
