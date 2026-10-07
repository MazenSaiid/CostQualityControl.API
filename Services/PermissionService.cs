using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Permissions;
using CostQualityControl.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CostQualityControl.API.Services;

public class PermissionService(AppDbContext db, IMemoryCache cache) : IPermissionService
{
    private static string CacheKey(string role, string resource) => $"perm:{role}:{resource}";

    public async Task<bool> HasPermissionAsync(string roleName, string resource, string action)
    {
        // Admin always has full access
        if (roleName == "Admin") return true;

        var key = CacheKey(roleName, resource);
        if (!cache.TryGetValue(key, out RolePermission? perm))
        {
            perm = await db.RolePermissions.FirstOrDefaultAsync(p => p.RoleName == roleName && p.Resource == resource);
            cache.Set(key, perm, TimeSpan.FromMinutes(5));
        }
        if (perm is null) return false;
        return action switch
        {
            "view" => perm.CanView,
            "write" => perm.CanWrite,
            "delete" => perm.CanDelete,
            _ => false
        };
    }

    public async Task<List<RolePermissionDto>> GetAllAsync() =>
        await db.RolePermissions.OrderBy(p => p.RoleName).ThenBy(p => p.Resource)
            .Select(p => new RolePermissionDto(p.Id, p.RoleName, p.Resource, p.CanView, p.CanWrite, p.CanDelete))
            .ToListAsync();

    public async Task<List<RolePermissionDto>> GetForRoleAsync(string roleName) =>
        await db.RolePermissions.Where(p => p.RoleName == roleName)
            .Select(p => new RolePermissionDto(p.Id, p.RoleName, p.Resource, p.CanView, p.CanWrite, p.CanDelete))
            .ToListAsync();

    public async Task<RolePermissionDto> UpsertAsync(UpsertPermissionRequest req)
    {
        var perm = await db.RolePermissions.FirstOrDefaultAsync(p => p.RoleName == req.RoleName && p.Resource == req.Resource);
        if (perm is null)
        {
            perm = new RolePermission { RoleName = req.RoleName, Resource = req.Resource };
            db.RolePermissions.Add(perm);
        }
        perm.CanView = req.CanView;
        perm.CanWrite = req.CanWrite;
        perm.CanDelete = req.CanDelete;
        await db.SaveChangesAsync();

        // Invalidate cache
        cache.Remove(CacheKey(req.RoleName, req.Resource));

        return new RolePermissionDto(perm.Id, perm.RoleName, perm.Resource, perm.CanView, perm.CanWrite, perm.CanDelete);
    }
}
