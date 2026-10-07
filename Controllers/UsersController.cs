using CostQualityControl.API.DTOs.Auth;
using CostQualityControl.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Users:view")]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await userManager.Users.ToListAsync();
        var result = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            result.Add(new UserDto(u.Id, u.UserName!, u.Email!, u.FullName, roles.FirstOrDefault() ?? "", u.IsActive, u.CreatedAt));
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Users:write")]
    public async Task<ActionResult<UserDto>> Update(string id, UpdateUserRequest req)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        user.FullName = req.FullName; user.Email = req.Email; user.IsActive = req.IsActive;
        await userManager.UpdateAsync(user);
        if (!string.IsNullOrEmpty(req.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, req.Password);
        }
        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, req.Role);
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDto(user.Id, user.UserName!, user.Email!, user.FullName, roles.FirstOrDefault() ?? "", user.IsActive, user.CreatedAt));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Users:delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        user.IsActive = false;
        await userManager.UpdateAsync(user);
        return NoContent();
    }
}
