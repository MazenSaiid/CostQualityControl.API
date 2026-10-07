using Microsoft.AspNetCore.Authorization;

namespace CostQualityControl.API.Authorization;

public class PermissionRequirement(string resource, string action) : IAuthorizationRequirement
{
    public string Resource { get; } = resource;
    public string Action { get; } = action; // "view" | "write" | "delete"
}
