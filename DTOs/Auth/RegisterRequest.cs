namespace CostQualityControl.API.DTOs.Auth;
public record RegisterRequest(string Username, string Email, string FullName, string Password, string Role);
