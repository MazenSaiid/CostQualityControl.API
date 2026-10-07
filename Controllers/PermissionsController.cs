using CostQualityControl.API.DTOs.Permissions;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController(IPermissionService svc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RolePermissionDto>>> GetAll() => Ok(await svc.GetAllAsync());

    [HttpGet("role/{roleName}")]
    public async Task<ActionResult<List<RolePermissionDto>>> GetForRole(string roleName) => Ok(await svc.GetForRoleAsync(roleName));

    [HttpGet("my-permissions")]
    public async Task<ActionResult<List<RolePermissionDto>>> MyPermissions()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role)) return Ok(new List<RolePermissionDto>());
        return Ok(await svc.GetForRoleAsync(role));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RolePermissionDto>> Upsert(UpsertPermissionRequest req) => Ok(await svc.UpsertAsync(req));

    [HttpPut("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<RolePermissionDto>>> BulkUpsert(List<UpsertPermissionRequest> reqs)
    {
        var results = new List<RolePermissionDto>();
        foreach (var req in reqs) results.Add(await svc.UpsertAsync(req));
        return Ok(results);
    }
}
