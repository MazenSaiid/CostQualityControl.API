namespace CostQualityControl.API.DTOs.Auth;
public record UpdateUserRequest(string FullName, string Email, string? Password, string Role, bool IsActive);
