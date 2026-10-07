namespace CostQualityControl.API.DTOs.Permissions;

public record RolePermissionDto(int Id, string RoleName, string Resource, bool CanView, bool CanWrite, bool CanDelete);
public record UpsertPermissionRequest(string RoleName, string Resource, bool CanView, bool CanWrite, bool CanDelete);
