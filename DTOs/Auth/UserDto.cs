namespace CostQualityControl.API.DTOs.Auth;
public record UserDto(string Id, string Username, string Email, string FullName, string Role, bool IsActive, DateTime CreatedAt);
