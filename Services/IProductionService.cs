using CostQualityControl.API.DTOs.Production;
namespace CostQualityControl.API.Services;
public interface IProductionService
{
    Task<List<BatchDto>> GetAllAsync();
    Task<BatchDto?> GetByIdAsync(int id);
    Task<BatchDto> CreateAsync(CreateBatchRequest request, string createdBy);
    Task<BatchDto?> UpdateAsync(int id, UpdateBatchRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<SupplierConsumptionDto>> GetSupplierConsumptionAsync();
}
