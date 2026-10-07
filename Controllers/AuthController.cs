using System.Security.Claims;
using CostQualityControl.API.DTOs.Auth;
using CostQualityControl.API.Models;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtService jwtService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await userManager.FindByNameAsync(req.Username);
        if (user is null || !user.IsActive) return Unauthorized("Invalid credentials");
        var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid credentials");
        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);
        return Ok(new AuthResponse(token, user.UserName!, user.FullName, roles.FirstOrDefault() ?? "", DateTime.UtcNow.AddHours(8)));
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest req)
    {
        var user = new AppUser { UserName = req.Username, Email = req.Email, FullName = req.FullName };
        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        await userManager.AddToRoleAsync(user, req.Role);
        return Ok(new UserDto(user.Id, user.UserName!, user.Email!, user.FullName, req.Role, user.IsActive, user.CreatedAt));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDto(user.Id, user.UserName!, user.Email!, user.FullName, roles.FirstOrDefault() ?? "", user.IsActive, user.CreatedAt));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (req.NewPassword != req.ConfirmPassword)
            return BadRequest(new { message = "New password and confirmation do not match." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);
        if (user is null) return NotFound(new { message = "User not found." });

        var result = await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = string.Join(" ", errors) });
        }

        return Ok(new { message = "Password changed successfully." });
    }
}
