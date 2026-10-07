using CostQualityControl.API.DTOs.Permissions;

namespace CostQualityControl.API.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string roleName, string resource, string action); // action: "view"|"write"|"delete"
    Task<List<RolePermissionDto>> GetAllAsync();
    Task<RolePermissionDto> UpsertAsync(UpsertPermissionRequest request);
    Task<List<RolePermissionDto>> GetForRoleAsync(string roleName);
}
