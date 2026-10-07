using CostQualityControl.API.DTOs.Ingredients;
namespace CostQualityControl.API.Services;
public interface IIngredientService
{
    Task<List<IngredientDto>> GetAllAsync();
    Task<IngredientDto?> GetByIdAsync(int id);
    Task<IngredientDto> CreateAsync(CreateIngredientRequest request);
    Task<IngredientDto?> UpdateAsync(int id, UpdateIngredientRequest request);
    Task<bool> DeleteAsync(int id);
}
