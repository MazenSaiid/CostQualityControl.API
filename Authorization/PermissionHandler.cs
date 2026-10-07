using System.Security.Claims;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace CostQualityControl.API.Authorization;

public class PermissionHandler(IPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim)) return;

        if (await permissionService.HasPermissionAsync(roleClaim, requirement.Resource, requirement.Action))
            context.Succeed(requirement);
    }
}
